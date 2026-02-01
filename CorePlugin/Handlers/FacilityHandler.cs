using CorePlugin.EF;
using CorePlugin.Models;
using Microsoft.Extensions.Logging;
using Stella.Abstractions;
using Stella.Abstractions.Plugins;

namespace CorePlugin.Handlers
{
    public class FacilityHandler : StellaHandler
    {
        [StellaHandler("facility", "get", typeof(GetFacilityRequest))]
        public async Task<GetFacilityResponse> GetFacility()
        {
            var facilityRequest = Request as GetFacilityRequest;
            var config = PluginConfig as CorePluginConfig;
            Logger.LogDebug("Current mode: {mode}", config.RegisterMode);


            var context = new CoreContext();

            var facility = context.Facilities.FirstOrDefault(x => x.PCBId == PCBId);

            if (config.RegisterMode && facility is null)
            {
                var id = "STLA" + new string(DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString().TakeLast(4).ToArray());
                facility = (await context.Facilities.AddAsync(new()
                {
                    FacilityId = id,
                    PCBId = PCBId,
                    Country = "JP",
                    Region = "1",
                    Name = "STELLA",
                    Type = 0,
                    CountryName = "STELLA",
                    CountryJName = "STELLA",
                    RegionName = "UNIVERSE",
                    RegionJName = "UNIVERSE",
                    CustomerCode = id,
                    CompanyCode = id
                })).Entity;
            } else if (facility is null && !config.PrivateMode)
            {
                facility = new Facility()
                {
                    FacilityId = "STELLAGUEST",
                    PCBId = PCBId,
                    Country = "JP",
                    Region = "1",
                    Name = "STELLA",
                    Type = 0,
                    CountryName = "STELLA",
                    CountryJName = "STELLA",
                    RegionName = "GUEST",
                    RegionJName = "GUEST",
                    CustomerCode = "STELLAGUEST",
                    CompanyCode = "STELLAGUEST"
                };
            }
            else if (facility is null && config.PrivateMode)
            {
                throw new StellaHandlerException(400);
            }

            await context.SaveChangesAsync();


            return new GetFacilityResponse
            {
                Location = new Location
                {
                    Id = facility.FacilityId,
                    Country = facility.Country,
                    Region = facility.Region,
                    Name = facility.Name,
                    Type = Convert.ToByte(facility.Type),
                    CountryName = facility.CountryName,
                    CountryJName = facility.CountryJName,
                    RegionName = facility.RegionName,
                    RegionJName = facility.RegionJName,
                    CustomerCode = facility.CustomerCode,
                    CompanyCode = facility.CompanyCode,
                    Latitude = 1273,
                    Longitude = 361,
                    Accuracy = 0
                },
                Line = new Line
                {
                    Id = "3",
                    Class = 3
                },
                PortFw = new PortFw
                {
                    GlobalIp = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString(),
                    GlobalPort = 5700,
                    PrivatePort = 5700
                },
                Public = new Public
                {
                    Flag = 1,
                    Name = facility.Name,
                    Latitude = "1273",
                    Longitude = "361"
                },
                Share = new Share
                {
                    EaCoin = new EaCoin
                    {
                        NotchAmount = 0,
                        NotchCount = 0,
                        SupplyLimit = 100000
                    },
                    Url = new Url
                    {
                        EaPass = "STELLA-UNIVERSE",
                        ArcadeFan = "STELLA-UNIVERSE",
                        KonaminetDx = "STELLA-UNIVERSE",
                        KonamiId = "STELLA-UNIVERSE",
                        EaGate = "STELLA-UNIVERSE"
                    }
                }
            };
        }
    }
}
