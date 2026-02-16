using CoursesApplication.Domain.DomainModels;

namespace CoursesApplication.Repository.Interface;
public interface ITakenSeatRepository : IRepository<TakenSeat>
{
    IEnumerable<TakenSeat> GetAllByClassroomId(Guid classroomId);
}