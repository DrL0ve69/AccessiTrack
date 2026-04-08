using AccessiTrack.Application;
using AccessiTrack.Infrastructure;
using AccessiTrack.Infrastructure.Persistence;
using AccessiTrack.Infrastructure.Persistence.Seed;
using AccessiTrack.API.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ====== Clean Architecture Layers ======
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ====== API ======
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "AccessiTrack API",
        Version = "v1",
        Description = "API de suivi d'audits d'accessibilité WCAG"
    });
});

// ====== CORS for Angular ======
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins(
                "http://localhost:4200",
                "https://accessi-track.vercel.app")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

var app = builder.Build();

// ====== Middleware Pipeline ======
// Global exception handler (must be first)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// ====== Auto-migrate and seed on startup ======
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, scope.ServiceProvider);
}

app.UseCors("AllowAngular");
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
