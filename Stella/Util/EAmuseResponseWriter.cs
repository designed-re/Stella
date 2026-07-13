using System.Text;
using System.Xml;
using System.Xml.Linq;
using KbinXml.Net;

namespace Stella.Util
{
    /// <summary>
    /// Builds an e-amusement style <c>&lt;response status="..."/&gt;</c> error reply,
    /// KBinXml-encoded (SHIFT-JIS), optionally LZ77-compressed and RC4-encrypted using
    /// the request's <c>X-Eamuse-Info</c> header. Shared by the route handlers (for
    /// handler exceptions) and the input middleware (for oversized/malformed requests).
    /// </summary>
    public static class EAmuseResponseWriter
    {
        public static (byte[] ResponseData, string CompressionAlgo) BuildStatusResponse(int code, string? eAmuseInfo)
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            XmlWriter writer = new XmlTextWriter(sw);

            writer.WriteStartElement("response");
            writer.WriteAttributeString("status", code.ToString());
            writer.WriteEndElement();

            XDocument document = XDocument.Parse(sb.ToString());

            // E-amusement error responses are always SHIFT-JIS encoded.
            byte[] resData = KbinConverter.Write(document, KnownEncodings.ShiftJIS, new WriteOptions());

            string algo = "none";

            // Try compression
            byte[] compressed = LZ77.Compress(resData, 32);
            if (compressed.Length < resData.Length)
            {
                resData = compressed;
                algo = "lz77";
            }

            // Apply encryption if the request was encrypted.
            if (eAmuseInfo != null)
                RC4.ApplyEAmuseInfo(eAmuseInfo, resData);

            return (resData, algo);
        }
    }
}
