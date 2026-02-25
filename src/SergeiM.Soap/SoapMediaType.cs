namespace SergeiM.Soap;

/// <summary>
/// Media type values for SOAP requests.
/// The <c>charset=utf-8</c> parameter is intentionally omitted: <see cref="SergeiM.Http.Wire.HttpWire"/>
/// sets the charset automatically via its <c>Encoding.UTF8</c> argument to <c>StringContent</c>.
/// Passing a media type that includes parameters (e.g. <c>text/xml; charset=utf-8</c>) causes a
/// <see cref="System.FormatException"/> in the <c>StringContent</c> constructor on .NET 5+.
/// </summary>
public static class SoapMediaType
{
    /// <summary>Media type for SOAP 1.1 requests: <c>text/xml</c>.</summary>
    public const string Soap11 = "text/xml";

    /// <summary>Media type for SOAP 1.2 requests: <c>application/soap+xml</c>.</summary>
    public const string Soap12 = "application/soap+xml";
}
