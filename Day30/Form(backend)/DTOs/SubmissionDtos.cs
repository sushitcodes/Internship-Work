using System.ComponentModel.DataAnnotations;

namespace Form.DTOs
{
    public class SubmissionDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public Guid ClassRoomId { get; set; }
        public string ClassRoomName { get; set; } = string.Empty;
        public int RollNo { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public string? SubmitterAvatarUrl { get; set; }
    }

    // HTTP-boundary DTO for POST /submissions.
    // Attributes are enforced by MVC before the action runs.
    // IValidatableObject covers rules attributes can't express (file length).
    public class CreateSubmissionFormRequest : IValidatableObject
    {
        [Required, StringLength(50, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public Guid ClassRoomId { get; set; }

        [Range(1, int.MaxValue)]
        public int RollNo { get; set; }

        [Required]
        public IFormFile? File { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (File is null || File.Length == 0)
            {
                yield return new ValidationResult(
                    "A file is required.",
                    new[] { nameof(File) });
            }
        }
    }

    // HTTP-boundary DTO for PUT /submissions/{id}.
    // File is OPTIONAL here — leaving it empty keeps the existing file.
    // A provided-but-empty file is still rejected as a client bug.
    public class UpdateSubmissionFormRequest : IValidatableObject
    {
        [Required, StringLength(50, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public Guid ClassRoomId { get; set; }

        [Range(1, int.MaxValue)]
        public int RollNo { get; set; }
        [Required]

        public IFormFile? File { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (File is not null && File.Length == 0)
            {
                yield return new ValidationResult(
                    "If a file is provided, it must not be empty.",
                    new[] { nameof(File) });
            }
        }
    }
}