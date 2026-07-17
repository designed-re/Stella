using System.Threading.Tasks;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.Models;

namespace StellaKFCPlugin.Handlers
{
    /// <summary>
    /// Unified <c>play_s</c>/<c>play_e</c>/<c>shop</c> handlers. asphyxia returns
    /// success (no-op) for play_s/play_e and a simple nxt_time for shop.
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

        [StellaHandler("game", "play_s", typeof(PlaySERequest))]
        public async Task<PlaySEResponse> PlaySBare() => new();

        [StellaHandler("game", "play_e", typeof(PlaySERequest))]
        public async Task<PlaySEResponse> PlayEBare() => new();

        [StellaHandler("game", "sv6_shop", typeof(ShopRequest))]
        public async Task<ShopResponse> Shop() => new();

        [StellaHandler("game", "sv7_shop", typeof(ShopRequest))]
        public async Task<ShopResponse> ShopNabla() => new();

        [StellaHandler("game", "shop", typeof(ShopRequest))]
        public async Task<ShopResponse> ShopBare() => new();
    }
}