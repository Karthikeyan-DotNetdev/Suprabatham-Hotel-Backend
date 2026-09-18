using System.Collections.Generic;

namespace laptop_service.Models.MASTERS
{
    public class RolePermissionItem
    {
        public long Permission_Id { get; set; }
        public string Role_Name { get; set; } = string.Empty;
        public string Category_Key { get; set; } = string.Empty; // 'DASHBOARD', 'POS', 'MASTERS', 'REPORTS', 'SETTINGS'
        public string Module_Key { get; set; } = string.Empty;
        public string Field_Key { get; set; } = "ACCESS";
        public string Display_Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Is_Allowed { get; set; }
        public int Display_Order { get; set; }
    }

    public class SaveRolePermissionsRequest
    {
        public string Role_Name { get; set; } = string.Empty;
        public string Updated_By { get; set; } = "ADMIN";
        public List<RolePermissionItem> Permissions { get; set; } = new List<RolePermissionItem>();
    }
}
