using System.Threading.Tasks;

namespace Stella.Abstractions.Cards;

/// <summary>
/// Implemented by the plugin that owns e-amusement cards (CorePlugin). The
/// WebUI resolves the provider via <see cref="StellaCardProviderRegistry"/>
/// without a direct project reference from the host to CorePlugin.
/// </summary>
public interface IStellaCardProvider
{
    Task<WebUICard?> GetCardAsync(string refid);
}
