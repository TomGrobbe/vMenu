using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMenu.Shared.Enums
{
    public enum PermissionList
    {
        #region Everything else
        Everything,
        Staff,
        Open,
        NoClip,
        #endregion

        #region Player Options
        POAll,
        POMenu,
        #endregion

        #region Vehicle Options
        VOAll,
        VOMenu,
        #endregion

        #region World Related Options
        WRAll,
        WRMenu,
        #endregion

        #region Voice Chat
        VCAll,
        VCMenu,
        #endregion
    }
}
