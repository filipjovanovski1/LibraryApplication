using CoursesApplication.Domain.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesApplication.Domain.DomainModels
{
    public class TakenSeat : BaseEntity
    {

   
        public Seat Seat { get; set; } = default!;
        public Guid SeatId { get; set; }

       
        [Required(ErrorMessage = "Please select a user.")]
        public Guid? UserId { get; set; }
        public User User { get; set; } = default!;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    }
}
