using System.Threading.Tasks;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.Models;

namespace StellaKFCPlugin.Handlers
{
    /// <summary>
    /// Unified <c>frozen</c> handler (sv6/sv7). asphyxia returns <c>true</c>
    /// (no-op); Stella returns an empty response.
    /// </summary>
    public class FrozenHandler : StellaHandler
    {
        [StellaHandler("game", "sv6_frozen", typeof(FrozenRequest))]
        public async Task<FrozenResponse> Frozen() => new();

        [StellaHandler("game", "sv7_frozen", typeof(FrozenRequest))]
        public async Task<FrozenResponse> FrozenNabla() => new();

        [StellaHandler("game", "frozen", typeof(FrozenRequest))]
        public async Task<FrozenResponse> FrozenBare() => new();

        [StellaHandler("game_3", "frozen", typeof(FrozenRequest))]
        public async Task<FrozenResponse> FrozenBareGame3() => new();

    }
}