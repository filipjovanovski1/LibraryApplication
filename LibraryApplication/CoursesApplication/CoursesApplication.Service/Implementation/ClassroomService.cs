using CoursesApplication.Repository.Data;
using CoursesApplication.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace CoursesApplication.Domain.DomainModels
{
    public class ClassroomService : IClassroomService
    {
        private readonly IClassroomRepository _classroomRepository;
        private readonly ApplicationDbContext _db;

        public ClassroomService(
           IClassroomRepository classroomRepository,
           ApplicationDbContext db)                
        {
            _classroomRepository = classroomRepository;
            _db = db;                                 
        }

        public Classroom DeleteById(Guid id)
        {
            var entity = GetById(id);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Delete: Classroom with id {id} not found.");
            }

            return _classroomRepository.Delete(entity);
        }

        public List<Classroom> GetAll()
        {
            return _classroomRepository.GetAll(selector: x => x).ToList();
        }

        public Classroom? GetById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("GetById Classroom: Invalid ID.", nameof(id));

            return _classroomRepository.GetById(id);
        }

        public Classroom Insert(Classroom classroom)
        {
            if (classroom.Capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(classroom.Capacity), "Capacity must be non-negative.");

            
            if (classroom.Seats == null || classroom.Seats.Count == 0)
            {
                classroom.Seats = Enumerable.Range(1, classroom.Capacity)
                    .Select(n => new Seat
                    {
                        Number = n,
                        IsTaken = false,  
                        TakenSeat = null,  
                        TakenSeatId = null
                    })
                    .ToList();
            }

            return _classroomRepository.Insert(classroom);
        }


        public ICollection<Classroom> InsertMany(ICollection<Classroom> classrooms)
        {
            return _classroomRepository.InsertMany(classrooms);
        }

        public Classroom Update(Classroom classroom)
        {
            return _classroomRepository.Update(classroom);
        }

        public Classroom GetByIdWithSeats(Guid id)
        {
            return _classroomRepository.GetByIdWithSeats(id);
        }

        public void SyncCapacity(Classroom cls, int newCapacity)
        {
            var current = cls.Seats.Count;

            if (newCapacity > current)
            {
                // add seats  (free by default)
                for (int n = current + 1; n <= newCapacity; n++)
                    cls.Seats.Add(new Seat { Number = n, IsTaken = false });
            }
            else if (newCapacity < current)
            {
               
                var toRemove = cls.Seats.Where(s => s.Number > newCapacity).OrderByDescending(s => s.Number).ToList();
                foreach (var s in toRemove)
                {
                    if (s.IsTaken) throw new InvalidOperationException($"Seat {s.Number} is taken; cannot shrink capacity.");
                    cls.Seats.Remove(s);
                }
            }

            cls.Capacity = newCapacity;
        }

        public void FreeAllSeats(Guid classroomId)
        {
            
            var seatIdsQuery = _db.Seats
                .Where(s => s.ClassroomId == classroomId)
                .Select(s => s.Id);

          
            _db.Set<TakenSeatLog>()
                .Where(l => seatIdsQuery.Contains(l.SeatId))
                .ExecuteDelete(); // EF Core 7+

           
            var takenSeatIdsQuery = _db.Seats
                .Where(s => s.ClassroomId == classroomId && s.TakenSeatId != null)
                .Select(s => s.TakenSeatId!.Value);

            _db.Set<TakenSeat>()
                .Where(ts => takenSeatIdsQuery.Contains(ts.Id))
                .ExecuteDelete();

           
            _db.Seats
                .Where(s => s.ClassroomId == classroomId)
                .ExecuteDelete();
        }

        public void FreeAllSeatsInRange(Guid classroomId, int newCapacity)
        {

          
            var seatIdsToDelete = _db.Seats
                .Where(s => s.ClassroomId == classroomId && s.Number > newCapacity)
                .Select(s => s.Id)
                .ToList();

           
            var classroom = _db.Classrooms.FirstOrDefault(c => c.Id == classroomId)
                            ?? throw new InvalidOperationException("Classroom not found.");

            if (seatIdsToDelete.Count == 0)
            {
                classroom.Capacity = newCapacity;
                _db.SaveChanges();
               
                return;
            }

            
            var takens = _db.TakenSeats
    .Where(ts => seatIdsToDelete.Contains(ts.SeatId))
    .ToList();
            if (takens.Count > 0)
                _db.TakenSeats.RemoveRange(takens);

           
            var logs = _db.TakenSeatLogs
                .Where(l => seatIdsToDelete.Contains(l.SeatId))
                .ToList();
            if (logs.Count > 0)
                _db.TakenSeatLogs.RemoveRange(logs);

            
            var seatsToDelete = _db.Seats
                .Where(s => seatIdsToDelete.Contains(s.Id))
                .ToList();
            _db.Seats.RemoveRange(seatsToDelete);

           
            classroom.Capacity = newCapacity;

            _db.SaveChanges();
            

        }

    }
}
