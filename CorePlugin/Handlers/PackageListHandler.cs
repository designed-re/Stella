using CorePlugin.Models;
using Stella.Abstractions.Plugins;

namespace CorePlugin.Handlers
{
    public class PackageListHandler : StellaHandler
    {
        [StellaHandler("package", "list", typeof(GetPackageListRequest))]
        public async Task<GetPackageListResponse> GetPackageList()
        {
            var packageListRequest = Request as GetPackageListRequest;

            return new GetPackageListResponse
            {
                Expire = 600,
                // TODO: Add package list items here
            };
        }
    }
}
