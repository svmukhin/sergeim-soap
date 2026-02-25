// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

namespace SergeiM.Soap.Tests;

[TestClass]
public sealed class SoapFaultTests
{
    [TestMethod]
    public void SoapFault_Constructor_SetsAllProperties()
    {
        var fault = new SoapFault("env:Server", "Internal error", "Stack trace here", "http://example.com/actor");
        Assert.AreEqual("env:Server", fault.Code);
        Assert.AreEqual("Internal error", fault.Reason);
        Assert.AreEqual("Stack trace here", fault.Detail);
        Assert.AreEqual("http://example.com/actor", fault.Actor);
    }

    [TestMethod]
    public void SoapFault_NullableProperties_AcceptNull()
    {
        var fault = new SoapFault("env:Client", "Bad request", null, null);
        Assert.IsNull(fault.Detail);
        Assert.IsNull(fault.Actor);
    }

    [TestMethod]
    public void SoapFaultException_Message_ContainsCodeAndReason()
    {
        var ex = new SoapFaultException(new SoapFault("env:Server", "Internal error", null, null));
        StringAssert.Contains(ex.Message, "env:Server");
        StringAssert.Contains(ex.Message, "Internal error");
    }

    [TestMethod]
    public void SoapFaultException_Fault_IsPreserved()
    {
        var fault = new SoapFault("env:Server", "Internal error", null, null);
        var ex = new SoapFaultException(fault);
        Assert.AreSame(fault, ex.Fault);
    }

    [TestMethod]
    public void SoapFaultException_IsException()
    {
        var ex = new SoapFaultException(new SoapFault("env:Server", "Internal error", null, null));
        Assert.IsInstanceOfType<Exception>(ex);
    }
}
