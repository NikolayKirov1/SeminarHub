using System.ComponentModel.DataAnnotations;
using static SeminarHub.Data.Common.Constraints;

namespace SeminarHub.Models
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(CategoryNameMaxLength, MinimumLength = CategoryNameMinLength)]
        public string Name { get; set; } = null!;
    }
}
