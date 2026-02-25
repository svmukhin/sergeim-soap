// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

namespace SergeiM.Soap;

/// <summary>
/// Thrown when a SOAP response contains a <c>&lt;Fault&gt;</c> element.
/// <example>
/// try { response.AssertNoFault(); }
/// catch (SoapFaultException ex) { Console.WriteLine(ex.Fault.Code); }
/// </example>
/// </summary>
public sealed class SoapFaultException : Exception
{
    /// <summary>The parsed <see cref="SoapFault"/> that caused this exception.</summary>
    public SoapFault Fault { get; }

    /// <summary>
    /// Initialises a new <see cref="SoapFaultException"/> from the given <paramref name="fault"/>.
    /// The exception message is formatted as <c>SOAP Fault [code]: reason</c>.
    /// </summary>
    /// <param name="fault">The fault decoded from the SOAP response.</param>
    public SoapFaultException(SoapFault fault)
        : base($"SOAP Fault [{fault.Code}]: {fault.Reason}")
    {
        Fault = fault;
    }
}
