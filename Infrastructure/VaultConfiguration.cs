/* =============================================================================
   Secret Vault configuration provider.
   Loads secret values from the self-hosted Secret Vault at startup and injects
   them into IConfiguration, keyed by the secret's Name. Name each vault secret
   exactly like the config key it replaces (e.g. "Stripe:SecretKey",
   "MasterDb:ConnectionString") and the rest of the app reads it unchanged.

   appsettings.json (the only thing that stays local):
     "Vault": {
       "BaseUrl": "https://sqlaccess.runasp.net",
       "ClientId": "app_xxx",
       "ClientSecret": "sk_xxx",
       "Optional": true,                     // if vault unreachable, fall back to appsettings
       "Keys": [                             // secret names to load (used if the vault has no
         "MasterDb:ConnectionString",        // bulk /my-secrets endpoint yet)
         "Jwt:SigningKey",
         "Smtp:UserName", "Smtp:Password",
         "Stripe:PublishableKey", "Stripe:SecretKey",
         "Twilio:AccountSid", "Twilio:AuthToken",
         "Cloudinary:ApiKey", "Cloudinary:ApiSecret",
         "Llm:ApiKey"
       ]
     }
   ============================================================================= */

using System.Net.Http.Json;
using System.Text.Json;

namespace WebApplication1.Infrastructure;

public static class VaultConfigurationExtensions
{
    public static IConfigurationBuilder AddVault(this IConfigurationBuilder builder, IConfiguration bootstrap)
    {
        var baseUrl = bootstrap["Vault:BaseUrl"];
        var clientId = bootstrap["Vault:ClientId"];
        var clientSecret = bootstrap["Vault:ClientSecret"];
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            return builder; // vault not configured — keep local appsettings values

        var optional = !bool.TryParse(bootstrap["Vault:Optional"], out var o) || o;
        var keys = bootstrap.GetSection("Vault:Keys").Get<string[]>() ?? Array.Empty<string>();
        builder.Add(new VaultConfigurationSource(baseUrl!, clientId!, clientSecret!, keys, optional));
        return builder;
    }
}

public sealed class VaultConfigurationSource(string baseUrl, string clientId, string clientSecret, string[] keys, bool optional)
    : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new VaultConfigurationProvider(baseUrl, clientId, clientSecret, keys, optional);
}

public sealed class VaultConfigurationProvider(string baseUrl, string clientId, string clientSecret, string[] keys, bool optional)
    : ConfigurationProvider
{
    public override void Load()
    {
        try
        {
            using var http = new HttpClient
            {
                BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromSeconds(25),
            };

            // 1. Authenticate (ClientId + ClientSecret) -> scoped token.
            using var loginResp = http.PostAsJsonAsync("api/vault/login",
                new { clientId, clientSecret }).GetAwaiter().GetResult();
            loginResp.EnsureSuccessStatusCode();
            var loginJson = loginResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var token = JsonDocument.Parse(loginJson).RootElement.GetProperty("token").GetString();

            var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            // 2a. Preferred: one bulk call for every authorized secret.
            var bulk = TryLoadBulk(http, token!);
            if (bulk is not null)
            {
                data = bulk;
            }
            else
            {
                // 2b. Fallback (older vault without /my-secrets): fetch each configured key by name.
                foreach (var key in keys)
                {
                    var name = Uri.EscapeDataString(key); // handles the ':' in names
                    using var req = new HttpRequestMessage(HttpMethod.Get, "api/vault/secrets/" + name);
                    req.Headers.Authorization = new("Bearer", token);
                    using var resp = http.Send(req);
                    if (!resp.IsSuccessStatusCode) continue;
                    var json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    data[key] = JsonDocument.Parse(json).RootElement.GetProperty("value").GetString();
                }
            }

            if (data.Count == 0 && !optional)
                throw new InvalidOperationException("Vault returned no secrets. Check the app's assignments and Vault:Keys.");

            Data = data;
        }
        catch (Exception ex)
        {
            if (!optional)
                throw new InvalidOperationException("Failed to load secrets from the vault: " + ex.Message, ex);
            Console.WriteLine("[Vault] Could not load secrets, using local appsettings. " + ex.Message);
        }
    }

    private static Dictionary<string, string?>? TryLoadBulk(HttpClient http, string token)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, "api/vault/my-secrets");
            req.Headers.Authorization = new("Bearer", token);
            using var resp = http.Send(req);
            if (!resp.IsSuccessStatusCode) return null; // e.g. 404 on an older vault -> use per-name fallback
            var json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in JsonDocument.Parse(json).RootElement.EnumerateArray())
            {
                var name = s.GetProperty("name").GetString();
                if (!string.IsNullOrEmpty(name))
                    data[name] = s.GetProperty("value").GetString();
            }
            return data;
        }
        catch
        {
            return null;
        }
    }
}
