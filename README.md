# SergeiM.Soap

SOAP client library built on top of
[`SergeiM.Http`](https://github.com/sergeim/sergeim-http).
Follows the same immutable-fluent API style as `BaseRequest` / `BaseResponse`.

## Installation

```bash
dotnet add package SergeiM.Soap
```

Requires `net8.0` or later.

## Quick start

```csharp
using SergeiM.Soap;

var response = new SoapRequest("https://example.com/service.asmx")
    .SoapAction("http://example.com/GetUser")
    .Envelope()
        .WithNamespace("svc", "http://example.com/")
        .WithBody("<svc:GetUser><svc:Id>42</svc:Id></svc:GetUser>")
        .Back()
    .Fetch();

response.AssertStatus(200).AssertNoFault();

var ns = new XmlNamespaceManager(new NameTable());
ns.AddNamespace("svc", "http://example.com/");
var name = response.Envelope.EvaluateXPath("//svc:Name", ns);
```

## SOAP 1.1 (default)

```csharp
var response = new SoapRequest("http://www.dneonline.com/calculator.asmx")
    .SoapAction("http://tempuri.org/Add")
    .Envelope()
        .WithNamespace("t", "http://tempuri.org/")
        .WithBody("<t:Add><t:intA>7</t:intA><t:intB>3</t:intB></t:Add>")
        .Back()
    .Fetch();

response.AssertStatus(200).AssertNoFault();
// result == "10"
var result = response.Envelope.EvaluateXPath("//t:AddResult", ns);
```

## SOAP 1.2

Pass `SoapVersion.Soap12` and an `IWire`:

```csharp
var response = new SoapRequest("http://www.dneonline.com/calculator.asmx",
        new HttpWire(), SoapVersion.Soap12)
    .SoapAction("http://tempuri.org/Add")
    .Envelope()
        .WithNamespace("t", "http://tempuri.org/")
        .WithBody("<t:Add><t:intA>7</t:intA><t:intB>3</t:intB></t:Add>")
        .Back()
    .Fetch();
```

Content-Type is set to `application/soap+xml` with the `action` parameter
appended automatically.

## Bring your own request

Supply a pre-configured `IRequest` (auth headers, custom wire, retries, etc.):

```csharp
IRequest inner = new BaseRequest("https://secure.example.com/soap", myWire)
    .Method("POST")
    .Header("Authorization", "Bearer " + token);

var response = new SoapRequest(inner)
    .Envelope()
        .WithBody("<svc:Ping/>")
        .Back()
    .Fetch();
```

## Fault handling

```csharp
try
{
    response.AssertNoFault();
}
catch (SoapFaultException ex)
{
    Console.WriteLine(ex.Fault.Code);    // e.g. "env:Sender"
    Console.WriteLine(ex.Fault.Reason);
}
```

Or use the fluent form that throws on non-200 or a SOAP fault:

```csharp
response.AssertStatus(200).AssertNoFault();
```
