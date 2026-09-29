using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Constants;
using Application.Settings;
using Azure.Storage.Blobs;
using Domain.Entities.Identity;
using Hangfire;
using Infrastructure.Abstractions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Services.Storage;
using Infrastructure.Settings;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public static class DI
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<AzureStorageSettings>(configuration.GetSection(AzureStorageSettings.SectionName));

        // Add infrastructure services here
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("The DefaultConnection connection string is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddIdentity<AppUser, AppRole>(options => {
            options.User.RequireUniqueEmail = true;

            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? throw new InvalidOperationException("JWT settings are required.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters {
                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = jwt.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                ValidateLifetime = true,
                NameClaimType = JwtRegisteredClaimNames.Sub,
                RoleClaimType = System.Security.Claims.ClaimTypes.Role
            });

        services.AddHangfire(options => {
            options.UseSqlServerStorage(connectionString);
        });

        services.AddHangfireServer(options => {
            options.Queues =
            [
                BackgroundJobQueuesPriority.Critical,
                BackgroundJobQueuesPriority.Default,
                BackgroundJobQueuesPriority.Low
            ];
        });

        services.AddSingleton(sp => {
            var settings = sp.GetRequiredService<IOptions<AzureStorageSettings>>().Value;
            return new BlobContainerClient(new Uri(settings.ConnectionString));
        });

        services.AddScoped<IStorageService, AzureBlobStorageService>();

        services.AddSingleton<IEmailTemplateRenderer, RazorLightRenderer>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddTransient<IEmailSender, MailKitEmailSender>();
        services.AddTransient<ISmtpClient, SmtpClient>();
        services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();
        services.AddScoped<IImageManipulationService, SixLaborsImageService>();

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
