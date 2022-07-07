using System.Net;
using System.Text;

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
        if (context.Request.Headers["Forwarded"].Count == 0) return _next(context);
        var forwardedElements = Split(',', context.Request.Headers["Forwarded"][0]);
        if (forwardedElements.Length == 0) return _next(context);
        var forwardedParts = Split(';', forwardedElements[0]);
        foreach (var forwardedPart in forwardedParts)
        {
            string key;
            string val;
            {
                var forwardedPartSeperated = Split('=', forwardedPart);
                if (forwardedPartSeperated.Length != 2)
                {
                    _logger.LogWarning("Malformed Forwarded Parts: {}", forwardedPart);
                    continue;
                }

                key = forwardedPartSeperated[0].Trim().ToLower();
                val = forwardedPartSeperated[1].Trim().ToLower();
            }


            if (val[0] == '"' && val[^1] == '"') val = val.Substring(1, val.Length - 2).Trim();

            switch (key)
            {
                case "proto":
                    HandleProto(context, val);
                    break;
                case "for":
                    HandleFor(context, val);
                    break;
                case "host":
                    HandleHost(context, val);
                    break;
                case "by":
                    HandleBy(context, val);
                    break;
            }
        }

        context.Request.Headers.Remove("Forwarded");
        return _next(context);
    }

    private void HandleProto(HttpContext context, string arg)
    {
        context.Request.IsHttps = arg switch
        {
            "https" => true,
            "http" => false,
            _ => context.Request.IsHttps
        };
        _logger.LogInformation("Detected request using {} upstream", context.Request.IsHttps ? "https" : "http");
    }

    private void HandleFor(HttpContext context, string arg)
    {
        var (ip, port) = GetHost(arg);
        context.Connection.RemoteIpAddress = ip;
        if (port.HasValue) context.Connection.RemotePort = port.Value;
        _logger.LogInformation("Detected request using ip: {} and port: {}",
            context.Connection.RemoteIpAddress.ToString(), context.Connection.RemotePort);
    }

    private void HandleHost(HttpContext context, string arg)
    {
        context.Request.Headers.Host = arg;
        _logger.LogInformation("Detected request using host: {}", context.Request.Headers.Host);
    }

    private void HandleBy(HttpContext context, string arg)
    {
        context.Request.Headers.UserAgent = arg;
        _logger.LogInformation("Detected request using UserAgent: {}", context.Request.Headers.UserAgent);
    }

    private static (IPAddress ip, int? port) GetHost(string src)
    {
        var (ip, portIndex) = src.Contains('[') ? HandleIp6(src) : HandleIp4(src);
        return (IPAddress.Parse(ip),
            portIndex.HasValue ? int.Parse(src.Substring(portIndex.Value, src.Length - portIndex.Value)) : null);
    }

    private static (string ip, int? portIndex) HandleIp4(string src)
    {
        var ipBuilder = new StringBuilder();
        for (var i = 0; i < src.Length; i++)
        {
            if (src[i] == ':')
            {
                if (ipBuilder.Length == 0) throw new ArgumentException("Host is invalid", nameof(src));
                return (ipBuilder.ToString(), ++i);
            }

            ipBuilder.Append(src[i]);
        }

        if (ipBuilder.Length == 0) throw new ArgumentException("Host is invalid", nameof(src));
        return (ipBuilder.ToString(), null);
    }

    private static (string ip, int? portIndex) HandleIp6(string src)
    {
        var ipBuilder = new StringBuilder();
        var isIpOpen = false;
        for (var i = 0; i < src.Length; i++)
        {
            if (src[i] == '[')
            {
                isIpOpen = true;
            }
            else if (src[i] == ']')
            {
                isIpOpen = false;
            }
            else if (src[i] == ':' && !isIpOpen)
            {
                if (ipBuilder.Length == 0) throw new ArgumentException("Host is invalid", nameof(src));
                return (ipBuilder.ToString(), ++i);
            }

            ipBuilder.Append(src[i]);
        }

        if (ipBuilder.Length == 0 || isIpOpen) throw new ArgumentException("Host is invalid", nameof(src));
        return (ipBuilder.ToString(), null);
    }


    private static string[] Split(char delimiter, string source)
    {
        var forwardedElementBuilders = new List<StringBuilder>();
        var builder = new StringBuilder();
        var isLiteral = false;
        foreach (var ch in source.ToCharArray())
        {
            if (ch == '"')
            {
                isLiteral = !isLiteral;
            }
            else if (ch == delimiter && !isLiteral)
            {
                forwardedElementBuilders.Add(builder);
                builder = new StringBuilder();
                continue;
            }

            builder.Append(ch);
        }

        forwardedElementBuilders.Add(builder);
        return forwardedElementBuilders.Select(b => b.ToString()).ToArray();
    }
}
