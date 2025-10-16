using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

public class SampleUser : IdentityUser
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Surname { get; set; }

    [Required]
    public string Nickname { get; set; }

    [Required]
    public DateTime BirthDate { get; set; }

    [Required]
    public bool Enabled { get; set; } = true; 

    [Required]
    public DateTime CreationDate { get; set; } = DateTime.Now;
}