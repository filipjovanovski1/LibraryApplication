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
    public class TakenSeatController : Controller
    {
        private readonly ITakenSeatService _takenSeatService;
        private readonly ISeatService _seatService;
        private readonly IUserService _userService;
        private readonly ITakenSeatLogService _takenSeatLogService;
        private readonly ApplicationDbContext _db;


        public TakenSeatController(ITakenSeatService takenSeatService,ISeatService seatService, IUserService userService, ITakenSeatLogService takenSeatLogService, ApplicationDbContext db)
        {
            _takenSeatService = takenSeatService;
            _seatService = seatService;
            _userService = userService;
            _takenSeatLogService = takenSeatLogService;
            _db = db;

        }

        public IActionResult Index()
        {
            return View(_takenSeatService.GetAll());
        }

        public IActionResult Details(Guid? id)
        {
            if (id == null) return NotFound();

            var takenSeat = _takenSeatService.GetById(id.Value);
            if (takenSeat == null) return NotFound();

            return View(takenSeat);
        }


        [HttpGet]
        [Authorize(Roles = "Keeper")]
        public IActionResult Create(Guid SeatId)
        {
            if (SeatId == Guid.Empty) return BadRequest("SeatId is required.");

            var seat = _db.Seats
                .AsNoTracking()
                .Include(s => s.Classroom)
                .FirstOrDefault(s => s.Id == SeatId);

            if (seat == null) return NotFound();

            var takenSeat = new TakenSeat { SeatId = SeatId, StartedAt = DateTime.Now };

            ViewBag.SeatNumber = seat.Number;
            ViewBag.ClassroomName = seat.Classroom?.Name ?? "N/A";
            ViewBag.ClassroomLocation = seat.Classroom?.Location ?? "N/A";

            var users = _userService.GetAll()
                .Select(u => new { u.Id, FullName = $"{u.Name ?? ""} {u.Surname ?? ""}".Trim() })
                .ToList();
            ViewBag.Users = new SelectList(users, "Id", "FullName");

            return View(takenSeat);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Keeper")]
        public IActionResult Create(TakenSeat model)
        {
            ModelState.Remove(nameof(TakenSeat.Seat));
            ModelState.Remove(nameof(TakenSeat.User));

            if (!ModelState.IsValid)
                return View(model);

            var seat = _seatService.GetById(model.SeatId);
            if (seat == null) { ModelState.AddModelError("", "Seat not found."); return View(model); }
            if (seat.IsTaken) { ModelState.AddModelError("", "This seat is already taken."); return View(model); }

            model.Id = Guid.NewGuid();
            model.StartedAt = DateTime.UtcNow;

            _takenSeatService.Insert(model);

            seat.IsTaken = true;
            seat.TakenSeatId = model.Id;
            _seatService.Update(seat);

            return RedirectToAction("Details", "Classroom", new { id = seat.ClassroomId });
        }



        private void RebuildViewBagsForCreate(Seat seat, Guid? selectedUserId)
        {
            ViewBag.SeatNumber = seat.Number;
            ViewBag.ClassroomName = seat.Classroom?.Name ?? "N/A";
            ViewBag.ClassroomLocation = seat.Classroom?.Location ?? "N/A";

            var users = _userService.GetAll()
                .Select(u => new { u.Id, FullName = $"{u.Name ?? ""} {u.Surname ?? ""}".Trim() })
                .ToList();
            ViewBag.Users = new SelectList(users, "Id", "FullName", selectedUserId);
        }


        [Authorize(Roles = "Keeper")]
        public IActionResult Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var takenSeat = _takenSeatService.GetById(id.Value);
            if (takenSeat == null) return NotFound();

            return View(takenSeat);
        }

        [Authorize(Roles = "Keeper")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, [Bind("Id,SeatId,UserId,StartedAt")] TakenSeat takenSeat)
        {
            if (id != takenSeat.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _takenSeatService.Update(takenSeat);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TakenSeatExists(takenSeat.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(takenSeat);
        }


        [Authorize(Roles = "Keeper")]
        public IActionResult Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var takenSeat = _takenSeatService.GetById(id.Value);
            if (takenSeat == null) return NotFound();

            return View(takenSeat);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Keeper")]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var taken = _takenSeatService.GetById(id);
            if (taken == null) return NotFound();

            if (taken.SeatId==Guid.Empty) return BadRequest("TakenSeat has no SeatId.");
            var seatId = taken.SeatId;                       

            var seat = _seatService.GetById(seatId);
            if (seat == null) return NotFound("Seat not found.");

            var classroomId = seat.ClassroomId;

            
            if (!taken.UserId.HasValue)
                throw new InvalidOperationException("TakenSeat has no UserId; cannot create log.");

            var log = new TakenSeatLog
            {
                Id = Guid.NewGuid(),
                SeatId = seatId,
                UserId = taken.UserId!.Value,                  
                StartedAt = taken.StartedAt,
                FreedAt = DateTime.UtcNow,
                Reason = "Taken-seat freed"
            };
            _takenSeatLogService.Insert(log);

            seat.IsTaken = false;
            seat.TakenSeatId = null;                              
            _seatService.Update(seat);

            _takenSeatService.DeleteById(id);

            TempData["Success"] = "Seat freed.";
            return RedirectToAction("Details", "Classroom", new { id = classroomId });
        }


        private bool TakenSeatExists(Guid id)
        {
            return _takenSeatService.GetById(id) != null;
        }
    }
}
