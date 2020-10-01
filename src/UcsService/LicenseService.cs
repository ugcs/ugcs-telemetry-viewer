using UGCS.Sdk.Protocol.Encoding;

namespace UGCS.UcsServices
{
    public class LicenseService
    {
        private readonly ConnectionService _connectionService;

        public LicenseService(ConnectionService cs)
        {
            _connectionService = cs;
        }

        public bool HasVideoPlayerPermission()
        {
            GetLicenseRequest request = new GetLicenseRequest
            {
                ClientId = _connectionService.GetClientId()
            };

            GetLicenseResponse response = _connectionService.Execute<GetLicenseResponse>(request);
            return response.LicensePermissions.UgcsVideoPlayer;
        }
    }
}
