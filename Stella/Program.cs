using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using KbinXml.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Stella.Abstractions;
using Stella.Middleware;
using Stella.Services;
using Stella.Abstractions.Configuration;
using Stella.WebUI;
using Stella.Util;

namespace Stella
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddLogging(x => x.AddConsole());

            builder.Services.AddControllers();

            // Bind Stella/WebUI options from appsettings.json (no env vars).
            StellaOptions.Bind(builder.Configuration);
            builder.WebHost.UseUrls(builder.Configuration["Urls"] ?? "http://+:80");

            var pluginService =
                new PluginService(LoggerFactory.Create(x => x.AddConsole()).CreateLogger<PluginService>());
            await pluginService.LoadPluginsAsync();

            builder.Services.AddSingleton(pluginService);

            foreach (var plugin in pluginService.LoadedPlugins)
            {
                await plugin.OnBuilderInitialize(builder);
                pluginService.RegisterPluginConfig(plugin);
                if (!plugin.PluginConfig.Enabled)
                {
                    continue;
                }
            }

            // Register WebUI services (Razor Pages, cookie auth, antiforgery, plugin
            // ApplicationParts) and per-plugin AJAX event handlers.
            if (StellaOptions.WebUIEnabled)
            {
                builder.Services.AddStellaWebUI(pluginService);
                foreach (var plugin in pluginService.LoadedPlugins)
                {
                    if (!plugin.PluginConfig.Enabled) continue;
                    var router = WebUIEventRegistryStore.GetOrCreate(plugin.Name);
                    plugin.RegisterWebUIEvents(router);
                }
            }

            var app = builder.Build();
            foreach (var plugin in pluginService.LoadedPlugins)
            {
                await plugin.OnAppInitialize(app);
            }

            if (StellaOptions.WebUIEnabled)
            {
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseStaticFiles();
                app.MapStellaWebUI(pluginService);
            }

            app.UseMiddleware<EAmuseXrpcInputMiddleware>();

            var eamuseGroup = app.MapGroup("eamuse");
            var core = app.MapGroup("core");

            eamuseGroup.MapPost("/", async ([FromQuery] string model, [FromQuery] string? module, [FromQuery] string? method, [FromQuery] string? f,
                HttpContext httpContext, ILogger<Program> logger, [FromServices] PluginService pluginService) =>
            {
                httpContext.Request.EnableBuffering();

                try
                {
                    // Get processed EAMUSE data from middleware
                    var eAmuseData = httpContext.Items["ea"] as EAmuseXrpcData;

                    // Resolve routing: prefer "f" param (modern: service.method),
                    // fall back to "module"+"method" params (legacy: e.g.
                    // module=services&method=get).
                    string service;
                    string method1;
                    if (!string.IsNullOrWhiteSpace(f) && f.IndexOf('.') >= 0)
                    {
                        var fParts = f.Split('.', 2);
                        service = fParts[0];
                        method1 = fParts[1];
                    }
                    else if (!string.IsNullOrWhiteSpace(module) && !string.IsNullOrWhiteSpace(method))
                    {
                        service = module;
                        method1 = method;
                    }
                    else
                    {
                        logger.LogWarning("Invalid or missing routing params: f={f}, module={module}, method={method}", f ?? "<null>", module ?? "<null>", method ?? "<null>");
                        return;
                    }

                    //TODO ADD PCBID Checking here
                    logger.LogInformation(model);

                    if (eAmuseData == null)
                    {
                        logger.LogWarning("No EAMUSE data available for {service}/{method1}", service, method1);
                        return;
                    }

                    try
                    {
                        var result = pluginService.InvokeHandler(service, method1, eAmuseData.Document, model, httpContext);

                        if (result.Result != null)
                        {
                            logger.LogInformation("Handler invoked successfully for {service}/{method1}", service, method1);

                            var eAmuseResponse = result.Result;

                            try
                            {
                                Type returnType = result.ReturnType;

                                // Handle async methods - await the task
                                IStellaEAmuseResponse actualResponse;
                                if (eAmuseResponse is Task<IStellaEAmuseResponse> asyncResponse)
                                {
                                    actualResponse = await asyncResponse;
                                }
                                else if (eAmuseResponse is Task task)
                                {
                                    await task;
                                    actualResponse = (IStellaEAmuseResponse)(task.GetType().GetProperty("Result")?.GetValue(task));
                                }
                                else
                                {
                                    actualResponse = eAmuseResponse as IStellaEAmuseResponse;
                                }

                                var data = await WriteEAmuseResponseAsync(httpContext.Request.Body, httpContext, actualResponse, returnType);
                                logger.LogInformation("EAMUSE response written: {bytes} bytes, compression: {algo}", data.length, data.compressionAlgo);

                                await httpContext.Response.BodyWriter.WriteAsync(data.ResponseData);

                                return;
                            }
                            catch (StellaHandlerException ex)
                            {
                                // logger.LogError(ex, "Handler error: {errorCode}", ex.ErrorCode);
                                var data = await WriteEAmuseExceptionResponseAsync(httpContext.Request.Body, httpContext, ex.ErrorCode);
                                logger.LogInformation("EAMUSE exception response written: {bytes} bytes, compression: {algo}", data.length, data.compressionAlgo);
                                await httpContext.Response.BodyWriter.WriteAsync(data.ResponseData);
                                return;
                            }
                            catch (AggregateException ex)
                            {
                                // Handle AggregateException from Task failures
                                var stellaException = ex.InnerException as StellaHandlerException;
                                if (stellaException != null)
                                {
                                    // logger.LogError(ex, "Handler error: {errorCode}", stellaException.ErrorCode);
                                    var data = await WriteEAmuseExceptionResponseAsync(httpContext.Request.Body, httpContext, stellaException.ErrorCode);
                                    logger.LogInformation("EAMUSE exception response written: {bytes} bytes, compression: {algo}", data.length, data.compressionAlgo);
                                    await httpContext.Response.BodyWriter.WriteAsync(data.ResponseData);
                                    return;
                                }
                                throw;
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Error processing EAMUSE response");
                            }
                            return;
                        }
                        else
                        {
                            logger.LogWarning("No handler found for {service}/{method1}", service, method1);
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error invoking handler");
                    }
                    
                    
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing request");
                    return;

                }
            });

            core.MapPost("/", async ([FromQuery] string model, [FromQuery] string? module, [FromQuery] string? method, [FromQuery] string? f,
                HttpContext httpContext, ILogger<Program> logger, [FromServices] PluginService pluginService) =>
            {
                httpContext.Request.EnableBuffering();

                try
                {
                    // Get processed EAMUSE data from middleware
                    var eAmuseData = httpContext.Items["ea"] as EAmuseXrpcData;

                    // Resolve routing: prefer "f" param (modern: service.method),
                    // fall back to "module"+"method" params (legacy: e.g.
                    // module=services&method=get).
                    string service;
                    string method1;
                    if (!string.IsNullOrWhiteSpace(f) && f.IndexOf('.') >= 0)
                    {
                        var fParts = f.Split('.', 2);
                        service = fParts[0];
                        method1 = fParts[1];
                    }
                    else if (!string.IsNullOrWhiteSpace(module) && !string.IsNullOrWhiteSpace(method))
                    {
                        service = module;
                        method1 = method;
                    }
                    else
                    {
                        logger.LogWarning("Invalid or missing routing params: f={f}, module={module}, method={method}", f ?? "<null>", module ?? "<null>", method ?? "<null>");
                        return;
                    }

                    //TODO ADD PCBID Checking here
                    logger.LogInformation(model);

                    if (eAmuseData == null)
                    {
                        logger.LogWarning("No EAMUSE data available for {service}/{method1}", service, method1);
                        return;
                    }

                    try
                    {
                        var result = pluginService.InvokeHandler(service, method1, eAmuseData.Document, model, httpContext);

                        if (result.Result != null)
                        {
                            logger.LogInformation("Handler invoked successfully for {service}/{method1}", service, method1);

                            var eAmuseResponse = result.Result;

                            try
                            {
                                Type returnType = result.ReturnType;

                                // Handle async methods - await the task
                                IStellaEAmuseResponse actualResponse;
                                if (eAmuseResponse is Task<IStellaEAmuseResponse> asyncResponse)
                                {
                                    actualResponse = await asyncResponse;
                                }
                                else if (eAmuseResponse is Task task)
                                {
                                    await task;
                                    actualResponse = (IStellaEAmuseResponse)(task.GetType().GetProperty("Result")?.GetValue(task));
                                }
                                else
                                {
                                    actualResponse = eAmuseResponse as IStellaEAmuseResponse;
                                }

                                var data = await WriteEAmuseResponseAsync(httpContext.Request.Body, httpContext, actualResponse, returnType);
                                logger.LogInformation("EAMUSE response written: {bytes} bytes, compression: {algo}", data.length, data.compressionAlgo);

                                await httpContext.Response.BodyWriter.WriteAsync(data.ResponseData);

                                return;
                            }
                            catch (StellaHandlerException ex)
                            {
                                // logger.LogError(ex, "Handler error: {errorCode}", ex.ErrorCode);
                                var data = await WriteEAmuseExceptionResponseAsync(httpContext.Request.Body, httpContext, ex.ErrorCode);
                                logger.LogInformation("EAMUSE exception response written: {bytes} bytes, compression: {algo}", data.length, data.compressionAlgo);
                                await httpContext.Response.BodyWriter.WriteAsync(data.ResponseData);
                                return;
                            }
                            catch (AggregateException ex)
                            {
                                // Handle AggregateException from Task failures
                                var stellaException = ex.InnerException as StellaHandlerException;
                                if (stellaException != null)
                                {
                                    // logger.LogError(ex, "Handler error: {errorCode}", stellaException.ErrorCode);
                                    var data = await WriteEAmuseExceptionResponseAsync(httpContext.Request.Body, httpContext, stellaException.ErrorCode);
                                    logger.LogInformation("EAMUSE exception response written: {bytes} bytes, compression: {algo}", data.length, data.compressionAlgo);
                                    await httpContext.Response.BodyWriter.WriteAsync(data.ResponseData);
                                    return;
                                }
                                throw;
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Error processing EAMUSE response");
                            }
                            return;
                        }
                        else
                        {
                            logger.LogWarning("No handler found for {service}/{method1}", service, method1);
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error invoking handler");
                    }


                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing request");
                    return;

                }
            });

            // app.UseMiddleware<EAmuseXrpcOutputMiddleware>();


            app.Use((context, func) =>
            {
                Console.WriteLine(context.Request.GetDisplayUrl());
                return func();
            });

            await app.RunAsync();
        }

        private static async Task<(byte[] ResponseData, int length, string compressionAlgo)> WriteEAmuseResponseAsync(Stream originalStream, HttpContext context, IStellaEAmuseResponse res, Type returnType)
        {
            var (rawData, compAlgo, eAmuseInfo) = await Task.Run(() =>
            {
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);
                XmlWriter writer = new XmlTextWriter(sw);

                writer.WriteStartElement("response");

                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");

                // Multi-element responses (asphyxia send.object([{...},{...},...]))
                // serialize as several sibling elements under <response>.
                if (res is IStellaMultiElementResponse multi)
                {
                    foreach (var element in multi.Elements)
                    {
                        new XmlSerializer(element.GetType()).Serialize(writer, element, ns);
                    }
                }
                else
                {
                    // Get the actual response type (not the Task type)
                    var serializationType = res.GetType();
                    if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        // If returnType is Task<T>, use T for serialization
                        serializationType = returnType.GetGenericArguments()[0];
                    }
                    new XmlSerializer(serializationType).Serialize(writer, res, ns);
                }

                var data = context.Items["ea"] as EAmuseXrpcData;

                writer.WriteEndElement();

                XDocument document = XDocument.Parse(sb.ToString());

                // XmlSerializer renders null nullable VALUE types (int?, ulong?, etc.)
                // as <foo xsi:nil="true" /> with xmlns:xsi/xsd namespace declarations.
                // KBinXML has no namespace support and the ':' in the attribute name
                // ("xsi:nil") corrupts the binary output. Strip these: remove elements
                // marked xsi:nil (they represent null values that should be omitted) and
                // drop all namespace-declaration attributes.
                var xsiNs = (XNamespace)"http://www.w3.org/2001/XMLSchema-instance";
                foreach (var nilEl in document.Descendants().Where(e => e.Attribute(xsiNs + "nil") != null).ToList())
                    nilEl.Remove();
                foreach (var attr in document.Descendants().SelectMany(e => e.Attributes()).Where(a => a.IsNamespaceDeclaration).ToList())
                    attr.Remove();

                // Add __type attributes to all elements based on the response type
                if (res is IStellaMultiElementResponse multiTypes)
                {
                    foreach (var element in multiTypes.Elements)
                        document.AddKBinTypesFromResponse(element);
                }
                else
                {
                    document.AddKBinTypesFromResponse(res);
                }

                byte[] resData;
                if (data.Encoding != null)
                    resData = KbinConverter.Write(document, data.Encoding.ToKnownEncoding(), new WriteOptions());
                else
                    resData = KbinConverter.Write(document, KnownEncodings.ShiftJIS, new WriteOptions());


                string algo = "none";

                // Try compression
                byte[] compressed = LZ77.Compress(resData, 32);
                if (compressed.Length < resData.Length)
                {
                    resData = compressed;
                    algo = "lz77";
                }

                // Apply encryption if needed
                string eAmuseInfoValue = data.EAmuseInfo;
                if (eAmuseInfoValue != null)
                    RC4.ApplyEAmuseInfo(eAmuseInfoValue, resData);

                return (resData, algo, eAmuseInfoValue);
            });

            // Set response headers
            if (eAmuseInfo != null)
                context.Response.Headers.Add("X-Eamuse-Info", eAmuseInfo);

            context.Response.Headers.Add("X-Compress", compAlgo);
            context.Response.ContentType = "application/octet-stream";
            context.Response.ContentLength = rawData.Length;

            return (rawData, rawData.Length, compAlgo);
        }

        private static async Task<(byte[] ResponseData, int length, string compressionAlgo)> WriteEAmuseExceptionResponseAsync(Stream originalStream, HttpContext context, int code)
        {
            var data = context.Items["ea"] as EAmuseXrpcData;
            string? eAmuseInfo = data?.EAmuseInfo;

            var (rawData, compAlgo) = await Task.Run(() => EAmuseResponseWriter.BuildStatusResponse(code, eAmuseInfo));

            // Set response headers
            if (eAmuseInfo != null)
                context.Response.Headers.Add("X-Eamuse-Info", eAmuseInfo);

            context.Response.Headers.Add("X-Compress", compAlgo);
            context.Response.ContentType = "application/octet-stream";
            context.Response.ContentLength = rawData.Length;

            return (rawData, rawData.Length, compAlgo);
        }
    }
}

