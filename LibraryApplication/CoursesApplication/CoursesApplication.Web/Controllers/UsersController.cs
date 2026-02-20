using CoursesApplication.Domain.DomainModels;
using CoursesApplication.Domain.Identity;
using CoursesApplication.Repository.Data;
using CoursesApplication.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using AppUser = CoursesApplication.Domain.Identity.User;

namespace CoursesApplication.Web.Controllers
{

    [Authorize(Roles="Librarian,Keeper")]

    public class UsersController : Controller
    {
        private readonly IUserService _users;
        private readonly IBookBorrowingService _bookBorrowingService;
        private readonly IBorrowingBookLogService _borrowingLogService;
        private readonly ITakenSeatLogService _takenSeatLogService;
        private readonly ITakenSeatService _takenSeatService;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ApplicationDbContext _db;
        public UsersController(
       IUserService users,
       UserManager<User> userManager,
       RoleManager<Role> roleManager, IBookBorrowingService bookBorrowingService,
        IBorrowingBookLogService borrowingLogService,
        ITakenSeatLogService takenSeatLogService,
        ITakenSeatService takenSeatService, ApplicationDbContext db)
        {
            _users = users;
            _userManager = userManager;
            _roleManager = roleManager;
            _bookBorrowingService = bookBorrowingService;
            _borrowingLogService = borrowingLogService;
            _takenSeatLogService = takenSeatLogService;
            _takenSeatService = takenSeatService;
            _db = db;
        }

       public async Task<IActionResult> Index()
        {
            var users = await _db.Users
                .Include(u => u.Role)              
                .ToListAsync();

            return View(users);
        }


        public async Task<IActionResult> Details(Guid id)
        {
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            TakenSeat? currentSeat = null;

            if (user.TakenSeatId.HasValue)
            {
                currentSeat = await _db.TakenSeats
                    .Include(ts => ts.Seat).ThenInclude(s => s.Classroom)
                    .Include(ts => ts.User)
                    .FirstOrDefaultAsync(ts => ts.Id == user.TakenSeatId.Value);
            }
            else
            {
                currentSeat = await _db.TakenSeats
                    .Include(ts => ts.Seat).ThenInclude(s => s.Classroom)
                    .Include(ts => ts.User)
                    .FirstOrDefaultAsync(ts => ts.UserId == user.Id);
            }

            ViewBag.CurrentSeat = currentSeat;

            ViewBag.Borrowings = _bookBorrowingService.GetAllForUserWithBook(id).ToList();
            ViewBag.BorrowingLogs = _borrowingLogService.GetAllByUserId(id).ToList();
            ViewBag.TakenSeatLogs = _takenSeatLogService.GetAllByUserId(id).ToList();

            return View(user);
        }

   
        [Authorize(Roles = "Librarian")]
        [HttpGet]
        public IActionResult Create()
        {
            LoadRoleList();
            return View(new User());
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Create(
    [Bind("Name,Surname")] CoursesApplication.Domain.Identity.User posted,
    string? selectedRole)
        {
            ModelState.Remove("Role");

            string baseSlug = $"{posted.Name}.{posted.Surname}".Trim();
            var safe = baseSlug.ToLowerInvariant()
                               .Where(c => char.IsLetterOrDigit(c) || c is '.' or '-' or '_' or '@' or '+')
                               .ToArray();
            var slug = new string(safe);
            if (string.IsNullOrWhiteSpace(slug)) slug = Guid.NewGuid().ToString("N");

            string userName = slug;
            for (int i = 1; await _userManager.FindByNameAsync(userName) != null; i++)
                userName = $"{slug}.{i}";

            var user = new CoursesApplication.Domain.Identity.User
            {
                Id = Guid.NewGuid(),
                Name = posted.Name?.Trim() ?? string.Empty,
                Surname = posted.Surname?.Trim() ?? string.Empty,
                UserName = userName,
                Email = null 
            };

         
            var createRes = await _userManager.CreateAsync(user);
            if (!createRes.Succeeded)
            {
                foreach (var e in createRes.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);

                LoadRoleList(selectedRole);
                return View(posted);
            }

            
            if (!string.IsNullOrWhiteSpace(selectedRole))
            {
                var roleEntity = await _roleManager.FindByNameAsync(selectedRole);
                if (roleEntity == null)
                {
                    ModelState.AddModelError(string.Empty, $"Role '{selectedRole}' does not exist.");
                    LoadRoleList(selectedRole);
                    return View(posted);
                }

                var roleRes = await _userManager.AddToRoleAsync(user, selectedRole);
                if (!roleRes.Succeeded)
                {
                    foreach (var e in roleRes.Errors)
                        ModelState.AddModelError(string.Empty, e.Description);

                    LoadRoleList(selectedRole);
                    return View(posted);
                }

               
                user.RoleId = roleEntity.Id;
                var updateRes = await _userManager.UpdateAsync(user); 
                if (!updateRes.Succeeded)
                {
                    foreach (var e in updateRes.Errors)
                        ModelState.AddModelError(string.Empty, e.Description);

                    LoadRoleList(selectedRole);
                    return View(posted);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var user = _users.GetById(id);
            if (user == null) return NotFound();

            var currentRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            LoadRoleList(currentRole);
            return View(user);
        }

       
        [Authorize(Roles = "Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, User model, string? selectedRole)
        {
            if (id != model.Id) return NotFound();

            ModelState.Remove("Role"); 

            if (string.IsNullOrWhiteSpace(selectedRole))
            {
                ModelState.AddModelError("selectedRole", "Role is required.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateRolesSelectListAsync(model, selectedRole);
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            user.Name = model.Name?.Trim();
            user.Surname = model.Surname?.Trim();

            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                foreach (var e in update.Errors) ModelState.AddModelError("", e.Description);
                await PopulateRolesSelectListAsync(user, selectedRole);
                return View(user);
            }

            var current = await _userManager.GetRolesAsync(user);
            if (current.Any()) await _userManager.RemoveFromRolesAsync(user, current);
            if (!string.IsNullOrWhiteSpace(selectedRole))
                await _userManager.AddToRoleAsync(user, selectedRole);

            var roleEntity = await _roleManager.FindByNameAsync(selectedRole!);
            user.RoleId = roleEntity?.Id;
            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateRolesSelectListAsync(CoursesApplication.Domain.Identity.User user, string? selectedRole)
        {
            var allRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
            var userRoles = await _userManager.GetRolesAsync(user);
            var current = selectedRole ?? userRoles.FirstOrDefault();

            ViewBag.Roles = allRoles
                .Select(r => new SelectListItem { Value = r, Text = r, Selected = r == current })
                .ToList();
        }


        private void LoadRoleList(string? current = null)
        {
            ViewBag.Roles = _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name,
                    Selected = r.Name == current
                })
                .ToList();
        }


        [Authorize(Roles = "Librarian")]
        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var user = _users.GetById(id);
            if (user == null) return NotFound();
            return View(user);
        }

    
        [Authorize(Roles = "Librarian")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            try
            {
                _users.DeleteUserAndRelated(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var user = _users.GetById(id);
                if (user == null) return NotFound();
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Delete", user);
            }
        }


    }
}
