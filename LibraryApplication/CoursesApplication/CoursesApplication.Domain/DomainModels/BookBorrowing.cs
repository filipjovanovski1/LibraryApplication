using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoursesApplication.Domain.Identity;


namespace CoursesApplication.Domain.DomainModels
{
    public class BookBorrowing : BaseEntity
    {

     
        public Guid BookCopyId { get; set; }
         public BookCopy? BookCopy { get; set; }   

        public Guid UserId { get; set; }
         public User? User { get; set; }          

        public DateTime BorrowedAt { get; set; }
        public DateTime? DueDate { get; set; }
    }
    

}
