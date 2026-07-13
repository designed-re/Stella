using System.Text;
using System.Xml.Linq;
using KbinXml.Net;
using Microsoft.AspNetCore.Http.Features;
using Stella.Abstractions;
using Stella.Util;

namespace Stella.Middleware
{
    public class EAmuseXrpcInputMiddleware
    {
        // Hard cap on the request body. e-amusement payloads are small; 64 MiB is a
        // generous upper bound that prevents memory-exhaustion DoS. Kestrel also
        // enforces this via IHttpMaxRequestBodySizeFeature so chunked bodies are capped.
        private const long MaxRequestBodySize = 64 * 1024 * 1024;

        private readonly RequestDelegate _next;
        private readonly ILogger<EAmuseXrpcInputMiddleware> _logger;

        public EAmuseXrpcInputMiddleware(RequestDelegate next, ILogger<EAmuseXrpcInputMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method == "GET" || !context.Request.Path.ToString().Contains("eamuse", StringComparison.CurrentCultureIgnoreCase))
            {
                await _next(context);
                return;
            }

            // Ask Kestrel to enforce the body size limit (memory-exhaustion DoS guard).
            var maxBodyFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
            if (maxBodyFeature is { IsReadOnly: false })
            {
                maxBodyFeature.MaxRequestBodySize = MaxRequestBodySize;
            }

            // Check if this is an EAMUSE request
            bool isEamuse = IsEAmuseRequest(context.Request);
            if (isEamuse)
            {
                _logger.LogInformation("Processing EAMUSE XRPC request");

                try
                {
                    var (data, eAmuseInfo) = await ReadAndProcessBodyAsync(context.Request);

                    if (data != null)
                    {
                        // Store processed data in HttpContext items for downstream handlers
                        context.Items["ea"] = new EAmuseXrpcData()
                        {
                            Document = data,
                            Encoding = Encoding.GetEncoding("SHIFT-JIS"),
                            EAmuseInfo = eAmuseInfo
                        };
                        _logger.LogInformation("Successfully processed EAMUSE request");
                    }
                }
                catch (RequestTooLargeException)
                {
                    // Body exceeded the limit. e-amusement clients ignore HTTP status
                    // codes, so reply with a KBinXml <response status="..."/> error
                    // (mirroring the handler exception response) instead of a 413.
                    _logger.LogWarning("EAMUSE request body exceeded {max} bytes; returning error response", MaxRequestBodySize);
                    await WriteErrorResponseAsync(context, StellaHandlerException.BadRequestCode);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing EAMUSE request body");
                }
            }

            await _next(context);
        }

        private async Task WriteErrorResponseAsync(HttpContext context, int code)
        {
            string? eAmuseInfo = null;
            if (context.Request.Headers.TryGetValue("X-Eamuse-Info", out var eAmuseHeader))
                eAmuseInfo = eAmuseHeader.ToString();

            var (rawData, compAlgo) = EAmuseResponseWriter.BuildStatusResponse(code, eAmuseInfo);

            if (eAmuseInfo != null)
                context.Response.Headers["X-Eamuse-Info"] = eAmuseInfo;
            context.Response.Headers["X-Compress"] = compAlgo;
            context.Response.ContentType = "application/octet-stream";
            context.Response.ContentLength = rawData.Length;

            await context.Response.BodyWriter.WriteAsync(rawData);
        }

        private static bool IsEAmuseRequest(HttpRequest request)
        {
            if (!request.Headers.TryGetValue("User-Agent", out var ua))
                return false;

            if (!ua.ToString().Equals("EAMUSE.XRPC/1.0", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!request.Headers.TryGetValue("X-Compress", out var compressHeader))
                return false;

            var compAlgo = compressHeader.ToString();
            return compAlgo.Equals("lz77", StringComparison.OrdinalIgnoreCase) ||
                   compAlgo.Equals("none", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<(XDocument?, string?)> ReadAndProcessBodyAsync(HttpRequest request)
        {
            if (!request.Headers.TryGetValue("X-Compress", out var compressHeader))
                return (null, null);

            var compAlgo = compressHeader.ToString();

            string eAmuseInfo = null;
            if (request.Headers.TryGetValue("X-Eamuse-Info", out var eAmuseHeader))
                eAmuseInfo = eAmuseHeader.ToString();

            // Reject oversized requests up front when Content-Length is known.
            if (request.ContentLength is long len && len > MaxRequestBodySize)
                throw new RequestTooLargeException();

            byte[] data;
            int capacity = (int)Math.Min(MaxRequestBodySize, request.ContentLength ?? 512);
            using (var ms = new MemoryStream(capacity))
            {
                // Bounded copy: cap at MaxRequestBodySize so spoofed/missing
                // Content-Length cannot exhaust memory.
                var buffer = new byte[64 * 1024];
                long total = 0;
                int read;
                while ((read = await request.Body.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    total += read;
                    if (total > MaxRequestBodySize)
                        throw new RequestTooLargeException();
                    ms.Write(buffer, 0, read);
                }
                data = ms.ToArray();
            }

            return await ProcessDataAsync(data, eAmuseInfo, compAlgo);
        }

        private async Task<(XDocument?, string?)> ProcessDataAsync(byte[] data, string eAmuseInfo, string compAlgo)
        {
            // Decrypt if needed
            if (eAmuseInfo != null)
                RC4.ApplyEAmuseInfo(eAmuseInfo, data);

            // Decompress if needed
            data = compAlgo.Equals("lz77", StringComparison.OrdinalIgnoreCase)
                ? LZ77.Decompress(data)
                : compAlgo.Equals("none", StringComparison.OrdinalIgnoreCase)
                    ? data
                    : null;

            if (data == null)
                return (null, eAmuseInfo);

            try
            {
                var result = await Task.Run(() => KbinConverter.ReadXmlLinq(data));
                return (result, eAmuseInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Got invalid binary XML input!");
                return (null, eAmuseInfo);
            }
        }

        private sealed class RequestTooLargeException : Exception { }
    }
}
