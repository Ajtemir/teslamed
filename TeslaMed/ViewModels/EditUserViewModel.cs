using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TeslaMed.Models;

namespace TeslaMed.ViewModels
{
    public class EditUserViewModel
    {
        public int Id { get; set; }


        [Remote(action: "CheckLogin", controller: "Account", AdditionalFields = "Id")]
        [Required(ErrorMessage = "UserNameRequired")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "NameRequired")]
        public string Name { get; set; }

        [Required(ErrorMessage = "SurnameRequired")]
        public string Surname { get; set; }

        public string? Patronymic { get; set; }
        [Required(ErrorMessage = "AddressRequired")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Поле 'Номер телефона' обязательно для заполнения.")]
        public string PhoneNumber { get; set; }
      
        [Required(ErrorMessage = "DepartmentRequired")]
        public List<int> Departments { get; set; }
        public List<Department>? DepartmentsInUser { get; set; }
        public List<Department>? _Departments { get; set; }

        public List<int>? Specializations { get; set; }
        public List<Specialization>? SpecializationsInUser { get; set; }
        public List<Specialization>? _Specializations { get; set; }

        public JodTitle? Post { get; set; }
        public List<JodTitle>? _Posts { get; set; }
        [Required(ErrorMessage = "PostRequired")]
        public int PostId { get; set; }

        [Required(ErrorMessage = "RoleRequired")]

        public string Role { get; set; }
    }
}
