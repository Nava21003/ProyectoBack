using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthAPI.Dtos
{
    public class UpdateUserDto
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public List<string> Roles { get; set; } = new List<string>();
    }
}
