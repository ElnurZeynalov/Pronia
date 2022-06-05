using System.ComponentModel.DataAnnotations;

namespace Pronia.ViewModel.Authorization
{
    public class RegisterVM
    {
        [Required,MaxLength(20)]
        public string FirstName { get; set; }
        [Required, MaxLength(30)]
        public string LastName { get; set; }
        [Required,DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required, MaxLength(30)]
        public string UserName { get; set; }
        [Required,DataType(DataType.Password)]
        public string Password { get; set; }
        [Required, DataType(DataType.Password),Compare(nameof(Password))]
        public string ConfrimPassword { get; set; }
    }
}
