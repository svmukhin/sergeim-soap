// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using System.Xml;

namespace SergeiM.Soap.Tests;

[TestClass]
public sealed class EnvelopeTests
{
    private const string Soap11Normal = """
        <?xml version="1.0" encoding="utf-8"?>
        <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
          <env:Header><auth>token</auth></env:Header>
          <env:Body>
            <svc:GetUserResponse xmlns:svc="http://example.com/svc">
              <svc:Name>Alice</svc:Name>
            </svc:GetUserResponse>
            <svc:Extra xmlns:svc="http://other.com/svc">x</svc:Extra>
          </env:Body>
        </env:Envelope>
        """;

    private const string Soap11NoHeader = """
        <?xml version="1.0" encoding="utf-8"?>
        <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
          <env:Body><ping/></env:Body>
        </env:Envelope>
        """;

    private const string Soap11Fault = """
        <?xml version="1.0" encoding="utf-8"?>
        <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
          <env:Body>
            <env:Fault>
              <faultcode>env:Server</faultcode>
              <faultstring>Internal error</faultstring>
              <detail>Stack trace here</detail>
              <faultactor>http://example.com/actor</faultactor>
            </env:Fault>
          </env:Body>
        </env:Envelope>
        """;

    private const string Soap12Normal = """
        <?xml version="1.0" encoding="utf-8"?>
        <env:Envelope xmlns:env="http://www.w3.org/2003/05/soap-envelope">
          <env:Body><ping/></env:Body>
        </env:Envelope>
        """;

    private const string Soap12Fault = """
        <?xml version="1.0" encoding="utf-8"?>
        <env:Envelope xmlns:env="http://www.w3.org/2003/05/soap-envelope">
          <env:Body>
            <env:Fault>
              <env:Code><env:Value>env:Receiver</env:Value></env:Code>
              <env:Reason><env:Text>Internal error</env:Text></env:Reason>
              <env:Detail>Stack trace here</env:Detail>
              <env:Role>http://example.com/role</env:Role>
            </env:Fault>
          </env:Body>
        </env:Envelope>
        """;

    [TestMethod]
    public void Parse_Soap11_DetectsVersion()
    {
        var env = new Envelope(Soap11Normal);
        Assert.AreEqual(SoapVersion.Soap11, env.Version);
    }

    [TestMethod]
    public void Parse_Soap12_DetectsVersion()
    {
        var env = new Envelope(Soap12Normal);
        Assert.AreEqual(SoapVersion.Soap12, env.Version);
    }

    [TestMethod]
    public void Header_PresentWhenInXml()
    {
        var env = new Envelope(Soap11Normal);
        Assert.IsNotNull(env.Header);
    }

    [TestMethod]
    public void Header_NullWhenAbsent()
    {
        var env = new Envelope(Soap11NoHeader);
        Assert.IsNull(env.Header);
    }

    [TestMethod]
    public void Body_ReturnsBodyElement()
    {
        var env = new Envelope(Soap11Normal);
        Assert.AreEqual("Body", env.Body.LocalName);
    }

    [TestMethod]
    public void IsFault_TrueForFaultResponse()
    {
        var env = new Envelope(Soap11Fault);
        Assert.IsTrue(env.IsFault);
    }

    [TestMethod]
    public void IsFault_FalseForNormalResponse()
    {
        var env = new Envelope(Soap11Normal);
        Assert.IsFalse(env.IsFault);
        Assert.IsNull(env.Fault);
    }

    [TestMethod]
    public void Fault_ParsedCorrectly_Soap11()
    {
        var fault = new Envelope(Soap11Fault).Fault!;
        Assert.AreEqual("env:Server", fault.Code);
        Assert.AreEqual("Internal error", fault.Reason);
        Assert.AreEqual("Stack trace here", fault.Detail);
        Assert.AreEqual("http://example.com/actor", fault.Actor);
    }

    [TestMethod]
    public void Fault_ParsedCorrectly_Soap12()
    {
        var fault = new Envelope(Soap12Fault).Fault!;
        Assert.AreEqual("env:Receiver", fault.Code);
        Assert.AreEqual("Internal error", fault.Reason);
        Assert.AreEqual("Stack trace here", fault.Detail);
        Assert.AreEqual("http://example.com/role", fault.Actor);
    }

    [TestMethod]
    public void GetBodyElement_ReturnsCorrectElement()
    {
        var env = new Envelope(Soap11Normal);
        var el = env.GetBodyElement("GetUserResponse");
        Assert.AreEqual("GetUserResponse", el.LocalName);
    }

    [TestMethod]
    public void GetBodyElement_WithNamespace_Filters()
    {
        var env = new Envelope(Soap11Normal);
        var el = env.GetBodyElement("Extra", "http://other.com/svc");
        Assert.AreEqual("Extra", el.LocalName);
        Assert.AreEqual("http://other.com/svc", el.NamespaceURI);
    }

    [TestMethod]
    public void GetBodyElement_ThrowsWhenNotFound()
    {
        var env = new Envelope(Soap11Normal);
        Assert.ThrowsException<InvalidOperationException>(() => env.GetBodyElement("NonExistent"));
    }

    [TestMethod]
    public void EvaluateXPath_ReturnsExpectedValue()
    {
        var env = new Envelope(Soap11Normal);
        var ns = new XmlNamespaceManager(new NameTable());
        ns.AddNamespace("svc", "http://example.com/svc");
        var result = env.EvaluateXPath("string(//svc:Name)", ns);
        Assert.AreEqual("Alice", result);
    }

    [TestMethod]
    public void Parse_MalformedXml_Throws()
    {
        var env = new Envelope("<broken");
        Assert.ThrowsException<XmlException>(() => _ = env.Body);
    }

    [TestMethod]
    public void XmlDocument_Constructor_RoundTrips()
    {
        var doc = new XmlDocument();
        doc.LoadXml(Soap11Normal);
        var env = new Envelope(doc);
        Assert.AreSame(doc, env.XmlDocument);
    }
}
