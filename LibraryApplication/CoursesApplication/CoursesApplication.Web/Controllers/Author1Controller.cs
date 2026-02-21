using Microsoft.AspNetCore.Mvc;
using System;
using CoursesApplication.Service.Interface;
using Microsoft.AspNetCore.Authorization;

namespace CoursesApplication.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Librarian")]
    public class Authors1Controller : ControllerBase
    {
        private readonly IAuthorService _authors;

        public Authors1Controller(IAuthorService authors)
        {
            _authors = authors;
        }

        // DELETE: api/Authors1/{id}
        [HttpDelete("{id:guid}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAuthor(Guid id)
        {
            var author = _authors.GetById(id);
            if (author == null)
            {
                return NotFound();
            }

            _authors.DeleteById(id);
            return NoContent();
        }
    }
}