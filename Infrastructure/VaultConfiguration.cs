/* =============================================================================
   Secret Vault configuration provider.
   Pulls secret values from the self-hosted Secret Vault at startup and injects
   them into IConfiguration, keyed by the secret's Name. Name each vault secret
   exactly like the config key it replaces (e.g. "Stripe:SecretKey",
   "MasterDb:ConnectionString") and the rest of the app reads it unchanged.

   Bootstrap config (appsettings.json / env), the only thing left locally:
     "Vault": {
       "BaseUrl": "https://sqlaccess.runasp.net",   // where the vault API lives
       "ClientId": "app_xxx",
       "ClientSecret": "sk_xxx",
       "Optional": true          // if the vault is unreachable, fall back to appsettings
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
        builder.Add(new VaultConfigurationSource(baseUrl!, clientId!, clientSecret!, optional));
        return builder;
    }
}

public sealed class VaultConfigurationSource(string baseUrl, string clientId, string clientSecret, bool optional)
    : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new VaultConfigurationProvider(baseUrl, clientId, clientSecret, optional);
}

public sealed class VaultConfigurationProvider(string baseUrl, string clientId, string clientSecret, bool optional)
    : ConfigurationProvider
{
    public override void Load()
    {
        try
        {
            using var http = new HttpClient
            {
                BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromSeconds(20),
            };

            // 1. Authenticate (ClientId + ClientSecret) -> scoped token.
            using var loginResp = http.PostAsJsonAsync("api/vault/login",
                new { clientId, clientSecret }).GetAwaiter().GetResult();
            loginResp.EnsureSuccessStatusCode();
            var loginJson = loginResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var token = JsonDocument.Parse(loginJson).RootElement.GetProperty("token").GetString();

            // 2. Pull every secret this application is authorized for.
            using var req = new HttpRequestMessage(HttpMethod.Get, "api/vault/my-secrets");
            req.Headers.Authorization = new("Bearer", token);
            using var resp = http.Send(req);
            resp.EnsureSuccessStatusCode();
            var json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in JsonDocument.Parse(json).RootElement.EnumerateArray())
            {
                var name = s.GetProperty("name").GetString();
                if (!string.IsNullOrEmpty(name))
                    data[name] = s.GetProperty("value").GetString(); // name == config key (e.g. "Stripe:SecretKey")
            }
            Data = data;
        }
        catch (Exception ex)
        {
            if (!optional)
                throw new InvalidOperationException("Failed to load secrets from the vault: " + ex.Message, ex);
            // Optional: vault unreachable -> leave local appsettings values in place.
            Console.WriteLine("[Vault] Could not load secrets, using local appsettings. " + ex.Message);
        }
    }
}
