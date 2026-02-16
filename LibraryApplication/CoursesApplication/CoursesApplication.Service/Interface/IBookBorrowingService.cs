using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoursesApplication.Domain.Identity;

namespace CoursesApplication.Domain.DomainModels
{
    public interface IBookBorrowingService 
    {
        List<BookBorrowing> GetAll();
        BookBorrowing? GetById(Guid id);
        BookBorrowing Insert(BookBorrowing bookBorrowing);
        ICollection<BookBorrowing> InsertMany(ICollection<BookBorrowing> bookBorrowings);
        BookBorrowing Update(BookBorrowing bookBorrowing);
        BookBorrowing DeleteById(Guid id);
        List<BookBorrowing> GetBorrowingsByUser(Guid userId);
        public BookBorrowing? GetActiveByBookCopyId(Guid bookCopyId);
        public IEnumerable<BookBorrowing> GetAllForUserWithBook(Guid userId);



    }
    

}
