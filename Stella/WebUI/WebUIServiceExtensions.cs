using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.FileProviders;
using Stella.Abstractions.WebUI;
using Stella.Services;

namespace Stella.WebUI;

public static class WebUIServiceExtensions
{
    public const string AuthScheme = "StellaWebUI";

    /// <summary>
    /// Registers WebUI services: cookie auth, antiforgery, and plugin
    /// ApplicationParts so plugin Razor Pages are discoverable. Must be called
    /// after <see cref="PluginService.LoadPluginsAsync"/>.
    /// </summary>
    public static IServiceCollection AddStellaWebUI(this IServiceCollection services, PluginService plugins)
    {
        services.AddAuthentication(AuthScheme)
            .AddCookie(AuthScheme, options =>
            {
                options.Cookie.Name = "stella_webui";
                options.LoginPath = "/webui/login";
                options.LogoutPath = "/webui/logout";
                options.AccessDeniedPath = "/webui/login";
                options.ExpireTimeSpan = System.TimeSpan.FromHours(12);
                options.SlidingExpiration = true;
            });
        services.AddAuthorization();
        services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
        services.AddRazorPages()
            .AddMvcOptions(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()))
            .AddRazorRuntimeCompilation();
        services.AddScoped<PluginViewRenderer>();
        services.AddScoped<Stella.Abstractions.WebUI.IPluginViewRenderer, PluginViewRenderer>();

        // Expose each plugin's embedded Views/ folder at /Plugins/<pluginId>/...
        // so the runtime Razor compiler can resolve plugin page views via the
        // PluginViewRenderer. Views are embedded with a manifest (see plugin csproj).
        // In .NET 10 the runtime-compilation file providers live on
        // MvcRazorRuntimeCompilationOptions (not RazorViewEngineOptions).
        services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
        {
            foreach (var asm in plugins.LoadedAssemblies)
            {
                var plugin = plugins.LoadedPlugins.FirstOrDefault(p => p.GetType().Assembly == asm);
                if (plugin is null) continue;
                try
                {
                    var inner = new ManifestEmbeddedFileProvider(asm, "Views");
                    var prefixed = new PrefixedFileProvider($"/Plugins/{plugin.Name}", inner);
                    options.FileProviders.Add(prefixed);

                // Plugin assemblies are loaded at runtime (from a byte array, so
                // Assembly.Location is empty) and are not part of the app's default
                // dependency context. The runtime Razor compiler therefore cannot
                // resolve plugin namespaces (e.g. StellaKFCPlugin.*) used in @model /
                // _ViewImports. Register the on-disk DLL path explicitly so the views
                // can reference the plugin's own types.
                if (plugins.AssemblyPaths.TryGetValue(asm, out var asmPath) && !string.IsNullOrEmpty(asmPath))
                    options.AdditionalReferencePaths.Add(asmPath);
                }
                catch
                {
                    // Plugin has no embedded Views manifest; skip.
                }
            }
        });

        return services;
    }

    /// <summary>
    /// Maps the WebUI: Razor Pages, plugin static assets, and the event-emit
    /// API. Everything lives under <c>/webui</c> so it never collides with the
    /// e-amusement <c>/eamuse</c> / <c>/core</c> POST routes.
    /// </summary>
    public static void MapStellaWebUI(this WebApplication app, PluginService plugins)
    {
        // Plugin static assets: /webui/static/{pluginId}/...
        foreach (var asm in plugins.LoadedAssemblies)
        {
            var plugin = plugins.LoadedPlugins.FirstOrDefault(p => p.GetType().Assembly == asm);
            if (plugin is null) continue;
            var pluginId = plugin.Name;
            try
            {
                var provider = new ManifestEmbeddedFileProvider(asm, "wwwroot");
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = provider,
                    RequestPath = $"/webui/static/{pluginId}",
                });
            }
            catch
            {
                // Plugin has no embedded wwwroot manifest; skip.
            }
        }

        // Razor Pages declare their own absolute routes (e.g. @page "/webui"),
        // so map them at the app root rather than under a prefixed group.
        app.MapRazorPages();

        // Event emit: POST /webui/api/emit/{pluginId}/{event} (auth required).
        app.MapPost("/webui/api/emit/{pluginId}/{event}", EmitHandler).RequireAuthorization();
    }

    private static async Task EmitHandler(HttpContext context, IAntiforgery antiforgery, string pluginId, string @event)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context);
        }
        catch
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("invalid antiforgery token");
            return;
        }

        var registry = WebUIEventRegistryStore.Get(pluginId);
        if (registry is null || !registry.HasHandler(@event))
        {
            context.Response.StatusCode = 404;
            return;
        }

        JsonElement data;
        if (context.Request.ContentType is not null && context.Request.ContentType.Contains("application/json"))
        {
            using var doc = await JsonDocument.ParseAsync(context.Request.Body);
            data = doc.RootElement.Clone();
        }
        else
        {
            var form = await context.Request.ReadFormAsync();
            var obj = new System.Collections.Generic.Dictionary<string, string?>();
            foreach (var kv in form) obj[kv.Key] = kv.Value.ToString();
            data = JsonDocument.Parse(JsonSerializer.Serialize(obj)).RootElement.Clone();
        }

        try
        {
            var result = await registry.InvokeAsync(@event, data);
            switch (result)
            {
                case WebUIResult.Json json:
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(json.Payload));
                    break;
                case WebUIResult.Redirect red:
                    context.Response.Redirect(red.Url);
                    break;
                default:
                    context.Response.StatusCode = 204;
                    break;
            }
        }
        catch (System.Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync(ex.Message);
        }
    }
}
