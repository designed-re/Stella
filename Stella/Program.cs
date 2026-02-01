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

            builder.WebHost.UseUrls("http://+:80");

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

            var app = builder.Build();
            foreach (var plugin in pluginService.LoadedPlugins)
            {
                await plugin.OnAppInitialize(app);
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

                    //TODO ADD PCBID Checking here
                    logger.LogInformation(model);
                    // Invoke handler from loaded plugins
                    var service = f.Split('.')[0];
                    var method1 = f.Split('.')[1];
                    
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

                    //TODO ADD PCBID Checking here
                    logger.LogInformation(model);
                    // Invoke handler from loaded plugins
                    var service = f.Split('.')[0];
                    var method1 = f.Split('.')[1];

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
                
                // Get the actual response type (not the Task type)
                var serializationType = res.GetType();
                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    // If returnType is Task<T>, use T for serialization
                    serializationType = returnType.GetGenericArguments()[0];
                }
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                new XmlSerializer(serializationType).Serialize(writer, res, ns);

                var data = context.Items["ea"] as EAmuseXrpcData;

                writer.WriteEndElement();

                XDocument document = XDocument.Parse(sb.ToString());

                // Add __type attributes to all elements based on the response type
                document.AddKBinTypesFromResponse(res);

                // Console.WriteLine(document);
                byte[] resData;
                if (data.Encoding != null)
                    resData = KbinConverter.Write(document, data.Encoding.ToKnownEncoding());
                else
                    resData = KbinConverter.Write(document, KnownEncodings.ShiftJIS);

                // Console.WriteLine(KbinConverter.ReadXmlLinq(resData));

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
            var (rawData, compAlgo, eAmuseInfo) = await Task.Run(() =>
            {
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);
                XmlWriter writer = new XmlTextWriter(sw);

                writer.WriteStartElement("response");
                writer.WriteAttributeString("status", code.ToString());
                var data = context.Items["ea"] as EAmuseXrpcData;

                writer.WriteEndElement();

                XDocument document = XDocument.Parse(sb.ToString());

                // Add __type attributes to all elements based on their values
                // Note: For exception response, there are no data elements to add types to

                Console.WriteLine(document);

                byte[] resData;
                if (data.Encoding != null)
                    resData = KbinConverter.Write(document, data.Encoding.ToKnownEncoding());
                else
                    resData = KbinConverter.Write(document, KnownEncodings.ShiftJIS);

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
    }
}

