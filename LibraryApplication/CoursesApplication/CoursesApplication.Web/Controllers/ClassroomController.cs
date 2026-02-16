using CoursesApplication.Domain.DomainModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace CoursesApplication.Web.Controllers
{
    public class ClassroomController : Controller
    {
        private readonly IClassroomService _classroomService;

        public ClassroomController(IClassroomService classroomService)
        {
            _classroomService = classroomService;
        }

        public IActionResult Index()
        {
            return View(_classroomService.GetAll());
        }

        
        public IActionResult Details(Guid? id)
        {
            if (id == null) return NotFound();

            var classroom = _classroomService.GetByIdWithSeats(id.Value);
            if (classroom == null) return NotFound();

            return View(classroom);
        }


        [Authorize(Roles = "Keeper")]
        public IActionResult Create()
        {
            return View();
        }

     
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Keeper")]
        public IActionResult Create([Bind("Id,Name,Location,Capacity")] Classroom classroom)
        {
            if (ModelState.IsValid)
            {
                _classroomService.Insert(classroom);
                return RedirectToAction(nameof(Index));
            }
            return View(classroom);
        }

        [Authorize(Roles = "Keeper")]
        public IActionResult Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var classroom = _classroomService.GetById(id.Value);
            if (classroom == null) return NotFound();

            return View(classroom);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Keeper")]
        public IActionResult Edit(Guid id, [Bind("Id,Name,Location,Capacity")] Classroom classroom)
        {
            var existing = _classroomService.GetByIdWithSeats(id); 
            if (existing == null) return NotFound();

            bool capacityChanged = classroom.Capacity != existing.Capacity;
            bool anyTaken = existing.Seats.Any(s => s.IsTaken);

            if (classroom.Capacity<existing.Capacity && anyTaken)
            {
                ModelState.AddModelError(string.Empty, "You cannot edit unless all seats are free.");
                ViewBag.BlockedByTakenSeats = true;  
                ViewBag.NewSeats = classroom.Capacity;


                existing.Name = classroom.Name;
                existing.Location = classroom.Location;
                existing.Capacity = classroom.Capacity; 

                return View(existing);
            }

 
            existing.Name = classroom.Name;
            existing.Location = classroom.Location;
            if (capacityChanged)
            {
                _classroomService.SyncCapacity(existing, classroom.Capacity);
            }
            else
            {
                existing.Capacity = classroom.Capacity;
            }

            _classroomService.Update(existing);
            TempData["Success"] = "Classroom updated.";
            return RedirectToAction(nameof(Details), new { id });


        }

        [Authorize(Roles = "Keeper")]
        public IActionResult Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var classroom = _classroomService.GetById(id.Value);
            if (classroom == null) return NotFound();

            return View(classroom);
        }

       
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Keeper")]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _classroomService.FreeAllSeats(id);

            _classroomService.DeleteById(id);

            TempData["Success"] = "Classroom and all related data were deleted.";
            return RedirectToAction(nameof(Index));
        }

        private bool ClassroomExists(Guid id)
        {
            return _classroomService.GetById(id) != null;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Keeper")]
        public IActionResult FreeAllSeats(Guid id, int? newCapacity, string? returnAction = "Edit")
        {
           

            if (newCapacity.HasValue)
            {
                _classroomService.FreeAllSeatsInRange(id,newCapacity.Value);
            }

            return RedirectToAction(returnAction ?? "Edit", new { id });
        }


    }
}
