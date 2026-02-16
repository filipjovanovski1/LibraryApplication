using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesApplication.Domain.DomainModels
{
    public interface IClassroomService
    {
        List<Classroom> GetAll();
        Classroom? GetById(Guid id);
        Classroom Insert(Classroom classroom);
        ICollection<Classroom> InsertMany(ICollection<Classroom> classrooms);
        Classroom Update(Classroom classroom);
        Classroom DeleteById(Guid id);
        public Classroom GetByIdWithSeats(Guid id);

        public void SyncCapacity(Classroom cls, int newCapacity);
        public void FreeAllSeats(Guid classroomId);
        public void FreeAllSeatsInRange(Guid classroomId, int newCapacity);

    }
}
