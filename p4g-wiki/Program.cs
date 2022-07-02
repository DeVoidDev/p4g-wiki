using P4GWiki.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(option => option.AddServerHeader = false);
builder.AddUnixSocket();
builder.Services.AddRazorPages();
var app = builder.Build();
app.MapControllers();
app.Run();
