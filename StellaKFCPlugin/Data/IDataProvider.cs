using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StellaKFCPlugin.EF.StaticData;

namespace StellaKFCPlugin.Data;

/// <summary>
/// Abstraction over static game-data sources (events, courses, valgene, arena,
/// extend, music limits, ...). Two implementations: <see cref="DbDataProvider"/>
/// (default, reads from EF static tables) and <see cref="JsonDataProvider"/>
/// (reads from Data/*.json files; selected via STELLA_KFC_DATA_MODE=json).
/// Mirrors the asphyxia plugin's data modules (data/exg.ts, data/nbl.ts, ...).
/// </summary>
public interface IDataProvider
{
    int GameVersion { get; }

    IReadOnlyList<string> GetEvents();
    IReadOnlyList<SvCourseData> GetCourses();
    IReadOnlyList<SvValgeneData> GetValgeneInfo();
    IReadOnlyList<SvValgeneCatalog> GetValgeneCatalog();
    IReadOnlyList<SvApigeneData> GetApigeneInfo();
    IReadOnlyList<SvApigeneCatalog> GetApigeneCatalog();
    IReadOnlyList<SvExtendData> GetExtends();
    IReadOnlyList<SvInformationData> GetInformation();
    IReadOnlyList<SvUnlockEventData> GetUnlockEvents();
    SvCurrentArena? GetCurrentArena();
    IReadOnlyList<SvArenaStationItem> GetArenaStationItems();
    IReadOnlyList<SvMusicOverride> GetMusicOverrides();
    IReadOnlyList<int> GetLicensedSongs();
    IReadOnlyList<int> GetValkyrieSongs();
    IReadOnlyList<int> GetAprilFoolsSongs();
    SvEgSongLockedCategory[] GetEgSongsLocked();
    IReadOnlyList<SvMegamixData> GetMegamix();
    int GetSongNum();
}