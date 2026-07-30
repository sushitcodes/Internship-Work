namespace BookTracker.Models
{
    public class Book
    {
        internal readonly object Count;

        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public bool IsRead { get; set; }
    }
}