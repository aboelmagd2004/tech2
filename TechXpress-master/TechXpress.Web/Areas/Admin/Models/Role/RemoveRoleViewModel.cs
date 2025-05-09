namespace TechXpress.Web.Areas.Admin.Models.Role
{
    public class RemoveRoleViewModel
    {
        public string UserId { get; set; }
        public string? UserName { get; set; }
        public List<string> Roles { get; set; } = new List<string>();  // Initialize to an empty list
        public List<string> RolesToRemove { get; set; } = new List<string>();  // Initialize to an empty list
    }

}
