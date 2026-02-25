namespace SergeiM.Soap;

/// <summary>Content-Type values for SOAP requests.</summary>
public static class SoapMediaType
{
    /// <summary>Content-Type for SOAP 1.1 requests: <c>text/xml; charset=utf-8</c>.</summary>
    public const string Soap11 = "text/xml; charset=utf-8";

    /// <summary>Content-Type for SOAP 1.2 requests: <c>application/soap+xml; charset=utf-8</c>.</summary>
    public const string Soap12 = "application/soap+xml; charset=utf-8";
}
