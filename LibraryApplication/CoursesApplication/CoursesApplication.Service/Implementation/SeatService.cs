using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoursesApplication.Repository.Interface;
using CoursesApplication.Domain.DomainModels;
using static System.Reflection.Metadata.BlobBuilder;
using CoursesApplication.Repository.Data;
using Microsoft.EntityFrameworkCore;


namespace CoursesApplication.Service.Interface
{
    public class SeatService : ISeatService
    {
        private readonly IRepository<Seat> _seatRepository;
        private readonly ApplicationDbContext _db;

        public SeatService(IRepository<Seat> seatRepository,ApplicationDbContext db)
        {
            _seatRepository = seatRepository;
            _db = db;
        }
        public Seat DeleteById(Guid id)
        {
            var entity = GetById(id);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Delete: Seat with id {id} not found.");
            }

            return _seatRepository.Delete(entity);
        }

        public List<Seat> GetAll()
        {
            return _seatRepository.GetAll(selector: x => x).ToList();
        }

        public Seat? GetById(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException("Invalid id.", nameof(id));

            return _db.Seats
                .Include(s => s.Classroom)
                .Include(s => s.TakenSeat)
                    .ThenInclude(ts => ts.User)
                .FirstOrDefault(s => s.Id == id);
        }

        public Seat Insert(Seat seat)
        {
            return _seatRepository.Insert(seat);
        }

        public ICollection<Seat> InsertMany(ICollection<Seat> seats)
        {
            return _seatRepository.InsertMany(seats);
        }

        public Seat Update(Seat seat)
        {
            return _seatRepository.Update(seat);
        }
    }
}
