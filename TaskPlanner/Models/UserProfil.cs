using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("userprofile")]
public class UserProfile
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid? UserId { get; set; } // Powiązanie z AspNetUsers.Id

    [Column("firstname")] 
    public string? FirstName { get; set; }

    [Column("lastname")]
    public string? LastName { get; set; }
}
