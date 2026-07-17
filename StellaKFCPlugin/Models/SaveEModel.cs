using System.Collections.Generic;
using System.Xml.Serialization;
using Stella.Abstractions;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]
    public class SaveERequest : IStellaEAmuseRequest
    {
        [XmlElement(ElementName = "refid")]
        public string RefId { get; set; } = string.Empty;

        [XmlElement(ElementName = "weekly_music")]
        public List<SaveEWeeklyMusicInput> WeeklyMusic { get; set; } = new();
    }

    public class SaveEWeeklyMusicInput
    {
        [XmlElement(ElementName = "week_id")]
        public int WeekId { get; set; }

        [XmlElement(ElementName = "music_id")]
        public int MusicId { get; set; }

        [XmlElement(ElementName = "music_type")]
        public int MusicType { get; set; }

        [XmlElement(ElementName = "exscore")]
        public int Exscore { get; set; }

        [XmlElement(ElementName = "play_cnt")]
        public int PlayCount { get; set; }

        [XmlElement(ElementName = "hiscore_cnt")]
        public int HiscoreCount { get; set; }
    }

    [XmlRoot(ElementName = "game")]
    public class SaveEResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";

        // asphyxia serialises `weekly_music: [...]` as repeated <weekly_music> elements.
        [XmlElement(ElementName = "weekly_music")]
        public List<SaveEWeeklyMusic> WeeklyMusic { get; set; } = new();
    }

    public class SaveEWeeklyMusic
    {
        [XmlElement(ElementName = "week_id")]
        public int WeekId { get; set; }

        [XmlElement(ElementName = "music_id")]
        public int MusicId { get; set; }

        [XmlElement(ElementName = "music_type")]
        public int MusicType { get; set; }

        [XmlElement(ElementName = "exscore")]
        public uint Exscore { get; set; }

        [XmlElement(ElementName = "rank")]
        public int Rank { get; set; }
    }
}
