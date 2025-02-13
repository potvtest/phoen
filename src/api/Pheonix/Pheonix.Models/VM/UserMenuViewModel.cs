using System.Collections.Generic;
namespace Pheonix.Models.VM
{
    public class UserMenuViewModel : IUserMenu
    {
        public int ID
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public bool IsActive
        {
            get;
            set;
        }

        public int? ParentID
        {
            get;
            set;
        }

        public string Action
        {
            get;
            set;
        }

        public string ImageUrl
        {
            get
            {
                return "https://avatars0.githubusercontent.com/u/125464?v=3&s=96";
            }
        }

        public string IconCSS
        {
            get;
            set;
        }

        public string CSS
        {
            get;
            set;
        }

        public bool IsPageLevelSubMenu
        {
            get;
            set;
        }

        public bool UseAsDefault
        {
            get;
            set;
        }

        public IEnumerable<IUserMenu> SubMenu
        {
            get;
            set;
        }
    }
}