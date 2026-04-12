using AccessiTrack.Application.Common.Interfaces;
using AccessiTrack.Domain.Interfaces;
using AccessiTrack.Infrastructure.Auditing;
using AccessiTrack.Infrastructure.Identity;
using AccessiTrack.Infrastructure.Persistence;
using AccessiTrack.Infrastructure.Persistence.Repositories;
using AccessiTrack.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AccessiTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(opts =>
        {
            opts.Password.RequiredLength = 6;
            opts.Password.RequireDigit = true;
            opts.Password.RequireUppercase = true;
            opts.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // JWT Authentication
        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is missing in configuration.");

        services.AddAuthentication(opts =>
        {
            opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(opts =>
        {
            opts.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Audit pipeline
        services.AddSingleton<IAuditQueue, AuditQueue>();       // singleton — shared channel
        services.AddScoped<IAuditRunner, PlaywrightAuditRunner>(); // scoped — holds DbContext
        services.AddHostedService<AuditBackgroundService>();

        // Repositories
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IViolationRepository, ViolationRepository>();

        // Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        //services.AddScoped<IAccessibilityScanner, AccessibilityScanner>();
        services.AddHttpClient<IAccessibilityScanner, AccessibilityScanner>();

        //    services.AddHttpClient<AccessibilityScanner>()
        //.ConfigureHttpClient(client =>
        //{
        //    client.Timeout = TimeSpan.FromSeconds(30);
        //    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        //        "AccessiTrack-Scanner/1.0 (+https://github.com/your-org/accessitrack)");
        //});

        //    services.AddScoped<IAccessibilityScanner, AccessibilityScanner>();

        return services;
    }
}
