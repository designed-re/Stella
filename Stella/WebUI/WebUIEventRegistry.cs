using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Stella.Abstractions.WebUI;

namespace Stella.WebUI;

/// <summary>
/// Per-plugin <see cref="IWebUIEventRouter"/> implementation. Created once per
/// plugin during host startup and stored in <see cref="WebUIEventRegistryStore"/>
/// keyed by plugin id (the plugin's <c>Name</c>).
/// </summary>
public sealed class WebUIEventRegistry : IWebUIEventRouter
{
    private readonly Dictionary<string, WebUIEventHandler> _handlers = new(StringComparer.Ordinal);

    public void Register(string eventName, WebUIEventHandler handler)
    {
        if (string.IsNullOrEmpty(eventName)) return;
        _handlers[eventName] = handler;
    }

    public bool HasHandler(string eventName) => _handlers.ContainsKey(eventName);

    public async Task<WebUIResult?> InvokeAsync(string eventName, JsonElement data)
    {
        if (_handlers.TryGetValue(eventName, out var handler))
            return await handler(data);
        return null;
    }
}

/// <summary>Process-wide store of per-plugin WebUI event routers.</summary>
public static class WebUIEventRegistryStore
{
    private static readonly Dictionary<string, WebUIEventRegistry> _stores = new(StringComparer.OrdinalIgnoreCase);

    public static WebUIEventRegistry GetOrCreate(string pluginId)
    {
        if (!_stores.TryGetValue(pluginId, out var reg))
        {
            reg = new WebUIEventRegistry();
            _stores[pluginId] = reg;
        }
        return reg;
    }

    public static WebUIEventRegistry? Get(string pluginId) =>
        _stores.TryGetValue(pluginId, out var reg) ? reg : null;
}
