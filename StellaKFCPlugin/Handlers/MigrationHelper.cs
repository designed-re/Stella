using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.EF;
using StellaKFCPlugin.Util;

namespace StellaKFCPlugin.Handlers;

/// <summary>
/// Performs one-shot data migration tasks mirroring asphyxia kfc/handlers/migrate.ts:
/// <list type="bullet">
/// <item><c>viiMigrate</c>: copies a profile from v6 (EXCEED GEAR) to v7 (NABLA),
///   remaps clear lamps, recomputes volforce from the music_db difficulty levels,
///   and resets the EX scores for the charts Konami reset between versions.</item>
/// <item><c>LoadMusicDb</c>: parses <c>Data/Seed/music_db.xml</c> into the
///   <c>SvMusic</c> table so difficulty levels are queryable for volforce.</item>
/// </list>
/// </summary>
public static class MigrationHelper
{
    // EG clear lamp order -> NABLA order. EG: 0..5 = [0,1,2,3,6,4,5] (mxv was added as id 6).
    private static readonly int[] EgToNablaClearLamp = { 0, 1, 2, 3, 5, 6, 4 };

    // Charts Konami reset between EXCEED GEAR and NABLA (asphyxia exScoreResetList).
    private static readonly (int Mid, int Type)[] ExScoreResetList =
    {
        (360, 3), (580, 2), (1121, 4), (1185, 2),
        (1199, 4), (1738, 4), (2242, 0),
    };

    // Difficulty-level overrides used during migration (asphyxia levelDifOverride).
    private static readonly (int Mid, int Type, double Lvl)[] LevelDifOverride =
    {
        (1,1,10),(18,1,8),(18,2,10),(73,2,17),(48,1,8),(75,2,12),
        (124,2,16),(65,1,7),(66,1,8),(27,1,7),(27,2,12),(68,1,9),
        (6,1,7),(6,2,12),(16,1,7),(2,1,10),(60,3,17),(5,2,13),
        (128,2,13),(9,2,1),(340,2,13),(247,3,18),(282,2,17),(288,2,13),
        (699,3,18),(595,2,17),(507,2,17),(1044,2,16),(948,4,16),(1115,4,16),
        (1215,2,15),(1152,2,15),(1282,3,17.5),(1343,2,16),(1300,3,17.5),(1938,2,18),
    };

    /// <summary>
    /// Parses <c>Data/Seed/music_db.xml</c> (shift_jis) and upserts the
    /// <see cref="SvMusic"/> table. Called once at plugin startup.
    /// </summary>
    public static async Task LoadMusicDbAsync(StellaKFCContext db, ILogger? logger)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Seed", "music_db.xml");
        if (!File.Exists(path))
            path = Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "music_db.xml");
        if (!File.Exists(path))
        {
            logger?.LogWarning("[MigrationHelper] music_db.xml not found; skipping SvMusic load.");
            return;
        }

        // shift_jis is a Windows code-page encoding; register the provider so
        // Encoding.GetEncoding("shift_jis") works on .NET 10.
        System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var enc = System.Text.Encoding.GetEncoding("shift_jis");
        var existing = db.SvMusics.ToDictionary(m => m.Id);
        var toAdd = new List<SvMusic>();
        var toUpdate = new List<SvMusic>();

        var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore };
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = XmlReader.Create(stream, settings);
        // Read raw bytes with shift_jis decoding by loading the whole file.
        var text = enc.GetString(File.ReadAllBytes(path));
        using var strReader = new StringReader(text);
        using var xmlReader = XmlReader.Create(strReader, settings);

        int count = 0;
        while (xmlReader.Read())
        {
            if (xmlReader.NodeType != XmlNodeType.Element || xmlReader.Name != "music") continue;
            int id = int.Parse(xmlReader.GetAttribute("id")!, CultureInfo.InvariantCulture);
            existing.TryGetValue(id, out var row);
            bool isNew = row is null;
            var music = row ?? new SvMusic { Id = id };
            // Capture old values BEFORE overwriting (row and music are the same
            // reference for existing rows, so compare against these snapshots).
            int oldInfVer = row?.InfVer ?? -1;
            int oldDistDate = row?.DistributionDate ?? -1;
            int oldVersion = row?.Version ?? -1;

            // Read subtree: info + difficulty
            using var sub = xmlReader.ReadSubtree();
            while (sub.Read())
            {
                if (sub.NodeType != XmlNodeType.Element) continue;
                if (sub.Name == "title_name") music.Title = sub.ReadElementContentAsString();
                else if (sub.Name == "title_yomigana") music.TitleYomigana = sub.ReadElementContentAsString();
                else if (sub.Name == "artist_name") music.Artist = sub.ReadElementContentAsString();
                else if (sub.Name == "artist_yomigana") music.ArtistYomigana = sub.ReadElementContentAsString();
                else if (sub.Name == "version") music.Version = int.Parse(sub.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                else if (sub.Name == "inf_ver") music.InfVer = int.Parse(sub.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                else if (sub.Name == "distribution_date") music.DistributionDate = int.Parse(sub.ReadElementContentAsString(), CultureInfo.InvariantCulture);
            }
            // Backfill inf_ver/distribution_date on existing rows when missing
            // (added after the initial sv_music schema), and refresh version.
            if (isNew) toAdd.Add(music);
            else if (music.InfVer != oldInfVer || music.DistributionDate != oldDistDate || music.Version != oldVersion)
                toUpdate.Add(music);
            count++;
            if (toAdd.Count >= 500)
            {
                db.SvMusics.AddRange(toAdd);
                await db.SaveChangesAsync();
                toAdd.Clear();
            }
            if (toUpdate.Count >= 500)
            {
                db.SvMusics.UpdateRange(toUpdate);
                await db.SaveChangesAsync();
                toUpdate.Clear();
            }
        }
        if (toAdd.Count > 0)
        {
            db.SvMusics.AddRange(toAdd);
            await db.SaveChangesAsync();
        }
        if (toUpdate.Count > 0)
        {
            db.SvMusics.UpdateRange(toUpdate);
            await db.SaveChangesAsync();
        }
        logger?.LogInformation("[MigrationHelper] Loaded {Count} music entries ({Upd} updated).", count, toUpdate.Count);
    }

    /// <summary>
    /// Migrates a profile from v6 (EXCEED GEAR) to v7 (NABLA). Mirrors asphyxia
    /// <c>viiMigrate</c> in handlers/migrate.ts: copies profile/items/params/scores,
    /// remaps clear lamps, recomputes volforce, resets specified EX scores.
    /// </summary>
    public static async Task ViiMigrateAsync(StellaKFCContext db, int profileId)
    {
        var v6 = await db.SvProfiles.SingleOrDefaultAsync(p => p.Id == profileId && p.Version == 6);
        if (v6 == null) return;

        // Avoid duplicate v7 profile.
        if (await db.SvProfiles.AnyAsync(p => p.RefId == v6.RefId && p.Version == 7))
            return;

        // 1. Clone profile to v7 (most counters reset; effC/effR, bplSupport, creatorItem kept).
        var v7 = new SvProfile
        {
            RefId = v6.RefId,
            Name = v6.Name,
            Code = v6.Code,
            AppealId = 0,
            LastMusicId = 0,
            LastMusicType = 0,
            SortType = 0,
            Headphone = 0,
            BlasterEnergy = 0,
            BlasterCount = 0,
            ExtrackEnergy = 0,
            Hispeed = v6.Hispeed,
            Lanespeed = v6.Lanespeed,
            GaugeOption = 0,
            ArsOption = 0,
            NotesOption = 0,
            EarlyLateDisp = 0,
            DrawAdjust = 0,
            EffCLeft = v6.EffCLeft,
            EffCRight = v6.EffCRight,
            KacId = v6.KacId,
            SkillLevel = 0,
            SkillBaseId = 0,
            SkillNameId = 0,
            Pcb = 0,
            Packets = 0,
            Blocks = 0,
            PlayCount = 0, DayCount = 0, TodayCount = 0,
            PlayChain = 0, MaxPlayChain = 0,
            WeekCount = 0, WeekPlayCount = 0, WeekChain = 0, MaxWeekChain = 0,
            Bgm = 0, SubBg = 0, Nemsys = 0,
            StampA = 0, StampB = 0, StampC = 0, StampD = 0,
            Version = 7,
            Akaname = 0,
            BplSupport = v6.BplSupport,
            CreatorItem = v6.CreatorItem,
            Datecode = v6.Datecode,
            PluginVer = 1,
            DbVer = 1,
        };
        db.SvProfiles.Add(v7);
        await db.SaveChangesAsync();

        // 2. Copy items (v6 -> v7).
        var items = db.SvItems.Where(i => i.Profile == profileId && i.Version == 6).AsEnumerable();
        foreach (var it in items)
        {
            db.SvItems.Add(new SvItem
            {
                Profile = v7.Id, Type = it.Type, ItemId = it.ItemId, Param = it.Param, Version = 7,
            });
        }

        // 3. Copy params (v6 -> v7); param type 2 id 1 -> zero out index 24 (HEXA_OVERDRIVE).
        var params_ = db.SvParams.Where(p => p.Profile == profileId && p.Version == 6).AsEnumerable();
        foreach (var p in params_)
        {
            var vals = p.Param.Split(' ').Select(int.Parse).ToList();
            if (p.Type == 2 && p.ParamId == 1 && vals.Count > 24) vals[24] = 0;
            db.SvParams.Add(new SvParam
            {
                Profile = v7.Id, Type = p.Type, ParamId = p.ParamId,
                Param = string.Join(' ', vals), ParamCount = (uint)vals.Count, Version = 7,
            });
        }

        // 4. Migrate scores with clear-lamp remap and volforce computation.
        // asphyxia viiMigrate: looks up the NABLA difficulty level from music_db
        // (difficulty[6]) to recompute volforce, and skips songs not present in
        // music_db. Stella's music_db.xml difnum values equal asphyxia's
        // difficulty[6], so we parse them here.
        var diffMap = LoadMusicDifficulties();
        var scores = db.SvScores.Where(s => s.Profile == profileId && s.Version == 6).AsEnumerable();
        foreach (var s in scores)
        {
            // Skip scores whose chart is not in music_db (asphyxia foundSongIndex === -1).
            if (!diffMap.TryGetValue(s.MusicId, out var diffs)) continue;

            int newClear = s.Clear >= 0 && s.Clear < EgToNablaClearLamp.Length
                ? EgToNablaClearLamp[s.Clear] : s.Clear;

            int exscore = s.Exscore;
            int idx = Array.FindIndex(ExScoreResetList, t => t.Mid == s.MusicId && t.Type == s.Type);
            if (idx >= 0) exscore = 0;

            // Difficulty level from music_db difnum for this chart type (0..5),
            // overridden by levelDifOverride when present (asphyxia migrate L289-292).
            double diff = s.Type >= 0 && s.Type < diffs.Length ? diffs[s.Type] : 0;
            int lvIdx = Array.FindIndex(LevelDifOverride, t => t.Mid == s.MusicId && t.Type == s.Type);
            if (lvIdx >= 0) diff = LevelDifOverride[lvIdx].Lvl;

            int volforce = KfcVersion.ComputeForce(diff, s.Score, newClear, s.Grade);

            db.SvScores.Add(new SvScore
            {
                Profile = v7.Id, MusicId = s.MusicId, Type = s.Type,
                Score = s.Score, Exscore = exscore, Clear = newClear, Grade = s.Grade,
                ButtonRate = s.ButtonRate, LongRate = s.LongRate, VolRate = s.VolRate,
                Volforce = volforce, PlayCount = 0, DbVer = 1, Version = 7,
            });
        }

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Parses <c>Data/Seed/music_db.xml</c> (shift_jis) and returns a map of
    /// music id -> difficulty levels [novice, advanced, exhaust, infinite,
    /// maximum, ultimate]. Mirrors asphyxia's <c>difficulty[6][diffName]</c>
    /// lookup used by <c>viiMigrate</c>.
    /// </summary>
    private static Dictionary<int, double[]>? _difficultyCache;
    private static readonly object _difficultyLock = new();

    /// <summary>
    /// Cached map of music id -> difficulty levels [novice, advanced, exhaust,
    /// infinite, maximum, ultimate] parsed from <c>music_db.xml</c>. A difnum
    /// of 0 means the chart does not exist (mirrors asphyxia
    /// <c>difficulty[absVersion][diffName] != '0'</c>). Used by
    /// <c>viiMigrate</c> and <c>BuildMusicLimited</c> (unlock_all_songs).
    /// </summary>
    public static Dictionary<int, double[]> GetMusicDifficulties()
    {
        lock (_difficultyLock)
        {
            if (_difficultyCache is not null) return _difficultyCache;
            _difficultyCache = LoadMusicDifficulties();
            return _difficultyCache;
        }
    }

    private static Dictionary<int, double[]> LoadMusicDifficulties()
    {
        var result = new Dictionary<int, double[]>();
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Seed", "music_db.xml");
        if (!File.Exists(path))
            path = Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "music_db.xml");
        if (!File.Exists(path)) return result;

        System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var enc = System.Text.Encoding.GetEncoding("shift_jis");
        var text = enc.GetString(File.ReadAllBytes(path));

        var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore };
        using var strReader = new StringReader(text);
        using var reader = XmlReader.Create(strReader, settings);

        // diffName index: 0=novice,1=advanced,2=exhaust,3=infinite,4=maximum,5=ultimate
        var diffNames = new[] { "novice", "advanced", "exhaust", "infinite", "maximum", "ultimate" };

        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element || reader.Name != "music") continue;
            int id = int.Parse(reader.GetAttribute("id")!, CultureInfo.InvariantCulture);

            using var sub = reader.ReadSubtree();
            double[] diffs = { 0, 0, 0, 0, 0, 0 };
            while (sub.Read())
            {
                if (sub.NodeType != XmlNodeType.Element) continue;
                int di = Array.IndexOf(diffNames, sub.Name);
                if (di < 0) continue;
                // Read the <difnum> child of this difficulty element.
                using var dsub = sub.ReadSubtree();
                while (dsub.Read())
                {
                    if (dsub.NodeType == XmlNodeType.Element && dsub.Name == "difnum")
                    {
                        if (double.TryParse(dsub.ReadElementContentAsString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                            diffs[di] = v;
                        break;
                    }
                }
            }
            result[id] = diffs;
        }
        return result;
    }
}
