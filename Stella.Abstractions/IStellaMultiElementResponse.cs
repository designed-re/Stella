using System.Collections.Generic;

namespace Stella.Abstractions
{
    /// <summary>
    /// A response that serializes as several sibling elements under the
    /// <c>&lt;response&gt;</c> wrapper (e.g. asphyxia
    /// <c>send.object([{...},{...},{...}])</c>). Each entry in
    /// <see cref="Elements"/> is serialized independently with
    /// <see cref="System.Xml.Serialization.XmlSerializer"/>, so each item type
    /// should carry an <c>[XmlRoot]</c> matching the wire element name.
    /// </summary>
    public interface IStellaMultiElementResponse : IStellaEAmuseResponse
    {
        /// <summary>The wire element name shared by every sibling element.</summary>
        string ElementName { get; }

        /// <summary>The sibling element objects to serialize in order.</summary>
        IReadOnlyList<object> Elements { get; }
    }
}
