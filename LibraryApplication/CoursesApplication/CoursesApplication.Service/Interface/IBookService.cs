using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesApplication.Domain.DomainModels
{
    public interface IBookService
    {
        List<Book> GetAll();
        Book? GetById(Guid id);
        Book Insert(Book book);
        ICollection<Book> InsertMany(ICollection<Book> books);
        Book Update(Book book);
        Book DeleteById(Guid id);
        public Book? GetByIdWithCopies(Guid id);
        public void Edit(Book model, List<Guid> authorIds);
       void IncrementNumberCopies(Guid bookId);
       
        void DecrementNumberCopies(Guid bookId);



    }
}
