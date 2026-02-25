// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

namespace SergeiM.Soap;

/// <summary>
/// Represents a decoded SOAP <c>&lt;Fault&gt;</c> element, unified across SOAP 1.1 and 1.2.
/// <example>
/// SOAP 1.1 source elements: <c>faultcode</c>, <c>faultstring</c>, <c>detail</c>, <c>faultactor</c>.
/// SOAP 1.2 source elements: <c>Code/Value</c>, <c>Reason/Text</c>, <c>Detail</c>, <c>Role</c>.
/// </example>
/// </summary>
public sealed class SoapFault
{
    /// <summary>Fault code (SOAP 1.1: <c>faultcode</c>; SOAP 1.2: <c>Code/Value</c>).</summary>
    public string Code { get; }

    /// <summary>Human-readable fault reason (SOAP 1.1: <c>faultstring</c>; SOAP 1.2: <c>Reason/Text</c>).</summary>
    public string Reason { get; }

    /// <summary>Optional fault detail as raw inner text (SOAP 1.1: <c>detail</c>; SOAP 1.2: <c>Detail</c>).</summary>
    public string? Detail { get; }

    /// <summary>Optional actor or role URI (SOAP 1.1: <c>faultactor</c>; SOAP 1.2: <c>Role</c>).</summary>
    public string? Actor { get; }

    /// <summary>Initialises a new <see cref="SoapFault"/> with all fault fields.</summary>
    /// <param name="code">The fault code.</param>
    /// <param name="reason">The fault reason.</param>
    /// <param name="detail">The optional fault detail text.</param>
    /// <param name="actor">The optional actor or role URI.</param>
    public SoapFault(string code, string reason, string? detail, string? actor)
    {
        Code = code;
        Reason = reason;
        Detail = detail;
        Actor = actor;
    }
}
