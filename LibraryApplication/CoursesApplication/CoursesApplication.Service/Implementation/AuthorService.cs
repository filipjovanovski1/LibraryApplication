using CoursesApplication.Domain.DomainModels;
using CoursesApplication.Repository.Interface;
using CoursesApplication.Service.Interface;

namespace CoursesApplication.Service.Implementation
{
    public class AuthorService : IAuthorService
    {
        private readonly IRepository<Author> _authorRepository;

        public AuthorService(IRepository<Author> authorRepository)
        {
            _authorRepository = authorRepository;
        }
        public Author DeleteById(Guid id)
        {
            var author = _authorRepository.Get(a => a, a => a.Id == id);

            if (author == null)
                throw new KeyNotFoundException($"Author with ID {id} was not found.");

            _authorRepository.Delete(author);
            return author;
        }

        public List<Author> GetAll()
        {
            return _authorRepository.GetAll(a => a).ToList();
        }

        public Author? GetById(Guid id)
        {
            return _authorRepository.Get(a => a, a => a.Id == id);

        }

        public Author Insert(Author author)
        {
            if (author == null)
                throw new ArgumentNullException(nameof(author));

            author.Id = Guid.NewGuid();
            _authorRepository.Insert(author);
            return author;
        }

        public ICollection<Author> InsertMany(ICollection<Author> authors)
        {
            if (authors == null || authors.Count == 0)
                throw new ArgumentException("Authors collection cannot be null or empty.");

            foreach (var author in authors)
            {
                author.Id = Guid.NewGuid();
                _authorRepository.Insert(author);
            }

            return authors;
        }

        public Author Update(Author author)
        {
            if (author == null)
                throw new ArgumentNullException(nameof(author));

            var existingAuthor = _authorRepository.Get(a => a, a => a.Id == author.Id);
            if (existingAuthor == null)
                throw new KeyNotFoundException($"Author with ID {author.Id} was not found.");

            existingAuthor.Name = author.Name;
            existingAuthor.Surname = author.Surname;
            existingAuthor.Books = author.Books;

            _authorRepository.Update(existingAuthor);
            return existingAuthor;
        }
    }
}
