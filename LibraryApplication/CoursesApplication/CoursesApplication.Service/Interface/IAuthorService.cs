using CoursesApplication.Domain.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesApplication.Service.Interface
{
    public interface IAuthorService
    {
        List<Author> GetAll();
        Author? GetById(Guid id);
        Author Insert(Author author);
        ICollection<Author> InsertMany(ICollection<Author> authors);
        Author Update(Author author);
        Author DeleteById(Guid id);
    }
}
