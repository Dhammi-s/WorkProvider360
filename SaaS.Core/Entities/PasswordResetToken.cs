namespace SaaS.Core.Entities;

/// <summary>
/// A single-use password reset token persisted in the tenant database and
/// emailed to the user as part of a reset link.
/// </summary>
public sealed class PasswordResetToken
{
    public long PasswordResetTokenId { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresOn { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedOn { get; set; }
}
