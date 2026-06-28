using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zyfy.Tests.Helpers;

/// <summary>
/// A test double for <see cref="HttpMessageHandler"/> that returns pre-configured responses
/// in sequence and captures all outgoing requests.
/// </summary>
internal sealed class MockHttpHandler : HttpMessageHandler
{
    private readonly Queue<MockResponse> _responses = new Queue<MockResponse>();
    private readonly List<CapturedRequest> _requests = new List<CapturedRequest>();

    /// <summary>All requests that have been sent through this handler, in order.</summary>
    public IReadOnlyList<CapturedRequest> Requests => _requests;

    /// <summary>Enqueue a response to be returned on the next request.</summary>
    public void Enqueue(HttpStatusCode statusCode, string body, Dictionary<string, string>? headers = null)
    {
        _responses.Enqueue(new MockResponse(statusCode, body, headers ?? new Dictionary<string, string>()));
    }

    /// <summary>Enqueue a successful 200 response with the given JSON body and default quota headers.</summary>
    public void EnqueueOk(string body, Dictionary<string, string>? extraHeaders = null)
    {
        var headers = new Dictionary<string, string>
        {
            ["X-Quota-Limit"] = "1000",
            ["X-Quota-Used"] = "42",
            ["X-Quota-Remaining"] = "958",
            ["X-Quota-Grace-Limit"] = "1100",
            ["X-Quota-Resets"] = "2026-07-01T00:00:00Z",
        };

        if (extraHeaders is not null)
        {
            foreach (var kvp in extraHeaders)
            {
                headers[kvp.Key] = kvp.Value;
            }
        }

        _responses.Enqueue(new MockResponse(HttpStatusCode.OK, body, headers));
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var requestBody = string.Empty;
        if (request.Content is not null)
        {
            requestBody = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        _requests.Add(new CapturedRequest(request.Method.Method, request.RequestUri?.AbsoluteUri ?? string.Empty, request.Headers, requestBody));

        if (_responses.Count == 0)
        {
            throw new InvalidOperationException("MockHttpHandler has no more responses queued.");
        }

        var mock = _responses.Dequeue();
        var response = new HttpResponseMessage(mock.StatusCode)
        {
            Content = new StringContent(mock.Body, Encoding.UTF8, "application/json"),
        };

        foreach (var kvp in mock.Headers)
        {
            response.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
        }

        return response;
    }

    private sealed record MockResponse(HttpStatusCode StatusCode, string Body, Dictionary<string, string> Headers);
}

/// <summary>A request captured by <see cref="MockHttpHandler"/>.</summary>
internal sealed record CapturedRequest(
    string Method,
    string Url,
    System.Net.Http.Headers.HttpRequestHeaders Headers,
    string Body);
