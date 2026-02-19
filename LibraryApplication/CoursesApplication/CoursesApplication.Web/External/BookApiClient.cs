using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CoursesApplication.Domain.DTO; // BookApiBookDto (Title, Category, Author)

namespace CoursesApplication.Web.External
{
    public sealed class ExternalBookApiException : Exception
    {
        public ExternalBookApiException(string message, Exception? inner = null) : base(message, inner) { }
    }

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

            try
            {
                var res = await _http.GetAsync($"api/Books?categories={Uri.EscapeDataString(cats)}", ct);
                if (res.StatusCode == HttpStatusCode.NotFound) return Array.Empty<BookApiBookDto>();
                res.EnsureSuccessStatusCode();
                return await res.Content.ReadFromJsonAsync<List<BookApiBookDto>>(JsonOpts, ct) ?? new();
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                throw new ExternalBookApiException("The external Book API did not respond in time. Please try again or check if the BookApi service is running.", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new ExternalBookApiException("Could not reach the external Book API. Verify the API is running and reachable.", ex);
            }
        }
    }
}
