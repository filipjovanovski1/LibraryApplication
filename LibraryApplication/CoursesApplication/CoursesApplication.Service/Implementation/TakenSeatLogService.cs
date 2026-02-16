using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoursesApplication.Repository.Interface;
using CoursesApplication.Domain.Identity;
using static System.Reflection.Metadata.BlobBuilder;
using Microsoft.EntityFrameworkCore;


namespace CoursesApplication.Domain.DomainModels
{
    public class TakenSeatLogService : ITakenSeatLogService
    {
        private readonly IRepository<TakenSeatLog> _takenSeatLogRepository;

        public TakenSeatLogService(IRepository<TakenSeatLog> takenSeatLogRepository)
        {
            _takenSeatLogRepository = takenSeatLogRepository;
        }
        public TakenSeatLog DeleteById(Guid id)
        {
            var entity = GetById(id);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Delete: TakenSeatLog with id {id} not found.");
            }

            return _takenSeatLogRepository.Delete(entity);
        }

        public List<TakenSeatLog> GetAll()
        {
            return _takenSeatLogRepository.GetAll(selector: x => x).ToList();
        }

        public TakenSeatLog? GetById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("GetById TakenSeatLog: Invalid ID.", nameof(id));

            return _takenSeatLogRepository.GetById(id);
        }

        public TakenSeatLog Insert(TakenSeatLog takenSeatLog)
        {
            return _takenSeatLogRepository.Insert(takenSeatLog);
        }

        public ICollection<TakenSeatLog> InsertMany(ICollection<TakenSeatLog> takenSeatLogs)
        {
            return _takenSeatLogRepository.InsertMany(takenSeatLogs);
        }

        public TakenSeatLog Update(TakenSeatLog takenSeatLog)
        {
            return _takenSeatLogRepository.Update(takenSeatLog);
        }

        public IEnumerable<TakenSeatLog> GetAllByUserId(Guid userId)
        => _takenSeatLogRepository.GetAll(
            selector: l => l,
            predicate: l => l.UserId == userId,
            include: q => q.Include(l => l.Seat), 
            orderBy: q => q.OrderByDescending(l => l.StartedAt)
        );

        public IEnumerable<TakenSeatLog> GetAllBySeatId(Guid seatId)
            => _takenSeatLogRepository.GetAll(
                selector: l => l,
                predicate: l => l.SeatId == seatId,
                include: q => q.Include(l => l.User), 
                orderBy: q => q.OrderByDescending(l => l.StartedAt)
            );

    }
}
