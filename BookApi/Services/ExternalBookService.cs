using System.Net.Http.Json;
using BookApi.Models;
using System.Text.Json;

namespace BookApi.Services
{
    public class ExternalBookService
    {
        private readonly HttpClient _httpClient;

        public ExternalBookService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Book>> GetBooksByCategoriesAsync(string[] categories, int limitPerCategory = 50)
        {
            var books = new List<Book>();

            foreach (var category in categories)
            {
                var url = $"https://openlibrary.org/subjects/{category}.json?limit={limitPerCategory}";
                try
                {
                    var response = await _httpClient.GetStringAsync(url);
                    using var doc = JsonDocument.Parse(response);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("works", out JsonElement works))
                    {
                        foreach (var work in works.EnumerateArray())
                        {
                            var title = work.GetProperty("title").GetString() ?? "Unknown Title";

                            string author = "Unknown Author";
                            if (work.TryGetProperty("authors", out var authors) && authors.GetArrayLength() > 0)
                            {
                                author = authors[0].GetProperty("name").GetString() ?? "Unknown Author";
                            }

                            books.Add(new Book
                            {
                                Title = title,
                                Author = author,
                                Category = category
                            });
                        }
                    }
                }
                catch
                {
                    // Ignore errors for this category
                    continue;
                }
            }

            return books;
        }
    }
}
