using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using FluentAssertions;
using Microsoft.Extensions.Options;


namespace pdbMate.Core.Tests
{
    public class PdbApiServiceTests
    {
        private ILogger<IPdbApiService> pdbApiServiceLogger;
        private IOptions<PdbApiServiceOptions> pdbApiOptions;

        private const string Sitename = "5KP" + "orn";

        [SetUp]
        public void Setup()
        {
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            pdbApiServiceLogger = loggerFactory.CreateLogger<IPdbApiService>();

            var jsonString = File.ReadAllText(Path.Combine("settings", "pdbapi.json"));
            PdbApiServiceOptions optionsPdbApi = JsonSerializer.Deserialize<PdbApiServiceOptions>(jsonString);

            pdbApiOptions = Options.Create(optionsPdbApi);
        }

        [Test]
        public void GetSites()
        {
            IPdbApiService pdbApiService = new PdbApiService(pdbApiServiceLogger, pdbApiOptions);
            var resultList = pdbApiService.GetSites();
            resultList.Should().NotBeNull();
            resultList.Count.Should().BeGreaterThan(0);
            resultList.First(x => x.Sitename.Equals(Sitename)).Id.Should().Be(606, "Site should be found in results searched by sitename.");
            resultList.First(x => x.Id == 606).Sitename.Should().Be("5KPorn", "Site should be found in results searched by id.");
        }

        [Test]
        public void GetVideos()
        {
            IPdbApiService pdbApiService = new PdbApiService(pdbApiServiceLogger, pdbApiOptions);
            var sites = pdbApiService.GetSites();
            sites.Should().NotBeNull();
            var site = sites.First(x => x.Sitename.Equals(Sitename));

            var videos = pdbApiService.GetVideosBySite(site);
            videos.Should().NotBeNull();
            videos.Count.Should().BeGreaterThan(0);
            var videoMishaMaver = videos.FirstOrDefault(x => x.Title.Contains("Misha Maver"));
            videoMishaMaver.Should().NotBeNull();
            videoMishaMaver?.Site.Should().NotBeNull();
            videoMishaMaver?.Site.Id.Should().Be(site.Id);
            videoMishaMaver?.Id.Should().BeGreaterThan(0);
            videoMishaMaver?.Title.Should().Be("Misha Maver");
            videoMishaMaver?.Actors.Should().NotBeNull();
            videoMishaMaver?.Actors.Should().HaveCountGreaterThan(0);
            videoMishaMaver?.Actors?.First().Actorname.Should().Be("Misha Maver");
        }
    }
}