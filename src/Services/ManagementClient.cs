using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi.Paging;
using Maincotech.Utilities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Auth0app
{
    public class ManagementClient
    {
        private readonly ApiSettings _apiSettings;
        private ManagementApiClient _apiClient;
        private TokenResponse _currentToken;
        private DateTime _tokenRetrievedOn;
        private readonly IDistributedCache _cache;
        private static string _rulesCacheKey = "Rules";
        private static string _regex01 = @"context.clientName === '\w+'";
        private static string _regex02 = @"context.clientName !== '\w+'";

        public ManagementClient(IOptions<ApiSettings> options, IDistributedCache distributedCache)
        {
            _apiSettings = options.Value;
            _cache = distributedCache;
        }

        private async Task<ManagementApiClient> GetAuthorizedClient()
        {
            if (_currentToken != null &&
                _tokenRetrievedOn.AddSeconds(_currentToken.ExpiresIn) > DateTime.UtcNow)
            {
                if (_apiClient != null)
                {
                    return _apiClient;
                }
            }
            await GetAccessToken();

            _apiClient = new ManagementApiClient(_currentToken.AccessToken, _apiSettings.Domain);
            return _apiClient;
        }

        private async Task GetAccessToken()
        {
            var started = DateTime.UtcNow;
            string uri = $"https://{_apiSettings.Domain}/oauth/token";
            var parameters = new Dictionary<string, string>()
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", _apiSettings.ClientId },
                    { "client_secret", _apiSettings.ClientSecret },
                    { "audience", $"https://{_apiSettings.Domain}/api/v2/" }
                };
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new FormUrlEncodedContent(parameters)
            };
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            _tokenRetrievedOn = started;
            var json = await response.Content.ReadAsStringAsync();
            _currentToken = JsonSerializer.Deserialize<TokenResponse>(json);
        }

        public async Task<IPagedList<Client>> GetAppsAsync(int pageNo, int pageSize)
        {
            var client = await GetAuthorizedClient();
            var getRequest = new GetClientsRequest { Fields = "name,description,client_id,app_type,logo_uri" };
            var paginationInfo = new PaginationInfo(pageNo, pageSize, true);
            return await client.Clients.GetAllAsync(getRequest, paginationInfo);
        }
        public async Task<IPagedList<Rule>> GetRulesAsync(int pageNo, int pageSize)
        {
            var client = await GetAuthorizedClient();
            var getRequest = new GetRulesRequest();
            var paginationInfo = new PaginationInfo(pageNo, pageSize, true);
            return await client.Rules.GetAllAsync(getRequest, paginationInfo);
        }

        public async Task<Client> GetApplicationAsync(string id)
        {
            var client = await GetAuthorizedClient();
            var result = await client.Clients.GetAsync(id);
            return result;
        }

        public async Task<IEnumerable<Rule>> GetApplicationRules(string appName)
        {
            var result = new List<Rule>();
            var rules = await GetAllRulesAsync();
            foreach (var rule in rules)
            {
                //Check is in the white list.
                var match = Regex.Match(rule.Script, _regex01, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    foreach(Capture m in match.Captures)
                    {
                        if (m.Value.Contains($"'{appName}'", StringComparison.OrdinalIgnoreCase))
                        {
                            result.Add(rule);
                            break;
                        }
                    }                 
                }
                else
                {
                    match = Regex.Match(rule.Script, _regex02, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        var isInExcluedList = false;
                        foreach (Capture m in match.Captures)
                        {
                            if (m.Value.Contains($"'{appName}'", StringComparison.OrdinalIgnoreCase))
                            {
                                isInExcluedList = true;
                                break;
                            }
                        }
                        if(isInExcluedList)
                        {
                            result.Add(rule);
                        }
                    }
                }
               
            }
            return result;
        }

        public async Task<List<Rule>> GetAllRulesAsync()
        {
            var bytes = await _cache.GetStringAsync(_rulesCacheKey);
            if (bytes != null)
            {
                return SerializerHelper.DeserializeFromXmlString<List<Rule>>(bytes);
            }
            const int size = 100;
            int pageNo = 0;
            var result = new List<Rule>();
            var rules = await GetRulesAsync(pageNo, size);
            result.AddRange(rules);
            while (result.Count < rules.Paging.Total)
            {
                pageNo++;
                rules = await GetRulesAsync(pageNo, size);
                result.AddRange(rules);
            }
            bytes = SerializerHelper.SerializeToXmlString(result);
            await _cache.SetStringAsync(_rulesCacheKey, bytes,
                new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(5) });
            return result;
        }
    }

    public class ApiSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Domain { get; set; }
        public string ConsoleBaseUrl { get; set; }
    }

    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
    }
}