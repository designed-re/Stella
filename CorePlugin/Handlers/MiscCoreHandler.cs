using System.Threading.Tasks;
using System.Xml.Serialization;
using Stella.Abstractions;
using Stella.Abstractions.Plugins;

namespace CorePlugin.Handlers
{
    /// <summary>
    /// Stub handlers for misc core services that asphyxia core implements as
    /// trivial stubs. Grouped here to keep handler count manageable.
    /// </summary>
    public class MiscCoreHandler : StellaHandler
    {
        // tax.get_phase — asphyxia returns phase: s32(0)
        [StellaHandler("tax", "get_phase", typeof(TaxGetPhaseRequest))]
        public async Task<TaxGetPhaseResponse> TaxGetPhase() => new();

        // dlstatus.progress — asphyxia send.success()
        [StellaHandler("dlstatus", "progress", typeof(DlstatusProgressRequest))]
        public async Task<DlstatusProgressResponse> DlstatusProgress() => new();

        // posevent.income.sales.sale — asphyxia returns 3 sibling status:0 elements.
        [StellaHandler("posevent", "income.sales.sale", typeof(PoseventRequest))]
        public async Task<PoseventMultiResponse> PoseventSale() => new();

        // ins.netlog — asphyxia returns empty object
        [StellaHandler("ins", "netlog", typeof(InsNetlogRequest))]
        public async Task<InsNetlogResponse> InsNetlog() => new();
    }

    [XmlRoot(ElementName = "tax")]
    public class TaxGetPhaseRequest : IStellaEAmuseRequest { }

    [XmlRoot(ElementName = "tax")]
    public class TaxGetPhaseResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";

        [XmlElement(ElementName = "phase")]
        public int Phase { get; set; }
    }

    [XmlRoot(ElementName = "dlstatus")]
    public class DlstatusProgressRequest : IStellaEAmuseRequest { }

    [XmlRoot(ElementName = "dlstatus")]
    public class DlstatusProgressResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";
    }

    [XmlRoot(ElementName = "posevent")]
    public class PoseventRequest : IStellaEAmuseRequest { }

    [XmlRoot(ElementName = "posevent")]
    public class PoseventResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";
    }

    /// <summary>
    /// asphyxia <c>posevent.income.sales.sale</c> returns
    /// <c>send.object([{@attr:{status:0}}, x3])</c> — three sibling
    /// <c>&lt;posevent status="0"/&gt;</c> elements under <c>&lt;response&gt;</c>.
    /// </summary>
    public class PoseventMultiResponse : IStellaMultiElementResponse
    {
        public string ElementName => "posevent";

        public IReadOnlyList<object> Elements { get; } = new object[]
        {
            new PoseventResponse(),
            new PoseventResponse(),
            new PoseventResponse(),
        };
    }

    [XmlRoot(ElementName = "ins")]
    public class InsNetlogRequest : IStellaEAmuseRequest { }

    [XmlRoot(ElementName = "ins")]
    public class InsNetlogResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";
    }
}