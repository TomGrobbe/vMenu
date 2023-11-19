using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMenu.Shared.Objects
{
    public class LanguageObj
    {
        public string Code = "";
        public string Name = "";
    }

    public class LanguagesList
    {
        public static List<dynamic> List = new List<dynamic>()
        {
            "English",
            "German",
            "Spanish",
            "French",
            "Maori"
        };
    }
}
