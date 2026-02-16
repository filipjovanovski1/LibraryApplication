using CoursesApplication.Domain.Identity;
using CoursesApplication.Repository.Interface;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesApplication.Repository.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _um;
        public UserRepository(UserManager<User> um) => _um = um;

        public IQueryable<User> Query() => _um.Users;

        public Task<User?> GetByIdAsync(Guid id) =>
            _um.FindByIdAsync(id.ToString());

        public Task<IdentityResult> CreateAsync(User user) =>
            _um.CreateAsync(user);                           

        public Task<IdentityResult> UpdateAsync(User user) =>
            _um.UpdateAsync(user);

        public Task<IdentityResult> DeleteAsync(User user) =>
            _um.DeleteAsync(user);
    }
}
