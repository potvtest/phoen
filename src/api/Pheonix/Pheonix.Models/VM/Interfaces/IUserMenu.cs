using System.Collections.Generic;
namespace Pheonix.Models.VM
{
    public interface IUserMenu
    {
        int ID { get; set; }
        string Name { get; set; }
        bool IsActive { get; set; }
        int? ParentID { get; set; }
        string Action { get; set; }
        string ImageUrl { get; }
        string IconCSS { get; set; }
        string CSS { get; set; }
        bool IsPageLevelSubMenu { get; set; }
        bool UseAsDefault { get; set; }

        IEnumerable<IUserMenu> SubMenu { get; set; }
    }
}