using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>Creates a Stripe Checkout session for an online invoice payment.</summary>
public sealed class CheckoutRequestDto
{
    [Required]
    public decimal Amount { get; set; }

    [Required, MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Must contain the literal {CHECKOUT_SESSION_ID} placeholder.</summary>
    [Required]
    public string SuccessUrl { get; set; } = string.Empty;

    [Required]
    public string CancelUrl { get; set; } = string.Empty;
}
