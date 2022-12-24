﻿using Steamworks;

namespace RainMeadow
{
    public class OnlinePlayer : System.IEquatable<OnlinePlayer>
    {
        public CSteamID id;

        public OnlinePlayer(CSteamID id)
        {
            this.id = id;
        }
        public override bool Equals(object obj) => this.Equals(obj as OnlinePlayer);

        public bool Equals(OnlinePlayer other)
        {
            return other!= null && id == other.id;
        }

        public override int GetHashCode() => id.GetHashCode();


        public static bool operator ==(OnlinePlayer lhs, OnlinePlayer rhs)
        {
            return lhs is null ? rhs is null : lhs.Equals(rhs);
        }

        public static bool operator !=(OnlinePlayer lhs, OnlinePlayer rhs) => !(lhs == rhs);
    }
}
