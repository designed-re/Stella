using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "eventlog")]
    public class EventlogResponse : IStellaEAmuseResponse
    {
        public EventlogResponse() { }

        [XmlAttribute(AttributeName = "status")]
        public int Status { get; set; } = 0;

        [XmlElement(ElementName = "gamesession")]
        public long GameSession { get; set; } = 1;

        [XmlElement(ElementName = "logsendflg")]
        public int LogSendFlg { get; set; } = 0;

        [XmlElement(ElementName = "logerrlevel")]
        public int LogErrLevel { get; set; } = 0;

        [XmlElement(ElementName = "evtidnosendflg")]
        public int EvtIdNoSendFlg { get; set; } = 0;
    }
}

