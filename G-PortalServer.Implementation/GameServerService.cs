using CODE.Framework.Services.Contracts;
using G_PortalServer.Contract;
using Newtonsoft.Json;

namespace G_PortalServer.Implementation
{
    public class GameServerService : IGameServerService
    {
        public GetGameServerResponse GetGameServer(GetGameServerRequest request)
        {
            try
            {
                var response = new GetGameServerResponse();
                var client = new HttpClient();
                HttpResponseMessage httpResponse = client.GetAsync($"https://api.g-portal.us/gameserver/query/{request.QueryCode}").Result;

                if (httpResponse != null && httpResponse.IsSuccessStatusCode)
                {
                    var result = httpResponse.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        response.Server = JsonConvert.DeserializeObject<GameServerInformation>(result);

                        if (!response.Server.Online && httpResponse.IsSuccessStatusCode && response.Server.Queried)
                        {
                            response.Server.Online = true;
                        }
                    }
                }

                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                return ServiceHelper.GetPopulatedFailureResponse<GetGameServerResponse>(ex);
            }
        }
    }
}