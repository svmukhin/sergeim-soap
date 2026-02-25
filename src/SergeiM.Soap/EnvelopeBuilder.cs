using System.Text;

namespace SergeiM.Soap;

/// <summary>
/// Fluent builder that constructs a valid SOAP envelope XML string.
/// Use the public constructor for standalone building, or obtain an instance
/// via <see cref="SoapRequest.Envelope()"/> to chain back to the parent request.
/// <example>
/// string xml = new EnvelopeBuilder(SoapVersion.Soap12)
///     .WithNamespace("svc", "http://example.com/svc")
///     .WithBody("&lt;svc:Ping/&gt;")
///     .Build();
/// </example>
/// </summary>
public sealed class EnvelopeBuilder
{
    private readonly SoapVersion _version;
    private readonly SoapRequest? _parent;
    private readonly List<string> _headerFragments = [];
    private readonly Dictionary<string, string> _namespaces = [];
    private string? _body;

    /// <summary>Creates a standalone <see cref="EnvelopeBuilder"/> with no parent request.</summary>
    /// <param name="version">The SOAP version to use. Defaults to <see cref="SoapVersion.Soap11"/>.</param>
    public EnvelopeBuilder(SoapVersion version = SoapVersion.Soap11)
    {
        _version = version;
    }

    /// <summary>Creates an <see cref="EnvelopeBuilder"/> wired to a parent <see cref="SoapRequest"/>.</summary>
    internal EnvelopeBuilder(SoapRequest parent, SoapVersion version)
    {
        _parent = parent;
        _version = version;
    }

    /// <summary>The Content-Type value for the selected SOAP version.</summary>
    public string ContentType => _version == SoapVersion.Soap11
        ? SoapMediaType.Soap11
        : SoapMediaType.Soap12;

    /// <summary>
    /// Appends a raw XML fragment inside <c>&lt;env:Header&gt;</c>.
    /// Can be called multiple times; fragments are concatenated in order.
    /// </summary>
    public EnvelopeBuilder WithHeader(string rawXml)
    {
        _headerFragments.Add(rawXml);
        return this;
    }

    /// <summary>Sets the content of <c>&lt;env:Body&gt;</c>, replacing any previously set body.</summary>
    public EnvelopeBuilder WithBody(string rawXml)
    {
        _body = rawXml;
        return this;
    }

    /// <summary>Adds an <c>xmlns:prefix="uri"</c> declaration to the root <c>&lt;env:Envelope&gt;</c> element.</summary>
    public EnvelopeBuilder WithNamespace(string prefix, string uri)
    {
        _namespaces[prefix] = uri;
        return this;
    }

    /// <summary>
    /// Applies the built envelope to the parent <see cref="SoapRequest"/> and returns it.
    /// Equivalent to calling <c>parent.Body(Build(), ContentType)</c>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called on a standalone builder created without a parent <see cref="SoapRequest"/>.
    /// </exception>
    public SoapRequest Back()
    {
        if (_parent is null)
            throw new InvalidOperationException(
                "Back() called on a standalone EnvelopeBuilder that has no parent SoapRequest.");
        return _parent.ApplyEnvelope(Build(), ContentType);
    }

    /// <summary>
    /// Builds and returns the complete SOAP envelope XML string.
    /// <example>
    /// string xml = new EnvelopeBuilder()
    ///     .WithBody("&lt;ping/&gt;")
    ///     .Build();
    /// // Result starts with: &lt;?xml version="1.0" encoding="utf-8"?&gt;
    /// </example>
    /// </summary>
    public string Build()
    {
        var envelopeNs = _version == SoapVersion.Soap11
            ? SoapNamespaces.Soap11Envelope
            : SoapNamespaces.Soap12Envelope;
        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.Append($"<{SoapNamespaces.EnvPrefix}:Envelope xmlns:{SoapNamespaces.EnvPrefix}=\"{envelopeNs}\"");
        foreach (var (prefix, uri) in _namespaces)
            sb.Append($" xmlns:{prefix}=\"{uri}\"");
        sb.Append('>');
        if (_headerFragments.Count > 0)
        {
            sb.Append($"<{SoapNamespaces.EnvPrefix}:Header>");
            foreach (var fragment in _headerFragments)
                sb.Append(fragment);
            sb.Append($"</{SoapNamespaces.EnvPrefix}:Header>");
        }
        sb.Append($"<{SoapNamespaces.EnvPrefix}:Body>");
        sb.Append(_body ?? string.Empty);
        sb.Append($"</{SoapNamespaces.EnvPrefix}:Body>");
        sb.Append($"</{SoapNamespaces.EnvPrefix}:Envelope>");
        return sb.ToString();
    }
}
