// System Libraries //
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// CitizenFX Libraries //
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FxEvents.Shared.Snowflakes;
using FxEvents;

namespace vMenu.Server
{
    public class Main : BaseScript
    {
        public Main()
        {
            //EventDispatcher.Initalize("vMenu:Inbound", "vMenu:Outbound", "vMenu:Signature");
            Debug.WriteLine("vMenu has started.");
        }
    }
}
