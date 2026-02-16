using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesApplication.Domain.DomainModels
{
    public class Classroom: BaseEntity
    {

        public string Name { get; set; } = default!;
        public string Location { get; set; }
        public int Capacity { get; set; }

        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}
