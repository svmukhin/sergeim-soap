using System.Net;
using System.Net.Http;
using System.Text;

namespace SergeiM.Soap.Tests;

[TestClass]
public sealed class SoapResponseTests
{
    private const string Soap11Normal = """
        <?xml version="1.0" encoding="utf-8"?>
        <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
          <env:Body>
            <svc:GetUserResponse xmlns:svc="http://example.com/svc">
              <svc:Name>Alice</svc:Name>
            </svc:GetUserResponse>
          </env:Body>
        </env:Envelope>
        """;

    private const string Soap11Fault = """
        <?xml version="1.0" encoding="utf-8"?>
        <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
          <env:Body>
            <env:Fault>
              <faultcode>env:Server</faultcode>
              <faultstring>Internal error</faultstring>
            </env:Fault>
          </env:Body>
        </env:Envelope>
        """;

    private static SoapResponse MakeResponse(string body, int statusCode = 200)
    {
        var msg = new HttpResponseMessage((HttpStatusCode)statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "text/xml")
        };
        return new SoapResponse(msg);
    }

    [TestMethod]
    public void Envelope_LazilyInitialised()
    {
        var response = MakeResponse(Soap11Normal);
        var first = response.Envelope;
        var second = response.Envelope;
        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void Envelope_ReturnsCorrectVersion()
    {
        var response = MakeResponse(Soap11Normal);
        Assert.AreEqual(SoapVersion.Soap11, response.Envelope.Version);
    }

    [TestMethod]
    public void Envelope_XmlDocument_NotNull()
    {
        var response = MakeResponse(Soap11Normal);
        Assert.IsNotNull(response.Envelope.XmlDocument);
    }

    [TestMethod]
    public void AssertNoFault_PassesOnNormalResponse()
    {
        var response = MakeResponse(Soap11Normal);
        var result = response.AssertNoFault();
        Assert.AreSame(response, result);
    }

    [TestMethod]
    public void AssertNoFault_ThrowsOnFaultResponse()
    {
        var response = MakeResponse(Soap11Fault);
        Assert.ThrowsException<SoapFaultException>(() => response.AssertNoFault());
    }

    [TestMethod]
    public void AssertNoFault_FaultExceptionContainsFault()
    {
        var response = MakeResponse(Soap11Fault);
        var ex = Assert.ThrowsException<SoapFaultException>(() => response.AssertNoFault());
        Assert.AreEqual("env:Server", ex.Fault.Code);
        Assert.AreEqual("Internal error", ex.Fault.Reason);
    }

    [TestMethod]
    public void AssertStatus_ReturnsThisSoapResponse()
    {
        var response = MakeResponse(Soap11Normal, 200);
        var result = response.AssertStatus(200);
        Assert.AreSame(response, result);
    }

    [TestMethod]
    public void AssertStatus_WrongCode_Throws()
    {
        var response = MakeResponse(Soap11Normal, 200);
        Assert.ThrowsException<System.Net.Http.HttpRequestException>(() => response.AssertStatus(500));
    }

    [TestMethod]
    public void AssertStatus_ChainWithAssertNoFault()
    {
        var response = MakeResponse(Soap11Normal, 200);
        var result = response.AssertStatus(200).AssertNoFault();
        Assert.AreSame(response, result);
    }
}
