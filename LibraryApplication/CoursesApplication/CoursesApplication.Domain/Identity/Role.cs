using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesApplication.Domain.Identity
{
    public class Role : IdentityRole<Guid> {

        public const string Librarian = "LIBRARIAN";
        public const string Keeper = "KEEPER";
        public const string Student = "STUDENT";

        // (Optional, only if you use EF HasData)
        public static readonly Guid LibrarianId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        public static readonly Guid KeeperId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        public static readonly Guid StudentId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    }
}
