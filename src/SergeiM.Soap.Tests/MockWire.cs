// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using SergeiM.Http;

namespace SergeiM.Soap.Tests;

/// <summary>
/// Test double for <see cref="IWire"/> that returns a pre-configured response
/// and records the last call's arguments for assertion.
/// <example>
/// var wire = new MockWire(200, soapXml);
/// var request = new SoapRequest("https://example.com/soap", wire);
/// request.Fetch();
/// Assert.AreEqual("POST", wire.LastMethod);
/// </example>
/// </summary>
internal sealed class MockWire : IWire
{
    private readonly HttpResponseMessage _response;

    /// <summary>The HTTP method received on the most recent call.</summary>
    public string? LastMethod { get; private set; }

    /// <summary>The URI received on the most recent call.</summary>
    public string? LastUri { get; private set; }

    /// <summary>The headers received on the most recent call.</summary>
    public Dictionary<string, string>? LastHeaders { get; private set; }

    /// <summary>The body received on the most recent call. May be <c>null</c> for body-less requests.</summary>
    public string? LastBody { get; private set; }

    /// <summary>Creates a <see cref="MockWire"/> that returns the specified status code, body, and content type.</summary>
    public MockWire(int statusCode, string body, string contentType = "text/xml")
    {
        _response = new HttpResponseMessage((System.Net.HttpStatusCode)statusCode)
        {
            Content = new System.Net.Http.StringContent(body, System.Text.Encoding.UTF8, contentType)
        };
    }

    /// <inheritdoc/>
    public Task<HttpResponseMessage> SendAsync(
        string method, string uri,
        Dictionary<string, string> headers, string? body)
    {
        LastMethod = method;
        LastUri = uri;
        LastHeaders = headers;
        LastBody = body;
        return Task.FromResult(_response);
    }

    /// <inheritdoc/>
    public HttpResponseMessage Send(
        string method, string uri,
        Dictionary<string, string> headers, string? body)
        => SendAsync(method, uri, headers, body).GetAwaiter().GetResult();
}
