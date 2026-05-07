// Program.cs — Blog API (complete, all phases)

using System.Text;
using BlogApi.Data;
using BlogApi.Mappings;
using BlogApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ══════════════════════════════════════════════════════════
// SERVICES
// ══════════════════════════════════════════════════════════

// ── Controllers ───────────────────────────────────────────
builder.Services.AddControllers();

// ── Database (Phase 2) ────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ── AutoMapper (Phase 3) ──────────────────────────────────
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ── Auth Service (Phase 5) ────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();

// ── JWT Authentication (Phase 5) ──────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"]!;

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            // Prevent clock skew adding extra expiry buffer
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

// ── Swagger (Phase 6) ─────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Blog API",
            Version = "v1",
            Description = "A blog platform API built with .NET 8",
        }
    );

    // Tell Swagger UI how to accept a JWT token
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token (without 'Bearer' prefix)",
        }
    );

    // Apply the JWT requirement to all endpoints globally
    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );
});

// ══════════════════════════════════════════════════════════
// MIDDLEWARE PIPELINE
// ══════════════════════════════════════════════════════════

var app = builder.Build();

// ── Global error handler — must be FIRST ──────────────────
// Catches all unhandled exceptions from everything below it
app.UseMiddleware<ExceptionMiddleware>();

// ── Swagger UI — development only ─────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blog API v1");
        c.RoutePrefix = "swagger"; // available at /swagger
    });
}

// ── HTTPS redirect ────────────────────────────────────────
app.UseHttpsRedirection();

// ── Auth — order is critical ──────────────────────────────
app.UseAuthentication(); // reads & validates the JWT token
app.UseAuthorization(); // enforces [Authorize] attributes

// ── Map controller routes ─────────────────────────────────
app.MapControllers();

app.Run();
