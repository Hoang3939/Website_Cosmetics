using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Website_Cosmetics.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email or username is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Email or username must be between 3 and 100 characters")]
        [Display(Name = "Email or Username")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; } = false;

        public string? ReturnUrl { get; set; }
    }
}
