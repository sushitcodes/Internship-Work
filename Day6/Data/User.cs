using System.ComponentModel.DataAnnotations;
public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength (100)]
    public string Email { get; set; }


    [Range(1, 120)]
    public int Age {  get; set; }

    public DateTime CreatedAt {  get; set; }= DateTime.Now;
}