using System.Text;
using System.Xml.Linq;
using KbinXml.Net;
using Stella.Util;

namespace Stella.Middleware
{
    public class EAmuseXrpcInputMiddleware
    {
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

            // Check if this is an EAMUSE request
            bool isEamuse = IsEAmuseRequest(context.Request);
            if (isEamuse)
            {
                _logger.LogInformation("Processing EAMUSE XRPC request");
                
                // Enable buffering to allow reading body multiple times
                // context.Request.EnableBuffering();

                // if (context.Request.Body.Length == 0)
                // {
                //     _logger.LogWarning("Empty request body");
                //     await _next(context);
                //     return;
                // }

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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing EAMUSE request body");
                }
            }

            await _next(context);
        }

        private bool IsEAmuseRequest(HttpRequest request)
        {
            // var contentType = request.Headers.ContentType;
            // Console.WriteLine(contentType);
            // if (string.IsNullOrEmpty(contentType) || contentType != "application/octet-stream")
            //     return false;

            if (!request.Headers.TryGetValue("User-Agent", out var ua))
                return false;
            
            if (!ua.ToString().Equals("EAMUSE.XRPC/1.0", StringComparison.OrdinalIgnoreCase))
                return false;
            
            if (!request.Headers.TryGetValue("X-Compress", out var compressHeader))
                return false;
            
            var compAlgo = compressHeader.ToString();
            return compAlgo.Equals("lz77", StringComparison.OrdinalIgnoreCase) || 
                   compAlgo.Equals("none", StringComparison.OrdinalIgnoreCase);
            // return true;
        }

        private async Task<(XDocument?, string?)> ReadAndProcessBodyAsync(HttpRequest request)
        {
            if (!request.Headers.TryGetValue("X-Compress", out var compressHeader))
                return (null, null);

            var compAlgo = compressHeader.ToString();

            string eAmuseInfo = null;
            if (request.Headers.TryGetValue("X-Eamuse-Info", out var eAmuseHeader))
                eAmuseInfo = eAmuseHeader.ToString();

            byte[] data;
            using (var ms = new MemoryStream((int)(request.ContentLength ?? 512)))
            {
                await request.Body.CopyToAsync(ms);
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
    }
}
