using CoursesApplication.Domain.DomainModels;
using CoursesApplication.Domain.Identity;
using CoursesApplication.Repository.Interface;
using CoursesApplication.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesApplication.Web.Controllers
{
    
    public class BookBorrowingController : Controller
    {
        private readonly IBookBorrowingService _bookBorrowingService;
        private readonly IBookCopyService _bookCopyService;
        private readonly IUserService _userService;

        public BookBorrowingController(IBookBorrowingService bookBorrowingService, IBookCopyService bookCopyService, IUserService userService)
        {
            _bookBorrowingService = bookBorrowingService;
            _bookCopyService = bookCopyService;
            _userService = userService;
        }
       
        public IActionResult Index()
        {
            return View(_bookBorrowingService.GetAll());
        }
        
        public IActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookBorrowing = _bookBorrowingService.GetById(id.Value);
            if (bookBorrowing == null)
            {
                return NotFound();
            }

            return View(bookBorrowing);
        }

      
        [HttpGet]
        [Authorize(Roles = "Librarian")]
        public IActionResult Create(Guid bookCopyId)
        {
            if (bookCopyId == Guid.Empty) return BadRequest();

            var copy = _bookCopyService.GetById(bookCopyId);
            if (copy == null) return NotFound();
            if (copy.BookBorrowingId != null)
            {
                TempData["Error"] = "This copy is already borrowed.";
                return RedirectToAction("Details", "BookCopy", new { id = bookCopyId });
            }

            ViewBag.InventoryCode = copy.InventoryCode;
            ViewBag.Users = _userService.GetAll()
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = $"{u.Name} {u.Surname}" })
                .ToList();

            return View(new BookBorrowing
            {
                BookCopyId = bookCopyId,
                BorrowedAt = DateTime.Today,
                DueDate = DateTime.Today.AddDays(14)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public IActionResult Create([Bind("Id,UserId,BookCopyId,BorrowedAt,DueDate")] BookBorrowing model)
        {
            if (model.UserId == Guid.Empty)
                ModelState.AddModelError(nameof(model.UserId), "Please select a user.");

            if (!ModelState.IsValid)
            {
                ViewBag.Users = _userService.GetAll()
                    .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = $"{u.Name} {u.Surname}" })
                    .ToList();
                ViewBag.InventoryCode = _bookCopyService.GetById(model.BookCopyId)?.InventoryCode;
                return View(model);
            }

            if (model.Id == Guid.Empty)
                model.Id = Guid.NewGuid();

            _bookBorrowingService.Insert(model);

            var copy = _bookCopyService.GetById(model.BookCopyId);
            if (copy != null)
            {
                copy.BookBorrowingId = model.Id;    
                _bookCopyService.Update(copy);
            }

            return RedirectToAction("Details", "BookCopy", new { id = model.BookCopyId });
        }


 
        [Authorize(Roles = "Librarian")]
        public IActionResult Edit(Guid id)

        {
            var borrowing = _bookBorrowingService.GetById(id); 
            if (borrowing == null) return NotFound();

           
            var copy = _bookCopyService.GetById(borrowing.BookCopyId);
            ViewBag.InventoryCode = copy?.InventoryCode;
            ViewBag.BookId = copy?.BookId ?? Guid.Empty;

            ViewBag.Users = _userService.GetAll()
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = $"{u.Name} {u.Surname}" })
                .ToList();

            return View(borrowing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public IActionResult Edit(Guid id, [Bind("Id,UserId,BorrowedAt,DueDate")] BookBorrowing model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
               
                var current = _bookBorrowingService.GetById(id);
                var copy = current != null ? _bookCopyService.GetById(current.BookCopyId) : null;
                ViewBag.InventoryCode = copy?.InventoryCode;
                ViewBag.BookId = copy?.BookId ?? Guid.Empty;
                ViewBag.Users = _userService.GetAll()
                    .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = $"{u.Name} {u.Surname}" })
                    .ToList();
                return View(model);
            }

            var borrowing = _bookBorrowingService.GetById(id);
            if (borrowing == null) return NotFound();

            borrowing.UserId = model.UserId;
            borrowing.BorrowedAt = model.BorrowedAt;
            borrowing.DueDate = model.DueDate;

            _bookBorrowingService.Update(borrowing);

            var copyAfter = _bookCopyService.GetById(borrowing.BookCopyId);
            return RedirectToAction("Details", "Books", new { id = copyAfter?.BookId });
        }


        [Authorize(Roles = "Librarian")]
        public IActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookBorrowing = _bookBorrowingService.GetById(id.Value);
            if (bookBorrowing == null)
            {
                return NotFound();
            }

            return View(bookBorrowing);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public IActionResult DeleteConfirmed(Guid bookCopyId)
        {
            return RedirectToAction("Create", "BorrowingBookLogs", new { bookCopyId });
        }
        private bool BookBorrowingExists(Guid id)
        {
            return _bookBorrowingService.GetById(id) != null;
        }

        
        public IActionResult ByUser(Guid? userId)
        {
            if (userId == null)
            {
                return NotFound();
            }

            var borrowings = _bookBorrowingService.GetBorrowingsByUser(userId.Value);

            if (borrowings == null || !borrowings.Any())
            {
                return NotFound();
            }

            return View(borrowings);
        }

    }


}
