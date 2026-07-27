using System.ComponentModel.DataAnnotations;

namespace BookApi.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Title { get; set; }

        [Required]
        [StringLength(50)]
        public string? Author { get; set; }

        public string? Genre { get; set; }

        public bool IsRead { get; set; }

        [Range(0, 5)]
        public double Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

