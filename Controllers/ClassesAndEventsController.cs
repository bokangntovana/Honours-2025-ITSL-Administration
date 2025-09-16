using ITSL_Administration.Data;
using ITSL_Administration.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ITSL_Administration.Controllers
{
    [Authorize]
    public class ClassesAndEventsController : Controller
    {
        private readonly AppDbContext _context;

        public ClassesAndEventsController(AppDbContext context)
        {
            _context = context;
        }

        // Main Calendar View
        public IActionResult Index()
        {
            return View();
        }

        // Fetch Events
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    start = e.Start.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = e.End.HasValue ? e.End.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null,
                    description = e.Description,
                    backgroundColor = e.BackgroundColor,
                    allDay = e.IsAllDay
                })
                .ToListAsync();

            return Json(events);
        }

        // Create Event
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] EventSchedule newEvent)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // Update Event
        [HttpPost]
        public async Task<IActionResult> UpdateEvent([FromBody] EventSchedule updatedEvent)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Events.FindAsync(updatedEvent.Id);
            if (existing == null)
                return NotFound();

            existing.Title = updatedEvent.Title;
            existing.Start = updatedEvent.Start;
            existing.End = updatedEvent.End;
            existing.Description = updatedEvent.Description;
            existing.BackgroundColor = updatedEvent.BackgroundColor;
            existing.IsAllDay = updatedEvent.IsAllDay;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // Delete Event
        [HttpPost]
        public async Task<IActionResult> DeleteEvent([FromBody] int id)
        {
            var evt = await _context.Events.FindAsync(id);
            if (evt == null)
                return NotFound();

            _context.Events.Remove(evt);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
