using System.ComponentModel.DataAnnotations;

namespace Authentication.DTOs
{
    public class VerifyTotpDTO
    {
        [Required(ErrorMessage = "TOTP code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "TOTP code must be 6 digits")]
        public required string totpCode { get; set; }
        public required string Email { get; set; }
    }
}