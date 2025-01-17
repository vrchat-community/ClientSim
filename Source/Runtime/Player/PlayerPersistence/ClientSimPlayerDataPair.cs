using System;

#if VRC_ENABLE_PLAYER_PERSISTENCE
namespace VRC.SDK3.ClientSim.Persistence
{
    public class ClientSimPlayerDataPair
    {
        public string Key { get; set; }
        public ClientSimPlayerDataTypeUnion Value { get; set; }
        public DateTime LastUpdated;


        public ClientSimPlayerDataPair() {
            Key = null;
            Value = null;
            LastUpdated = DateTime.Now;
        }
    }
}
#endif