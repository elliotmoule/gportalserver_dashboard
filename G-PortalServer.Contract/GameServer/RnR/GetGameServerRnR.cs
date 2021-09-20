using CODE.Framework.Services.Contracts;
using System.Runtime.Serialization;

namespace G_PortalServer.Contract
{
    [DataContract]
    public class GetGameServerRequest
    {
        [DataMember(IsRequired = true)] public string QueryCode { get; set; } = string.Empty;
    }

    [DataContract]
    public class GetGameServerResponse : BaseServiceResponse
    {
        [DataMember(IsRequired = true)] public GameServerInformation Server { get; set; } = new GameServerInformation();
    }
}
