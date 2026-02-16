using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoursesApplication.Domain.DomainModels;
using Microsoft.AspNetCore.Identity;


namespace CoursesApplication.Domain.Identity
{

    public class User : IdentityUser<Guid>
    {

        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public ICollection<BookBorrowing>? Borrowings { get; set; } = new List<BookBorrowing>();

        public ICollection<TakenSeatLog>? TakenSeatLogs { get; set; } = new List<TakenSeatLog>();
        // One user for one TakenSeat (optional — user might not be seated)
        public TakenSeat? TakenSeat { get; set; }
        public Guid? TakenSeatId { get; set; }
        public Role? Role { get; set; }
        public Guid? RoleId { get; set; }

    }
}
