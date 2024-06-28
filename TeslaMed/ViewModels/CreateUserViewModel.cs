using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TeslaMed.Models;

namespace TeslaMed.ViewModels
{
    public class CreateUserViewModel
    {
        [Remote(action: "CheckLogin", controller: "Account", ErrorMessage = "UserNameRemote")]
        [Required(ErrorMessage = "UserNameRequired")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "NameRequired")]
        public string Name { get; set; }
        [Required(ErrorMessage = "SurnameRequired")]
        public string Surname { get; set; }
        public string? Patronymic { get; set; }

        [Required(ErrorMessage = "PasswordRequired")]
        [Remote(action: "PasswordCheck", controller: "Account")]
        [Display(Name = "PasswordDisplay")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "ConfirmRequired")]
        [Compare("Password", ErrorMessage = "PasswordCompare")]
        [Display(Name = "ConfirmDisplay")]
        [DataType(DataType.Password)]
        public string PasswordConfirm { get; set; }

        [Required(ErrorMessage = "AddressRequired")]
        public string Address { get; set; }
      
        public List<int>? Specializations { get; set; }
      
        [Required]
        public List<int> Departments { get; set; }
      
        [Required(ErrorMessage = "PostRequired")]
        public int Post { get; set; }
        [Required(ErrorMessage = "Введите номер телефона!")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "RoleRequired")]
        public string Role { get; set; }
        public bool Active { get; set; }

        public List<Specialization>? _Specializations { get; set; }
        public List<Department>? _Departments { get; set; }
        public List<JodTitle>? _Posts { get; set; }

    }
}
