using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesApplication.Domain.DomainModels
{
    [Index(nameof(InventoryCode), IsUnique = true)]
    public class BookCopy: BaseEntity
    {

        public Guid BookId { get; set; }
        public Book Book { get; set; }


        [Required]
        public string InventoryCode { get; set; } = default!; 
        
        public BookBorrowing? CurrentBorrowing { get; set; }
        public Guid? BookBorrowingId { get; set; }

        
        public ICollection<BorrowingBookLog> BorrowingLogs { get; set; } = new List<BorrowingBookLog>();
        

    }
}
