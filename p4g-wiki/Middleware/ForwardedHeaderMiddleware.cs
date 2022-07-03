namespace P4GWiki.Middleware;

public class ForwardedHeaderMiddleware
{
    private readonly ILogger<ForwardedHeaderMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ForwardedHeaderMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<ForwardedHeaderMiddleware>();
    }

    public Task InvokeAsync(HttpContext context)
    {
        _logger.LogWarning("Got forwarded header: {}", context.Request.Headers["Forwarded"]);
        return _next(context);
    }
}
