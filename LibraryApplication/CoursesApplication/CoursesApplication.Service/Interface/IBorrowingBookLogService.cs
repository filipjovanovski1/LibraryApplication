using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoursesApplication.Domain.Identity;

namespace CoursesApplication.Domain.DomainModels
{
    public interface IBorrowingBookLogService 
    {

        List<BorrowingBookLog> GetAll();
        BorrowingBookLog? GetById(Guid id);
        BorrowingBookLog Insert(BorrowingBookLog borrowingBookLog);
        ICollection<BorrowingBookLog> InsertMany(ICollection<BorrowingBookLog> borrowingBookLogs);
        BorrowingBookLog Update(BorrowingBookLog borrowingBookLog);
        BorrowingBookLog DeleteById(Guid id);
        IEnumerable<BorrowingBookLog> GetAllByUserId(Guid userId);
        IEnumerable<BorrowingBookLog> GetAllByBookCopyId(Guid bookCopyId);

    }
}
