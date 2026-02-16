using Microsoft.EntityFrameworkCore;
using CoursesApplication.Domain.DomainModels;
using CoursesApplication.Repository.Data;
using CoursesApplication.Domain.DTO;      
using CoursesApplication.Web.External;   

namespace CoursesApplication.Web.Services
{
    public interface IBookImportService
    {
        Task<int> ImportAsync(IEnumerable<string>? categories, CancellationToken ct = default);
    }

    public sealed class BookImportService : IBookImportService
    {
        private readonly IBookApiClient _api;
        private readonly ApplicationDbContext _db;

        public BookImportService(IBookApiClient api, ApplicationDbContext db)
        {
            _api = api;
            _db = db;
        }

        public async Task<int> ImportAsync(IEnumerable<string>? categories, CancellationToken ct = default)
        {
            var apiBooks = await _api.GetByCategoriesAsync(categories, ct);

            var authorsSet = _db.Set<Author>();
            var booksSet = _db.Set<Book>();

            const char SEP = '\u241F';
            static string Key(string a, string b) => $"{a}{SEP}{b}";
            static string BookKey(string title, string authorFull) => $"{title}{SEP}{authorFull}";

            var existingAuthors = await authorsSet
                .ToDictionaryAsync(a => Key(a.Name, a.Surname), ct);

            var existingBooks = await booksSet
                .Include(b => b.Authors)
                .ToListAsync(ct);

            var existingBookMap = existingBooks.ToDictionary(
                b => BookKey(b.Title, b.Authors.FirstOrDefault() is { } aa ? $"{aa.Name} {aa.Surname}".Trim() : "")
            );

            var newAuthors = new List<Author>();
            var newBooks = new List<Book>();

            foreach (var dto in apiBooks)
            {
                var rawCategory = dto.Category?.Trim();
                if (string.IsNullOrEmpty(rawCategory)) continue;

                if (!Enum.TryParse<BookCategory>(rawCategory, ignoreCase: true, out var enumCategory))
                    continue; 

                var (first, last) = SplitAuthor(dto.Author);

                var authorKey = Key(first, last);
                if (!existingAuthors.TryGetValue(authorKey, out var author))
                {
                    author = new Author { Name = first, Surname = last };
                    existingAuthors[authorKey] = author;
                    newAuthors.Add(author);
                }

                var bkKey = BookKey(dto.Title, $"{first} {last}".Trim());
                if (!existingBookMap.TryGetValue(bkKey, out var book))
                {
                    book = new Book
                    {
                        Title = dto.Title,
                        Category = enumCategory,
                        NumberCopies = 0,
                        Authors = new List<Author> { author }
                    };
                    existingBookMap[bkKey] = book;
                    newBooks.Add(book);
                }
                else
                {
                }
            }

            var isInMemory = _db.Database.IsInMemory();   

            if (isInMemory)
            { 
                if (newAuthors.Count > 0) authorsSet.AddRange(newAuthors);
                if (newBooks.Count > 0) booksSet.AddRange(newBooks);
                return await _db.SaveChangesAsync(ct);
            }
            else
            {
                using var tx = await _db.Database.BeginTransactionAsync(ct);
                if (newAuthors.Count > 0) authorsSet.AddRange(newAuthors);
                if (newBooks.Count > 0) booksSet.AddRange(newBooks);
                var changed = await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return changed;
            }
        }

        private static (string first, string last) SplitAuthor(string full)
        {
            var parts = (full ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= 1) return (full ?? "", "");
            return (string.Join(' ', parts[..^1]), parts[^1]);
        }
    }
}
