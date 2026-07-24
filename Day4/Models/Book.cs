namespace LibraryManagementSystem.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;

        // We don't store "who has it" here — that lives in BorrowRecord.
        // IsAvailable is derived, not stored, so it can never go stale.
        public bool IsAvailable { get; set; } = true;

        public override string ToString()
        {
            string status = IsAvailable ? "Available" : "Borrowed";
            return $"[{Id}] \"{Title}\" by {Author} (ISBN: {Isbn}) - {status}";
        }
    }
}
