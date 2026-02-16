using CoursesApplication.Repository.Data;
using CoursesApplication.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesApplication.Domain.DomainModels
{
    public class BookCopyService : IBookCopyService
    {
        private readonly IRepository<BookCopy> _bookCopyRepository;
        private readonly ApplicationDbContext _context;
        public BookCopyService(IRepository<BookCopy> bookCopyRepository, ApplicationDbContext context)
        {
            _bookCopyRepository = bookCopyRepository;
            _context = context;
        }

        public BookCopy DeleteById(Guid id)
        {
            var entity = GetById(id);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Delete: BookCopy with id {id} not found.");
            }

            return _bookCopyRepository.Delete(entity);
        }

        public List<BookCopy> GetAll()
        {
            return _bookCopyRepository.GetAll(selector: x => x).ToList();
        }

        public BookCopy? GetById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("GetById BookCopy: Invalid ID.", nameof(id));

            return _bookCopyRepository.GetById(id);
        }

        public BookCopy Insert(BookCopy bookCopy)
        {
            if (bookCopy == null)
                throw new ArgumentNullException(nameof(bookCopy));

            _context.BookCopies.Add(bookCopy); // ← This is the key
            _context.SaveChanges();            // Persist to DB
            return bookCopy;
        }


        public ICollection<BookCopy> InsertMany(ICollection<BookCopy> bookCopies)
        {
            return _bookCopyRepository.InsertMany(bookCopies);
        }

        public BookCopy Update(BookCopy bookCopy)
        {
            return _bookCopyRepository.Update(bookCopy);
        }
        public IEnumerable<BookCopy> GetAllByBookId(Guid bookId) =>
       _bookCopyRepository.GetAll(
           selector: bc => bc,
           predicate: bc => bc.BookId == bookId,
           include: q => q.Include(bc => bc.Book) // optional, if you need Book data
       );
    }
}
