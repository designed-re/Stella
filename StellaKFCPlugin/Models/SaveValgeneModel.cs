using Stella.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using StellaKFCPlugin.Classes;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]
    public class SaveValgeneRequest : IStellaEAmuseRequest
    {
        public SaveValgeneRequest() { }

        [XmlElement(ElementName = "refid")]
        public string RefId { get; set; }

        [XmlElement(ElementName = "valgene_id")]
        public uint ValgeneId { get; set; }

        [XmlElement(ElementName = "play_id")]
        public uint PlayId { get; set; }

        [XmlElement(ElementName = "consume_type")]
        public int ConsumeType { get; set; }

        [XmlElement(ElementName = "price")]
        public int Price { get; set; }

        [XmlElement(ElementName = "use_ticket")]
        public bool UseTicket { get; set; }

        [XmlElement(ElementName = "item")]
        public SaveValgeneItemElement Item { get; set; } = new();

        [XmlElement(ElementName = "locid")]
        public string LocId { get; set; }
    }

    public class SaveValgeneItemElement
    {
        public SaveValgeneItemElement() { }

        [XmlElement(ElementName = "info")]
        public List<SaveValgeneItemInfo> Infos { get; set; } = new();
    }

    public class SaveValgeneItemInfo
    {
        public SaveValgeneItemInfo() { }

        [XmlElement(ElementName = "id")]
        public uint Id { get; set; }

        [XmlElement(ElementName = "type")]
        public uint Type { get; set; }

        [XmlElement(ElementName = "param")]
        public uint Param { get; set; }
    }

    [XmlRoot(ElementName = "game")]
    public class SaveValgeneResponse : IStellaEAmuseResponse
    {
        public SaveValgeneResponse() { }

        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";

        [XmlElement(ElementName = "result")] 
        public int Result { get; set; } = 1;

        [XmlElement(ElementName = "ticket_num")]
        public int TicketNum { get; set; }

        [XmlElement(ElementName = "limit_date")]
        public ulong LimitDate { get; set; }
    }
}




