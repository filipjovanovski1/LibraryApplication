using CoursesApplication.Repository.Data;
using CoursesApplication.Repository.Interface;
using CoursesApplication.Domain.DomainModels;
using Microsoft.EntityFrameworkCore;

namespace CoursesApplication.Repository.Implementation
{
    public class TakenSeatRepository : Repository<TakenSeat>, ITakenSeatRepository
    {
        private readonly ApplicationDbContext _context;

        public TakenSeatRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

       
        public IEnumerable<TakenSeat> GetAllByClassroomId(Guid classroomId)
        {
            return _context.TakenSeats
                .Include(ts => ts.Seat)                  
                .ThenInclude(s => s.Classroom)          
                .Where(ts => ts.Seat.ClassroomId == classroomId)
                .AsNoTracking()
                .ToList();
        }
    }
}
