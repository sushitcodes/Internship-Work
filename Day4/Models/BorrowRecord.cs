namespace LibraryManagementSystem.Models
{
    // A borrow record is the "join" between a Book and a Member,
    // plus the dates that make it a history, not just a link.
    public class BorrowRecord
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public DateTime BorrowedOn { get; set; }
        public DateTime? ReturnedOn { get; set; } // null = still out

        public bool IsActive => ReturnedOn is null;

        public override string ToString()
        {
            string returned = ReturnedOn is null ? "still out" : $"returned {ReturnedOn:d}";
            return $"[{Id}] BookId {BookId} -> MemberId {MemberId} | borrowed {BorrowedOn:d} | {returned}";
        }
    }
}
