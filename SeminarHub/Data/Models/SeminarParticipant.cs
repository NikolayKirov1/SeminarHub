using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeminarHub.Data.Models
{
    [Comment("Seminars and participants")]
    public class SeminarParticipant
    {
        [Comment("Id of the seminar")]
        [Required]
        public int SeminarId { get; set; }

        [Comment("Seminar")]
        [ForeignKey(nameof(SeminarId))]
        public Seminar Seminar { get; set; } = null!;

        [Comment("Id of the participant")]
        [Required]
        public string ParticipantId { get; set; } = null!;

        [Comment("Participant of the seminar")]
        [ForeignKey(nameof(ParticipantId))]
        public IdentityUser Participant { get; set; } = null!;
    }
}
