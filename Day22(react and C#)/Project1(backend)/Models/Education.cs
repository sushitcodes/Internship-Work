using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Project1_backend_.Models
{
    public class Education
    {

        [Key]
        public int Id { get; set; }


        [Required]
        [StringLength(100)]
        public string Degree { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        [RegularExpression(@"^[0-9]{4}$", ErrorMessage = "Year must be 4 digits")]
        public string Year { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]

        public string School {  get; set; } = string.Empty;
        //foreign key
        public int SubmissionId { get; set; }

        [JsonIgnore]
        public Submission? Submission { get; set; }
    }
}
