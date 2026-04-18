using System.Net;

namespace ParqueoCarga.Services;

public sealed class ApiServiceException : Exception
{
    public ApiServiceException(string message, HttpStatusCode statusCode, IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }

    public HttpStatusCode StatusCode { get; }

    public IReadOnlyDictionary<string, string[]>? Errors { get; }
}