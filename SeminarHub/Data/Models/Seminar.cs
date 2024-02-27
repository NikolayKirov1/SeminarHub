using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static SeminarHub.Data.Common.Constraints;
using Microsoft.VisualBasic;

namespace SeminarHub.Data.Models
{
    [Comment("Seminar")]
    public class Seminar
    {
        [Comment("Primary key")]
        [Key]
        public int Id { get; set; }

        [Comment("Topic of the seminar")]
        [Required]
        [MaxLength(SeminarTopicMaxLength)]
        public string Topic { get; set; } = null!;

        [Comment("Lecturer of the seminar")]
        [Required]
        [MaxLength(SeminarLecturerMaxLength)]
        public string Lecturer { get; set; } = null!;

        [Comment("Details of the seminar")]
        [Required]
        [MaxLength(SeminarDetailsMaxLength)]
        public string Details { get; set; } = null!;

        [Comment("Id of the organizer")]
        [Required]
        public string OrganizerId { get; set; } = null!;

        [Comment("Organizer of the seminar")]
        [Required]
        [ForeignKey(nameof(OrganizerId))]
        public IdentityUser Organizer { get; set; } = null!;

        [Comment("Date and time of the seminar")]
        [Required]
        public DateTime DateAndTime { get; set; }

        [Comment("Duration of the seminar")]
        [Range(SeminarDurationMinValue, SeminarDurationMaxValue)]
        public int Duration { get; set; }

        [Comment("Id of the category")]
        [Required]
        public int CategoryId { get; set; }

        [Comment("Category of the seminar")]
        [Required]
        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; } = null!;

        [Comment("Collection of seminar participant")]
        public virtual ICollection<SeminarParticipant> SeminarsParticipants { get; set; } = new List<SeminarParticipant>();
    }
}
