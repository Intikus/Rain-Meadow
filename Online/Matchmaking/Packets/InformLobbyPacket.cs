using System.IO;

namespace RainMeadow
{
    public class InformLobbyPacket : Packet
    {
        public int currentplayercount = default;
        public int maxplayers = default;
        public bool passwordprotected = default;
        public string name = "";
        public string mode = "";
        public string mods = "";
        public string bannedMods = "";

        public InformLobbyPacket(): base() {}
        public InformLobbyPacket(int maxplayers, string name, bool passwordprotected, string mode, int currentplayercount, string highImpactMods = "", string bannedMods = "")
        {
            this.currentplayercount = currentplayercount;
            this.mode = mode;
            this.maxplayers = maxplayers;
            this.name = name;
            this.passwordprotected = passwordprotected;
            this.mods = highImpactMods;
            this.bannedMods = bannedMods;
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(maxplayers);
            writer.Write(currentplayercount);
            writer.Write(passwordprotected);
            writer.Write(name);
            writer.Write(mode);
            writer.Write(mods);
            writer.Write(bannedMods);
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            maxplayers = reader.ReadInt32();
            currentplayercount = reader.ReadInt32();
            passwordprotected = reader.ReadBoolean();
            name = reader.ReadString();
            mode = reader.ReadString();
            mods = reader.ReadString();
            bannedMods = reader.ReadString();
        }


        public override Type type => Type.InformLobby;

        public override void Process()
        {
            if (MatchmakingManager.currentDomain != MatchmakingManager.MatchMakingDomain.LAN) return;
            RainMeadow.DebugMe();
            var lobbyinfo = MakeLobbyInfo();
            (MatchmakingManager.instances[MatchmakingManager.MatchMakingDomain.LAN] as LANMatchmakingManager).addLobby(lobbyinfo);
        }

        public LANMatchmakingManager.LANLobbyInfo MakeLobbyInfo() {
            return new LANMatchmakingManager.LANLobbyInfo(
                (processingPlayer.id as LANMatchmakingManager.LANPlayerId).endPoint, name, mode, currentplayercount, passwordprotected, maxplayers, mods, bannedMods); 
        }

    }
}