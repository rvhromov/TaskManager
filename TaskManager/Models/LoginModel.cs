using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must not be less than 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}