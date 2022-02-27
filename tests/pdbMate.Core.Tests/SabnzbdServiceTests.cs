using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using FluentAssertions;
using Microsoft.Extensions.Options;


namespace pdbMate.Core.Tests
{
    // To run these tests start a docker vm with sabnzbd
    //
    public class SabnzbdServiceTests
    {
        private ILogger<ISabnzbdService> sabnzbdServiceLogger;
        private IOptions<SabnzbdServiceOptions> nzbgetOptions;

        [SetUp]
        public void Setup()
        {
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            sabnzbdServiceLogger = loggerFactory.CreateLogger<ISabnzbdService>();

            var jsonString = File.ReadAllText(Path.Combine("settings", "sabnzbd.json"));
            nzbgetOptions = Options.Create(JsonSerializer.Deserialize<SabnzbdServiceOptions>(jsonString));
        }

        [Test]
        public void GetVersion()
        {
            ISabnzbdService service = new SabnzbdService(sabnzbdServiceLogger, nzbgetOptions);

            service.CheckConnection();
            service.GetVersion().Should().NotBeNullOrEmpty();

            service.GetQueue(0, 100).Should().NotBeNull();
            service.GetHistory(0, 100).Should().NotBeNull();
        }
    }
}