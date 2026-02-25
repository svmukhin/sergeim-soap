using System.Xml;

namespace SergeiM.Soap.Tests;

[TestClass]
public sealed class EnvelopeBuilderTests
{
    private const string Soap11Response = """
        <?xml version="1.0" encoding="utf-8"?>
        <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
          <env:Body><ping/></env:Body>
        </env:Envelope>
        """;

    [TestMethod]
    public void Build_NoHeader_OmitsHeaderElement()
    {
        var xml = new EnvelopeBuilder().WithBody("<ping/>").Build();
        StringAssert.DoesNotMatch(xml, new System.Text.RegularExpressions.Regex("env:Header"));
    }

    [TestMethod]
    public void Build_WithHeader_IncludesHeaderElement()
    {
        var xml = new EnvelopeBuilder().WithHeader("<auth>token</auth>").Build();
        StringAssert.Contains(xml, "env:Header");
        StringAssert.Contains(xml, "<auth>token</auth>");
    }

    [TestMethod]
    public void Build_WithHeader_MultipleCallsConcatenate()
    {
        var xml = new EnvelopeBuilder()
            .WithHeader("<a/>")
            .WithHeader("<b/>")
            .Build();
        StringAssert.Contains(xml, "<a/>");
        StringAssert.Contains(xml, "<b/>");
    }

    [TestMethod]
    public void Build_WithBody_IncludesBodyContent()
    {
        var xml = new EnvelopeBuilder().WithBody("<ping/>").Build();
        StringAssert.Contains(xml, "env:Body");
        StringAssert.Contains(xml, "<ping/>");
    }

    [TestMethod]
    public void Build_Default_EmptyBody()
    {
        var xml = new EnvelopeBuilder().Build();
        StringAssert.Contains(xml, "env:Body");
    }

    [TestMethod]
    public void WithNamespace_AddsDeclaration()
    {
        var xml = new EnvelopeBuilder()
            .WithNamespace("svc", "http://example.com/svc")
            .Build();
        StringAssert.Contains(xml, "xmlns:svc=\"http://example.com/svc\"");
    }

    [TestMethod]
    public void WithNamespace_MultipleNamespaces()
    {
        var xml = new EnvelopeBuilder()
            .WithNamespace("svc", "http://example.com/svc")
            .WithNamespace("sec", "http://example.com/sec")
            .Build();
        StringAssert.Contains(xml, "xmlns:svc=\"http://example.com/svc\"");
        StringAssert.Contains(xml, "xmlns:sec=\"http://example.com/sec\"");
    }

    [TestMethod]
    public void ContentType_Soap11()
    {
        Assert.AreEqual(SoapMediaType.Soap11, new EnvelopeBuilder(SoapVersion.Soap11).ContentType);
    }

    [TestMethod]
    public void ContentType_Soap12()
    {
        Assert.AreEqual(SoapMediaType.Soap12, new EnvelopeBuilder(SoapVersion.Soap12).ContentType);
    }

    [TestMethod]
    public void Build_Soap11_UsesCorrectNamespace()
    {
        var xml = new EnvelopeBuilder(SoapVersion.Soap11).Build();
        StringAssert.Contains(xml, SoapNamespaces.Soap11Envelope);
    }

    [TestMethod]
    public void Build_Soap12_UsesCorrectNamespace()
    {
        var xml = new EnvelopeBuilder(SoapVersion.Soap12).Build();
        StringAssert.Contains(xml, SoapNamespaces.Soap12Envelope);
    }

    [TestMethod]
    public void Back_StandaloneBuilder_Throws()
    {
        Assert.ThrowsException<InvalidOperationException>(() => new EnvelopeBuilder().Back());
    }

    [TestMethod]
    public void Back_WiredToSoapRequest_ReturnsSoapRequest()
    {
        var wire = new MockWire(200, Soap11Response);
        var result = new SoapRequest("https://example.com", wire)
            .Envelope()
            .WithBody("<ping/>")
            .Back();
        Assert.IsInstanceOfType<SoapRequest>(result);
    }

    [TestMethod]
    public void Back_WiredToSoapRequest_SetsCorrectBody()
    {
        var wire = new MockWire(200, Soap11Response);
        var builder = new SoapRequest("https://example.com", wire).Envelope().WithBody("<ping/>");
        var expected = builder.Build();
        builder.Back().Fetch();
        Assert.AreEqual(expected, wire.LastBody);
    }

    [TestMethod]
    public void Back_WiredToSoapRequest_SetsContentType()
    {
        var wire = new MockWire(200, Soap11Response);
        var builder = new SoapRequest("https://example.com", wire).Envelope().WithBody("<ping/>");
        builder.Back().Fetch();
        Assert.AreEqual(builder.ContentType, wire.LastHeaders?.GetValueOrDefault("Content-Type"));
    }

    [TestMethod]
    public void Build_OutputIsValidXml()
    {
        var xml = new EnvelopeBuilder()
            .WithNamespace("svc", "http://example.com/svc")
            .WithHeader("<auth>token</auth>")
            .WithBody("<svc:Ping/>")
            .Build();
        var doc = new XmlDocument();
        doc.LoadXml(xml);
        Assert.IsNotNull(doc.DocumentElement);
    }
}
