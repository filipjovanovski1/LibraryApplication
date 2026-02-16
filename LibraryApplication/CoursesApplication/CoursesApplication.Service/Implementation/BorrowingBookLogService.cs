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
    public class BorrowingBookLogService : IBorrowingBookLogService
    {
        private readonly IRepository<BorrowingBookLog> _borrowingBookLogRepository;

        public BorrowingBookLogService(IRepository<BorrowingBookLog> borrowingBookLogRepository)
        {
            _borrowingBookLogRepository = borrowingBookLogRepository;
        }
        public BorrowingBookLog DeleteById(Guid id)
        {
            var entity = GetById(id);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Delete: BorrowingBookLog with id {id} not found.");
            }

            return _borrowingBookLogRepository.Delete(entity);
        }

        public List<BorrowingBookLog> GetAll()
        {
            return _borrowingBookLogRepository.GetAll(selector: x => x).ToList();
        }

        public BorrowingBookLog? GetById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("GetById BorrowingBookLog: Invalid ID.", nameof(id));

            return _borrowingBookLogRepository.GetById(id);
        }

        public BorrowingBookLog Insert(BorrowingBookLog borrowingBookLog)
        {
            return _borrowingBookLogRepository.Insert(borrowingBookLog);
        }

        public ICollection<BorrowingBookLog> InsertMany(ICollection<BorrowingBookLog> borrowingBookLogs)
        {
            return _borrowingBookLogRepository.InsertMany(borrowingBookLogs);
        }

        public BorrowingBookLog Update(BorrowingBookLog borrowingBookLog)
        {
            return _borrowingBookLogRepository.Update(borrowingBookLog);
        }

        public IEnumerable<BorrowingBookLog> GetAllByUserId(Guid userId) =>
      _borrowingBookLogRepository.GetAll(
          selector: l => l,
          predicate: l => l.UserId == userId,
          include: q => q
              .Include(l => l.User)                 
              .Include(l => l.BookCopy).ThenInclude(bc => bc.Book), 
          orderBy: q => q.OrderByDescending(l => l.BorrowedAt)
      );

       
        public IEnumerable<BorrowingBookLog> GetAllByBookCopyId(Guid bookCopyId) =>
            _borrowingBookLogRepository.GetAll(
                selector: l => l,
                predicate: l => l.BookCopyId == bookCopyId,
                include: q => q
                    .Include(l => l.User)
                    .Include(l => l.BookCopy).ThenInclude(bc => bc.Book),
                orderBy: q => q.OrderByDescending(l => l.BorrowedAt)
            );
    }
}
