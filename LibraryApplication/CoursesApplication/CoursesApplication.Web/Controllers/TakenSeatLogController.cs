using CoursesApplication.Domain.DomainModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace CoursesApplication.Web.Controllers
{
    public class TakenSeatLogsController : Controller
    {
        private readonly ITakenSeatLogService _takenSeatLogService;

        public TakenSeatLogsController(ITakenSeatLogService takenSeatLogService)
        {
            _takenSeatLogService = takenSeatLogService;
        }


        public IActionResult Index()
        {
            return View(_takenSeatLogService.GetAll());
        }

       
        public IActionResult Details(Guid? id)
        {
            if (id == null) return NotFound();

            var log = _takenSeatLogService.GetById(id.Value);
            if (log == null) return NotFound();

            return View(log);
        }

        [Authorize(Roles = "Keeper")]
        public IActionResult Create()
        {
            return View();
        }
        [Authorize(Roles = "Keeper")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,SeatId,UserId,StartedAt,FreedAt,Reason")] TakenSeatLog log)
        {
            if (ModelState.IsValid)
            {
                _takenSeatLogService.Insert(log);
                return RedirectToAction(nameof(Index));
            }
            return View(log);
        }

        [Authorize(Roles = "Keeper")]
        public IActionResult Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var log = _takenSeatLogService.GetById(id.Value);
            if (log == null) return NotFound();

            return View(log);
        }

        [Authorize(Roles = "Keeper")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, [Bind("Id,SeatId,UserId,StartedAt,FreedAt,Reason")] TakenSeatLog log)
        {
            if (id != log.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _takenSeatLogService.Update(log);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TakenSeatLogExists(log.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(log);
        }

        [Authorize(Roles = "Keeper")]
        public IActionResult Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var log = _takenSeatLogService.GetById(id.Value);
            if (log == null) return NotFound();

            return View(log);
        }

        [Authorize(Roles = "Keeper")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _takenSeatLogService.DeleteById(id);
            return RedirectToAction(nameof(Index));
        }

        private bool TakenSeatLogExists(Guid id)
        {
            return _takenSeatLogService.GetById(id) != null;
        }


        
        [HttpGet]
        public IActionResult User(Guid userId)
        {
            var logs = _takenSeatLogService.GetAllByUserId(userId);
            return View("Index", logs); 
        }

        
        [HttpGet]
        public IActionResult Seat(Guid seatId)
        {
            var logs = _takenSeatLogService.GetAllBySeatId(seatId);
            return View("Index", logs); 
        }

    }
}
