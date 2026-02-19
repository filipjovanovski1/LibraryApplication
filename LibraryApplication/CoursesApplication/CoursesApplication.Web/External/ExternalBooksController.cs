// Controllers/ExternalBooksController.cs
using CoursesApplication.Web.Services;
using Microsoft.AspNetCore.Mvc;
using CoursesApplication.Web.External;

namespace CoursesApplication.Web.Controllers
{
    public class ExternalBooksController : Controller
    {
        private readonly IBookImportService _import;
        public ExternalBooksController(IBookImportService import) => _import = import;

        // POST /ExternalBooks/Import
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(string? categories, CancellationToken ct)
        {
            var catList = string.IsNullOrWhiteSpace(categories)
                ? null
                : categories.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            try
            {
                var changed = await _import.ImportAsync(catList, ct);
                TempData["Message"] = $"Imported/updated {changed} record(s).";
            }
            catch (ExternalBookApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            TempData["Categories"] = categories;              // keep last used value
            return RedirectToAction("Index", "Books", new { categories }); // repopulate on next render
        }

    }
}
