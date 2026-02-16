using CoursesApplication.Domain.DomainModels;
using CoursesApplication.Repository.Data;
using CoursesApplication.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;

namespace CoursesApplication.Web.Controllers
{
    public class SeatController : Controller
    {
        private readonly ISeatService _seatService;
        private readonly IClassroomService _classRoomService;
        private readonly ApplicationDbContext _db;

        public SeatController(ISeatService seatService, IClassroomService classRoomService, ApplicationDbContext db)
        {
            _seatService = seatService;
            _classRoomService = classRoomService;
            _db = db;
        }

        public IActionResult Index()
        {
            return View(_seatService.GetAll());
        }

     
        [HttpGet]
        public IActionResult Details(Guid? id)
        {
            if (id is null) return BadRequest();

            var seat = _db.Seats
                .AsNoTracking()
                .Include(s => s.Classroom)
                .Include(s => s.TakenSeat).ThenInclude(ts => ts.User)
                .Include(s => s.SeatLogs).ThenInclude(l => l.User)   // 👈 add this
                .FirstOrDefault(s => s.Id == id.Value);

            if (seat is null) return NotFound();
            return View("Details", seat);
        }

        [Authorize(Roles = "Keeper")]
        public IActionResult Create()
        {
            var classrooms = _classRoomService.GetAll().ToList();
            ViewBag.Classrooms = new SelectList(classrooms, "Id", "Name");
            return View();
        }


        [Authorize(Roles = "Keeper")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,ClassroomId,Number")] Seat seat)
        {
            if (ModelState.IsValid)
            {
                _seatService.Insert(seat);
                return RedirectToAction(nameof(Index));
            }
            return View(seat);
        }

        [Authorize(Roles = "Keeper")]
        [HttpGet]
        public IActionResult Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var seat = _seatService.GetById(id.Value);
            if (seat == null) return NotFound();

            return View(seat);
        }

        [Authorize(Roles = "Keeper")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, [Bind("Id,ClassroomId,Number,TakenSeatId")] Seat seat)
        {
            if (id != seat.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _seatService.Update(seat);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SeatExists(seat.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(seat);
        }

        [Authorize(Roles = "Keeper")]
        public IActionResult Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var seat = _seatService.GetById(id.Value);
            if (seat == null) return NotFound();

            return View(seat);
        }

        [Authorize(Roles = "Keeper")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _seatService.DeleteById(id);
            return RedirectToAction(nameof(Index));
        }

        private bool SeatExists(Guid id)
        {
            return _seatService.GetById(id) != null;
        }
    }
}
