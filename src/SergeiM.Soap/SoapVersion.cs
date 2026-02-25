namespace SergeiM.Soap;

/// <summary>Identifies the SOAP protocol version used by a request or response.</summary>
public enum SoapVersion
{
    /// <summary>SOAP 1.1 — uses <c>text/xml</c> and the <c>SOAPAction</c> header.</summary>
    Soap11,

    /// <summary>SOAP 1.2 — uses <c>application/soap+xml</c> with an <c>action</c> parameter.</summary>
    Soap12,
}
