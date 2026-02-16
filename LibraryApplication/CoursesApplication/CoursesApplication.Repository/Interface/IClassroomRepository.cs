using CoursesApplication.Domain.DomainModels;

namespace CoursesApplication.Repository.Interface;
public interface IClassroomRepository : IRepository<Classroom>
{
    Classroom? GetByIdWithSeats(Guid id);
}