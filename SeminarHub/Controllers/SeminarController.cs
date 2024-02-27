using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SeminarHub.Data;
using SeminarHub.Data.Models;
using SeminarHub.Models;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Claims;
using static SeminarHub.Data.Common.Constraints;

namespace SeminarHub.Controllers
{
    [Authorize]
    public class SeminarController : Controller
    {
        private readonly SeminarHubDbContext dbContext;

        public SeminarController(SeminarHubDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> All()
        {
            var model = await dbContext
                .Seminars
                .AsNoTracking()
                .Select(s => new AllSeminarsViewModel
                {
                    Id = s.Id,
                    Topic = s.Topic,
                    Lecturer = s.Lecturer,
                    Category = s.Category.Name,
                    DateAndTime = s.DateAndTime.ToString(DateAndTimeFormat),
                    Organizer = s.Organizer.UserName
                }).ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var seminarForm = new AddSeminarViewModel();
            seminarForm.Categories = await GetCategories();

            return View(seminarForm);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddSeminarViewModel seminarForm)
        {
            DateTime dateAndTime;

            if (!DateTime.TryParseExact(seminarForm.DateAndTime, DateAndTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateAndTime))
            {
                ModelState.AddModelError(nameof(seminarForm.DateAndTime), $"Invalid date! Format must be {DateAndTimeFormat}");
            }

            int duration;

            if (!int.TryParse(seminarForm.Duration, out duration) || duration < SeminarDurationMinValue || duration > SeminarDurationMaxValue)
            {
                ModelState.AddModelError(nameof(seminarForm.Duration), "Duration must be a number between 30 and 180.");

                seminarForm.Categories = await GetCategories();

                return View(seminarForm);
            }

            if (!ModelState.IsValid)
            {
                seminarForm.Categories = await GetCategories();

                return View(seminarForm);
            }

            Seminar newSeminar = new Seminar()
            {
                Topic = seminarForm.Topic,
                Lecturer = seminarForm.Lecturer,
                Details = seminarForm.Details,
                DateAndTime = dateAndTime,
                Duration = duration,
                CategoryId = seminarForm.CategoryId,
                OrganizerId = GetUserId()
            };

            await dbContext.AddAsync(newSeminar);
            await dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(All));
        }

        [HttpPost]
        public async Task<IActionResult> Join(int id)
        {
            var currentUserId = GetUserId();

            if (!await dbContext.Seminars
                .AsNoTracking()
                .AnyAsync(s => s.Id == id))
            {
                return BadRequest();
            }

            if (await dbContext.SeminarParticipants
                .AsNoTracking()
                .AnyAsync(sp => sp.SeminarId == id && sp.ParticipantId == currentUserId))
            {
                return RedirectToAction(nameof(All));
            }

            var userSeminar = new SeminarParticipant()
            {
                SeminarId = id,
                ParticipantId = currentUserId
            };

            await dbContext.SeminarParticipants.AddAsync(userSeminar);
            await dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Joined));
        }

        [HttpPost]
        public async Task<IActionResult> Leave(int id)
        {
            if (!await dbContext.Seminars
                .AsNoTracking()
                .AnyAsync(s => s.Id == id))
            {
                return BadRequest();
            }

            var currentUserId = GetUserId();

            var userSeminar = await dbContext.SeminarParticipants
                .Where(sp => sp.SeminarId == id && sp.ParticipantId == currentUserId)
                .FirstOrDefaultAsync();

            if (userSeminar == null)
            {
                return BadRequest();
            }

            dbContext.SeminarParticipants.Remove(userSeminar);
            await dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Joined));
        }

        [HttpGet]
        public async Task<IActionResult> Joined()
        {
            var currentUser = GetUserId();

            var model = await dbContext
                .Seminars
                .Include(s => s.SeminarsParticipants)
                .Where(s => s.SeminarsParticipants.Any(sp => sp.ParticipantId == currentUser))
                .AsNoTracking()
                .Select(s => new JoinedSeminarsViewModel
                {
                    Id = s.Id,
                    Topic = s.Topic,
                    Lecturer = s.Lecturer,
                    DateAndTime = s.DateAndTime.ToString(DateAndTimeFormat),
                    Organizer = s.Organizer.UserName
                }).ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var currentSeminar = await dbContext
                .Seminars
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new DetailsSeminarViewModel
                {
                    Id = s.Id,
                    Topic = s.Topic,
                    DateAndTime = s.DateAndTime.ToString(DateAndTimeFormat),
                    Duration = s.Duration.ToString(),
                    Lecturer = s.Lecturer,
                    Category = s.Category.Name,
                    Details = s.Details,
                    Organizer = s.Organizer.UserName
                })
                .FirstOrDefaultAsync();

            if (currentSeminar == null)
            {
                return BadRequest();
            }

            return View(currentSeminar);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var currentSeminar = await dbContext.Seminars
                .FindAsync(id);

            if (currentSeminar == null)
            {
                return BadRequest();
            }

            if (currentSeminar.OrganizerId != GetUserId())
            {
                return Unauthorized();
            }

            var seminarForm = new EditSeminarViewModel()
            {
                Topic = currentSeminar.Topic,
                Lecturer = currentSeminar.Lecturer,
                Details = currentSeminar.Details,
                DateAndTime = currentSeminar.DateAndTime.ToString(DateAndTimeFormat),
                Duration = currentSeminar.Duration.ToString(),
                CategoryId = currentSeminar.CategoryId
            };

            seminarForm.Categories = await GetCategories();

            return View(seminarForm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditSeminarViewModel seminarForm, int id)
        {
            DateTime dateAndTime;

            if (!DateTime.TryParseExact(seminarForm.DateAndTime, DateAndTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateAndTime))
            {
                ModelState.AddModelError(nameof(seminarForm.DateAndTime), $"Invalid date! Format must be {DateAndTimeFormat}");
            }

            int duration;

            if (!int.TryParse(seminarForm.Duration, out duration) || duration < SeminarDurationMinValue || duration > SeminarDurationMaxValue)
            {
                ModelState.AddModelError(nameof(seminarForm.Duration), "Duration must be a number between 30 and 180.");

                seminarForm.Categories = await GetCategories();

                return View(seminarForm);
            }

            if (!ModelState.IsValid)
            {
                seminarForm.Categories = await GetCategories();

                return View(seminarForm);
            }

            Seminar? editedSeminar = await dbContext.Seminars
                .Where(s => s.Id == id)
                .FirstOrDefaultAsync();

            editedSeminar.Topic = seminarForm.Topic;
            editedSeminar.Lecturer = seminarForm.Lecturer;
            editedSeminar.Details = seminarForm.Details;
            editedSeminar.DateAndTime = dateAndTime;
            editedSeminar.Duration = duration;
            editedSeminar.CategoryId = seminarForm.CategoryId;
            
            await dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(All));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var currentSeminar = await dbContext.Seminars
                .FindAsync(id);

            if (currentSeminar == null)
            {
                return BadRequest();
            }

            if (currentSeminar.OrganizerId != GetUserId())
            {
                return Unauthorized();
            }

            var seminarForm = new DeleteSeminarViewModel()
            {
                Id = currentSeminar.Id,
                Topic = currentSeminar.Topic,
                DateAndTime = currentSeminar.DateAndTime.ToString(DateAndTimeFormat),
            };

            return View(seminarForm);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(EditSeminarViewModel seminarForm, int id)
        {
            var currentSeminar = await dbContext.Seminars
                .FindAsync(id);

            if (currentSeminar == null)
            {
                return BadRequest();
            }

            if (currentSeminar.OrganizerId != GetUserId())
            {
                return Unauthorized();
            }

            dbContext.Remove(currentSeminar);
            await dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(All));
        }



        private async Task<IEnumerable<CategoryViewModel>> GetCategories()
        {
            return await dbContext.Categories
                .AsNoTracking()
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }

        private string GetUserId()
        {
            string id = string.Empty;

            if (User != null)
            {
                id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            return id;
        }
    }
}
