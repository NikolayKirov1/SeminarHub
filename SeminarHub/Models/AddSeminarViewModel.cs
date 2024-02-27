using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SeminarHub.Data.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static SeminarHub.Data.Common.Constraints;

namespace SeminarHub.Models
{
    public class AddSeminarViewModel
    {
        [Required]
        [StringLength(SeminarTopicMaxLength, MinimumLength = SeminarTopicMinLength)]
        public string Topic { get; set; } = null!;

        [Required]
        [StringLength(SeminarLecturerMaxLength, MinimumLength = SeminarLecturerMinLength)]
        public string Lecturer { get; set; } = null!;

        [Required]
        [StringLength(SeminarDetailsMaxLength, MinimumLength = SeminarDetailsMinLength)]
        public string Details { get; set; } = null!;

        [Required]
        public string DateAndTime { get; set; } = null!;

        public string Duration { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        public IEnumerable<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    }
}
