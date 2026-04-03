using AccessiTrack.Application;
using AccessiTrack.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ====== Couches Clean Architecture ======
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

// ====== CORS pour Angular ======
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins(
                "http://localhost:4200",           // Développement local
                "https://ton-app.vercel.app")      // Production Vercel
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAngular");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
