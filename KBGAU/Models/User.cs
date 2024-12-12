using System.ComponentModel.DataAnnotations;

namespace KBGAU.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Логин обязателен для заполнения")]
        public required string Login { get; set; }
        [Required]
        public required string Password { get; set; }
        [Required]
        public required string FirstName { get; set; } 
        [Required]
        public required string MiddleName { get; set; }
        [Required]
        public required string LastName { get; set; }
    }
    
    


}