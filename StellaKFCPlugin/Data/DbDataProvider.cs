using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using StellaKFCPlugin.EF;
using StellaKFCPlugin.EF.StaticData;

namespace StellaKFCPlugin.Data;

/// <summary>
/// Default <see cref="IDataProvider"/> reading static data from EF tables
/// (sv_static_*). Seeded once at plugin startup by <see cref="Seed/KfcSeeder"/>.
/// </summary>
public class DbDataProvider : IDataProvider
{
    private readonly StellaKFCContext _db;
    public int GameVersion { get; }

    public DbDataProvider(StellaKFCContext db, int gameVersion)
    {
        _db = db;
        GameVersion = gameVersion;
    }

    public IReadOnlyList<string> GetEvents() =>
        _db.SvEventDatas.Where(e => e.Version == GameVersion).OrderBy(e => e.SortOrder)
            .AsEnumerable().Select(e => e.EventId).ToList();

    public IReadOnlyList<SvCourseData> GetCourses() =>
        _db.SvCourseDatas.Where(c => c.Version == GameVersion).ToList();

    public IReadOnlyList<SvValgeneData> GetValgeneInfo() =>
        _db.SvValgeneDatas.Where(v => v.Version == GameVersion).ToList();

    public IReadOnlyList<SvValgeneCatalog> GetValgeneCatalog() =>
        _db.SvValgeneCatalogs.Where(v => v.Version == GameVersion).ToList();

    public IReadOnlyList<SvApigeneData> GetApigeneInfo() =>
        _db.SvApigeneDatas.Where(a => a.Version == GameVersion).ToList();

    public IReadOnlyList<SvApigeneCatalog> GetApigeneCatalog() =>
        _db.SvApigeneCatalogs.Where(a => a.Version == GameVersion).ToList();

    public IReadOnlyList<SvExtendData> GetExtends() =>
        _db.SvExtendDatas.Where(e => e.Version == GameVersion).ToList();

    public IReadOnlyList<SvInformationData> GetInformation() =>
        _db.SvInformationDatas.Where(i => i.Version == GameVersion).ToList();

    public IReadOnlyList<SvUnlockEventData> GetUnlockEvents() =>
        _db.SvUnlockEventDatas.Where(e => e.Version == GameVersion).ToList();

    public SvCurrentArena? GetCurrentArena() =>
        _db.SvCurrentArenas.FirstOrDefault(a => a.Version == GameVersion);

    public IReadOnlyList<SvArenaStationItem> GetArenaStationItems() =>
        _db.SvArenaStationItems.Where(a => a.Version == GameVersion).ToList();

    public IReadOnlyList<SvMusicOverride> GetMusicOverrides() =>
        _db.SvMusicOverrides.Where(m => m.Version == GameVersion).ToList();

    public IReadOnlyList<int> GetLicensedSongs() =>
        _db.SvLicensedSongs.Where(l => l.Version == GameVersion).Select(l => l.MusicId).ToList();

    public IReadOnlyList<int> GetValkyrieSongs() =>
        _db.SvValkyrieSongs.Where(v => v.Version == GameVersion).Select(v => v.MusicId).ToList();

    public IReadOnlyList<int> GetAprilFoolsSongs() =>
        _db.SvAprilFoolsSongs.Where(a => a.Version == GameVersion).Select(a => a.MusicId).ToList();

    public SvEgSongLockedCategory[] GetEgSongsLocked() =>
        _db.SvEgSongLockeds.Where(e => e.Version == GameVersion)
            .AsEnumerable()
            .GroupBy(e => e.Category)
            .Select(g => new SvEgSongLockedCategory(g.Key, g.Select(x => x.MusicId).ToList()))
            .ToArray();

    public IReadOnlyList<SvMegamixData> GetMegamix() =>
        _db.SvMegamixDatas.Where(m => m.Version == GameVersion).ToList();

    public int GetSongNum() => GameVersion switch
    {
        6 => 2342,
        7 => 2400,
        _ => 2342,
    };
}