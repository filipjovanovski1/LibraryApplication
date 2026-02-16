using CoursesApplication.Domain.DomainModels;
using CoursesApplication.Repository.Data;
using CoursesApplication.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace CoursesApplication.Web.Controllers
{
    public class BookCopyController : Controller
    {
        private readonly IBookCopyService _bookCopyService;
        private readonly IBookService _bookService;
        private readonly IBookBorrowingService _bookBorrowingService;
        private readonly IBorrowingBookLogService _borrowingLogService;
        private readonly ApplicationDbContext _db;

        public BookCopyController(IBookCopyService bookCopyService, IBookService bookService, IBookBorrowingService bookBorrowingService, IBorrowingBookLogService borrowingLogService, ApplicationDbContext db)
        {
            _bookCopyService = bookCopyService;
            _bookService = bookService;
            _bookBorrowingService = bookBorrowingService;
            _borrowingLogService = borrowingLogService;
            _db = db;
        }

        public IActionResult Index()
        {
            var copies = _db.BookCopies
        .AsNoTracking()
        .Include(c => c.Book)
        .Include(c => c.CurrentBorrowing)
            .ThenInclude(bb => bb.User)
        .ToList();

            return View(copies);
        }

       
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var bookCopy = await _db.BookCopies
                .AsNoTracking()
                .AsSplitQuery() // optional
                .Include(c => c.Book)
                .Include(c => c.CurrentBorrowing).ThenInclude(bb => bb.User)
                .Include(c => c.BorrowingLogs).ThenInclude(l => l.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (bookCopy == null) return NotFound();
            return View(bookCopy);
        }



        [Authorize(Roles = "Librarian")]
        public IActionResult Create(Guid bookId)
        {
            var book = _bookService.GetById(bookId);
            if (book == null) return NotFound();

            var model = new BookCopy { BookId = bookId };
            return View(model);
        }


        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public IActionResult Create([Bind("Id,BookId")] BookCopy bookCopy)
        {
            if (bookCopy.Id == Guid.Empty) bookCopy.Id = Guid.NewGuid();
            bookCopy.BookBorrowingId = null; 

            bookCopy.InventoryCode = $"BC-{bookCopy.Id.ToString("N")[..8].ToUpper()}";

            if (!ModelState.IsValid)
                return View(bookCopy);

            _bookCopyService.Insert(bookCopy);

            _bookService.IncrementNumberCopies(bookCopy.BookId);

            return RedirectToAction("Details", "Books", new { id = bookCopy.BookId });
        }

    
        [HttpGet]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == null) return NotFound();

            var bookCopy = await _db.BookCopies
            .AsNoTracking()
            .Include(c => c.Book)           
            .FirstOrDefaultAsync(c => c.Id == id);

            if (bookCopy == null) return NotFound();

            return View(bookCopy);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public IActionResult CreateInline(Guid bookId)
        {
            if (bookId == Guid.Empty) return BadRequest();

            var id = Guid.NewGuid();

            var copy = new BookCopy
            {
                Id = id,
                BookId = bookId,
                BookBorrowingId = null,
                InventoryCode = $"BC-{id.ToString("N")[..8].ToUpper()}"
            };

            _bookCopyService.Insert(copy);
            _bookService.IncrementNumberCopies(bookId);

            return RedirectToAction("Details", "Books", new { id = bookId });
        }


      
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public IActionResult Edit(Guid id, [Bind("Id,BookId,InventoryCode,BookBorrowingId")] BookCopy bookCopy)
        {
            if (id != bookCopy.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _bookCopyService.Update(bookCopy);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookCopyExists(bookCopy.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(bookCopy);
        }

        [Authorize(Roles = "Librarian")]
        public IActionResult Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var bookCopy = _bookCopyService.GetById(id.Value);
            if (bookCopy == null) return NotFound();

            return View(bookCopy);
        }

        


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public IActionResult DeleteConfirmed(Guid id)
        {
           
            var copy = _bookCopyService.GetById(id);
            if (copy == null) return NotFound();

            
            var current = _bookBorrowingService.GetActiveByBookCopyId(copy.Id);
            if (current != null)
            {
                copy.BookBorrowingId = null;            
                _bookCopyService.Update(copy);         

                _bookBorrowingService.DeleteById(current.Id);
            }

            foreach (var log in _borrowingLogService.GetAllByBookCopyId(copy.Id).ToList())
                _borrowingLogService.DeleteById(log.Id);

            _bookCopyService.DeleteById(copy.Id);

            _bookService.DecrementNumberCopies(copy.BookId);

            return RedirectToAction("Details", "Books", new { id = copy.BookId });
        }




        private bool BookCopyExists(Guid id)
        {
            return _bookCopyService.GetById(id) != null;
        }
      
        [HttpGet]
        public IActionResult ByBook(Guid bookId)
        {
            var copies = _bookCopyService.GetAllByBookId(bookId);
            return View("Index", copies); 
        }

    }
}
