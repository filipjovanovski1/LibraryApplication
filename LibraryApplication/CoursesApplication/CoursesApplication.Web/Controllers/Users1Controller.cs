using System;
using CoursesApplication.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoursesApplication.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Librarian")]
    public class Users1Controller : ControllerBase
    {
        private readonly IUserService _users;

        public Users1Controller(IUserService users)
        {
            _users = users;
        }
        
        // DELETE: api/Users1/{id}
        [HttpDelete("{id:guid}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(Guid id)
        {
            var user = _users.GetById(id);
            if (user == null)
            {
                return NotFound();
            }

            _users.DeleteUserAndRelated(id);
            return NoContent();
        }
    }
}