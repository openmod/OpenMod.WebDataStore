using System;
using OpenMod.API.Plugins;
using OpenMod.Core.Plugins;

[assembly: PluginMetadata("OpenMod.WebDataStore", Author = "OpenMod", DisplayName = "OpenMod Web DataStore", Website = "https://github.com/openmodplugins/OpenMod.WebDataStore/")]

namespace OpenMod.WebDataStore
{
    public class WebDataStorePlugin : OpenModUniversalPlugin
    {
        public WebDataStorePlugin(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }
    }
}
