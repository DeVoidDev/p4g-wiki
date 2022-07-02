namespace P4GWiki.Extensions;

public static class UnixSocketExtensions
{
    public static void AddUnixSocket(this WebApplicationBuilder builder)
    {
        string unixSocket;
        if ((unixSocket = builder.Configuration["Kestrel:UnixSocket"]) == null) return;
        builder.WebHost.ConfigureKestrel(options => options.ListenUnixSocket(unixSocket));
    }
}
