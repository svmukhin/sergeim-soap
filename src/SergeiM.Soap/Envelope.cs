using System.Xml;
using System.Xml.XPath;

namespace SergeiM.Soap;

/// <summary>
/// Encapsulates a parsed SOAP envelope and exposes its structure as typed properties.
/// Parsing is lazy — the raw XML is not parsed until a property or method is first accessed.
/// <example>
/// var env = new Envelope(rawXml);
/// var name = env.EvaluateXPath("//svc:Name", ns => ns.AddNamespace("svc", "http://example.com/svc"));
/// </example>
/// </summary>
public sealed class Envelope
{
    private readonly string? _rawXml;
    private XmlDocument? _document;
    private XmlNamespaceManager? _nsManager;
    private SoapFault? _fault;

    /// <summary>Initialises an <see cref="Envelope"/> from a raw SOAP XML string. Parsing is deferred.</summary>
    /// <param name="rawXml">The raw SOAP XML string to parse.</param>
    public Envelope(string rawXml)
    {
        _rawXml = rawXml;
    }

    /// <summary>Initialises an <see cref="Envelope"/> by wrapping an already-parsed <see cref="XmlDocument"/>.</summary>
    /// <param name="document">The parsed XML document to wrap.</param>
    public Envelope(XmlDocument document)
    {
        _document = document;
    }

    private XmlDocument ResolvedDocument
    {
        get
        {
            if (_document is not null)
                return _document;
            var doc = new XmlDocument();
            doc.LoadXml(_rawXml!);
            _document = doc;
            return _document;
        }
    }

    private XmlNamespaceManager NsManager
    {
        get
        {
            if (_nsManager is not null)
                return _nsManager;
            var envelopeNs = Version == SoapVersion.Soap11
                ? SoapNamespaces.Soap11Envelope
                : SoapNamespaces.Soap12Envelope;
            var mgr = new XmlNamespaceManager(ResolvedDocument.NameTable);
            mgr.AddNamespace(SoapNamespaces.EnvPrefix, envelopeNs);
            _nsManager = mgr;
            return _nsManager;
        }
    }

    /// <summary>The SOAP protocol version detected from the root element namespace.</summary>
    public SoapVersion Version
    {
        get
        {
            var ns = ResolvedDocument.DocumentElement?.NamespaceURI;
            return ns switch
            {
                SoapNamespaces.Soap11Envelope => SoapVersion.Soap11,
                SoapNamespaces.Soap12Envelope => SoapVersion.Soap12,
                _ => throw new InvalidOperationException($"Unknown SOAP envelope namespace: '{ns}'.")
            };
        }
    }

    /// <summary>The underlying parsed XML document.</summary>
    public XmlDocument XmlDocument => ResolvedDocument;

    /// <summary>The <c>&lt;env:Header&gt;</c> element, or <c>null</c> if the header is absent.</summary>
    public XmlElement? Header =>
        ResolvedDocument.SelectSingleNode($"//{SoapNamespaces.EnvPrefix}:Header", NsManager) as XmlElement;

    /// <summary>The <c>&lt;env:Body&gt;</c> element. Always present in a valid SOAP envelope.</summary>
    /// <exception cref="InvalidOperationException">Thrown when no <c>&lt;Body&gt;</c> element is found.</exception>
    public XmlElement Body =>
        ResolvedDocument.SelectSingleNode($"//{SoapNamespaces.EnvPrefix}:Body", NsManager) as XmlElement
        ?? throw new InvalidOperationException("SOAP envelope does not contain a <Body> element.");

    /// <summary><c>true</c> when the <c>&lt;env:Body&gt;</c> contains a direct <c>&lt;env:Fault&gt;</c> child.</summary>
    public bool IsFault =>
        Body.SelectSingleNode($"{SoapNamespaces.EnvPrefix}:Fault", NsManager) is not null;

    /// <summary>The parsed <see cref="SoapFault"/>, or <c>null</c> when <see cref="IsFault"/> is <c>false</c>.</summary>
    public SoapFault? Fault
    {
        get
        {
            if (!IsFault)
                return null;
            return _fault ??= ParseFault();
        }
    }

    private SoapFault ParseFault()
    {
        var faultNode = Body.SelectSingleNode($"{SoapNamespaces.EnvPrefix}:Fault", NsManager) as XmlElement
            ?? throw new InvalidOperationException("Fault element not found.");
        if (Version == SoapVersion.Soap11)
            return ParseSoap11Fault(faultNode);
        return ParseSoap12Fault(faultNode);
    }

    private static SoapFault ParseSoap11Fault(XmlElement fault)
    {
        var code = fault.SelectSingleNode("faultcode")?.InnerText ?? string.Empty;
        var reason = fault.SelectSingleNode("faultstring")?.InnerText ?? string.Empty;
        var detail = fault.SelectSingleNode("detail")?.InnerText;
        var actor = fault.SelectSingleNode("faultactor")?.InnerText;
        return new SoapFault(code, reason, detail, actor);
    }

    private SoapFault ParseSoap12Fault(XmlElement fault)
    {
        var code = fault.SelectSingleNode($"{SoapNamespaces.EnvPrefix}:Code/{SoapNamespaces.EnvPrefix}:Value", NsManager)?.InnerText ?? string.Empty;
        var reason = fault.SelectSingleNode($"{SoapNamespaces.EnvPrefix}:Reason/{SoapNamespaces.EnvPrefix}:Text", NsManager)?.InnerText ?? string.Empty;
        var detail = fault.SelectSingleNode($"{SoapNamespaces.EnvPrefix}:Detail", NsManager)?.InnerText;
        var actor = fault.SelectSingleNode($"{SoapNamespaces.EnvPrefix}:Role", NsManager)?.InnerText;
        return new SoapFault(code, reason, detail, actor);
    }

    /// <summary>
    /// Returns the first child element of <c>&lt;env:Body&gt;</c> matching <paramref name="localName"/>
    /// and optionally <paramref name="ns"/>.
    /// <example>
    /// var el = envelope.GetBodyElement("GetUserResponse", "http://example.com/svc");
    /// </example>
    /// </summary>
    /// <param name="localName">The local name of the element to find.</param>
    /// <param name="ns">The namespace URI to match, or <c>null</c> to ignore namespace.</param>
    /// <exception cref="InvalidOperationException">Thrown when no matching element is found.</exception>
    public XmlElement GetBodyElement(string localName, string? ns = null)
    {
        foreach (XmlNode node in Body.ChildNodes)
        {
            if (node is not XmlElement el)
                continue;
            if (el.LocalName != localName)
                continue;
            if (ns is not null && el.NamespaceURI != ns)
                continue;
            return el;
        }
        throw new InvalidOperationException(
            $"No child element '{localName}' (ns: '{ns ?? "any"}') found in <Body>.");
    }

    /// <summary>
    /// Evaluates an XPath expression against the document root.
    /// The <c>env</c> prefix is pre-registered to the correct envelope namespace.
    /// Additional prefixes may be supplied via <paramref name="additionalNs"/>.
    /// <example>
    /// var nav = envelope.Navigate("//env:Body/svc:GetResponse",
    ///     additionalNs: BuildNs("svc", "http://example.com/svc"));
    /// </example>
    /// </summary>
    /// <param name="xpath">The XPath expression to evaluate.</param>
    /// <param name="additionalNs">Additional namespace manager to merge, or <c>null</c>.</param>
    /// <exception cref="InvalidOperationException">Thrown when the expression matches no node.</exception>
    public XPathNavigator Navigate(string xpath, XmlNamespaceManager? additionalNs = null)
    {
        var nav = ResolvedDocument.CreateNavigator()!;
        var ns = additionalNs is null ? NsManager : BuildCombinedNsManager(additionalNs);
        return nav.SelectSingleNode(xpath, ns)
            ?? throw new InvalidOperationException($"XPath expression '{xpath}' matched no nodes.");
    }

    /// <summary>
    /// Evaluates an XPath expression and returns the result as a string.
    /// <example>
    /// string name = envelope.EvaluateXPath("//env:Body/svc:Name/text()",
    ///     additionalNs: BuildNs("svc", "http://example.com/svc"));
    /// </example>
    /// </summary>
    /// <param name="xpath">The XPath expression to evaluate.</param>
    /// <param name="additionalNs">Additional namespace manager to merge, or <c>null</c>.</param>
    public string EvaluateXPath(string xpath, XmlNamespaceManager? additionalNs = null)
    {
        var nav = ResolvedDocument.CreateNavigator()!;
        var ns = additionalNs is null ? NsManager : BuildCombinedNsManager(additionalNs);
        return nav.Evaluate(xpath, ns)?.ToString() ?? string.Empty;
    }

    private XmlNamespaceManager BuildCombinedNsManager(XmlNamespaceManager additionalNs)
    {
        var combined = new XmlNamespaceManager(ResolvedDocument.NameTable);
        foreach (var pair in NsManager.GetNamespacesInScope(XmlNamespaceScope.All))
            combined.AddNamespace(pair.Key, pair.Value);
        foreach (var pair in additionalNs.GetNamespacesInScope(XmlNamespaceScope.All))
            combined.AddNamespace(pair.Key, pair.Value);
        return combined;
    }
}
