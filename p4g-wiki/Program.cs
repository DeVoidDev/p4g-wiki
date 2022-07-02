using P4GWiki.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(option => option.AddServerHeader = false);
builder.AddUnixSocket();
builder.Services.AddRazorPages();
Console.WriteLine(builder.Services);
var app = builder.Build();
app.MapGet("/", () => "Hello World!");

app.Run();
