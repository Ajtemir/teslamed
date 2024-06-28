using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TeslaMed.ViewModels
{
    public class ChangePasswordViewModel
    {
        public int Id { get; set; }
      
        [Required(ErrorMessage = "PasswordRequired")]
        [Remote(action: "PasswordCheck", controller: "Account")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "PasswordRequired")]
        [Compare("Password", ErrorMessage = "PasswordCompare")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
