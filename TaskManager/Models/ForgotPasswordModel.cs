using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
    }
}