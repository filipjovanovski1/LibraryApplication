using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesApplication.Domain.DomainModels
{
    public class Seat : BaseEntity
    {

        public Guid ClassroomId { get; set; }
        public Classroom Classroom { get; set; } = default!;

        public int Number { get; set; } 

        public bool IsTaken { get; set; } = false;
        
        public TakenSeat? TakenSeat { get; set; }
        public Guid? TakenSeatId { get; set; }

        public ICollection<TakenSeatLog> SeatLogs { get; set; } = new List<TakenSeatLog>();
    }
   }
