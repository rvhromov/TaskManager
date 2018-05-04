using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class EditModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Invalid name length")]
        [Display(Name = "Name")]
        public string FirstName { get; set; }
    }
}