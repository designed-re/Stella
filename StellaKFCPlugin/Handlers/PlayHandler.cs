using System.Threading.Tasks;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.Models;

namespace StellaKFCPlugin.Handlers
{
    /// <summary>
    /// Unified <c>play_s</c>/<c>play_e</c> handlers. asphyxia returns success
    /// (no-op) for both; Stella returns an empty response.
    /// </summary>
    public class PlayHandler : StellaHandler
    {
        [StellaHandler("game", "sv6_play_s", typeof(PlaySERequest))]
        public async Task<PlaySEResponse> PlayS() => new();

        [StellaHandler("game", "sv6_play_e", typeof(PlaySERequest))]
        public async Task<PlaySEResponse> PlayE() => new();

        [StellaHandler("game", "sv7_play_s", typeof(PlaySERequest))]
        public async Task<PlaySEResponse> PlaySNabla() => new();

        [StellaHandler("game", "sv7_play_e", typeof(PlaySERequest))]
        public async Task<PlaySEResponse> PlayENabla() => new();
    }
}