// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using SergeiM.Http.Response;

namespace SergeiM.Soap;

/// <summary>
/// HTTP response wrapper for SOAP responses. Extends <see cref="XmlResponse"/> with
/// a parsed <see cref="Soap.Envelope"/> and fault assertion helpers.
/// <example>
/// response.AssertStatus(200).AssertNoFault();
/// var name = response.Envelope.EvaluateXPath("//svc:Name", ns);
/// </example>
/// </summary>
public class SoapResponse : XmlResponse
{
    private Envelope? _envelope;

    /// <summary>Initialises a new <see cref="SoapResponse"/> wrapping the given HTTP response.</summary>
    /// <param name="response">The raw HTTP response message.</param>
    public SoapResponse(HttpResponseMessage response) : base(response) { }

    /// <summary>
    /// Lazily constructs and returns the parsed <see cref="Soap.Envelope"/> from the response body.
    /// The same instance is returned on every subsequent access.
    /// </summary>
    public Envelope Envelope => _envelope ??= new Envelope(XmlDocument);

    /// <summary>
    /// Asserts that the response does not contain a SOAP fault.
    /// </summary>
    /// <returns>This <see cref="SoapResponse"/> for fluent chaining.</returns>
    /// <exception cref="SoapFaultException">Thrown when the envelope contains a <c>&lt;Fault&gt;</c> element.</exception>
    public SoapResponse AssertNoFault()
    {
        if (Envelope.IsFault)
            throw new SoapFaultException(Envelope.Fault!);
        return this;
    }

    /// <summary>
    /// Asserts that the HTTP status code matches <paramref name="expectedStatus"/>.
    /// </summary>
    /// <returns>This <see cref="SoapResponse"/> for fluent chaining.</returns>
    /// <exception cref="System.Net.Http.HttpRequestException">Thrown when the status code does not match.</exception>
    public new SoapResponse AssertStatus(int expectedStatus)
    {
        base.AssertStatus(expectedStatus);
        return this;
    }
}
