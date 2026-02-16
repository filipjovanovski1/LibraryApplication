using CoursesApplication.Domain.DomainModels;
using CoursesApplication.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class BorrowingBookLogsController : Controller
{
    private readonly IRepository<BookCopy> _copyRepo;
    private readonly IRepository<BookBorrowing> _borrowingRepo;
    private readonly IRepository<BorrowingBookLog> _logRepo;

    public BorrowingBookLogsController(
        IRepository<BookCopy> copyRepo,
        IRepository<BookBorrowing> borrowingRepo,
        IRepository<BorrowingBookLog> logRepo)
    {
        _copyRepo = copyRepo;
        _borrowingRepo = borrowingRepo;
        _logRepo = logRepo;
    }


    [Authorize(Roles = "Librarian")]
    public IActionResult Create(Guid bookCopyId)
    {
        var copy = _copyRepo.GetAll(
            selector: c => c,
            predicate: c => c.Id == bookCopyId,
            include: q => q
                .Include(c => c.Book)
                .Include(c => c.CurrentBorrowing!)
                    .ThenInclude(b => b.User)
        ).FirstOrDefault();

        if (copy == null) return NotFound();

        if (copy.CurrentBorrowing == null)
        {
            return RedirectToAction("Details", "BookCopies", new { id = bookCopyId });
        }

        var b = copy.CurrentBorrowing;

        var log = new BorrowingBookLog
        {
            BookCopyId = copy.Id,
            UserId = b.UserId,
            BorrowedAt = b.BorrowedAt,
            DueDate = b.DueDate,
            ReturnedAt = DateTime.Today
        };

        ViewBag.UserFullName = $"{b.User?.Name} {b.User?.Surname}".Trim();
        ViewBag.BookId = copy.Book.Id;         
        ViewBag.InventoryCode = copy.InventoryCode;

        return View(log);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Librarian")]
    public IActionResult Create([Bind("BookCopyId,UserId,BorrowedAt,DueDate,ReturnedAt,Notes")] BorrowingBookLog log)
    {
        ModelState.Remove(nameof(BorrowingBookLog.BookCopy));
        ModelState.Remove(nameof(BorrowingBookLog.User));

        if (!ModelState.IsValid)
        {
            var copyForView = _copyRepo.GetAll(
                selector: c => c,
                predicate: c => c.Id == log.BookCopyId,
                include: q => q
                    .Include(c => c.Book)
                    .Include(c => c.CurrentBorrowing!)
                        .ThenInclude(b => b.User)
            ).FirstOrDefault();

            ViewBag.InventoryCode = copyForView?.InventoryCode;
            ViewBag.BookId = copyForView?.Book.Id ?? Guid.Empty;
            ViewBag.UserFullName = copyForView?.CurrentBorrowing != null
                ? $"{copyForView.CurrentBorrowing.User?.Name} {copyForView.CurrentBorrowing.User?.Surname}".Trim()
                : "";

            return View(log);
        }

        var copy = _copyRepo.GetAll(
            selector: c => c,
            predicate: c => c.Id == log.BookCopyId,
            include: q => q
                .Include(c => c.Book)
                .Include(c => c.CurrentBorrowing!)
        ).FirstOrDefault();

        if (copy == null) return NotFound();

        
        log.Id = Guid.NewGuid();
        _logRepo.Insert(log);   

        var current = copy.CurrentBorrowing;
        copy.BookBorrowingId = null;
        _copyRepo.Update(copy);

        if (current != null)
        {
            _borrowingRepo.Delete(current);
        }

        return RedirectToAction("Details", "Books", new { id = copy.Book.Id });
    }


}
