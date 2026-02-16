using CoursesApplication.Domain.DomainModels;
using CoursesApplication.Repository.Data;        
using CoursesApplication.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


namespace CoursesApplication.Web.Controllers
{
   
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IAuthorService _authorService;
        private readonly IBookCopyService _bookCopyService;
        private readonly IBorrowingBookLogService _borrowingLogService;
        private readonly IBookBorrowingService _bookBorrowingService;
        private readonly ApplicationDbContext _db;   

        public BooksController(IBookService bookService, ApplicationDbContext db, IAuthorService authorService, IBookCopyService bookCopyService,IBookBorrowingService bookBorrowingService, IBorrowingBookLogService borrowingLogService)
        {
            _bookService = bookService;
            _authorService = authorService;
            _bookCopyService = bookCopyService;               
            _borrowingLogService = borrowingLogService;       
            _bookBorrowingService = bookBorrowingService;   
            _db = db;
        }

       
        private List<SelectListItem> BuildAuthorItems(IEnumerable<Guid>? selectedIds = null)
        {
            var selected = selectedIds != null ? new HashSet<Guid>(selectedIds) : new HashSet<Guid>();
            return _db.Set<Author>()                       
                      .Select(a => new SelectListItem
                      {
                          Value = a.Id.ToString(),
                          Text = a.Name + " " + a.Surname,
                          Selected = selected.Contains(a.Id)
                      })
                      .ToList();
        }

      
        private async Task<List<SelectListItem>> BuildCategoryOptionsAsync()
        {
            var cats = await _db.Set<Book>()
                .Where(b => b.Category != null)
                .Select(b => b.Category!.Value)
                .Distinct()
                .OrderBy(c => c.ToString())
                .ToListAsync();

            return cats
                .Select(c => new SelectListItem
                {
                    Value = c.ToString(),
                    Text = c.ToString().Replace('_', ' ')
                })
                .ToList();
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? category)
        {
            var query = _db.Set<Book>()
                .Include(b => b.Authors)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category) &&
                Enum.TryParse<BookCategory>(category, ignoreCase: true, out var catEnum))
            {
                query = query.Where(b => b.Category == catEnum);
                ViewBag.SelectedCategory = catEnum.ToString();
            }
            else
            {
                ViewBag.SelectedCategory = null;
            }

            ViewBag.CategoryOptions = await BuildCategoryOptionsAsync();

            var books = await query.ToListAsync();
            return View(books);
        }


        public IActionResult Details(Guid? id)
        {
            var book = _db.Books
        .AsNoTracking()
        .Where(b => b.Id == id)
        .Include(b => b.Authors)
        .Include(b => b.Copies!)                          
            .ThenInclude(c => c.CurrentBorrowing)
                .ThenInclude(bb => bb.User)
        .FirstOrDefault();

            if (book == null) return NotFound();
            return View(book);
        }


        [Authorize(Roles = "Librarian")]
        public IActionResult Create()
        {
            ViewBag.Authors = _authorService.GetAll()
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = $"{a.Name} {a.Surname}" })
                .ToList();
            ViewBag.SelectedAuthorIds = Enumerable.Empty<Guid>();
            return View(new Book());
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public IActionResult Create(Book book, List<Guid> selectedAuthorIds)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Authors = _authorService.GetAll()
                    .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = $"{a.Name} {a.Surname}" })
                    .ToList();
                ViewBag.SelectedAuthorIds = selectedAuthorIds;
                return View(book);
            }

            book.Authors = _authorService.GetAll()
                .Where(a => selectedAuthorIds.Contains(a.Id))
                .ToList();

            var copiesToCreate = book.NumberCopies;

            var createdBook = _bookService.Insert(book);
            for (int i = 0; i < copiesToCreate; i++)
            {
                var copy = new BookCopy
                {
                    Id = Guid.NewGuid(),
                    BookId = createdBook.Id,
                    InventoryCode = $"BC-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    
                };
                _bookCopyService.Insert(copy);
                Console.WriteLine($"{copy.Id}: {copy.InventoryCode} - Seat {copy.Book.Title}");

            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id is null) return NotFound();

            var book = await _db.Set<Book>()
                                .Include(b => b.Authors)
                                .FirstOrDefaultAsync(b => b.Id == id.Value);
            if (book is null) return NotFound();

            ViewBag.Authors = BuildAuthorItems(book.Authors.Select(a => a.Id));
            return View(book);
        }

        

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public IActionResult Edit(
      Guid id,
      [Bind("Id,Title,Category,NumberCopies")] Book model,
      [FromForm] List<Guid> selectedAuthorIds)
        {
            if (id != model.Id) return NotFound();

            if (selectedAuthorIds == null || selectedAuthorIds.Count == 0)
            {
                ModelState.AddModelError("Authors", "Select at least one author.");
                ViewBag.Authors = BuildAuthorItems(selectedAuthorIds);
                return View(model);
            }

            try
            {
                _bookService.Edit(model, selectedAuthorIds);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }


        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id is null) return NotFound();

            var book = await _db.Set<Book>()
                                .Include(b => b.Authors)
                                .FirstOrDefaultAsync(b => b.Id == id.Value);
            if (book is null) return NotFound();

            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var book = _bookService.GetByIdWithCopies(id); 
            if (book == null)
                return NotFound();

            foreach (var copy in book.Copies ?? Enumerable.Empty<BookCopy>())
            {
                if (copy.BorrowingLogs != null)
                {
                    foreach (var log in copy.BorrowingLogs.ToList())
                    {
                        _borrowingLogService.DeleteById(log.Id);
                    }
                }

                if (copy.CurrentBorrowing != null && copy.BookBorrowingId.HasValue)
                {
                   
                        _bookBorrowingService.DeleteById(copy.BookBorrowingId.Value);
                    
                }

                _bookCopyService.DeleteById(copy.Id);
            }

            _bookService.DeleteById(id);

            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(Guid id) => _bookService.GetById(id) != null;
    }
}
