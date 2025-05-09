using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TechXpress.Web.Areas.Admin.Models.Role
{
    public class AssignRoleViewModel
    {
        public string UserId { get; set; }

        public string? UserName { get; set; }

        public List<string>? ExistingRoles { get; set; } = new List<string>();


        public List<string> RolesToAdd { get; set; } 

        public List<IdentityRole>? AvailableRoles { get; set; } = new List<IdentityRole>();
    }

}
