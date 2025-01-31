using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Jellyfin.Data.Enums;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.ThemeSongs
{
    public class ThemeSongsManager : IScheduledTask, IDisposable
    {
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger<ThemeSongsManager> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private bool _disposed;

        public ThemeSongsManager(
            ILibraryManager libraryManager,
            ILogger<ThemeSongsManager> logger,
            IHttpClientFactory httpClientFactory)
        {
            _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public string Name => "Download Theme Songs";
        public string Key => "DownloadThemeSongs";
        public string Description => "Downloads missing theme songs for TV series";
        public string Category => "Theme Songs";

        private IEnumerable<Series> GetSeriesFromLibrary()
        {
            return _libraryManager.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.Series },
                IsVirtualItem = false,
                Recursive = true,
                HasTvdbId = true
            }).OfType<Series>();
        }

        public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            var series = GetSeriesFromLibrary().ToList();
            var totalSeries = series.Count;
            var processedSeries = 0;

            foreach (var serie in series)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!serie.GetThemeSongs().Any())
                {
                    await DownloadThemeSongAsync(serie, cancellationToken);
                }

                processedSeries++;
                progress.Report((double)processedSeries / totalSeries * 100);
            }
        }

        private async Task DownloadThemeSongAsync(Series serie, CancellationToken cancellationToken)
        {
            var tvdbId = serie.GetProviderId(MetadataProvider.Tvdb);
            if (string.IsNullOrEmpty(tvdbId))
            {
                _logger.LogWarning("No TVDB ID found for {Name}", serie.Name);
                return;
            }

            var themeSongPath = Path.Combine(serie.Path, "theme.mp3");
            var url = $"http://tvthemes.plexapp.com/{tvdbId}.mp3";

            try
            {
                using var client = _httpClientFactory.CreateClient("ThemeSongs");
                client.Timeout = TimeSpan.FromMinutes(2);

                using var response = await client.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Theme song not found for {Name} (Status: {Status})", 
                        serie.Name, response.StatusCode);
                    return;
                }

                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var fileStream = File.Create(themeSongPath);
                await stream.CopyToAsync(fileStream, cancellationToken);

                _logger.LogInformation("Successfully downloaded theme song for {Name}", serie.Name);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Download cancelled for {Name}", serie.Name);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download theme song for {Name}", serie.Name);
            }
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return new[]
            {
                new TaskTriggerInfo
                {
                    Type = TaskTriggerInfo.TriggerDaily,
                    TimeOfDayTicks = TimeSpan.FromHours(4).Ticks
                }
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Dispose managed resources if needed
            }

            _disposed = true;
        }
    }
}
