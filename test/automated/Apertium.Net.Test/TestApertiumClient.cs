using NUnit.Framework;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Apertium.Net.Test;

[TestFixture]
public class TestApertiumClient
{
    private Mock<HttpMessageHandler> _mockHttpMessageHandler = null!;
    private const string BaseUrl = "http://fakeapertium.org/apy/";
    private const string ApiKey = "testkey";

    [SetUp]
    public void Setup()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
    }

    private ApertiumClient CreateClient(
        string? apiKey = null,
        bool autoLoad = false,
        string defaultFrom = "eng",
        string defaultTo = "spa",
        bool validatePair = false,
        string? baseUrl = null)
    {
        return new ApertiumClient(
            baseUrl ?? BaseUrl,
            apiKey ?? string.Empty,
            autoLoad,
            defaultFrom,
            defaultTo,
            validatePair);
    }

    private void SetupHttpMock(string expectedUrlContains, string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains(expectedUrlContains)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            })
            .Verifiable();
    }
    
    private HttpClient GetMockHttpClient(string expectedUrlContains, string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        Mock<HttpMessageHandler> mockHandler = new();
        
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains(expectedUrlContains)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            })
            .Verifiable();
        
        return new HttpClient(mockHandler.Object);
    }

    [Test]
    public async Task GetValidLanguagePairsAsync_Success()
    {
        var jsonResponse = @"{""responseData"": [{""sourceLanguage"": ""eng"", ""targetLanguage"": ""spa""}, {""sourceLanguage"": ""fra"", ""targetLanguage"": ""eng""}]}";
        SetupHttpMock("listPairs", jsonResponse);

        var client = CreateClient();
        var mockHttpClient = GetMockHttpClient("listPairs", jsonResponse);
        client.SetHttpClient(mockHttpClient);
        
        var pairs = await client.GetValidLanguagePairsAsync();

        Assert.That(pairs.Count, Is.EqualTo(2));
        Assert.That(pairs, Does.Contain(("eng", "spa")));
        Assert.That(pairs, Does.Contain(("fra", "eng")));
    }
    
    [Test]
    public void Constructor_ValidateDefaultLanguagePair_Failure_ThrowsException()
    {
        var jsonResponse = @"{""responseData"": [{""sourceLanguage"": ""eng"", ""targetLanguage"": ""spa""}]}";
        SetupHttpMock("listPairs", jsonResponse);

        Assert.Throws<AggregateException>(() => CreateClient(autoLoad: false, defaultFrom: "fra", defaultTo: "deu", validatePair: true));
    }
}
