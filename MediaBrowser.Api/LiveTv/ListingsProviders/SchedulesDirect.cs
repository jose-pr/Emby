
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.Querying;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Server.Implementations.LiveTv.Listings;

namespace MediaBrowser.Api.LiveTv.ListingsProviders
{
    [Route("/LiveTv/ListingProviders/SchedulesDirect/Countries", "GET", Summary = "Gets available Countries")]
    [Authenticated(AllowBeforeStartupWizard = true)]
    public class GetSchedulesDirectCountries
    {
    }

    [Route("/LiveTv/ListingProviders/SchedulesDirect/Headends", "GET", Summary = "Gets available lineups")]
    [Authenticated(AllowBeforeStartupWizard = true)]
    public class GetHeadends : IReturn<List<NameIdPair>>
    {
        [ApiMember(Name = "Id", Description = "Provider id", IsRequired = true, DataType = "string", ParameterType = "query", Verb = "GET")]
        public string Id { get; set; }
        [ApiMember(Name = "Country", Description = "Country", IsRequired = true, DataType = "string", ParameterType = "query", Verb = "GET")]
        public string Country { get; set; }
        [ApiMember(Name = "Location", Description = "Location/ZipCode", IsRequired = true, DataType = "string", ParameterType = "query", Verb = "GET")]
        public string Location { get; set; }

    }
    class SchedulesDirect : BaseApiService
    {
        private readonly ILiveTvManager _liveTvManager;
        private readonly IUserManager _userManager;
        private readonly IConfigurationManager _config;
        private readonly IHttpClient _httpClient;

        public SchedulesDirect(ILiveTvManager liveTvManager, IUserManager userManager, IConfigurationManager config, IHttpClient httpClient)
        {
            _liveTvManager = liveTvManager;
            _userManager = userManager;
            _config = config;
            _httpClient = httpClient;
        }

        public async Task<object> Get(GetSchedulesDirectCountries request)
        {
            // https://json.schedulesdirect.org/20141201/available/countries

            var response = await _httpClient.Get(new HttpRequestOptions
            {
                Url = "https://json.schedulesdirect.org/20141201/available/countries",
                CacheLength = TimeSpan.FromDays(1),
                CacheMode = CacheMode.Unconditional

            }).ConfigureAwait(false);

            return ResultFactory.GetResult(response, "application/json");
        }

        public async Task<object> Get(GetHeadends request)
        {
            var info = new List<NameIdPair>();
            var provider = _config.GetConfiguration<LiveTvOptions>("livetv").ListingProviders.FirstOrDefault( p => p.Id == request.Id);
            if (provider != null)
            {
                info = await Server.Implementations.LiveTv.Listings.SchedulesDirect.Instance.GetHeadends(
                    provider, request.Country, request.Location, CancellationToken.None).ConfigureAwait(false);
            }
            return ToOptimizedSerializedResultUsingCache(info);
        }
    }
}
 

    