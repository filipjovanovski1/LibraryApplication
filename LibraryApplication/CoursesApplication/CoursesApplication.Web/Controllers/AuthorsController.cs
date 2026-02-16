using CoursesApplication.Domain.DomainModels;
using CoursesApplication.Repository.Data;
using CoursesApplication.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CoursesApplication.Web.Controllers
{
   
    public class AuthorsController : Controller
    {
        private readonly IAuthorService _authorService;
        private readonly ApplicationDbContext _db;

        public AuthorsController(IAuthorService authorService, ApplicationDbContext db)
        {
            _authorService = authorService;
            _db = db;
        }
        public IActionResult Index()
        {
            var authors = _authorService.GetAll();
            return View(authors);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var author = await _db.Authors
         .Include(a => a.Books)               
         .AsNoTracking()
         .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null) return NotFound();
            return View(author);
        }

       
        [Authorize(Roles = "Librarian")]
        public IActionResult Create() => View();

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public IActionResult Create([Bind("Name,Surname")] Author author)
        {
            if (!ModelState.IsValid) return View(author);

            _authorService.Insert(author);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Librarian")]
        public IActionResult Edit(Guid id)
        {
            var author = _authorService.GetById(id);
            if (author == null) return NotFound();
            return View(author);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public IActionResult Edit(Guid id, [Bind("Id,Name,Surname")] Author author)
        {
            if (id != author.Id) return BadRequest();
            if (!ModelState.IsValid) return View(author);

            _authorService.Update(author);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Librarian")]
        public IActionResult Delete(Guid id)
        {
            var author = _authorService.GetById(id);
            if (author == null) return NotFound();
            return View(author);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _authorService.DeleteById(id);
            return RedirectToAction(nameof(Index));
        }
      

    }
}
