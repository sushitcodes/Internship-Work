using System;
using System.ComponentModel.DataAnnotations;

namespace BookMvc.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [Display(Name = "Book Title")]
        [StringLength(100, MinimumLength = 1)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Author is required")]
        [Display(Name = "Author")]
        [StringLength(50, MinimumLength = 1)]
        public string Author { get; set; }

        [Required(ErrorMessage = "Genre is required")]
        [Display(Name = "Genre")]
        public string Genre { get; set; }

        [Display(Name = "Have you read this?")]
        public bool IsRead { get; set; }

        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
        [Display(Name = "Rating (0-5)")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Published year is required")]
        [Display(Name = "Published Year")]
        [Range(1000, 2024, ErrorMessage = "Year must be between 1000 and 2024")]
        public int PublishedYear { get; set; }

        [Display(Name = "Added Date")]
        public DateTime AddedDate { get; set; } = DateTime.Now;
    }
}