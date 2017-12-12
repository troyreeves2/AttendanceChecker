using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AttendanceCheckerSystem.Data;
using AttendanceCheckerSystem.Models;

namespace AttendanceCheckerSystem.Controllers
{
    public class MeetingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MeetingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Meetings
        public async Task<IActionResult> Index()
        {

            var days = await _context.Meetings.ToListAsync();
            var sorteddays = days.OrderBy(d => d.MeetingDay);

            return View(sorteddays);
        }

        [HttpPost, ActionName("Attend")]
        public IActionResult StudentAttends(AttendanceViewModel vm)
        {

            var selectedList = _context.AttendMeeting.Where(m => m.MeetingID == vm.MeetingID &&
                                                             m.StudentID == vm.StudentID);

            var selected = _context.AttendMeeting.Find(selectedList.First().ID);

            selected.Attend = true;

            _context.SaveChanges();

            return RedirectToAction("StudentAttendanceConfirmed", vm);
            //return View("StudentAttendanceConfirmed", selected);
        }

        public async Task<IActionResult> StudentAttendanceConfirmed(AttendanceViewModel attended)
        {
            var days = await _context.Meetings.ToListAsync();
            var students = await _context.Students.ToListAsync();

            var meetings = days.OrderBy(d => d.MeetingDay);
            ViewData["MeetingList"] = meetings;

            var studentsSorted = students.OrderBy(s => s.LastName);
            ViewData["StudentList"] = studentsSorted;

            var student = students.Where(s => s.ID == attended.StudentID).First();
            var meeting = meetings.Where(m => m.ID == attended.MeetingID).First();

            StudentAttendanceConfirmedViewModel vm = new StudentAttendanceConfirmedViewModel
            {
                StudentName = student.LastName + ", " + student.FirstName,
                DateAttended = meeting.MeetingDay
            };

            return View(vm);
        }

        // GET: Meetings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meetings
                .SingleOrDefaultAsync(m => m.ID == id);
            if (meeting == null)
            {
                return NotFound();
            }

            ViewData["MeetingInfo"] = meeting.MeetingDay.ToLongDateString();


            var model = _context.Students
                .Join(_context.AttendMeeting,
                      s => s.ID,
                      a => a.StudentID,
                      ((student, attend) => new AttendanceDetailViewModel { Student = student, Attend = attend }))
                .Where(m => m.Attend.MeetingID == meeting.ID)
                .OrderBy(m => m.Student.LastName);

            return View(model);
        }

        // GET: Meetings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Meetings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,MeetingTimes,MeetingDay,MeetingLocation")] Meeting meeting)
        {
            if (ModelState.IsValid)
            {
                _context.Add(meeting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(meeting);
        }

        // GET: Meetings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meetings.SingleOrDefaultAsync(m => m.ID == id);
            if (meeting == null)
            {
                return NotFound();
            }
            return View(meeting);
        }

        // POST: Meetings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,MeetingTimes,MeetingDay,MeetingLocation")] Meeting meeting)
        {
            if (id != meeting.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(meeting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MeetingExists(meeting.ID))
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
            return View(meeting);
        }

        // GET: Meetings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meetings
                .SingleOrDefaultAsync(m => m.ID == id);
            if (meeting == null)
            {
                return NotFound();
            }

            return View(meeting);
        }

        // POST: Meetings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var meeting = await _context.Meetings.SingleOrDefaultAsync(m => m.ID == id);
            _context.Meetings.Remove(meeting);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MeetingExists(int id)
        {
            return _context.Meetings.Any(e => e.ID == id);
        }

        public async Task<IActionResult> AttendanceSummary()
        {
            IEnumerable<Student> list = await _context.Students.ToListAsync();

            //var filtered_list = list.Where(s => s.FirstName.StartsWith("Ja"));

            var filtered_list = from s in list
                                where s.FirstName.StartsWith("")
                                select s;


            return View(filtered_list);
        }

    }
}
