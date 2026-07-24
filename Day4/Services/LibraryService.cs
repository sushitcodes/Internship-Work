using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services
{
    // Everything a console menu (or, later, a Web API controller) needs to
    // call lives here. This class doesn't know or care that data comes from
    // JSON files — it only talks to JsonDataStore<T>. Swap the storage
    // mechanism later and this class barely changes.
    public class LibraryService
    {
        private readonly JsonDataStore<Book> _bookStore;
        private readonly JsonDataStore<Member> _memberStore;
        private readonly JsonDataStore<BorrowRecord> _recordStore;

        private List<Book> _books;
        private List<Member> _members;
        private List<BorrowRecord> _records;

        public LibraryService()
        {
            _bookStore = new JsonDataStore<Book>("books.json");
            _memberStore = new JsonDataStore<Member>("members.json");
            _recordStore = new JsonDataStore<BorrowRecord>("borrowRecords.json");

            _books = _bookStore.Load();
            _members = _memberStore.Load();
            _records = _recordStore.Load();
        }

        // ---------------- BOOKS ----------------

        public Book AddBook(string title, string author, string isbn)
        {
            var book = new Book
            {
                Id = NextId(_books.Select(b => b.Id)),
                Title = title,
                Author = author,
                Isbn = isbn,
                IsAvailable = true
            };

            _books.Add(book);
            _bookStore.Save(_books);
            return book;
        }

        public List<Book> GetAllBooks() => _books;

        public bool RemoveBook(int bookId)
        {
            Book? book = _books.FirstOrDefault(b => b.Id == bookId);
            if (book is null) return false;

            if (!book.IsAvailable)
            {
                throw new InvalidOperationException("Cannot remove a book that is currently borrowed.");
            }

            _books.Remove(book);
            _bookStore.Save(_books);
            return true;
        }

        // ---------------- MEMBERS ----------------

        public Member AddMember(string name, string email)
        {
            var member = new Member
            {
                Id = NextId(_members.Select(m => m.Id)),
                Name = name,
                Email = email
            };

            _members.Add(member);
            _memberStore.Save(_members);
            return member;
        }

        public List<Member> GetAllMembers() => _members;

        // ---------------- BORROWING ----------------

        public BorrowRecord BorrowBook(int bookId, int memberId)
        {
            Book? book = _books.FirstOrDefault(b => b.Id == bookId)
                ?? throw new InvalidOperationException($"No book with Id {bookId}.");

            Member? member = _members.FirstOrDefault(m => m.Id == memberId)
                ?? throw new InvalidOperationException($"No member with Id {memberId}.");

            if (!book.IsAvailable)
            {
                throw new InvalidOperationException($"\"{book.Title}\" is already borrowed.");
            }

            var record = new BorrowRecord
            {
                Id = NextId(_records.Select(r => r.Id)),
                BookId = bookId,
                MemberId = memberId,
                BorrowedOn = DateTime.Now,
                ReturnedOn = null
            };

            book.IsAvailable = false;

            _records.Add(record);
            _recordStore.Save(_records);
            _bookStore.Save(_books);

            return record;
        }

        public bool ReturnBook(int bookId)
        {
            BorrowRecord? activeRecord = _records
                .FirstOrDefault(r => r.BookId == bookId && r.IsActive);

            if (activeRecord is null)
            {
                return false; // no active loan for this book
            }

            activeRecord.ReturnedOn = DateTime.Now;

            Book? book = _books.FirstOrDefault(b => b.Id == bookId);
            if (book is not null)
            {
                book.IsAvailable = true;
            }

            _recordStore.Save(_records);
            _bookStore.Save(_books);
            return true;
        }

        public List<BorrowRecord> GetActiveLoans() => _records.Where(r => r.IsActive).ToList();

        public List<BorrowRecord> GetAllRecords() => _records;
        private static int NextId(IEnumerable<int> existingIds)
        {
            return existingIds.Any() ? existingIds.Max() + 1 : 1;
        }
    }

}