using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Mime;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.ThemeSongs.Configuration;

namespace Jellyfin.Plugin.ThemeSongs.Api
{
    /// <summary>
    /// Contrôleur API pour la gestion des thèmes musicaux.
    /// </summary>
    [ApiController]
    [Route("ThemeSongs")]
    [Produces(MediaTypeNames.Application.Json)]
    [Authorize(Policy = "DefaultAuthorization")]
    public class ThemeSongsController : ControllerBase
    {
        private readonly ThemeSongsManager _themeSongsManager;
        private readonly ILogger<ThemeSongsController> _logger;
        private readonly ILibraryManager _libraryManager;
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur.
        /// </summary>
        public ThemeSongsController(
            ILibraryManager libraryManager,
            ILogger<ThemeSongsController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _themeSongsManager = new ThemeSongsManager(libraryManager, logger, httpClientFactory);
        }

        /// <summary>
        /// Démarre le téléchargement des thèmes musicaux.
        /// </summary>
        /// <param name="cancellationToken">Token d'annulation.</param>
        /// <response code="202">Téléchargement démarré avec succès.</response>
        /// <response code="400">Configuration invalide.</response>
        /// <response code="500">Erreur lors du téléchargement.</response>
        /// <returns>Un statut indiquant le résultat de l'opération.</returns>
        [HttpPost("DownloadTVShows")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DownloadTVThemeSongsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var config = Plugin.Instance?.Configuration;
                if (config == null)
                {
                    return BadRequest("Configuration du plugin non trouvée");
                }

                _logger.LogInformation("Démarrage du téléchargement des thèmes musicaux");
                
                // Démarrage asynchrone du téléchargement
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var progress = new Progress<double>(percent =>
                        {
                            _logger.LogInformation("Progression : {Percent}%", Math.Round(percent, 2));
                        });

                        await _themeSongsManager.ExecuteAsync(progress, cancellationToken);
                        _logger.LogInformation("Téléchargement terminé avec succès");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erreur lors du téléchargement des thèmes");
                    }
                }, cancellationToken);

                return Accepted("Téléchargement démarré");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du démarrage du téléchargement");
                return StatusCode(500, "Une erreur est survenue lors du démarrage du téléchargement");
            }
        }

        /// <summary>
        /// Récupère l'état actuel du téléchargement.
        /// </summary>
        /// <returns>L'état actuel du téléchargement.</returns>
        [HttpGet("Status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<DownloadStatus> GetStatus()
        {
            return Ok(new DownloadStatus
            {
                IsRunning = _themeSongsManager.IsDownloading,
                Progress = _themeSongsManager.CurrentProgress,
                LastRun = _themeSongsManager.LastRunTime
            });
        }
    }

    /// <summary>
    /// Représente l'état du téléchargement.
    /// </summary>
    public class DownloadStatus
    {
        /// <summary>
        /// Indique si un téléchargement est en cours.
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// Progression actuelle en pourcentage.
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        /// Date de la dernière exécution.
        /// </summary>
        public DateTime? LastRun { get; set; }
    }
}
