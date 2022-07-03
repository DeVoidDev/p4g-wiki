using P4GWiki.Extensions;
using P4GWiki.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(option => option.AddServerHeader = false);
builder.AddUnixSocket();
builder.Services.AddRazorPages();
var app = builder.Build();
app.UseMiddleware<ForwardedHeaderMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.MapControllers();
app.Run();
