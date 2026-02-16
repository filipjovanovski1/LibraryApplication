using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoursesApplication.Domain.DomainModels;

namespace CoursesApplication.Service.Interface
{
    public interface ISeatService
    {
        List<Seat> GetAll();
        Seat? GetById(Guid id);
        Seat Insert(Seat seat);
        ICollection<Seat> InsertMany(ICollection<Seat> seats);
        Seat Update(Seat seat);
        Seat DeleteById(Guid id);
    }
}
