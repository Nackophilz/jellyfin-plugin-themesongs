using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.ThemeSongs.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ThemeSongs.ScheduledTasks
{
    /// <summary>
    /// Tâche planifiée pour le téléchargement des thèmes musicaux.
    /// </summary>
    public class DownloadThemeSongsTask : IScheduledTask
    {
        private readonly ILogger<DownloadThemeSongsTask> _logger;
        private readonly ThemeSongsManager _themeSongsManager;
        private readonly ILibraryManager _libraryManager;

        public DownloadThemeSongsTask(
            ILibraryManager libraryManager,
            ILogger<DownloadThemeSongsTask> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
            _themeSongsManager = new ThemeSongsManager(libraryManager, logger, httpClientFactory);
        }

        /// <inheritdoc />
        public string Name => "Téléchargement des Thèmes Musicaux";

        /// <inheritdoc />
        public string Key => "DownloadTVThemeSongs";

        /// <inheritdoc />
        public string Description => "Analyse les bibliothèques pour télécharger les thèmes musicaux manquants";

        /// <inheritdoc />
        public string Category => "Theme Songs";

        /// <inheritdoc />
        public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Démarrage du téléchargement des thèmes musicaux");
                
                // Vérification de la configuration
                var config = Plugin.Instance?.Configuration;
                if (config?.EnableAutoDownload != true)
                {
                    _logger.LogInformation("Le téléchargement automatique est désactivé dans la configuration");
                    return;
                }

                await _themeSongsManager.ExecuteAsync(progress, cancellationToken);
                
                _logger.LogInformation("Téléchargement des thèmes musicaux terminé");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Téléchargement des thèmes musicaux annulé");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du téléchargement des thèmes musicaux");
                throw;
            }
        }

        /// <inheritdoc />
        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            var config = Plugin.Instance?.Configuration;
            var interval = TimeSpan.FromHours(config?.ScanIntervalHours ?? 24);

            yield return new TaskTriggerInfo
            {
                Type = TaskTriggerInfo.TriggerInterval,
                IntervalTicks = interval.Ticks
            };

            // Ajoute un trigger quotidien à 4h du matin
            yield return new TaskTriggerInfo
            {
                Type = TaskTriggerInfo.TriggerDaily,
                TimeOfDayTicks = TimeSpan.FromHours(4).Ticks
            };
        }

        /// <inheritdoc />
        [Obsolete("Utiliser ExecuteAsync à la place")]
        public Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            return ExecuteAsync(progress, cancellationToken);
        }
    }
}
