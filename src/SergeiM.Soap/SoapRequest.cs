// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using SergeiM.Http;
using SergeiM.Http.Request;
using SergeiM.Http.Wire;

namespace SergeiM.Soap;

/// <summary>
/// Fluent SOAP request builder. All fluent methods return a new <see cref="SoapRequest"/> — the builder is immutable.
/// Wraps an <see cref="IRequest"/> and delegates all HTTP concerns to it, adding only SOAP-specific behaviour on top.
/// <example>
/// // Bring a pre-configured request (auth, retries, tenant headers, etc.):
/// IRequest inner = new BaseRequest("https://example.com/soap", wire).Method("POST");
/// var response = new SoapRequest(inner)
///     .SoapAction("http://example.com/GetUser")
///     .Envelope()
///         .WithNamespace("svc", "http://example.com/svc")
///         .WithBody("&lt;svc:GetUser&gt;&lt;svc:Id&gt;42&lt;/svc:Id&gt;&lt;/svc:GetUser&gt;")
///         .Back()
///     .Fetch();
/// </example>
/// </summary>
public sealed class SoapRequest
{
    private readonly IRequest _inner;
    private readonly SoapVersion _version;
    private readonly string? _soap12Action;

    /// <summary>Creates a SOAP 1.1 POST request for the given URI using the default <see cref="HttpWire"/>.</summary>
    public SoapRequest(string uri)
        : this(new BaseRequest(uri).Method("POST")) { }

    /// <summary>Creates a SOAP 1.1 POST request for the given URI using a custom <see cref="IWire"/>.</summary>
    public SoapRequest(string uri, IWire wire)
        : this(new BaseRequest(uri, wire).Method("POST")) { }

    /// <summary>Creates a SOAP POST request for the given URI using a custom <see cref="IWire"/> and version.</summary>
    public SoapRequest(string uri, IWire wire, SoapVersion version)
        : this(new BaseRequest(uri, wire).Method("POST"), version) { }

    /// <summary>
    /// Wraps an already-configured <see cref="IRequest"/> with SOAP behaviour.
    /// Use this overload to supply auth, retries, custom headers, or any other pre-configured HTTP state.
    /// </summary>
    public SoapRequest(IRequest inner, SoapVersion version = SoapVersion.Soap11)
        : this(inner, version, null) { }

    private SoapRequest(IRequest inner, SoapVersion version, string? soap12Action)
    {
        _inner = inner;
        _version = version;
        _soap12Action = soap12Action;
    }

    /// <summary>
    /// Sets the SOAP action.
    /// For SOAP 1.1 adds a <c>SOAPAction</c> header to the inner request.
    /// For SOAP 1.2 stores the action and merges it into Content-Type when <see cref="EnvelopeBuilder.Back"/> is called.
    /// </summary>
    public SoapRequest SoapAction(string action)
    {
        if (_version == SoapVersion.Soap11)
            return new(_inner.Header("SOAPAction", $"\"{action}\""), _version, null);
        return new(_inner, _version, action);
    }

    /// <summary>Returns a new <see cref="SoapRequest"/> with the specified SOAP version.</summary>
    public SoapRequest WithVersion(SoapVersion version) => new(_inner, version, _soap12Action);

    /// <summary>
    /// Returns an <see cref="EnvelopeBuilder"/> pre-wired to this request.
    /// Call <see cref="EnvelopeBuilder.Back"/> on the builder to get back an updated <see cref="SoapRequest"/>.
    /// </summary>
    public EnvelopeBuilder Envelope() => new(this, _version);

    /// <summary>
    /// Applies a built envelope XML and Content-Type to a new <see cref="SoapRequest"/> via <see cref="IRequest.Body(string, string)"/>.
    /// For SOAP 1.2, appends the stored <c>action</c> parameter to the Content-Type before delegating.
    /// Called internally by <see cref="EnvelopeBuilder.Back"/>.
    /// </summary>
    internal SoapRequest ApplyEnvelope(string xml, string contentType)
    {
        var ct = _soap12Action is not null
            ? $"{contentType}; action=\"{_soap12Action}\""
            : contentType;
        return new(_inner.Body(xml, ct), _version, _soap12Action);
    }

    /// <summary>
    /// Executes the request asynchronously and returns a <see cref="SoapResponse"/>.
    /// Delegates to the inner <see cref="IRequest.FetchAsync"/> and converts the result via <c>As&lt;SoapResponse&gt;()</c>.
    /// </summary>
    public async Task<SoapResponse> FetchAsync()
    {
        var response = await _inner.FetchAsync();
        return response.As<SoapResponse>();
    }

    /// <summary>Executes the request synchronously and returns a <see cref="SoapResponse"/>.</summary>
    public SoapResponse Fetch() => FetchAsync().GetAwaiter().GetResult();
}
