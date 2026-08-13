using System.ComponentModel.DataAnnotations;

namespace Project1_backend_.Models
{
    public class Submission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;


        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Education>? Education {  get; set; }
    }

};