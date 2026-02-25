// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

namespace SergeiM.Soap.Tests;

[TestClass]
public sealed class SoapRequestTests
{
    private const string Soap11Normal = """
        <?xml version="1.0" encoding="utf-8"?>
        <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
          <env:Body><ping/></env:Body>
        </env:Envelope>
        """;

    private static MockWire Wire(string body = Soap11Normal, int status = 200)
        => new(status, body);

    [TestMethod]
    public void DefaultMethod_IsPost()
    {
        var wire = Wire();
        new SoapRequest("https://example.com", wire).Fetch();
        Assert.AreEqual("POST", wire.LastMethod);
    }

    [TestMethod]
    public void Fetch_ReturnsSoapResponse()
    {
        var result = new SoapRequest("https://example.com", Wire()).Fetch();
        Assert.IsInstanceOfType<SoapResponse>(result);
    }

    [TestMethod]
    public async Task FetchAsync_ReturnsSoapResponse()
    {
        var result = await new SoapRequest("https://example.com", Wire()).FetchAsync();
        Assert.IsInstanceOfType<SoapResponse>(result);
    }

    [TestMethod]
    public void SoapAction_Soap11_SetsHeader()
    {
        var wire = Wire();
        new SoapRequest("https://example.com", wire)
            .SoapAction("http://example.com/GetUser")
            .Fetch();
        Assert.IsTrue(wire.LastHeaders!.ContainsKey("SOAPAction"));
        StringAssert.Contains(wire.LastHeaders["SOAPAction"], "http://example.com/GetUser");
    }

    [TestMethod]
    public void SoapAction_Soap12_SetsContentTypeParam()
    {
        var wire = Wire();
        new SoapRequest("https://example.com", wire, SoapVersion.Soap12)
            .SoapAction("http://example.com/GetUser")
            .Envelope()
            .WithBody("<ping/>")
            .Back()
            .Fetch();
        StringAssert.Contains(wire.LastHeaders!["Content-Type"], "action=\"http://example.com/GetUser\"");
    }

    [TestMethod]
    public void Envelope_ReturnsEnvelopeBuilder()
    {
        var builder = new SoapRequest("https://example.com", Wire()).Envelope();
        Assert.IsInstanceOfType<EnvelopeBuilder>(builder);
    }

    [TestMethod]
    public void Envelope_WithBody_ProducesExpectedXml()
    {
        var request = new SoapRequest("https://example.com", Wire());
        var xml = request.Envelope().WithBody("<ping/>").Build();
        StringAssert.Contains(xml, "<ping/>");
        StringAssert.Contains(xml, "env:Body");
    }

    [TestMethod]
    public void Envelope_Back_ReturnsSoapRequest()
    {
        var result = new SoapRequest("https://example.com", Wire())
            .Envelope()
            .WithBody("<ping/>")
            .Back();
        Assert.IsInstanceOfType<SoapRequest>(result);
    }

    [TestMethod]
    public void Envelope_Back_SetsCorrectBodyOnWire()
    {
        var wire = Wire();
        var request = new SoapRequest("https://example.com", wire);
        var expected = request.Envelope().WithBody("<ping/>").Build();
        request.Envelope().WithBody("<ping/>").Back().Fetch();
        Assert.AreEqual(expected, wire.LastBody);
    }

    [TestMethod]
    public void Envelope_Back_SetsContentType()
    {
        var wire = Wire();
        var builder = new SoapRequest("https://example.com", wire).Envelope();
        builder.WithBody("<ping/>").Back().Fetch();
        var sentContentType = wire.LastHeaders?.GetValueOrDefault("Content-Type");
        Assert.AreEqual(builder.ContentType, sentContentType);
    }

    [TestMethod]
    public void WithVersion_Soap12_ChangesVersion()
    {
        var request = new SoapRequest("https://example.com", Wire())
            .WithVersion(SoapVersion.Soap12);
        var ct = request.Envelope().ContentType;
        Assert.AreEqual(SoapMediaType.Soap12, ct);
    }
}
