using SergeiM.Http;
using SergeiM.Http.Request;

namespace SergeiM.Soap;

/// <summary>Stub — full implementation in Phase 6.</summary>
public class SoapRequest : BaseRequest
{
    /// <summary>Creates a SOAP request for the given URI.</summary>
    public SoapRequest(string uri) : base(uri) { }

    internal SoapRequest ApplyEnvelope(string xml, string contentType)
        => throw new NotImplementedException("Full SoapRequest implemented in Phase 6.");
}
