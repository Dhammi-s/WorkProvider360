/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Settings;

/// <summary>
/// LLM (Groq, OpenAI-compatible) configuration for the in-app assistant.
/// Keep the real key OUT of the repo — set it via the env var Llm__ApiKey.
/// </summary>
public sealed class LlmSettings
{
    public const string SectionName = "Llm";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "llama-3.3-70b-versatile";
    public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ApiKey)
        && !ApiKey.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase);
}
