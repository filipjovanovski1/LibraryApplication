using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoursesApplication.Domain.Identity;

namespace CoursesApplication.Domain.DomainModels
{
    public interface ITakenSeatLogService 
    {

        List<TakenSeatLog> GetAll();
        TakenSeatLog? GetById(Guid id);
        TakenSeatLog Insert(TakenSeatLog seatSeatLog);
        ICollection<TakenSeatLog> InsertMany(ICollection<TakenSeatLog> takenSeatLogs);
        TakenSeatLog Update(TakenSeatLog takenSeatLog);
        TakenSeatLog DeleteById(Guid id);
        IEnumerable<TakenSeatLog> GetAllByUserId(Guid userId);
        IEnumerable<TakenSeatLog> GetAllBySeatId(Guid seatId);
    }
}
