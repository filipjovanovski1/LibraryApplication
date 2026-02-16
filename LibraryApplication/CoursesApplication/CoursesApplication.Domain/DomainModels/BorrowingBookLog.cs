using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoursesApplication.Domain.Identity;

namespace CoursesApplication.Domain.DomainModels
{
    public class BorrowingBookLog : BaseEntity
    {

        public Guid BookCopyId { get; set; }
        public BookCopy? BookCopy { get; set; } = default!;

        public Guid UserId { get; set; }
        public User? User { get; set; } = default!;

        public DateTime BorrowedAt { get; set; }
        public DateTime ReturnedAt { get; set; }
        public DateTime? DueDate { get; set; }

        public string? Notes { get; set; }
    }
}
