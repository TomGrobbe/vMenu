using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace vMenu.Shared.Objects
{
    public struct OnlinePlayersCB
    {
        public Player Player;
        public string? SteamIdentifier;
        public string? DiscordIdentifier;
    }

    public struct Player
    {
        public string Name;
        public int ServerId;
        public int LocalId;
        public int CharacterHandle;
    }
}