using CoursesApplication.Domain.DomainModels;
using CoursesApplication.Domain.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoursesApplication.Repository.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<BookCopy> BookCopies { get; set; }
    public DbSet<BookBorrowing> BookBorrowings { get; set; }

    public DbSet<Author> Authors { get; set; } 
    public DbSet<TakenSeat> TakenSeats { get; set; }
    public DbSet<TakenSeatLog> TakenSeatLogs { get; set; }
    public DbSet<Classroom> Classrooms { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<BorrowingBookLog> BorrowingBookLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // BookCopy → many BookBorrowings (history)
        modelBuilder.Entity<BookBorrowing>()
            .HasOne(bb => bb.BookCopy)
            .WithMany()
            .HasForeignKey(bb => bb.BookCopyId)
            .OnDelete(DeleteBehavior.Restrict);

        // BookCopy → one CurrentBorrowing (optional)
        modelBuilder.Entity<BookCopy>()
            .HasOne(bc => bc.CurrentBorrowing)
            .WithOne() // no back nav on BookBorrowing
            .HasForeignKey<BookCopy>(bc => bc.BookBorrowingId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Seat>()
       .HasOne(s => s.TakenSeat)
       .WithOne(ts => ts.Seat)
       .HasForeignKey<Seat>(s => s.TakenSeatId)
       .IsRequired(false)
       .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BookCopy>()
        .HasIndex(bc => bc.InventoryCode)
        .IsUnique();

        // TakenSeat → one User (required)
        modelBuilder.Entity<TakenSeat>()
            .HasOne(ts => ts.User)
            .WithMany() // or .WithMany(u => u.TakenSeats) if you add collection on User
            .HasForeignKey(ts => ts.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Classroom → many Seats
        modelBuilder.Entity<Seat>()
            .HasOne(s => s.Classroom)
            .WithMany(c => c.Seats)
            .HasForeignKey(s => s.ClassroomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seat → many TakenSeatLogs
        modelBuilder.Entity<TakenSeatLog>()
            .HasOne(l => l.Seat)
            .WithMany(s => s.SeatLogs)
            .HasForeignKey(l => l.SeatId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Book>()
       .HasMany(b => b.Authors)
       .WithMany(a => a.Books)
       .UsingEntity(j =>
       {
           j.ToTable("BookAuthors");                // join table name
       });

        modelBuilder.Entity<User>()
          .HasOne(u => u.Role)
          .WithMany() // or .WithMany(r => r.Users) if Role has Users
          .HasForeignKey(u => u.RoleId)
          .OnDelete(DeleteBehavior.SetNull);

        
    }


}

