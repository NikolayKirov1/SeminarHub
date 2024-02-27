using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static SeminarHub.Data.Common.Constraints;

namespace SeminarHub.Data.Models
{
    [Comment("Category of the seminar")]
    public class Category
    {
        [Comment("Primary key")]
        [Key]
        public int Id { get; set; }

        [Comment("Name of the category")]
        [Required]
        [MaxLength(CategoryNameMaxLength)]
        public string Name { get; set; } = null!;

        [Comment("Collection of seminars")]
        public virtual ICollection<Seminar> Seminars { get; set; } = new List<Seminar>();
    }
}
