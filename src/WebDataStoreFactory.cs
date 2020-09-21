using Microsoft.Extensions.Logging;
using OpenMod.API.Persistence;
using OpenMod.API.Plugins;

namespace OpenMod.WebDataStore
{
    public class WebDataStoreFactory : IDataStoreFactory
    {
        private readonly IDataStoreFactory _parentDataStoreFactory;
        private readonly IPluginAccessor<WebDataStorePlugin> _pluginAccessor;
        private readonly ILoggerFactory _loggerFactory;

        public WebDataStoreFactory(
            IDataStoreFactory parentDataStoreFactory, 
            IPluginAccessor<WebDataStorePlugin> pluginAccessor,
            ILoggerFactory loggerFactory)
        {
            _parentDataStoreFactory = parentDataStoreFactory;
            _pluginAccessor = pluginAccessor;
            _loggerFactory = loggerFactory;
        }

        public IDataStore CreateDataStore(DataStoreCreationParameters parameters)
        {
            var logger = _loggerFactory.CreateLogger<WebDataStore>();
            var configuration = _pluginAccessor.Instance.Configuration;
            var baseDataStore = _parentDataStoreFactory.CreateDataStore(parameters);
            return new WebDataStore(parameters.ComponentId, configuration, baseDataStore, logger);
        }
    }
}