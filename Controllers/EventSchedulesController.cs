using ITSL_Administration.Data;
using ITSL_Administration.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITSL_Administration.Controllers
{
    [Authorize]
    public class EventSchedulesController : Controller
    {
        private readonly AppDbContext _context;

        public EventSchedulesController(AppDbContext context)
        {
            _context = context;
        }

        // Default calendar page
        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // Fetch all events as JSON for the calendar
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events.ToListAsync();

            var eventList = events.Select(e => new
            {
                id = e.Id,
                title = e.Title,
                start = e.Start.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = e.End?.ToString("yyyy-MM-ddTHH:mm:ss"),
                description = e.Description,
                color = GetEventColor(e)
            });

            return Json(eventList);
        }

        // Determines color dynamically
        private string GetEventColor(EventSchedule e)
        {
            if (e.End < DateTime.Now) return "#ff4c4c"; // red for past
            if (e.Start <= DateTime.Now && e.End >= DateTime.Now) return "#4CAF50"; // green for current
            return "#87CEFA"; // light blue for future
        }

        [Authorize(Roles = "Admin,Lecturer,Tutor")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EventSchedule model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return Ok(model);
            }
            return BadRequest(ModelState);
        }

        [Authorize(Roles = "Admin,Lecturer,Tutor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EventSchedule model)
        {
            if (id != model.Id)
                return BadRequest();

            var existing = await _context.Events.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Title = model.Title;
            existing.Description = model.Description;
            existing.Start = model.Start;
            existing.End = model.End;
            existing.IsAllDay = model.IsAllDay;
            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        [Authorize(Roles = "Admin,Lecturer,Tutor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var evt = await _context.Events.FindAsync(id);
            if (evt == null)
                return NotFound();

            _context.Events.Remove(evt);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // For modal detail view
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var evt = await _context.Events.FindAsync(id);
            if (evt == null)
                return NotFound();

            return PartialView("_EventDetails", evt);
        }
    }
}
