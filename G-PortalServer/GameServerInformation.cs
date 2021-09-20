using System.Runtime.Serialization;

namespace G_PortalServer
{
    [DataContract]
    public class GameServerInformation
    {
        [DataMember(IsRequired = true)] public bool Online { get; set; } = false;
        [DataMember(IsRequired = true)] public int MaxPlayers { get; set; } = 0;
        [DataMember(IsRequired = true)] public string Name { get; set; } = string.Empty;
        [DataMember(IsRequired = true)] public string IPAddress { get; set; } = string.Empty;
        [DataMember(IsRequired = true)] public int Port { get; set; } = 0;
        [DataMember(IsRequired = true)] public string Key { get; set; } = string.Empty;
        [DataMember(IsRequired = true)] public int CurrentPlayers { get; set; } = 0;
        [DataMember(IsRequired = true)] public bool Queried { get; set; } = false;
        [DataMember(IsRequired = true)] public bool Cached { get; set; } = false;
    }
}
