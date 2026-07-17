using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.EF;
using StellaKFCPlugin.Models;
using StellaKFCPlugin.Util;

namespace StellaKFCPlugin.Handlers;

/// <summary>
/// GRAVITY WARS (sv3) new profile handler. Ported from asphyxia kfc/handlers/profiles.ts
/// create function.
/// </summary>
public class Sv3NewHandler : StellaHandler
{
    [StellaHandler("game_3", "new", typeof(Sv3NewRequest))]
    public async Task<NewResponse> NewGame3() => await NewSv3Internal(3);

    private async Task<NewResponse> NewSv3Internal(int gameVersion)
    {
        var request = Request as Sv3NewRequest;
        if (request is null) return new NewResponse { Result = 1 };

        // sv3 uses dataid element for refid
        var refid = request.Refid ?? request.Dataid;
        if (string.IsNullOrEmpty(refid))
        {
            Logger?.LogWarning("sv3 new: refid is null");
            return new NewResponse { Result = 1 };
        }

        using var db = new StellaKFCContext();
        var dVersion = KfcVersion.GetDateCode(Model);

        var existing = await db.SvProfiles.SingleOrDefaultAsync(x => x.RefId == refid && x.Version == gameVersion);
        if (existing is not null)
        {
            Logger?.LogInformation("sv3 new: profile exists for refid {Refid}", refid);
            return new NewResponse { Result = 1 };
        }

        // Generate unique code
        var code = GetCode(db);

        var profile = new SvProfile
        {
            RefId = refid,
            Name = string.IsNullOrEmpty(request.Name) ? "GUEST" : request.Name,
            Code = code,
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

        Logger?.LogInformation("sv3 new: created profile for refid {Refid}, code {Code}", refid, code);
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
