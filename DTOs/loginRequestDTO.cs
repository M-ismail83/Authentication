
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace Authentication.DTOs
{
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public required string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [PasswordPropertyText]
        public required string Password { get; set; }
    }
}