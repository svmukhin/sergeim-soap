// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

namespace SergeiM.Soap.Tests;

/// <summary>
/// Real-network smoke tests against public SOAP endpoints.
/// Skipped in CI — run manually with:
///   dotnet test --filter "TestCategory=Integration"
/// </summary>
[TestClass]
public sealed class IntegrationTests
{
    // ── DNE Calculator ────────────────────────────────────────────────────────
    // WSDL: http://www.dneonline.com/calculator.asmx?wsdl
    private const string DneEndpoint = "http://www.dneonline.com/calculator.asmx";
    private const string DneNs = "http://tempuri.org/";

    [TestMethod]
    [TestCategory("Integration")]
    public void Dne_Add_Soap11_ReturnsCorrectSum()
    {
        var response = new SoapRequest(DneEndpoint)
            .SoapAction($"{DneNs}Add")
            .Envelope()
                .WithNamespace("t", DneNs)
                .WithBody("<t:Add><t:intA>7</t:intA><t:intB>3</t:intB></t:Add>")
                .Back()
            .Fetch();
        response.AssertStatus(200).AssertNoFault();
        var result = response.Envelope.EvaluateXPath("//t:AddResult", BuildNs("t", DneNs));
        Assert.AreEqual("10", result);
    }

    [TestMethod]
    [TestCategory("Integration")]
    public void Dne_Add_Soap12_ReturnsCorrectSum()
    {
        var response = new SoapRequest(DneEndpoint, new SergeiM.Http.Wire.HttpWire(), SoapVersion.Soap12)
            .SoapAction($"{DneNs}Add")
            .Envelope()
                .WithNamespace("t", DneNs)
                .WithBody("<t:Add><t:intA>7</t:intA><t:intB>3</t:intB></t:Add>")
                .Back()
            .Fetch();
        response.AssertStatus(200).AssertNoFault();
        var result = response.Envelope.EvaluateXPath("//t:AddResult", BuildNs("t", DneNs));
        Assert.AreEqual("10", result);
    }

    // ── DataAccess NumberConversion ──────────────────────────────────────────
    // WSDL: https://www.dataaccess.com/webservicesserver/NumberConversion.wso?WSDL
    private const string DaEndpoint = "https://www.dataaccess.com/webservicesserver/NumberConversion.wso";
    private const string DaNs = "http://www.dataaccess.com/webservicesserver/";

    [TestMethod]
    [TestCategory("Integration")]
    public void DataAccess_NumberToWords_ReturnsEnglishText()
    {
        var response = new SoapRequest(DaEndpoint)
            .SoapAction(string.Empty)
            .Envelope()
                .WithNamespace("d", DaNs)
                .WithBody("<d:NumberToWords><d:ubiNum>42</d:ubiNum></d:NumberToWords>")
                .Back()
            .Fetch();
        response.AssertStatus(200).AssertNoFault();
        var result = response.Envelope.EvaluateXPath("//d:NumberToWordsResult", BuildNs("d", DaNs));
        StringAssert.Contains(result.ToLowerInvariant(), "forty");
    }

    private static System.Xml.XmlNamespaceManager BuildNs(string prefix, string uri)
    {
        var ns = new System.Xml.XmlNamespaceManager(new System.Xml.NameTable());
        ns.AddNamespace(prefix, uri);
        return ns;
    }
}
