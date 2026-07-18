using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stella.Abstractions.WebUI;
using StellaKFCPlugin.EF;
using Newtonsoft.Json.Linq;

namespace StellaKFCPlugin.WebUI;

/// <summary>
/// Renders the KFC WebUI pages by querying <see cref="StellaKFCContext"/> and
/// handing the resulting view model to <see cref="PluginViewRenderer"/>. Each
/// page view lives at <c>Views/Pages/&lt;View&gt;.cshtml</c> and is exposed under
/// <c>/Plugins/StellaKFCPlugin/Pages/&lt;View&gt;.cshtml</c>.
/// </summary>
public sealed class KfcWebUIPageRenderer
{
    private readonly IPluginViewRenderer _renderer;
    public KfcWebUIPageRenderer(IPluginViewRenderer renderer) => _renderer = renderer;

    private static string V(string view) => $"/Plugins/StellaKFCPlugin/Pages/{view}";
    private static Task<string> Render<T>(IPluginViewRenderer r, string view, T model) =>
        RenderInternal(r, view, model);

    private static async Task<string> RenderInternal<T>(IPluginViewRenderer r, string view, T model)
    {
        var html = await r.RenderAsync(V(view), model);
        return html.ToString();
    }

    public Task<string?> RenderPageAsync(string slug, IServiceProvider services)
    {
        var r = services.GetRequiredService<IPluginViewRenderer>();
        return slug switch
        {
            "data" => RenderData(r),
            "songs-list" => RenderSongsList(r),
            "startup-flags" => RenderStartupFlags(r),
            "unlock-events" => RenderUnlockEvents(r),
            "update-webui-assets" => RenderUpdateWebUIAssets(r),
            "weekly-score-attack" => RenderWeekly(r),
            _ => Task.FromResult<string?>(null),
        };
    }

    public Task<string?> RenderProfileTabAsync(string slug, string refid, IServiceProvider services)
    {
        var r = services.GetRequiredService<IPluginViewRenderer>();
        return slug switch
        {
            "detail" => RenderProfileDetail(r, refid),
            "score" => RenderProfileScore(r, refid),
            "skill" => RenderProfileSkill(r, refid),
            "achievements" => RenderProfileAchievements(r, refid),
            "rivals" => RenderProfileRivals(r, refid),
            "customization" => RenderProfileCustomization(r, refid),
            "valkyrie-generator" => RenderProfileValkyrie(r, refid),
            "premium-generator" => RenderProfilePremium(r, refid),
            _ => Task.FromResult<string?>(null),
        };
    }

    // ---- top-level ----
    private async Task<string?> RenderData(IPluginViewRenderer r)
    {
        using var db = new StellaKFCContext();
        var m = new ViewModels.DataModel
        {
            StaticEventCount = db.SvEventDatas.Count(),
            MusicCount = db.SvMusics.Count(),
            MusicDbPresent = File.Exists(KfcWebUISeeder.ResolveMusicDbPath() ?? ""),
        };
        return await RenderInternal(r, "Data.cshtml", m);
    }

    private async Task<string?> RenderSongsList(IPluginViewRenderer r)
    {
        using var db = new StellaKFCContext();
        var m = new ViewModels.SongsListModel
        {
            Version = 6,
            Songs = db.SvMusics.OrderBy(x => x.Id).Take(500)
                .Select(x => new ViewModels.SongsListModel.SongRow(x.Id, x.Title, x.Artist, x.Version, x.Date.ToString("yyyy-MM-dd"))).ToList(),
        };
        return await RenderInternal(r, "SongsList.cshtml", m);
    }

    private async Task<string?> RenderStartupFlags(IPluginViewRenderer r)
    {
        var path = StellaKFCContext.ResolvePluginConfigPath("plugin_kfc.json");
        var cfg = new StellaKFCPluginConfig();
        if (File.Exists(path))
            cfg = JsonSerializer.Deserialize<StellaKFCPluginConfig>(File.ReadAllText(path), JsonOpts) ?? cfg;
        return await RenderInternal(r, "StartupFlags.cshtml", new ViewModels.StartupFlagsModel { Config = cfg });
    }

    private async Task<string?> RenderUnlockEvents(IPluginViewRenderer r)
    {
        using var db = new StellaKFCContext();
        var lists = db.SvEventLists.OrderBy(e => e.Version).ThenBy(e => e.Type).ThenBy(e => e.StartDate).ToList();
        var items = db.SvEventItems.ToList();
        var rows = new List<ViewModels.UnlockEventsModel.EventRow>();
        foreach (var e in lists)
        {
            bool isPrefix = !string.IsNullOrEmpty(e.VersionsJson);
            var subs = new List<ViewModels.UnlockEventsModel.SubItem>();
            if (isPrefix)
            {
                // Object-toggle (prefix) gift/cross events: one sub-toggle per
                // EVENT_ITEMS key matching <eventId>_<idx>, state in SettingsJson.toggle.
                JObject? toggle = null;
                if (!string.IsNullOrEmpty(e.SettingsJson))
                    try { toggle = JObject.Parse(e.SettingsJson)?["toggle"] as JObject; } catch { }
                foreach (var it in items.Where(i => i.Version == e.Version && i.ItemKey.StartsWith(e.EventId + "_")).OrderBy(i => i.ItemKey))
                {
                    bool on = toggle?[it.ItemKey]?.Type == JTokenType.Boolean && toggle[it.ItemKey].Value<bool>();
                    subs.Add(new ViewModels.UnlockEventsModel.SubItem(it.ItemKey, on));
                }
            }
            rows.Add(new ViewModels.UnlockEventsModel.EventRow(e.Version, e.EventId, e.Type, e.MinVersion, e.StartDate, e.Enabled, e.Name, isPrefix, subs));
        }
        var m = new ViewModels.UnlockEventsModel { Events = rows };
        return await RenderInternal(r, "UnlockEvents.cshtml", m);
    }

    private async Task<string?> RenderUpdateWebUIAssets(IPluginViewRenderer r)
        => await RenderInternal(r, "UpdateWebUIAssets.cshtml", new object());

    private async Task<string?> RenderWeekly(IPluginViewRenderer r)
    {
        using var db = new StellaKFCContext();
        var m = new ViewModels.WeeklyScoreAttackModel
        {
            Rows = db.SvWeeklyMusicScores.OrderByDescending(s => s.Version).ThenBy(s => s.Week).ThenByDescending(s => s.Exscore).Take(200)
                .Select(s => new ViewModels.WeeklyScoreAttackModel.WeeklyRow(s.Version, s.Week, s.Mid, s.Mtype, s.Exscore, s.Name, s.PlayCount)).ToList(),
        };
        return await RenderInternal(r, "WeeklyScoreAttack.cshtml", m);
    }

    // ---- profile tabs ----
    private async Task<string?> RenderProfileDetail(IPluginViewRenderer r, string refid)
    {
        using var db = new StellaKFCContext();
        var p = db.SvProfiles.FirstOrDefault(x => x.RefId == refid);
        if (p is null) return "<p class=\"text-zinc-500\">Profile not found.</p>";
        var m = new ViewModels.ProfileDetailModel
        {
            RefId = refid, Profile = p,
            ScoreCount = db.SvScores.Count(s => s.Profile == p.Id),
            ItemCount = db.SvItems.Count(i => i.Profile == p.Id),
            RivalCount = db.SvRivals.Count(x => x.RefId == refid),
        };
        return await RenderInternal(r, "ProfileDetail.cshtml", m);
    }

    private async Task<string?> RenderProfileScore(IPluginViewRenderer r, string refid)
    {
        using var db = new StellaKFCContext();
        var p = db.SvProfiles.FirstOrDefault(x => x.RefId == refid);
        if (p is null) return null;
        var m = new ViewModels.ProfileScoreModel
        {
            RefId = refid,
            Scores = db.SvScores.Where(s => s.Profile == p.Id).OrderByDescending(s => s.Score).Take(300)
                .Select(s => new ViewModels.ProfileScoreModel.ScoreRow(s.MusicId, s.Type, s.Score, s.Exscore, s.Clear, s.Grade, s.Volforce, s.PlayCount)).ToList(),
        };
        return await RenderInternal(r, "ProfileScore.cshtml", m);
    }

    private async Task<string?> RenderProfileSkill(IPluginViewRenderer r, string refid)
    {
        using var db = new StellaKFCContext();
        var p = db.SvProfiles.FirstOrDefault(x => x.RefId == refid);
        if (p is null) return null;
        var m = new ViewModels.ProfileSkillModel
        {
            RefId = refid, SkillLevel = p.SkillLevel, SkillBaseId = p.SkillBaseId,
            Skills = db.SvSkills.Where(s => s.Profile == p.Id)
                .Select(s => new ViewModels.ProfileSkillModel.SkillRow(s.Type, s.Base, s.Level, s.Name)).ToList(),
        };
        return await RenderInternal(r, "ProfileSkill.cshtml", m);
    }

    private async Task<string?> RenderProfileAchievements(IPluginViewRenderer r, string refid)
    {
        using var db = new StellaKFCContext();
        var p = db.SvProfiles.FirstOrDefault(x => x.RefId == refid);
        if (p is null) return null;
        var m = new ViewModels.ProfileAchievementsModel
        {
            RefId = refid,
            Items = db.SvItems.Where(i => i.Profile == p.Id).OrderBy(i => i.Type).ThenBy(i => i.ItemId).Take(300)
                .Select(i => new ViewModels.ProfileAchievementsModel.ItemRow(i.Type, i.ItemId, i.Param)).ToList(),
        };
        return await RenderInternal(r, "ProfileAchievements.cshtml", m);
    }

    private async Task<string?> RenderProfileRivals(IPluginViewRenderer r, string refid)
    {
        using var db = new StellaKFCContext();
        var m = new ViewModels.ProfileRivalsModel
        {
            RefId = refid,
            Rivals = db.SvRivals.Where(x => x.RefId == refid)
                .Select(x => new ViewModels.ProfileRivalsModel.RivalRow(x.RivalRefId, x.Name, x.SdvxId, x.Mutual, x.Version)).ToList(),
        };
        return await RenderInternal(r, "ProfileRivals.cshtml", m);
    }

    private async Task<string?> RenderProfileCustomization(IPluginViewRenderer r, string refid)
    {
        using var db = new StellaKFCContext();
        var p = db.SvProfiles.FirstOrDefault(x => x.RefId == refid);
        if (p is null) return null;
        var m = new ViewModels.ProfileCustomizationModel
        {
            RefId = refid, Name = p.Name, Version = p.Version, AppealId = p.AppealId,
            Params = db.SvParams.Where(x => x.Profile == p.Id).OrderBy(x => x.Type).ThenBy(x => x.ParamId)
                .Select(x => new ViewModels.ProfileCustomizationModel.ParamRow(x.Type, x.ParamId, x.Param, x.ParamCount)).ToList(),
        };
        return await RenderInternal(r, "ProfileCustomization.cshtml", m);
    }

    private async Task<string?> RenderProfileValkyrie(IPluginViewRenderer r, string refid)
    {
        using var db = new StellaKFCContext();
        var m = new ViewModels.ProfileValkyrieGeneratorModel
        {
            RefId = refid,
            ValkSongs = db.SvValkyrieSongs.Where(v => v.Version == 6).OrderBy(v => v.MusicId)
                .Select(v => new ViewModels.ProfileValkyrieGeneratorModel.ValkRow(v.MusicId, v.Version)).ToList(),
        };
        return await RenderInternal(r, "ProfileValkyrieGenerator.cshtml", m);
    }

    private async Task<string?> RenderProfilePremium(IPluginViewRenderer r, string refid)
    {
        using var db = new StellaKFCContext();
        var m = new ViewModels.ProfilePremiumGeneratorModel
        {
            RefId = refid,
            PreGenes = db.SvApigeneDatas.Where(a => a.Version == 7).OrderBy(a => a.ApigeneId)
                .Select(a => new ViewModels.ProfilePremiumGeneratorModel.PremRow(a.Version, a.ApigeneId, a.Name)).ToList(),
        };
        return await RenderInternal(r, "ProfilePremiumGenerator.cshtml", m);
    }

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
}
