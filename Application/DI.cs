using Application;
using Application.Behaviors;
using Application.Features.Account.Services;
using Application.Settings;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DI
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Add application services here
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<ShippingSettings>(configuration.GetSection(ShippingSettings.SectionName));

        services.AddStackExchangeRedisCache(options => {
            options.Configuration = configuration.GetConnectionString("Redis");
        });
        services.AddHybridCache();

        services.AddAutoMapper(config => config.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));
        services.AddMediatR(config => {
            config.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly);

            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(CachingBehavior<,>));
            config.AddOpenBehavior(typeof(CacheInvalidationBehavior<,>));
            config.AddOpenBehavior(typeof(CacheInvalidationNonGenericBehavior<,>));
        });
        services.AddValidatorsFromAssembly(typeof(AssemblyReference).Assembly);
        services.AddScoped<AccountTokenService>();

        return services;
    }
}

