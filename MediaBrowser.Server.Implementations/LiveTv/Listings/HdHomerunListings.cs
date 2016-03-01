using MediaBrowser.Controller.LiveTv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Model.LiveTv;
using System.Threading;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Common.Net;
using MediaBrowser.Common;
using System.IO;
using System.Collections.Concurrent;

namespace MediaBrowser.Server.Implementations.LiveTv.Listings
{
    public class HdHomerunListings : BaseListingsProvider, IListingsProvider
    {
        private readonly IHttpClient _httpClient;
        private readonly SemaphoreSlim _tokenSemaphore = new SemaphoreSlim(1, 1);
        private readonly IApplicationHost _appHost;

        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphoreLocks = new ConcurrentDictionary<string, SemaphoreSlim>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, GuideCache> _guideCache = new Dictionary<string, GuideCache>();

        private SemaphoreSlim GetLock(string url)
        {
            return _semaphoreLocks.GetOrAdd(url, key => new SemaphoreSlim(1, 1));
        }

        public HdHomerunListings(ILogger logger, IJsonSerializer jsonSerializer, IHttpClient httpClient, IApplicationHost appHost)
            : base(logger, jsonSerializer)
        {
            _httpClient = httpClient;
            _appHost = appHost;
        }
        public string Name
        {
            get { return "HD Homerun"; }
        }
        public static string ProviderType
        {
            get { return "hdhomerun"; }
        }
        public string Type
        {
            get { return ProviderType; }
        }

        public Task<List<NameIdPair>> GetLineups(ListingsProviderInfo info)
        {
            var lineup = new NameIdPair();
            lineup.Name = "HDHomerun at " + info.Path;
            lineup.Id = "native";
            return Task.FromResult(new List<NameIdPair> { lineup });
        }

        public Task Validate(ListingsProviderInfo info, bool validateLogin, bool validateListings)
        {
            return Task.FromResult(true);
        }

        protected override async Task<IEnumerable<ProgramInfo>> GetProgramsAsyncInternal(ListingsProviderInfo info, Station station, DateTime startDateUtc, DateTime endDateUtc, CancellationToken cancellationToken)
        {
            var wait = GetLock(info.Id);
            await wait.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (_guideCache[info.Id].Updated.AddSeconds(60) > DateTime.Now) { await GetStations(info, info.Id, cancellationToken); }
            wait.Release();            
            return _guideCache[info.Id].Data[station.Id];
        }

        protected override async Task<IEnumerable<Station>> GetStations(ListingsProviderInfo info, string lineup, CancellationToken cancellationToken)
        {
            _logger.Info("Pulling listings data with HDHomerun service for: " + info.Id);
            List<Station> stations = new List<Station>();
            _guideCache[info.Id] = new GuideCache();
            try
            {
                using (Stream responce = await _httpClient.Get(new HttpRequestOptions
                {
                    Url = string.Format("{0}/discover.json", info.Path),
                    CancellationToken = CancellationToken.None
                }))
                {
                    var deviceData = _jsonSerializer.DeserializeFromStream<HdHomerunDeviceData>(responce);
                    using (Stream guideResponce = await _httpClient.Get(new HttpRequestOptions
                    {
                        Url = string.Format("http://my.hdhomerun.com/api/guide.php?DeviceAuth={0}", deviceData.DeviceAuth),
                        CancellationToken = CancellationToken.None
                    }))
                    {
                        var channels = _jsonSerializer.DeserializeFromStream<List<HdHomerunChannelData>>(guideResponce);
                        channels.ForEach(c => {                           
                            stations.Add(new Station
                            {
                                Callsign = c.GuideName,
                                Name = c.GuideName,
                                Affiliate = c.Affiliate,
                                Id = c.GuideNumber,
                                Lineup = lineup,
                                ChannelNumbers = new List<string> { c.GuideNumber },
                                ImageUrl = c.ImageURL
                            });
                            _guideCache[info.Id].Data[c.GuideNumber] = GetChannelPrograms(c.Guide.ToList(), c.GuideNumber);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error getting channels:", ex);
            }
            _logger.Info("Found " + stations.Count + " Stations");
            return stations;
        }
        private DateTime GetDate(int value)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(value);
        }
        private List<ProgramInfo> GetChannelPrograms(List<HdHomerunChannelData.Program> guide, string channel)
        {
            var programs = new List<ProgramInfo>();
            guide.ForEach(g => {
                var filters = (g.Filter ?? new string[0]).ToList();
                var program = new ProgramInfo {
                    ChannelId = channel,
                    Id = channel + g.StartTime,
                    StartDate = GetDate(g.StartTime),
                    EndDate = GetDate(g.EndTime),
                    Name = g.Title,
                    OfficialRating = null,
                    CommunityRating = null,
                    EpisodeTitle = g.EpisodeTitle ?? "",
                    Audio = ProgramAudio.Stereo,
                    IsSeries = g.EpisodeNumber != null ? true : false,
                    ImageUrl = g.ImageURL,
                    IsKids = filters.Contains("Kids"),
                    IsSports = filters.Contains("Sports"),
                    IsMovie = filters.Contains("Movies"),
                    ShowId = g.SeriesID,
                    Overview = g.Synopsis
                };
                programs.Add(program);
            });
            return programs;
        }


    }
    public class GuideCache
    {
        public DateTime Updated;

        public Dictionary<string, List<ProgramInfo>> Data { get; set; }

        public GuideCache()
        {
            Updated = DateTime.Now;
            Data = new Dictionary<string, List<ProgramInfo>>();
        }

    }
    public class HdHomerunDeviceData
    {
        public string FriendlyName { get; set; }
        public string ModelNumber { get; set; }
        public string FirmwareName { get; set; }
        public string FirmwareVersion { get; set; }
        public string DeviceID { get; set; }
        public string DeviceAuth { get; set; }
        public int ConditionalAccess { get; set; }
        public string BaseURL { get; set; }
        public string LineupURL { get; set; }
    }


    public class HdHomerunChannelData
    {
        public string GuideNumber { get; set; }
        public string GuideName { get; set; }
        public Program[] Guide { get; set; }
        public string Affiliate { get; set; }
        public string ImageURL { get; set; }

        public class Program
        {
            public int StartTime { get; set; }
            public int EndTime { get; set; }
            public string Title { get; set; }
            public string Synopsis { get; set; }
            public int OriginalAirdate { get; set; }
            public string ImageURL { get; set; }
            public string SeriesID { get; set; }
            public string EpisodeTitle { get; set; }
            public string[] Filter { get; set; }
            public string EpisodeNumber { get; set; }
        }
    }



}
