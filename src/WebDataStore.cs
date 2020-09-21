using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nito.Disposables;
using OpenMod.API;
using OpenMod.API.Persistence;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OpenMod.WebDataStore
{
    public class WebDataStore : IDataStore
    {
        private readonly string _componentId;
        private readonly IConfiguration _configuration;
        private readonly IDataStore _baseDataStore;
        private readonly ILogger<WebDataStore> _logger;
        private readonly HttpClient _client;

        private readonly ISerializer m_Serializer;
        private readonly IDeserializer m_Deserializer;

        public WebDataStore(string componentId, IConfiguration configuration, IDataStore baseDataStore, ILogger<WebDataStore> logger)
        {
            _componentId = componentId;
            _configuration = configuration;
            _baseDataStore = baseDataStore;
            _logger = logger;
            _client = new HttpClient();

            m_Serializer = new SerializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .EmitDefaults()
                .Build();

            m_Deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();
        }

        public async Task SaveAsync<T>(string key, T data) where T : class
        {
            var configuration = GetConfiguration(key);
            if (configuration == null)
            {
                _logger.LogInformation($"Saving data file locally: {_componentId}:{key}");
                await _baseDataStore.SaveAsync(key, data);
                return;
            }

            var url = GetUrl(key);
            var serializedYaml = m_Serializer.Serialize(data);
            var response = await _client.PostAsync(url, new StringContent(serializedYaml, Encoding.UTF8));
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Server responded with {(int)response.StatusCode} - {response.StatusCode} for URL: PUT {url}");
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            var configuration = GetConfiguration(key);
            if (configuration == null)
            {
                return await _baseDataStore.ExistsAsync(key);
            }

            var url = GetUrl(key);
            var response = await _client.GetAsync(url);
            return response.IsSuccessStatusCode;
        }

        public async Task<T> LoadAsync<T>(string key) where T : class
        {
            var configuration = GetConfiguration(key);
            if (configuration == null)
            {
                _logger.LogInformation($"Loading data file locally: {_componentId}:{key}");
                return await _baseDataStore.LoadAsync<T>(key);
            }

            var url = GetUrl(key);
            var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Server responded with {(int)response.StatusCode} - {response.StatusCode} for URL: GET {url}");
            }

            var serializedYaml = await response.Content.ReadAsStringAsync();
            return m_Deserializer.Deserialize<T>(serializedYaml);
        }

        private IConfiguration GetConfiguration(string key)
        {
            var section = _configuration.GetSection("syncedData");
            if (section.Exists())
            {
                foreach (var child in section.GetChildren())
                {
                    if (string.Equals(child["name"], $"{_componentId}:{key}"))
                    {
                        return child;
                    }
                }
            }

            return null;
        }

        private string GetUrl(string key)
        {
            var template = _configuration["urlTemplate"];
            return template
                .Replace("{componentId}", _componentId)
                .Replace("{fileName}", key);
        }

        public IDisposable AddChangeWatcher(string key, IOpenModComponent component, Action onChange)
        {
            // not supported
            return NoopDisposable.Instance;
        }
    }
}