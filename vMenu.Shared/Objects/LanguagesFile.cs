using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMenu.Shared.Objects
{
    public struct LanguagesFile
    {
        public Dictionary<string, Menu> Menus;
        public Notifications Notifications;
    }

    public struct Menu
    {
        public string Subtitle;
        public Dictionary<string, MenuItem>? Items;
    }

    public struct MenuItem
    {
        public string? Name;
        public string? Description;
        public dynamic? DynamicDetails;
    }

    public struct Notifications
    {
        // You can add properties here if there are any notifications
    }
}
