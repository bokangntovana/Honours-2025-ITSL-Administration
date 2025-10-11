// EventSchedulesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITSL_Administration.Data;
using ITSL_Administration.Models;
using Microsoft.AspNetCore.Authorization;

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

        // GET: EventSchedules
        public IActionResult Index()
        {
            return View();
        }

        // GET: EventSchedules/GetEvents
        [HttpGet]
        public async Task<JsonResult> GetEvents()
        {
            var events = await _context.Events
                .Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    start = e.Start,
                    end = e.End,
                    description = e.Description,
                    backgroundColor = e.BackgroundColor,
                    allDay = e.IsAllDay
                })
                .ToListAsync();

            return Json(events);
        }

        // GET: EventSchedules/Create
        [Authorize(Roles = "Admin,Lecturer,Tutor")]
        public IActionResult Create(string date)
        {
            var eventSchedule = new EventSchedule();

            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
            {
                eventSchedule.Start = parsedDate;
                eventSchedule.End = parsedDate.AddHours(3); // Default 3-hour duration
            }
            else
            {
                eventSchedule.Start = DateTime.Today.AddHours(9); // Default to today 9 AM
                eventSchedule.End = DateTime.Today.AddHours(12); // Default to today 12 PM
            }

            return View(eventSchedule);
        }

        // POST: EventSchedules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Lecturer,Tutor")]
        public async Task<IActionResult> Create(EventSchedule eventSchedule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(eventSchedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(eventSchedule);
        }

        // GET: EventSchedules/Edit/5
        [Authorize(Roles = "Admin,Lecturer,Tutor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventSchedule = await _context.Events.FindAsync(id);
            if (eventSchedule == null)
            {
                return NotFound();
            }
            return View(eventSchedule);
        }

        // POST: EventSchedules/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Lecturer,Tutor")]
        public async Task<IActionResult> Edit(int id, EventSchedule eventSchedule)
        {
            if (id != eventSchedule.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eventSchedule);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventScheduleExists(eventSchedule.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(eventSchedule);
        }

        // GET: EventSchedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventSchedule = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventSchedule == null)
            {
                return NotFound();
            }

            return View(eventSchedule);
        }

        // GET: EventSchedules/Delete/5
        [Authorize(Roles = "Admin,Lecturer,Tutor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventSchedule = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventSchedule == null)
            {
                return NotFound();
            }

            return View(eventSchedule);
        }

        // POST: EventSchedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Lecturer,Tutor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventSchedule = await _context.Events.FindAsync(id);
            if (eventSchedule != null)
            {
                _context.Events.Remove(eventSchedule);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventScheduleExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}