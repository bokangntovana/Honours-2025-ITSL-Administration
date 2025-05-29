using ITSL_Administration.Data;
using ITSL_Administration.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        // GET: ClassesAndEvents (Main Calendar View)
        public IActionResult Index()
        {
            return View();
        }

        // GET: Fetch Events (for FullCalendar)
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

        // POST: Create Event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent([FromBody] Event newEvent)
        {
            if (ModelState.IsValid)
            {
                _context.Events.Add(newEvent);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors) });
        }

        // POST: Update Event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEvent([FromBody] Event updatedEvent)
        {
            if (ModelState.IsValid)
            {
                var existingEvent = await _context.Events.FindAsync(updatedEvent.Id);
                if (existingEvent == null)
                {
                    return NotFound();
                }

                existingEvent.Title = updatedEvent.Title;
                existingEvent.Start = updatedEvent.Start;
                existingEvent.End = updatedEvent.End;
                existingEvent.Description = updatedEvent.Description;
                existingEvent.BackgroundColor = updatedEvent.BackgroundColor;
                existingEvent.IsAllDay = updatedEvent.IsAllDay;

                _context.Events.Update(existingEvent);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors) });
        }

        // POST: Delete Event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventToDelete = await _context.Events.FindAsync(id);
            if (eventToDelete == null)
            {
                return NotFound();
            }

            _context.Events.Remove(eventToDelete);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
