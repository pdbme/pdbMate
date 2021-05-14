using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pdbMate.Core.Data;
using pdbMate.Core.Data.Nzbget;
using RestSharp;
using RestSharp.Serializers.SystemTextJson;

namespace pdbMate.Core
{
    public class NzbgetService : INzbgetService
    {
        private readonly ILogger<INzbgetService> logger;
        private readonly NzbgetServiceOptions options;
        private readonly RestClient client;
        private readonly string baseUrl;

        public NzbgetService(ILogger<INzbgetService> logger, IOptions<NzbgetServiceOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;

            var authString = "";
            if (!string.IsNullOrEmpty(this.options.Username) && !string.IsNullOrEmpty(this.options.Password))
            {
                authString = this.options.Username + ":" + this.options.Password + "@";
            }

            baseUrl = (this.options.UseHttps ? "https://" : "http://") + authString + this.options.Hostname + ":" +
                          this.options.Port;

            client = new RestClient(baseUrl);
            client.UseSystemTextJson(new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            });
        }

        public bool CheckConnection()
        {
            int version = GetVersion();
            logger.LogInformation($"Connection to {baseUrl} was {(version > 0 ? "successful" : "not successful")} - Version: {version}");
            return version > 0;
        }

        public int GetVersion()
        {
            var request = new RestRequest("jsonrpc/version", DataFormat.Json);

            var response = client.Get<NzbgetResult<string>>(request);
            var versionData = response.Data;

            if (versionData == null)
                return 0;

            return (double.TryParse(versionData.Result, NumberStyles.Any, CultureInfo.InvariantCulture, out var versionNumber) ? (int) Math.Round(versionNumber) : 0);
        }

        public List<NzbgetQueue> GetQueue()
        {
            var request = new RestRequest("jsonrpc/listgroups", DataFormat.Json);

            var response = client.Get<NzbgetResult<List<NzbgetQueue>>>(request);
            return response?.Data?.Result;
        }

        public List<NzbgetHistory> GetHistory()
        {
            var request = new RestRequest("jsonrpc/history", DataFormat.Json);

            var response = client.Get<NzbgetResult<List<NzbgetHistory>>>(request);
            var data = response.Data;

            return data?.Result;
        }

        public bool AddDownload(string url)
        {
            var paramsArray = new object[] {
                "",
                url,
                "",
                0,
                false,
                false,
                "",
                0,
                "FORCE"
            };
            var request = new RestRequest("jsonrpc/append", DataFormat.Json);
            request.AddJsonBody(new NzbgetRequestAddDownload()
            {
                Method = "append",
                Id = 0,
                Params = paramsArray
            });

            var response = client.Post<NzbgetResultAddDownload>(request);
            if (response.Data.Result > 0)
            {
                return true;
            }

            return false;
        }

        public bool IsActive()
        {
            return options.IsActive;
        }
    }
}
