using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StellaKFCPlugin.Data.Seed;
using StellaKFCPlugin.EF;
using StellaKFCPlugin.Handlers;

namespace StellaKFCPlugin.WebUI;

/// <summary>
/// On-demand static-data seeding for the KFC WebUI. Wraps the existing
/// <see cref="KfcSeeder"/> (sv_static_* from asphyxia_data.json) and
/// <see cref="MigrationHelper.LoadMusicDbAsync"/> (music_db.xml -> sv_music),
/// which are no longer run at startup. Triggered from the WebUI "Data" page.
/// </summary>
public static class KfcWebUISeeder
{
    public sealed record SeedResult(bool Ok, string Message);

    public static SeedResult SeedStatic(StellaKFCContext db, ILogger? logger)
    {
        try { KfcSeeder.Seed(db); return new SeedResult(true, "Static data seeded."); }
        catch (Exception ex) { logger?.LogWarning(ex, "KfcSeeder failed"); return new SeedResult(false, ex.Message); }
    }

    public static async Task<SeedResult> LoadMusicDbAsync(StellaKFCContext db, ILogger? logger)
    {
        try { await MigrationHelper.LoadMusicDbAsync(db, logger); return new SeedResult(true, "music_db.xml loaded into sv_music."); }
        catch (Exception ex) { logger?.LogWarning(ex, "LoadMusicDbAsync failed"); return new SeedResult(false, ex.Message); }
    }

    public static async Task<bool> SaveUploadedMusicDbAsync(byte[] data)
    {
        var path = ResolveMusicDbPath();
        if (path is null) return false;
        await File.WriteAllBytesAsync(path, data);
        return true;
    }

    public static string? ResolveMusicDbPath()
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Seed");
        if (!Directory.Exists(dir))
        {
            dir = Path.Combine(AppContext.BaseDirectory, "Data", "Seed");
            Directory.CreateDirectory(dir);
        }
        return Path.Combine(dir, "music_db.xml");
    }
}
