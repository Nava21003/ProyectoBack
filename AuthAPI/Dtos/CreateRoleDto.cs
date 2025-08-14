using System.ComponentModel.DataAnnotations;

namespace AuthAPI.Dtos
{
    public class CreateRoleDto
    {
        [Required(ErrorMessage = "Role Name es required")]
        public string RoleName { get; set; } = null!;
    }
}
