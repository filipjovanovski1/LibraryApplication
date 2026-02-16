using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoursesApplication.Repository.Interface;
using CoursesApplication.Domain.Identity;
using static System.Reflection.Metadata.BlobBuilder;

namespace CoursesApplication.Domain.DomainModels
{
    public class TakenSeatService : ITakenSeatService
    {
        private readonly IRepository<TakenSeat> _takenSeatRepository;

        public TakenSeatService(IRepository<TakenSeat> takenSeatRepository)
        {
            _takenSeatRepository = takenSeatRepository;
        }
        public TakenSeat DeleteById(Guid id)
        {
            var entity = GetById(id);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Delete: TakenSeat with id {id} not found.");
            }

            return _takenSeatRepository.Delete(entity);
        }

        public List<TakenSeat> GetAll()
        {
            return _takenSeatRepository.GetAll(selector: x => x).ToList();
        }

        public TakenSeat? GetById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("GetById TakenSeat: Invalid ID.", nameof(id));

            return _takenSeatRepository.GetById(id);
        }

        public TakenSeat Insert(TakenSeat takenSeat)
        {
            return _takenSeatRepository.Insert(takenSeat);
        }

        public ICollection<TakenSeat> InsertMany(ICollection<TakenSeat> takenSeats)
        {
            return _takenSeatRepository.InsertMany(takenSeats);
        }

        public TakenSeat Update(TakenSeat takenSeat)
        {
            return _takenSeatRepository.Update(takenSeat);
        }
    }
}
