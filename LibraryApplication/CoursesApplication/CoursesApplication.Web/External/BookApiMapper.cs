using CoursesApplication.Domain.DomainModels;
using CoursesApplication.Domain.DTO;

namespace CoursesApplication.Web.External;
public static class BookApiMapper
{
    public static Book ToDomain(this BookApiBookDto dto)
    {
        BookCategory? cat = null;
        if (!string.IsNullOrWhiteSpace(dto.Category) &&
            Enum.TryParse<BookCategory>(dto.Category, true, out var parsed))
        {
            cat = parsed;
        }

        // crude split "First Last" → tweak if you store differently
        var (first, last) = SplitAuthor(dto.Author);

        return new Book
        {
            Title = dto.Title,
                         // API doesn't provide
            Category = cat,
            NumberCopies = 0,          // you can set this later
            Authors = new List<Author>
            {
                new Author { Name = first, Surname = last }
            }
        };
    }

    private static (string first, string last) SplitAuthor(string full)
    {
        if (string.IsNullOrWhiteSpace(full)) return ("", "");
        var parts = full.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return (parts[0], "");
        return (string.Join(' ', parts[..^1]), parts[^1]); // everything except last → first
    }
}
