/* =============================================================================
   In-Memory Cache client.
   Talks to the self-hosted Redis-like cache exposed by the SqlAccess API over
   REST (/api/cache/*). It reuses the SAME application credential this app already
   uses for the Secret Vault (Vault:ClientId / Vault:ClientSecret) — the cache
   endpoints accept that token because it is a valid app JWT on the same server.

   Design: FAIL-OPEN. If the cache is unreachable or errors, reads return null
   (treated as a miss) and writes are best-effort — the caller keeps working
   against its source of truth. A cache outage must never take the app down.

   appsettings.json (optional — falls back to the Vault:* values):
     "Cache": {
       "BaseUrl": "https://sqlaccess.runasp.net",   // defaults to Vault:BaseUrl
       "ClientId": "app_xxx",                        // defaults to Vault:ClientId
       "ClientSecret": "sk_xxx"                      // defaults to Vault:ClientSecret
     }
   ============================================================================= */

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace WebApplication1.Infrastructure;

/// <summary>A minimal, fail-open client for the self-hosted in-memory cache.</summary>
public interface ICacheClient
{
    /// <summary>GET a raw string value. Returns <c>null</c> on a miss or any error.</summary>
    Task<string?> GetStringAsync(string key, CancellationToken ct = default);

    /// <summary>GET and JSON-deserialize a value. Returns <c>default</c> on a miss or any error.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);

    /// <summary>SET a raw string value with an optional TTL. Best-effort.</summary>
    Task SetStringAsync(string key, string value, TimeSpan? ttl = null, CancellationToken ct = default);

    /// <summary>JSON-serialize and SET a value with an optional TTL. Best-effort.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default);

    /// <summary>DEL a key. Returns true if a live key was removed.</summary>
    Task<bool> DeleteAsync(string key, CancellationToken ct = default);

    /// <summary>EXISTS. Returns false on error.</summary>
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);

    /// <summary>INCR a counter and return the new value; returns 0 on error.</summary>
    Task<long> IncrementAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Cache-aside helper: return the cached value, or run <paramref name="factory"/>, cache its
    /// result for <paramref name="ttl"/>, and return it. If the cache is down the factory still runs,
    /// so the caller always gets a value.
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, TimeSpan? ttl, Func<Task<T>> factory, CancellationToken ct = default);
}

/// <summary>Options for <see cref="CacheClient"/>, bound from "Cache" (falling back to "Vault").</summary>
public sealed class CacheClientOptions
{
    public string BaseUrl { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(BaseUrl)
        && !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
}

/// <inheritdoc />
public sealed class CacheClient : ICacheClient
{
    private readonly HttpClient _http;
    private readonly CacheClientOptions _options;
    private readonly ILogger<CacheClient> _log;
    private readonly SemaphoreSlim _tokenGate = new(1, 1);
    private string? _token;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public CacheClient(HttpClient http, CacheClientOptions options, ILogger<CacheClient> log)
    {
        _http = http;
        _options = options;
        _log = log;
    }

    // ---------------- reads ----------------

    public async Task<string?> GetStringAsync(string key, CancellationToken ct = default)
    {
        try
        {
            using var resp = await SendAsync(HttpMethod.Get, $"api/cache/get/{Esc(key)}", null, ct);
            if (resp.StatusCode == HttpStatusCode.NotFound) return null; // miss
            if (!resp.IsSuccessStatusCode) return null;
            var result = await resp.Content.ReadFromJsonAsync<CommandResult>(Json, ct);
            return result?.Value;
        }
        catch (Exception ex) { return FailRead<string?>(key, ex); }
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var raw = await GetStringAsync(key, ct);
        if (raw is null) return default;
        try { return JsonSerializer.Deserialize<T>(raw, Json); }
        catch (Exception ex) { return FailRead<T?>(key, ex); }
    }

    // ---------------- writes ----------------

    public async Task SetStringAsync(string key, string value, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        try
        {
            var body = new { key, value, ttlSeconds = ttl.HasValue ? (int?)Math.Ceiling(ttl.Value.TotalSeconds) : null };
            using var resp = await SendAsync(HttpMethod.Post, "api/cache/set", JsonContent.Create(body, options: Json), ct);
            if (!resp.IsSuccessStatusCode) _log.LogWarning("Cache SET {Key} returned {Status}.", key, resp.StatusCode);
        }
        catch (Exception ex) { FailWrite(key, ex); }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
        => SetStringAsync(key, JsonSerializer.Serialize(value, Json), ttl, ct);

    public async Task<bool> DeleteAsync(string key, CancellationToken ct = default)
    {
        try
        {
            using var resp = await SendAsync(HttpMethod.Delete, $"api/cache/del/{Esc(key)}", null, ct);
            if (!resp.IsSuccessStatusCode) return false;
            var result = await resp.Content.ReadFromJsonAsync<CommandResult>(Json, ct);
            return result?.Number == 1;
        }
        catch (Exception ex) { FailWrite(key, ex); return false; }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        try
        {
            using var resp = await SendAsync(HttpMethod.Get, $"api/cache/exists/{Esc(key)}", null, ct);
            if (!resp.IsSuccessStatusCode) return false;
            var result = await resp.Content.ReadFromJsonAsync<CommandResult>(Json, ct);
            return result?.Number == 1;
        }
        catch (Exception ex) { return FailRead<bool>(key, ex); }
    }

    public async Task<long> IncrementAsync(string key, CancellationToken ct = default)
    {
        try
        {
            using var resp = await SendAsync(HttpMethod.Post, $"api/cache/incr/{Esc(key)}", null, ct);
            if (!resp.IsSuccessStatusCode) return 0;
            var result = await resp.Content.ReadFromJsonAsync<CommandResult>(Json, ct);
            return result?.Number ?? 0;
        }
        catch (Exception ex) { return FailRead<long>(key, ex); }
    }

    // ---------------- cache-aside ----------------

    public async Task<T> GetOrSetAsync<T>(string key, TimeSpan? ttl, Func<Task<T>> factory, CancellationToken ct = default)
    {
        var cached = await GetAsync<T>(key, ct);
        if (cached is not null) return cached;

        var value = await factory();                    // source of truth — always runs on a miss
        if (value is not null) await SetAsync(key, value, ttl, ct); // best-effort populate
        return value;
    }

    // ---------------- transport + auth ----------------

    /// <summary>Sends a request with the app token, refreshing it once on a 401.</summary>
    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, HttpContent? content, CancellationToken ct)
    {
        var token = await GetTokenAsync(ct);
        var resp = await SendOnceAsync(method, path, content, token, ct);
        if (resp.StatusCode != HttpStatusCode.Unauthorized) return resp;

        resp.Dispose();
        token = await GetTokenAsync(ct, force: true);   // token expired/revoked — refresh and retry once
        return await SendOnceAsync(method, path, content, token, ct);
    }

    private async Task<HttpResponseMessage> SendOnceAsync(HttpMethod method, string path, HttpContent? content, string? token, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(method, path);
        if (content is not null) req.Content = content;
        if (!string.IsNullOrEmpty(token)) req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _http.SendAsync(req, ct);
    }

    /// <summary>Returns a cached app token, logging in if needed. Thread-safe.</summary>
    private async Task<string?> GetTokenAsync(CancellationToken ct, bool force = false)
    {
        if (!force && _token is not null) return _token;

        await _tokenGate.WaitAsync(ct);
        try
        {
            if (!force && _token is not null) return _token;
            using var resp = await _http.PostAsJsonAsync("api/vault/login",
                new { clientId = _options.ClientId, clientSecret = _options.ClientSecret }, ct);
            resp.EnsureSuccessStatusCode();
            var doc = await resp.Content.ReadFromJsonAsync<JsonElement>(ct);
            _token = doc.GetProperty("token").GetString();
            return _token;
        }
        finally
        {
            _tokenGate.Release();
        }
    }

    private static string Esc(string key) => Uri.EscapeDataString(key);

    private T? FailRead<T>(string key, Exception ex)
    {
        _log.LogWarning(ex, "Cache read failed for {Key}; treating as a miss.", key);
        return default;
    }

    private void FailWrite(string key, Exception ex)
        => _log.LogWarning(ex, "Cache write failed for {Key}; ignored (fail-open).", key);

    /// <summary>Mirrors the API's CommandResult DTO.</summary>
    private sealed record CommandResult(bool Ok, string? Value, long? Number, string? Message);
}

public static class CacheClientExtensions
{
    /// <summary>
    /// Registers <see cref="ICacheClient"/>. Reads the "Cache" section, falling back to "Vault" for
    /// BaseUrl/ClientId/ClientSecret so no extra config is needed when the cache lives on the same
    /// server as the vault. No-ops gracefully (a null-object client) if nothing is configured.
    /// </summary>
    public static IServiceCollection AddVaultCache(this IServiceCollection services, IConfiguration config)
    {
        var options = new CacheClientOptions
        {
            BaseUrl = config["Cache:BaseUrl"] ?? config["Vault:BaseUrl"] ?? "",
            ClientId = config["Cache:ClientId"] ?? config["Vault:ClientId"] ?? "",
            ClientSecret = config["Cache:ClientSecret"] ?? config["Vault:ClientSecret"] ?? "",
        };
        services.AddSingleton(options);

        if (!options.IsConfigured)
        {
            services.AddSingleton<ICacheClient, NullCacheClient>();
            return services;
        }

        services.AddHttpClient<ICacheClient, CacheClient>(http =>
        {
            http.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            http.Timeout = TimeSpan.FromSeconds(10);
        });
        return services;
    }
}

/// <summary>Used when the cache isn't configured: every read misses, every write is a no-op.</summary>
public sealed class NullCacheClient : ICacheClient
{
    public Task<string?> GetStringAsync(string key, CancellationToken ct = default) => Task.FromResult<string?>(null);
    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) => Task.FromResult<T?>(default);
    public Task SetStringAsync(string key, string value, TimeSpan? ttl = null, CancellationToken ct = default) => Task.CompletedTask;
    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default) => Task.CompletedTask;
    public Task<bool> DeleteAsync(string key, CancellationToken ct = default) => Task.FromResult(false);
    public Task<bool> ExistsAsync(string key, CancellationToken ct = default) => Task.FromResult(false);
    public Task<long> IncrementAsync(string key, CancellationToken ct = default) => Task.FromResult(0L);
    public async Task<T> GetOrSetAsync<T>(string key, TimeSpan? ttl, Func<Task<T>> factory, CancellationToken ct = default) => await factory();
}
