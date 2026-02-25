// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

namespace SergeiM.Soap;

/// <summary>XML namespace URIs and prefixes used in SOAP envelopes.</summary>
public static class SoapNamespaces
{
    /// <summary>SOAP 1.1 envelope namespace URI: <c>http://schemas.xmlsoap.org/soap/envelope/</c>.</summary>
    public const string Soap11Envelope = "http://schemas.xmlsoap.org/soap/envelope/";

    /// <summary>SOAP 1.2 envelope namespace URI: <c>http://www.w3.org/2003/05/soap-envelope</c>.</summary>
    public const string Soap12Envelope = "http://www.w3.org/2003/05/soap-envelope";

    /// <summary>SOAP 1.1 encoding namespace URI: <c>http://schemas.xmlsoap.org/soap/encoding/</c>.</summary>
    public const string Soap11Encoding = "http://schemas.xmlsoap.org/soap/encoding/";

    /// <summary>SOAP 1.2 encoding namespace URI: <c>http://www.w3.org/2003/05/soap-encoding</c>.</summary>
    public const string Soap12Encoding = "http://www.w3.org/2003/05/soap-encoding";

    /// <summary>Namespace prefix used in XPath queries for envelope elements, e.g. <c>env:Body</c>.</summary>
    public const string EnvPrefix = "env";
}
