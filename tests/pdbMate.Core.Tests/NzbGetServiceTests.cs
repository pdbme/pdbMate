using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using FluentAssertions;
using Microsoft.Extensions.Options;
using pdbMate.Core.Interfaces;

namespace pdbMate.Core.Tests
{
    // To run these tests start a docker vm with nzbget
    //
    // e.g.
    // docker run --name=nzbget -p 6789:6789 linuxserver/nzbget:latest
    public class NzbGetServiceTests
    {
        private ILogger<INzbgetService> nzbgetServiceLogger;
        private IOptions<NzbgetServiceOptions> nzbgetOptions;

        private const string Sitename = "5KP" + "orn";

        [SetUp]
        public void Setup()
        {
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            nzbgetServiceLogger = loggerFactory.CreateLogger<INzbgetService>();

            var jsonString = File.ReadAllText(Path.Combine("settings", "nzbget.json"));
            nzbgetOptions = Options.Create(JsonSerializer.Deserialize<NzbgetServiceOptions>(jsonString));
        }

        [Test]
        public void GetVersion()
        {
            INzbgetService service = new NzbgetService(nzbgetServiceLogger, nzbgetOptions);

            service.CheckConnection();
            service.GetVersion().Should().BeGreaterThanOrEqualTo(1);

            service.GetQueue().Should().NotBeNull();
            service.GetHistory().Should().NotBeNull();
            //service.AddDownload();
            /*
            var resultList = pdbApiService.GetSites();
            resultList.Should().NotBeNull();
            resultList.Count.Should().BeGreaterThan(0);
            resultList.First(x => x.Sitename.Equals(Sitename)).Id.Should().Be(606, "Site should be found in results searched by sitename.");
            resultList.First(x => x.Id == 606).Sitename.Should().Be("5KPorn", "Site should be found in results searched by id.");
            */
        }
        /*
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
        */
    }
}