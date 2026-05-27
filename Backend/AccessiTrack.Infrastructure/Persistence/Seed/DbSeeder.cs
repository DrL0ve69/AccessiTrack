using AccessiTrack.Domain.Constants;
using AccessiTrack.Domain.Entities;
using AccessiTrack.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AccessiTrack.Infrastructure.Persistence.Seed;

/// <summary>
/// Database seeder for initial data and development seeding.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Seeds initial data into the database.
    /// </summary>
    public static async Task SeedAsync(
        ApplicationDbContext context,
        IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Seed Roles
        string[] roles = [Roles.Admin, Roles.Member];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole(role));
        }

        // Seed Admin User
        const string adminEmail = "admin@accessitrack.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                DisplayName = "Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@123!");
            // await userManager.AddToRoleAsync(admin, Roles.Admin);
            // await context.SaveChangesAsync();

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, Roles.Admin);

                // --- AJOUT ICI : Création du profil associé ---
                if (!context.UserProfiles.Any(p => p.IdentityId == admin.Id.ToString()))
                {
                    context.UserProfiles.Add(new UserProfile
                    {
                        Id = Guid.NewGuid(),
                        IdentityId = admin.Id.ToString(), // Le lien crucial
                        FullName = "Administrateur Système",
                        Email = adminEmail,
                        PreferredLanguage = "fr-CA",
                        CreatedAt = DateTime.UtcNow
                    });
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
