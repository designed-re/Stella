namespace Stella.Abstractions.WebUI;

/// <summary>Result returned by a WebUI event handler.</summary>
public abstract class WebUIResult
{
    public sealed class Json : WebUIResult
    {
        public object? Payload { get; }
        public Json(object? payload) => Payload = payload;
    }

    public sealed class Redirect : WebUIResult
    {
        public string Url { get; }
        public Redirect(string url) => Url = url;
    }

    public sealed class Empty : WebUIResult { }

    public static WebUIResult JsonFrom(object? payload) => new Json(payload);
    public static WebUIResult RedirectTo(string url) => new Redirect(url);
    public static WebUIResult NoContent() => new Empty();
}
