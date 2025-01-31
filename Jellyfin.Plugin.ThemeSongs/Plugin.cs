using System;
using System.Collections.Generic;
using Jellyfin.Plugin.ThemeSongs.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ThemeSongs
{
    /// <summary>
    /// Plugin principal pour la gestion des thèmes musicaux.
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        private readonly ILogger<Plugin> _logger;

        /// <summary>
        /// Identifiant unique du plugin.
        /// </summary>
        private static readonly Guid _pluginId = new Guid("afe1de9c-63e4-4692-8d8c-7c964df19eb2");

        /// <summary>
        /// Instance singleton du plugin.
        /// </summary>
        public static Plugin? Instance { get; private set; }

        /// <summary>
        /// Constructeur du plugin.
        /// </summary>
        public Plugin(
            IServerApplicationPaths applicationPaths,
            IXmlSerializer xmlSerializer,
            ILogger<Plugin> logger)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            _logger = logger;

            _logger.LogInformation("Theme Songs plugin initialized");
        }

        /// <inheritdoc />
        public override string Name => "Theme Songs";

        /// <inheritdoc />
        public override Guid Id => _pluginId;

        /// <inheritdoc />
        public override string Description => "Télécharge automatiquement les thèmes musicaux pour vos séries";

        /// <inheritdoc />
        public override PluginInfo GetPluginInfo()
        {
            return new PluginInfo
            {
                Name = Name,
                Id = Id.ToString(),
                Description = Description,
                Version = Version.ToString(),
                ConfigurationFileName = ConfigurationFileName
            };
        }

        /// <inheritdoc />
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = Name,
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.configurationpage.html",
                    EnableInMainMenu = true,
                    MenuSection = "server",
                    DisplayName = "Gestionnaire de Thèmes Musicaux"
                }
            };
        }

        /// <inheritdoc />
        public override void UpdateConfiguration(BasePluginConfiguration configuration)
        {
            if (configuration is PluginConfiguration pluginConfiguration)
            {
                // Validation de la configuration
                ValidateConfiguration(pluginConfiguration);
                
                base.UpdateConfiguration(configuration);
                _logger.LogInformation("Configuration updated");
            }
        }

        private void ValidateConfiguration(PluginConfiguration configuration)
        {
            if (configuration.ScanIntervalHours <= 0)
            {
                configuration.ScanIntervalHours = 24;
                _logger.LogWarning("Invalid scan interval specified, defaulting to 24 hours");
            }

            if (string.IsNullOrEmpty(configuration.ThemeSongBaseUrl))
            {
                configuration.ThemeSongBaseUrl = "http://tvthemes.plexapp.com";
                _logger.LogWarning("Invalid base URL specified, using default");
            }
        }
    }
}
