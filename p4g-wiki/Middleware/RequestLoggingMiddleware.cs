using System.Text;

namespace P4GWiki.Middleware;

public class RequestLoggingMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<RequestLoggingMiddleware>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("Received request:\n");
        stringBuilder.Append("Headers:\n");
        foreach (var (name, values) in context.Request.Headers)
        foreach (var value in values)
            stringBuilder.Append($"{name}: {value}\n");

        stringBuilder.Append("Body:\n");
        stringBuilder.Append(await new StreamReader(context.Request.Body).ReadToEndAsync());
        _logger.LogInformation("{}", stringBuilder.ToString());
        await _next(context);
    }
}
