namespace laptop_service.Models
{
    public class ControlPanelMenu
    {
        public string? HomeMenu { get; set; }
        public string? groupIndex { get; set; }
        public string? Display_Order { get; set; }
        public string? Is_Active { get; set; }
    }

    public class SubMenu
    {
        public string? HomeMenu { get; set; }
        public string? Module { get; set; }
        public string? groupIndex { get; set; }
        public string? RouterLink { get; set; }
        public string? SubMenuIcon { get; set; }
        public string? SubMenuImg { get; set; }
        public string? Menu { get; set; }

        public string? Is_Active { get; set; }

        public string? Display_Order { get; set; }
    }

    public class MenuAccess
    {
        public string? module { get; set; }
        public string? roleid { get; set; }
        public string? opFormView { get; set; }
        public string? opFullControl { get; set; }
        public string? opInsert { get; set; }
        public string? opEdit { get; set; }
        public string? opDelete { get; set; }
        public string? opExport { get; set; }
        public string? opView { get; set; }
        public string? opPrint { get; set; }
    }
}
