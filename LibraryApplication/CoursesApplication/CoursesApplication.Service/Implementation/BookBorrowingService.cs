using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoursesApplication.Repository.Interface;
using CoursesApplication.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace CoursesApplication.Domain.DomainModels
{
    public class BookBorrowingService : IBookBorrowingService
    {
        private readonly IRepository<BookBorrowing> _bookBorrowingRepository;

        public BookBorrowingService(IRepository<BookBorrowing> bookBorrowingRepository)
        {
            _bookBorrowingRepository = bookBorrowingRepository;
        }
        public List<BookBorrowing> GetAll()
        {
            return _bookBorrowingRepository.GetAll(selector: x => x).ToList();
        }

        public BookBorrowing? GetById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("GetById BookBorrowing: Invalid ID.", nameof(id));

            return _bookBorrowingRepository.GetById(id);
        }

        public BookBorrowing Insert(BookBorrowing bookBorrowing)
        {
            return _bookBorrowingRepository.Insert(bookBorrowing);
        }

        public ICollection<BookBorrowing> InsertMany(ICollection<BookBorrowing> bookBorrowings)
        {
            return _bookBorrowingRepository.InsertMany(bookBorrowings);
        }

        public BookBorrowing Update(BookBorrowing bookBorrowing)
        {
            return _bookBorrowingRepository.Update(bookBorrowing);
        }

        public BookBorrowing DeleteById(Guid id)
        {
            var entity = GetById(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Delete: BookBorrowing with id {id} not found.");

            }
            return _bookBorrowingRepository.Delete(entity);
        }

        public List<BookBorrowing> GetBorrowingsByUser(Guid userId)
        {
            return _bookBorrowingRepository
                .GetAll(bb => bb, predicate: bb => bb.UserId == userId)
                .ToList();
        }

        public List<BookBorrowing> GetBorrowingsByBookCopy(Guid bookCopyId)
        {
            return _bookBorrowingRepository
                .GetAll(bb => bb, predicate: bb => bb.BookCopyId == bookCopyId)
                .ToList();
        }

        public BookBorrowing? GetActiveByBookCopyId(Guid bookCopyId)
        {
            return _bookBorrowingRepository
                .GetAll(b => b, b => b.BookCopyId == bookCopyId)
                .FirstOrDefault();
        }
        public IEnumerable<BookBorrowing> GetAllForUserWithBook(Guid userId) =>
        _bookBorrowingRepository.GetAll(
            selector: b => b,
            predicate: b => b.UserId == userId,
            include: q => q
                .Include(b => b.BookCopy)
                .ThenInclude(c => c.Book)
        ).ToList();


    }

}
