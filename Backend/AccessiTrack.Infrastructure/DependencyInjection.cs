using System;
using System.Collections.Generic;
using System.Text;
using AccessiTrack.Domain.Interfaces;
using AccessiTrack.Infrastructure.Persistence;
using AccessiTrack.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccessiTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(
                    typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IViolationRepository, ViolationRepository>();

        return services;
    }
}
