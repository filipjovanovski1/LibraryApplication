using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CoursesApplication.Domain.DTO; // BookApiBookDto (Title, Category, Author)

namespace CoursesApplication.Web.External
{
    public interface IBookApiClient
    {
        Task<IReadOnlyList<BookApiBookDto>> GetByCategoriesAsync(IEnumerable<string>? categories = null, CancellationToken ct = default);
    }

    public sealed class BookApiClient : IBookApiClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        public BookApiClient(IHttpClientFactory factory) => _http = factory.CreateClient("BookApi");

        public async Task<IReadOnlyList<BookApiBookDto>> GetByCategoriesAsync(IEnumerable<string>? categories = null, CancellationToken ct = default)
        {
            var cats = categories == null || !categories.Any()
                ? "fiction,fantasy,romance" // default matches your API controller
                : string.Join(',', categories);

            var res = await _http.GetAsync($"api/Books?categories={Uri.EscapeDataString(cats)}", ct);
            if (res.StatusCode == HttpStatusCode.NotFound) return Array.Empty<BookApiBookDto>();
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<BookApiBookDto>>(JsonOpts, ct) ?? new();
        }
    }
}
