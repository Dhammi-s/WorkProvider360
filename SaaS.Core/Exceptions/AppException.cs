/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Exceptions;

/// <summary>
/// Represents an expected, user-facing error (bad credentials, not found, etc.).
/// The API maps this to a controlled HTTP status instead of a 500.
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }

    public static AppException Unauthorized(string message = "Invalid credentials.") => new(message, 401);
    public static AppException Forbidden(string message = "You do not have access to this resource.") => new(message, 403);
    public static AppException NotFound(string message = "Resource not found.") => new(message, 404);
    public static AppException BadRequest(string message) => new(message, 400);
    public static AppException Conflict(string message) => new(message, 409);
}
