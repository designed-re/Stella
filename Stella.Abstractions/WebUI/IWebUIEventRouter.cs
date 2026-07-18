using System.Text.Json;
using System.Threading.Tasks;

namespace Stella.Abstractions.WebUI;

/// <summary>
/// Per-plugin registry for WebUI AJAX event handlers. Mirrors asphyxia's
/// <c>R.WebUIEvent('name', handler)</c>. The host POSTs to
/// <c>/webui/api/emit/{pluginId}/{event}</c> and dispatches here.
/// </summary>
public interface IWebUIEventRouter
{
    /// <summary>Registers a handler invoked when the browser emits <paramref name="eventName"/>.</summary>
    void Register(string eventName, WebUIEventHandler handler);

    /// <summary>True when a handler is registered for <paramref name="eventName"/>.</summary>
    bool HasHandler(string eventName);

    /// <summary>Invokes the handler for <paramref name="eventName"/>, or null if none.</summary>
    Task<WebUIResult?> InvokeAsync(string eventName, JsonElement data);
}

/// <summary>Handler signature: receives the parsed JSON body, returns a <see cref="WebUIResult"/>.</summary>
public delegate Task<WebUIResult?> WebUIEventHandler(JsonElement data);
