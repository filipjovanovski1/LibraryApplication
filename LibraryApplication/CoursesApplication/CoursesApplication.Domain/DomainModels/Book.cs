using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesApplication.Domain.DomainModels
{
    public class Book:BaseEntity
    {
        public string Title { get; set; } = default!;
       
   
        public BookCategory? Category { get; set; }

       
        public ICollection<BookCopy>? Copies { get; set; }
        public ICollection<Author> Authors { get; set; } = new List<Author>();
        public int NumberCopies { get; set; }
    }
}
