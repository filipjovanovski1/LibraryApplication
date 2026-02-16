using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoursesApplication.Domain.Identity;

namespace CoursesApplication.Domain.DomainModels
{
    public class TakenSeatLog : BaseEntity
    {

        public Guid SeatId { get; set; }
        public Seat Seat { get; set; } = default!; 

        public Guid UserId { get; set; }
        public User User { get; set; } = default!;

        public DateTime StartedAt { get; set; }
        public DateTime FreedAt { get; set; }

        public string? Reason { get; set; }
    }
}
