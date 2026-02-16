using CoursesApplication.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesApplication.Domain.DomainModels
{
    public class BookService : IBookService
    {
        private readonly IRepository<Book> _bookRepository;
        private readonly IRepository<Author> _authorRepository;
        private readonly IRepository<BookCopy> _bookCopyRepository;
        private readonly IRepository<BookBorrowing> _bookBorrowingRepository;
        private readonly IRepository<BorrowingBookLog> _borrowingLogRepository;

        public BookService(
           IRepository<Book> bookRepository,
           IRepository<Author> authorRepository,
           IRepository<BookCopy> bookCopyRepository,
           IRepository<BookBorrowing> bookBorrowingRepository,
           IRepository<BorrowingBookLog> borrowingLogRepository)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _bookCopyRepository = bookCopyRepository;
            _bookBorrowingRepository = bookBorrowingRepository;
            _borrowingLogRepository = borrowingLogRepository;
        }

        public Book DeleteById(Guid id)
        {
            var entity = GetById(id);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Delete: Book with id {id} not found.");
            }

            return _bookRepository.Delete(entity);
        }

        public List<Book> GetAll()
        {
            return _bookRepository.GetAll(selector: x => x).ToList();
        }

        public Book? GetById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("GetById Book: Invalid ID.", nameof(id));

            return _bookRepository.GetById(id);
        }

        public Book Insert(Book book)
        {
            return _bookRepository.Insert(book);
        }

        public ICollection<Book> InsertMany(ICollection<Book> books)
        {
            return _bookRepository.InsertMany(books);
        }

        public Book Update(Book book)
        {
            return _bookRepository.Update(book);
        }
        public Book? GetByIdWithCopies(Guid id)
        {
            return _bookRepository.GetAll(
                selector: b => b,
                predicate: b => b.Id == id,
                include: q => q
                    .Include(b => b.Copies!)                
                        .ThenInclude(c => c.CurrentBorrowing)
                    .Include(b => b.Copies!)                
                        .ThenInclude(c => c.BorrowingLogs!)
            ).FirstOrDefault();
        }

        public void Edit(Book model, List<Guid> authorIds)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (model.Id == Guid.Empty) throw new ArgumentException("Invalid Book Id.", nameof(model));

          
            var book = _bookRepository.GetAll(
                selector: b => b,
                predicate: b => b.Id == model.Id,
                include: q => q
                    .Include(b => b.Authors)
                    .Include(b => b.Copies!)
                        .ThenInclude(c => c.CurrentBorrowing)
                    .Include(b => b.Copies!)
                        .ThenInclude(c => c.BorrowingLogs!)
            ).FirstOrDefault();

            if (book is null)
                throw new KeyNotFoundException($"Book {model.Id} not found.");

            
            book.Title = model.Title;
            book.Category = model.Category;

            
            var newAuthors = _authorRepository
                .GetAll(selector: a => a, predicate: a => authorIds.Contains(a.Id))
                .ToList();

            book.Authors.Clear();
            foreach (var a in newAuthors) book.Authors.Add(a);

           
            var oldCount = book.Copies?.Count ?? 0;
            var newCount = model.NumberCopies;
            book.NumberCopies = newCount;

            if (newCount != oldCount)
            {
                if (newCount > oldCount)
                {
                    var toAdd = newCount - oldCount;
                    for (int i = 0; i < toAdd; i++)
                    {
                        var copy = new BookCopy
                        {
                            Id = Guid.NewGuid(),
                            BookId = book.Id,
                            InventoryCode = $"BC-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
                        };
                        _bookCopyRepository.Insert(copy);
                        book.Copies!.Add(copy);
                    }
                }
                else
                {
                    var toRemove = oldCount - newCount;

                    
                    var notBorrowed = book.Copies!
                        .Where(c => c.CurrentBorrowing == null)
                        .OrderBy(c => (c.BorrowingLogs?.Count ?? 0))
                        .ToList();

                    foreach (var copy in notBorrowed)
                    {
                        if (toRemove == 0) break;

                        if (copy.BorrowingLogs != null && copy.BorrowingLogs.Count > 0)
                        {
                            foreach (var log in copy.BorrowingLogs.ToList())
                                _borrowingLogRepository.Delete(log);
                        }

                        _bookCopyRepository.Delete(copy);
                        book.Copies!.Remove(copy);
                        toRemove--;
                    }

                    
                    if (toRemove > 0)
                    {
                        var borrowed = book.Copies!
                            .Where(c => c.CurrentBorrowing != null)
                            .ToList();

                        foreach (var copy in borrowed)
                        {
                            if (toRemove == 0) break;

                            if (copy.BorrowingLogs != null && copy.BorrowingLogs.Count > 0)
                            {
                                foreach (var log in copy.BorrowingLogs.ToList())
                                    _borrowingLogRepository.Delete(log);
                            }

                            if (copy.CurrentBorrowing != null)
                                _bookBorrowingRepository.Delete(copy.CurrentBorrowing);

                            _bookCopyRepository.Delete(copy);
                            book.Copies!.Remove(copy);
                            toRemove--;
                        }
                    }
                }
            }

            _bookRepository.Update(book);
        }
        public void IncrementNumberCopies(Guid bookId)
        {
            var book = _bookRepository.GetById(bookId);
            if (book == null) throw new KeyNotFoundException($"Book {bookId} not found.");

            
            book.NumberCopies = Math.Max(0, book.NumberCopies) + 1;

            _bookRepository.Update(book);
        }
       
        public void DecrementNumberCopies(Guid bookId)
        {
            var book = _bookRepository.GetById(bookId)
                ?? throw new KeyNotFoundException($"Book {bookId} not found.");

            book.NumberCopies = Math.Max(0, book.NumberCopies - 1);
            _bookRepository.Update(book);
        }



    }
}
