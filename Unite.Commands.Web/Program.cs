using Unite.Commands.Web.Configuration.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddServices();
builder.Services.AddControllers();

builder.WebHost.ConfigureKestrel(options => 
{
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(61);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(60);
});

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();
