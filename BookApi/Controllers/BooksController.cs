using BookApi.Models;
using BookApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly ExternalBookService _bookService;

        public BooksController(ExternalBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Book>>> Get([FromQuery] string? categories = "fiction,fantasy,romance")
        {
            // Split comma-separated categories
            var categoryList = categories.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var books = await _bookService.GetBooksByCategoriesAsync(categoryList, 50); // 50 books per category
            return Ok(books);
        }
    }
}
