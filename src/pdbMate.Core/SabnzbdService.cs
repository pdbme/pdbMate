using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pdbMate.Core.Data;
using pdbMate.Core.Data.Nzbget;
using pdbMate.Core.Data.Sabnzbd;
using RestSharp;
using RestSharp.Serializers.SystemTextJson;

namespace pdbMate.Core
{
    public class SabnzbdService : ISabnzbdService
    {
        private readonly ILogger<ISabnzbdService> logger;
        private readonly SabnzbdServiceOptions options;
        private readonly RestClient client;
        private readonly string baseUrl;

        public SabnzbdService(ILogger<ISabnzbdService> logger, IOptions<SabnzbdServiceOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;

            baseUrl = this.options.Url;

            client = new RestClient(baseUrl);
            client.UseSystemTextJson(new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            });
        }

        public bool CheckConnection()
        {
            var version = GetVersion();
            logger.LogInformation($"Connection to {baseUrl} was {(string.IsNullOrEmpty(version) ? "successful" : "not successful")} - Version: {version}");
            return !string.IsNullOrEmpty(version);
        }

        public string GetVersion()
        {
            var request = new RestRequest("/", DataFormat.Json);
            request.AddQueryParameter("mode", "version");
            request.AddQueryParameter("output", "json");

            var response = client.Get<SabnzbdResult>(request);
            var versionData = response.Data;

            return versionData == null ? "" : versionData.Version;
        }

        public SabnzbdQueue GetQueue(int start, int limit)
        {
            var request = new RestRequest("/", DataFormat.Json);
            request.AddQueryParameter("mode", "queue");
            request.AddQueryParameter("output", "json");
            request.AddQueryParameter("apikey", options.ApiKey);
            request.AddQueryParameter("start", start.ToString());
            request.AddQueryParameter("limit", limit.ToString());

            var response = client.Get<SabnzbdResult>(request);
            var data = response.Data;

            return data?.Queue;
        }

        public SabnzbdHistory GetHistory(int start, int limit)
        {
            var request = new RestRequest("/", DataFormat.Json);
            request.AddQueryParameter("mode", "history");
            request.AddQueryParameter("output", "json");
            request.AddQueryParameter("apikey", options.ApiKey);
            request.AddQueryParameter("start", start.ToString());
            request.AddQueryParameter("limit", limit.ToString());

            var response = client.Get<SabnzbdResult>(request);
            var data = response.Data;

            return data?.History;
        }
    }
}
