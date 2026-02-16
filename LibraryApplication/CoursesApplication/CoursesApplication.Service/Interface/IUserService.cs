using CoursesApplication.Domain.DomainModels;
using CoursesApplication.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesApplication.Service.Interface
{
    public interface IUserService
    {
        List<User> GetAll();
        User? GetById(Guid id);
        User Insert(User user);
        ICollection<User> InsertMany(ICollection<User> users);
        User Update(User book);
        User DeleteById(Guid id);
        void DeleteUserAndRelated(Guid userId);
    }
}
