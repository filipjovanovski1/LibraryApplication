using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesApplication.Domain.DomainModels
{
    public interface IBookCopyService
    {

        List<BookCopy> GetAll();
        BookCopy? GetById(Guid id);
        BookCopy Insert(BookCopy bookCopy);
        ICollection<BookCopy> InsertMany(ICollection<BookCopy> bookCopies);
        BookCopy Update(BookCopy bookCopy);
        BookCopy DeleteById(Guid id);
        IEnumerable<BookCopy> GetAllByBookId(Guid bookId);
    }
}
