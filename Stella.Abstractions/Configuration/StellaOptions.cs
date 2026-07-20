using Microsoft.Extensions.Configuration;

namespace Stella.Abstractions.Configuration;

/// <summary>
/// Process-wide Stella server options resolved once at startup from
/// <c>appsettings.json</c> (<c>Stella</c> / <c>WebUI</c> sections). Replaces the
/// old <c>STELLA_*</c> environment variables so the whole server is configured
/// from JSON files only (self-hosted friendly).
/// </summary>
public static class StellaOptions
{
    /// <summary>Base URL returned by <c>services.get</c> (e.g. http://host:80/eamuse).</summary>
    public static string ServerUrl { get; set; } = "http://localhost:80/eamuse";

    /// <summary>Host used for the keepalive URL.</summary>
    public static string ServerHost { get; set; } = "127.0.0.1";

    /// <summary>Optional explicit keepalive URL; empty = derived from <see cref="ServerHost"/>.</summary>
    public static string KeepaliveUrl { get; set; } = string.Empty;

    /// <summary>WebUI feature toggle.</summary>
    public static bool WebUIEnabled { get; set; } = true;

    /// <summary>WebUI login password (plaintext; for local/self-hosted use).</summary>
    public static string WebUIPassword { get; set; } = "stella";

    /// <summary>Resolved keepalive URL (explicit or derived).</summary>
    public static string ResolvedKeepaliveUrl =>
        !string.IsNullOrWhiteSpace(KeepaliveUrl)
            ? KeepaliveUrl
            : $"http://{ServerHost}/keepalive?pa=127.0.0.1&ia=127.0.0.1&ga=127.0.0.1&ma=127.0.0.1&t1=2&t2=10";

    /// <summary>Bind from the <c>Stella</c> and <c>WebUI</c> configuration sections.</summary>
    public static void Bind(IConfiguration configuration)
    {
        var stella = configuration.GetSection("Stella");
        if (stella.Exists())
        {
            ServerUrl = stella["ServerUrl"] ?? ServerUrl;
            ServerHost = stella["ServerHost"] ?? ServerHost;
            KeepaliveUrl = stella["KeepaliveUrl"] ?? KeepaliveUrl;
        }

        var webui = configuration.GetSection("WebUI");
        if (webui.Exists())
        {
            WebUIEnabled = bool.TryParse(webui["Enabled"], out var enabled) ? enabled : WebUIEnabled;
            WebUIPassword = webui["Password"] ?? WebUIPassword;
        }
    }
}
