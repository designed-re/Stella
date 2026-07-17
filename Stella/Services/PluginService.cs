using System.Collections.Concurrent;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using Stella.Abstractions;
using Stella.Abstractions.Plugins;

namespace Stella.Services
{
    public class PluginService(ILogger<PluginService> logger)
    {
        private readonly List<IStellaPlugin> _loadedPlugins = new();
        private readonly Dictionary<string, (Type HandlerType, MethodInfo Method, IStellaPluginConfig PluginConfig, ILogger Logger)> _handlerCache = new();
        private static readonly ConcurrentDictionary<Type, XmlSerializer> _serializerCache = new();

        public IReadOnlyList<IStellaPlugin> LoadedPlugins => _loadedPlugins.AsReadOnly();

    public async Task LoadPluginsAsync()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "plugins");

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        var dlls = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
        foreach (var dll in dlls)
        {
            try
            {
                var assembly = Assembly.Load(await File.ReadAllBytesAsync(dll));
                logger.LogInformation($"Loaded plugin assembly: {assembly.FullName}");

                // Load IStellaPlugin implementations only.
                // Handler classes are cached later by RegisterPluginConfig, once each
                // plugin's configuration has been resolved (see Program.cs init order).
                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(IStellaPlugin).IsAssignableFrom(t) && !t.IsInterface);

                foreach (var pluginType in pluginTypes)
                {
                    try
                    {
                        if (Activator.CreateInstance(pluginType) is IStellaPlugin instance)
                        {
                            _loadedPlugins.Add(instance);
                            StellaPluginRegistry.Register(instance);
                            logger.LogInformation($"Loaded plugin: {instance.Name} v{instance.Version}");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Failed to instantiate plugin {pluginType.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to load plugin from {dll}: {ex.Message}");
            }
        }
    }

        public void RegisterPluginConfig(IStellaPlugin pluginInstance)
        {
            var config = pluginInstance.PluginConfig;

            var handlers = pluginInstance.GetType().Assembly.GetTypes()
                .Where(t => !t.IsInterface && !t.IsAbstract && t.GetMethods()
                    .Any(m => m.GetCustomAttribute<StellaHandlerAttribute>() != null));

            foreach (var handlerType in handlers)
            {
                CacheHandlersFromType(handlerType, config);
            }

            logger.LogInformation($"Total handlers cached: {_handlerCache.Count}");
        }

        private void CacheHandlersFromType(Type handlerType, IStellaPluginConfig pluginConfig = null)
        {
            // Get all public instance methods from the class and all its base classes
            var methods = handlerType.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance);

            foreach (var methodInfo in methods)
            {
                // Skip methods from base Object class
                if (methodInfo.DeclaringType == typeof(object))
                    continue;

                var handlerAttr = methodInfo.GetCustomAttribute<StellaHandlerAttribute>();
                if (handlerAttr != null)
                {
                    var key = $"{handlerAttr.Service}:{handlerAttr.Module}";

                    if (_handlerCache.ContainsKey(key))
                    {
                        logger.LogWarning($"Handler {key} already exists, overwriting with {handlerType.Name}.{methodInfo.Name}");
                    }

                    _handlerCache[key] = (handlerType, methodInfo, pluginConfig, LoggerFactory.Create(x => x.AddConsole()).CreateLogger(handlerType));
                    logger.LogInformation($"Cached handler: {key} -> {handlerType.Name}.{methodInfo.Name}");
                }
            }
        }

        public (Task<IStellaEAmuseResponse>? Result, Type ReturnType) InvokeHandler(string service, string method, XDocument requestData, string model, HttpContext context)
        {
            var key = $"{service}:{method}";

            if (!_handlerCache.TryGetValue(key, out var handler))
            {
                logger.LogWarning($"No handler found for {service}/{method}");
                return (null, null);
            }

            try
            {
                logger.LogDebug($"Invoking handler: {service}/{method}");

                // Create a fresh handler instance per request. Handler instances carry
                // per-request state (Request, HttpContext, PCBId, ...) and must NOT be
                // shared across concurrent requests.
                var instance = Activator.CreateInstance(handler.HandlerType);
                if (instance is not StellaHandler stellaHandler)
                {
                    logger.LogError($"Handler {handler.HandlerType.Name} does not extend StellaHandler");
                    return (null, null);
                }

                if (requestData?.Root != null)
                {
                    var requestType = handler.Method.GetCustomAttribute<StellaHandlerAttribute>()?.RequestType;
                    if (requestType != null)
                    {
                        // Pre-process XML to convert space-separated numeric arrays to individual elements
                        PreprocessXmlForArrays(requestData);

                        var serializer = GetSerializer(requestType);
                        using (var reader = requestData.Root.FirstNode.CreateReader())
                        {
                            var reqData = serializer.Deserialize(reader) as IStellaEAmuseRequest;
                            if (reqData == null)
                            {
                                logger.LogError($"Failed to deserialize request body for {service}/{method} ({requestType.Name})");
                                throw new StellaHandlerException(StellaHandlerException.BadRequestCode);
                            }
                            stellaHandler.Request = reqData;
                        }
                    }
                }

                stellaHandler.Model = model;
                stellaHandler.PluginConfig = handler.PluginConfig;
                stellaHandler.Logger = handler.Logger;
                stellaHandler.PCBId = requestData?.Root?.Attribute("srcid")?.Value;
                stellaHandler.HttpContext = context;

                var returnType = handler.Method.ReturnType;
                var result = handler.Method.Invoke(instance, Array.Empty<object>());

                // Handle async methods (Task<T>) and sync methods
                if (result is Task task)
                {
                    // For async methods, convert to Task<IStellaEAmuseResponse>
                    return (ConvertTaskToGeneric(task), returnType);
                }
                else if (result is IStellaEAmuseResponse response)
                {
                    // For sync methods, wrap in completed task
                    return (Task.FromResult(response), returnType);
                }

                logger.LogError($"Handler returned invalid type: {result?.GetType().Name ?? "null"}");
                return (null, null);
            }
            catch (TargetInvocationException ex)
            {
                // Unwrap the actual exception from reflection
                logger.LogError(ex.InnerException, $"Error invoking handler {service}/{method}");
                if (ex.InnerException is StellaHandlerException she)
                    throw she;
                return (null, null);
            }
            catch (StellaHandlerException)
            {
                // Propagate so Program.cs can build a proper e-amusement error response.
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error invoking handler {service}/{method}");
                return (null, null);
            }
        }

        private static XmlSerializer GetSerializer(Type requestType)
        {
            return _serializerCache.GetOrAdd(requestType, static t => new XmlSerializer(t));
        }

        /// <summary>
        /// Pre-processes XML to convert space-separated numeric arrays into individual elements.
        /// For example: &lt;judge __count="7"&gt;0 0 0 8 0 0 0&lt;/judge&gt;
        /// Becomes: &lt;judge&gt;0&lt;/judge&gt;&lt;judge&gt;0&lt;/judge&gt;...
        /// </summary>
        private void PreprocessXmlForArrays(XDocument doc)
        {
            if (doc?.Root == null)
                return;

            ProcessElementForArrays(doc.Root);
        }

        private void ProcessElementForArrays(XElement element)
        {
            // Process all child elements
            var childElements = element.Elements().ToList();

            foreach (var child in childElements)
            {
                // Check if element has __count attribute (indicates it's an array)
                var countAttr = child.Attribute("__count");
                if (countAttr != null && int.TryParse(countAttr.Value, out var count))
                {
                    // Split the space-separated values
                    var values = child.Value.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (values.Length > 0)
                    {
                        // Create new elements for each value
                        var elementName = child.Name.LocalName;
                        var parentElement = child.Parent;

                        // Get the index of current element
                        var siblings = parentElement.Elements(elementName).ToList();
                        var currentIndex = siblings.IndexOf(child);

                        // Remove the original element
                        child.Remove();

                        // Insert new elements with individual values
                        XElement insertAfter = currentIndex > 0 ? siblings[currentIndex - 1] : null;

                        foreach (var value in values)
                        {
                            var newElement = new XElement(child.Name, value);

                            // Copy attributes except __count
                            foreach (var attr in child.Attributes())
                            {
                                if (attr.Name.LocalName != "__count")
                                {
                                    newElement.Add(new XAttribute(attr.Name, attr.Value));
                                }
                            }

                            if (insertAfter == null)
                            {
                                parentElement.AddFirst(newElement);
                            }
                            else
                            {
                                insertAfter.AddAfterSelf(newElement);
                            }

                            insertAfter = newElement;
                        }
                    }
                }
                else
                {
                    // Recursively process child elements
                    ProcessElementForArrays(child);
                }
            }
        }

        private Task<IStellaEAmuseResponse> ConvertTaskToGeneric(Task task)
        {
            return task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    // If there's only one inner exception, throw it directly
                    // Otherwise, throw the AggregateException
                    if (t.Exception.InnerExceptions.Count == 1)
                    {
                        throw t.Exception.InnerException;
                    }
                    else
                    {
                        throw t.Exception;
                    }
                }

                // Get the Result property from the generic Task<T>
                var resultProperty = t.GetType().GetProperty("Result");
                if (resultProperty == null)
                {
                    logger.LogError($"Task type {t.GetType().Name} does not have a Result property");
                    return null;
                }

                var result = resultProperty.GetValue(t) as IStellaEAmuseResponse;
                if (result == null)
                {
                    logger.LogError($"Task Result is not an IStellaEAmuseResponse: {resultProperty.GetValue(t)?.GetType().Name ?? "null"}");
                }

                return result;
            }, TaskScheduler.Default);
        }
    }
}
