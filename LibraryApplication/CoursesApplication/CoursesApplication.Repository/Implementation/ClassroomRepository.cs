using CoursesApplication.Repository.Data;
using CoursesApplication.Repository.Interface;
using CoursesApplication.Domain.DomainModels;
using Microsoft.EntityFrameworkCore;

namespace CoursesApplication.Repository.Implementation
{
    public class ClassroomRepository : Repository<Classroom>, IClassroomRepository
    {
        private readonly ApplicationDbContext _context;

        public ClassroomRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public Classroom? GetByIdWithSeats(Guid id)
        {
            return _context.Classrooms
                .Include(c => c.Seats)
                .FirstOrDefault(c => c.Id == id);
        }
    }
}


