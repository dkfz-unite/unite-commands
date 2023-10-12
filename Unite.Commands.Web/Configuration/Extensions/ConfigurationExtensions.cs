using Unite.Commands.Web.Configuration.Options;

namespace Unite.Commands.Web.Configuration.Extensions;

public static class ConfigurationExtensions
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddTransient<CommandOptions>();
    }
}
