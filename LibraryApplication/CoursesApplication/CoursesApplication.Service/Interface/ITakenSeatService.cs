using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoursesApplication.Domain.Identity;

namespace CoursesApplication.Domain.DomainModels
{
    public interface ITakenSeatService 
    {
        List<TakenSeat> GetAll();
        TakenSeat? GetById(Guid id);
        TakenSeat Insert(TakenSeat takenSeat);
        ICollection<TakenSeat> InsertMany(ICollection<TakenSeat> takenSeats);
        TakenSeat Update(TakenSeat takenSeat);
        TakenSeat DeleteById(Guid id);


    }
}
