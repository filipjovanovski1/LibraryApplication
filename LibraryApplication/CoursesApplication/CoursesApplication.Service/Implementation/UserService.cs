using CoursesApplication.Domain.DomainModels;
using CoursesApplication.Domain.Identity;
using CoursesApplication.Repository.Interface;
using CoursesApplication.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace CoursesApplication.Service.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _users;
        private readonly IRepository<BookBorrowing> _borrowings;
        private readonly IRepository<BorrowingBookLog> _borrowingLogs;
        private readonly IRepository<TakenSeatLog> _seatLogs;
        private readonly IRepository<TakenSeat> _seats;
        private readonly IRepository<BookCopy> _bookCopies;
        public UserService(
           IUserRepository users,
           IRepository<BookBorrowing> borrowings,
           IRepository<BorrowingBookLog> borrowingLogs,
           IRepository<TakenSeatLog> seatLogs,
           IRepository<TakenSeat> seats,
           IRepository<BookCopy> bookCopies)
        {
            _users = users;
            _borrowings = borrowings;
            _borrowingLogs = borrowingLogs;
            _seatLogs = seatLogs;
            _seats = seats;
            _bookCopies = bookCopies;
        }
        public User DeleteById(Guid id)
        {
            var user = _users.GetByIdAsync(id).GetAwaiter().GetResult()
                       ?? throw new KeyNotFoundException("User not found.");
            var res = _users.DeleteAsync(user).GetAwaiter().GetResult();
            if (!res.Succeeded)
                throw new InvalidOperationException(string.Join("; ", res.Errors.Select(e => e.Description)));
            return user;
        }

        public List<User> GetAll()
        {
          
            return _users.Query().ToList();
        }

        public User? GetById(Guid id)
        {
            return _users.GetByIdAsync(id).GetAwaiter().GetResult();
        }

        public User Insert(User user)
        {
            var res = _users.CreateAsync(user).GetAwaiter().GetResult();  
            if (!res.Succeeded)
                throw new InvalidOperationException(string.Join("; ", res.Errors.Select(e => e.Description)));
            return user;
        }

        public ICollection<User> InsertMany(ICollection<User> users)
        {
            var created = new List<User>();
            foreach (var u in users)
            {
                var res = _users.CreateAsync(u).GetAwaiter().GetResult(); 
                if (!res.Succeeded)
                    throw new InvalidOperationException(string.Join("; ", res.Errors.Select(e => e.Description)));
                created.Add(u);
            }
            return created;
        }

        public User Update(User user)
        {
            var res = _users.UpdateAsync(user).GetAwaiter().GetResult();
            if (!res.Succeeded)
                throw new InvalidOperationException(string.Join("; ", res.Errors.Select(e => e.Description)));
            return user;
        }

        public void DeleteUserAndRelated(Guid userId)
        {
            if (userId == Guid.Empty) throw new ArgumentException("Invalid user id.", nameof(userId));

            var user = _users.GetByIdAsync(userId).GetAwaiter().GetResult();
            if (user == null) throw new KeyNotFoundException("User not found.");

            
            var logs = _borrowingLogs.GetAll(l => l, l => l.UserId == userId).ToList();
            if (logs.Any())
            {
                foreach (var log in logs) _borrowingLogs.Delete(log);
            }

            
            var borrowings = _borrowings.GetAll(
                selector: b => b,
                predicate: b => b.UserId == userId,
                include: q => q.Include(b => b.BookCopy)
            ).ToList();

            if (borrowings.Any())
            {
                
                var borrowingIds = borrowings.Select(b => b.Id).ToHashSet();
                var copiesNeedingClear = _bookCopies.GetAll(
                    selector: c => c,
                    predicate: c => c.BookBorrowingId != null && borrowingIds.Contains(c.BookBorrowingId.Value)
                ).ToList();

                foreach (var copy in copiesNeedingClear)
                {
                    copy.BookBorrowingId = null;
                    copy.CurrentBorrowing = null;
                    _bookCopies.Update(copy);
                }

               
                foreach (var b in borrowings) _borrowings.Delete(b);
            }

            
            var seatLogs = _seatLogs.GetAll(s => s, s => s.UserId == userId).ToList();
            if (seatLogs.Any())
            {
                foreach (var s in seatLogs) _seatLogs.Delete(s);
            }

           
            if (user.TakenSeatId.HasValue)
            {
                var seat = _seats.GetById(user.TakenSeatId.Value);
                if (seat != null) _seats.Delete(seat);
            }

           
            var res = _users.DeleteAsync(user).GetAwaiter().GetResult();
            if (!res.Succeeded)
                throw new InvalidOperationException(string.Join("; ", res.Errors.Select(e => e.Description)));
        }
    }

}
