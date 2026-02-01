using Stella.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using StellaKFCPlugin.Classes;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]
    public class SaveCourseRequest : IStellaEAmuseRequest
    {
        public SaveCourseRequest() { }

        [XmlElement(ElementName = "refid")]
        public string Refid { get; set; }

        [XmlElement(ElementName = "course")]
        public SaveCourseElement? Course { get; set; }
    }
}



