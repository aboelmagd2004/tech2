using System.ComponentModel.DataAnnotations;

namespace TechXpress.Web.Areas.Admin.Models.Role
{
    public class CreateRoleViewModel
    {
        [Required(ErrorMessage = "Role name is required.")]
        [StringLength(256, MinimumLength = 3, ErrorMessage = "Role name must be between 3 and 256 characters.")]
        public string RoleName { get; set; }  // The name of the role to be created

    }
}
