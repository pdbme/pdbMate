using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pdbMate.Core.Data.Nzbget;
using pdbMate.Core.Interfaces;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.Json;

namespace pdbMate.Core
{
    public class NzbgetService : INzbgetService
    {
        private readonly ILogger<INzbgetService> logger;
        private NzbgetServiceOptions options;
        private RestClient client;

        public NzbgetService(ILogger<INzbgetService> logger, IOptions<NzbgetServiceOptions> options)
        {
            this.logger = logger;

            setOptions(options);
        }

        public void setOptions(IOptions<NzbgetServiceOptions> optionsToSet)
        {
            options = optionsToSet.Value;

            var authString = "";
            if (!string.IsNullOrEmpty(this.options.Username) && !string.IsNullOrEmpty(this.options.Password))
            {
                authString = this.options.Username + ":" + this.options.Password + "@";
            }

            if (string.IsNullOrEmpty(this.options.Hostname))
            {
                return;
            }

            var baseUrl = (this.options.UseHttps ? "https://" : "http://") + authString + this.options.Hostname + ":" +
                          this.options.Port;

            client = new RestClient(baseUrl);
            client.UseSystemTextJson(new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            });

            if (!string.IsNullOrEmpty(this.options.Username) || !string.IsNullOrEmpty(this.options.Password))
            {
                client.Authenticator = new HttpBasicAuthenticator(this.options.Username, this.options.Password);
            }
        }

        public void CheckConnection()
        {
            ValidateSettings();
            GetVersion();
        }

        public int GetVersion()
        {
            var request = new RestRequest("jsonrpc/version", Method.Get);

            var response = client.ExecuteGetAsync<NzbgetResult<string>>(request).GetAwaiter().GetResult();
            if (response.IsSuccessful)
            {
                string versionData = response.Data.Version;

                if (versionData == null)
                {
                    throw new ApplicationException("No version returned.");
                }

                return (double.TryParse(versionData, NumberStyles.Any, CultureInfo.InvariantCulture, out var versionNumber) ? (int)Math.Round(versionNumber) : 0);
            }

            var errormessage = "nzbget returned " + response.StatusCode.ToString() + " HTTP-Status-Code. " + response.ErrorMessage;
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                errormessage += " Hint: nzbget probably needs username and password for authentication.";
            }

            throw new ApplicationException(errormessage);
        }

        public List<NzbgetQueue> GetQueue()
        {
            var request = new RestRequest("jsonrpc/listgroups", Method.Get);

            var response = client.GetAsync<NzbgetResult<List<NzbgetQueue>>>(request).GetAwaiter().GetResult();
            return response.Result;
        }

        public List<NzbgetHistory> GetHistory()
        {
            var request = new RestRequest("jsonrpc/history", Method.Get);

            var response = client.GetAsync<NzbgetResult<List<NzbgetHistory>>>(request).GetAwaiter().GetResult();
            var data = response;

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
            var request = new RestRequest("jsonrpc/append", Method.Post);
            request.AddJsonBody(new NzbgetRequestAddDownload()
            {
                Method = "append",
                Id = 0,
                Params = paramsArray
            });

            var response = client.PostAsync<NzbgetResultAddDownload>(request).GetAwaiter().GetResult();
            if (response.Result > 0)
            {
                return true;
            }

            return false;
        }

        private void ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(options.Hostname))
            {
                throw new ArgumentException("Hostname for nzbget is empty.");
            }

            if (options.Hostname.StartsWith("http"))
            {
                throw new ArgumentException($"Given hostname {options.Hostname} should not contain http oder https. Valid example: myapp.mydomain.com or 176.56.59.1");
            }

            if (options.Port == 0)
            {
                throw new ArgumentException($"Please provide a valid port for nzbget.");
            }
        }
    }
}
