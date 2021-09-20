using CODE.Framework.Services.Contracts;
using System.ServiceModel;
namespace G_PortalServer.Contract
{
    [ServiceContract]
    public interface IGameServerService
    {
        [OperationContract, Rest(Method = RestMethods.Get)]
        GetGameServerResponse GetGameServer(GetGameServerRequest request);
    }
}