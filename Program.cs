using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SaaS.BLL;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Settings;
using SaaS.DAL;
using WebApplication1.Hubs;
using WebApplication1.Infrastructure;
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

        // SignalR sends the bearer token as an "access_token" query-string value
        // on the WebSocket/SSE request (headers aren't available there).
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ILocationBroadcaster, LocationBroadcaster>();

// ---------------------------------------------------------------------------
// CORS — allow the deployed frontend origin(s) to call the API from a browser.
// Origins are read from configuration ("Cors:AllowedOrigins") so they can be
// changed per-environment without recompiling.
// ---------------------------------------------------------------------------
const string FrontendCorsPolicy = "FrontendCors";
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();
var allowAnyOrigin = corsOrigins.Length == 0 || corsOrigins.Contains("*");
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        // Safe here because auth uses a Bearer token (not cookies), so we never
        // combine AllowAnyOrigin with AllowCredentials.
        if (allowAnyOrigin)
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            policy.WithOrigins(corsOrigins);
        }

        policy
            .AllowAnyHeader()   // Authorization, Content-Type, X-Tenant-Domain
            .AllowAnyMethod();  // GET, POST, OPTIONS (preflight), etc.
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

// Must run before authentication so preflight (OPTIONS) requests are answered
// with the CORS headers instead of hitting the auth/endpoint pipeline.
app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();

// Tenant resolution runs AFTER authentication so the agency_id claim is
// available, and BEFORE authorization/controllers so repositories have a
// resolved connection string.
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthorization();

// Security monitoring: runs after tenant resolution + authorization so it can
// see the real client IP, the resolved tenant, and the final 401/403 status.
app.UseMiddleware<SecurityAuditMiddleware>();

app.MapControllers();
app.MapHub<LocationHub>("/hubs/location");

app.Run();
