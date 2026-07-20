using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Stella.WebUI;

/// <summary>
/// Wraps an inner <see cref="IFileProvider"/> so its files appear under a fixed
/// URL prefix. Used to expose each plugin's embedded <c>Views/</c> folder at
/// <c>/Plugins/&lt;pluginId&gt;/...</c> without collisions between plugins.
/// </summary>
public sealed class PrefixedFileProvider : IFileProvider
{
    private readonly string _prefix;
    private readonly IFileProvider _inner;

    public PrefixedFileProvider(string prefix, IFileProvider inner)
    {
        _prefix = prefix.TrimEnd('/');
        _inner = inner;
    }

    private string Map(string subpath)
    {
        if (string.IsNullOrEmpty(subpath)) return subpath;
        var p = subpath.TrimStart('/');
        var prefix = _prefix.TrimStart('/');
        if (!p.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return null;
        var rest = p[prefix.Length..].TrimStart('/');
        return "/" + rest;
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        var mapped = Map(subpath);
        return mapped is null ? new NotFoundFileInfo(subpath) : _inner.GetFileInfo(mapped);
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        var mapped = Map(subpath);
        return mapped is null ? NotFoundDirectoryContents.Singleton : _inner.GetDirectoryContents(mapped);
    }

    public IChangeToken Watch(string filter) => NullChangeToken.Singleton;
}
