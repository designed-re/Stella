using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.EF;
using StellaKFCPlugin.Models;
using StellaKFCPlugin.Util;

namespace StellaKFCPlugin.Handlers
{
    /// <summary>
    /// Unified <c>new</c> handler: creates a fresh profile for the requested
    /// version. When a v6 (EXCEED GEAR) profile already exists and a v7 (NABLA)
    /// profile is being created, performs the EG→∇ migration (asphyxia
    /// <c>viiMigrate</c>) before returning success.
    /// </summary>
    public class NewHandler : StellaHandler
    {
        [StellaHandler("game", "sv6_new", typeof(NewRequest))]
        public async Task<NewResponse> New() => await NewInternal(6);

        [StellaHandler("game", "sv7_new", typeof(NewRequest))]
        public async Task<NewResponse> NewNabla() => await NewInternal(7);

        [StellaHandler("game", "new", typeof(NewRequest))]
        public async Task<NewResponse> NewBare() => await NewInternal(Math.Abs(KfcVersion.GetVersion(Model)));

        [StellaHandler("game_3", "new", typeof(NewRequest))]
        public async Task<NewResponse> NewBareGame3() => await NewInternal(Math.Abs(KfcVersion.GetVersion(Model)));


        private async Task<NewResponse> NewInternal(int gameVersion)
        {
            var request = Request as NewRequest;
            if (request is null) return new NewResponse { Result = 1 };
            using var db = new StellaKFCContext();
            var dVersion = KfcVersion.GetDateCode(Model);

            var existing = await db.SvProfiles.SingleOrDefaultAsync(x => x.RefId == request.Refid && x.Version == gameVersion);
            if (existing is not null)
            {
                Logger?.LogInformation("profile exists for refid {Refid}", request.Refid);
                return new NewResponse { Result = 1 };
            }

            // v7: if a v6 profile exists, migrate it instead of creating a blank one
            // (asphyxia create L1049-1056).
            if (gameVersion == 7)
            {
                var v6 = await db.SvProfiles.SingleOrDefaultAsync(x => x.RefId == request.Refid && x.Version == 6);
                if (v6 is not null)
                {
                    await MigrationHelper.ViiMigrateAsync(db, v6.Id);
                    return new NewResponse { Result = 0 };
                }
            }

            var profile = new SvProfile
            {
                RefId = request.Refid,
                Name = string.IsNullOrEmpty(request.Name) ? "GUEST" : request.Name,
                Code = GetCode(db),
                KacId = "VOLTEX",
                Version = gameVersion,
                EffCRight = 1,
                CreatorItem = 1,
                Datecode = dVersion,
                PluginVer = 1,
                DbVer = 1,
                Packets = 10000,
                Blocks = 10000,
            };
            db.SvProfiles.Add(profile);
            await db.SaveChangesAsync();
            return new NewResponse { Result = 0 };
        }

        private static string GetCode(StellaKFCContext db)
        {
            var r = new Random();
            string code;
            do
            {
                code = r.Next(0, 10000).ToString("D4") + "-" + r.Next(0, 10000).ToString("D4");
            } while (db.SvProfiles.Any(x => x.Code == code));
            return code;
        }
    }
}