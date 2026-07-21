using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SaaS.BLL;
using SaaS.Core.Settings;
using SaaS.DAL;
using WebApplication1.Middleware;
using WebApplication1.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Configuration-bound settings
// ---------------------------------------------------------------------------
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Missing 'Jwt' configuration section.");

// ---------------------------------------------------------------------------
// Layers (DAL + BLL). Each layer owns its own DI registrations.
// ---------------------------------------------------------------------------
builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddBusinessLogic(builder.Configuration);

// ---------------------------------------------------------------------------
// AuthN / AuthZ — Bearer tokens signed with HMAC-SHA512
// ---------------------------------------------------------------------------
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha512 },
        };
    });

builder.Services.AddAuthorization();
const string FrontendCorsPolicy = "FrontendCors";
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();
var allowAnyOrigin = corsOrigins.Length == 0 || corsOrigins.Contains("*");

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        if (allowAnyOrigin)
        {
                        policy.AllowAnyOrigin();
 // GET, POST, OPTIONS (preflight), etc.
        }
            else
        {
            policy.WithOrigins(corsOrigins);
        }
    });
});
builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

// ---------------------------------------------------------------------------
// HTTP pipeline
// ---------------------------------------------------------------------------
// OpenAPI document (/openapi/v1.json) + Swagger UI (/swagger) in ALL environments.
// NOTE: this exposes your full API surface publicly in production. If you'd rather
// keep it internal, wrap this block in `if (app.Environment.IsDevelopment())`
// again, or put it behind auth / an allow-list.
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "WorkProvider360 API v1");
    options.RoutePrefix = "swagger";
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();

// Tenant resolution runs AFTER authentication so the agency_id claim is
// available, and BEFORE authorization/controllers so repositories have a
// resolved connection string.
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
