using System;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.ThemeSongs.Configuration
{
    /// <summary>
    /// Configuration du plugin Theme Songs.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        private const int DefaultScanInterval = 24;
        private const int DefaultMaxConcurrentDownloads = 3;
        private const string DefaultThemeSongFileName = "theme.mp3";

        /// <summary>
        /// Initialise une nouvelle instance de la configuration.
        /// </summary>
        public PluginConfiguration()
        {
            EnableAutoDownload = true;
            ScanIntervalHours = DefaultScanInterval;
            MaxConcurrentDownloads = DefaultMaxConcurrentDownloads;
            ThemeSongFileName = DefaultThemeSongFileName;
            ThemeSongBaseUrl = "http://tvthemes.plexapp.com";
            SkipExistingFiles = true;
            EnableNotifications = true;
        }

        /// <summary>
        /// Active ou désactive le téléchargement automatique.
        /// </summary>
        public bool EnableAutoDownload { get; set; }

        /// <summary>
        /// Intervalle entre les scans en heures.
        /// </summary>
        public int ScanIntervalHours { get; set; }

        /// <summary>
        /// Nombre maximum de téléchargements simultanés.
        /// </summary>
        public int MaxConcurrentDownloads { get; set; }

        /// <summary>
        /// Nom du fichier de thème musical.
        /// </summary>
        public string ThemeSongFileName { get; set; }

        /// <summary>
        /// URL de base pour le téléchargement des thèmes.
        /// </summary>
        public string ThemeSongBaseUrl { get; set; }

        /// <summary>
        /// Ignore les fichiers déjà existants.
        /// </summary>
        public bool SkipExistingFiles { get; set; }

        /// <summary>
        /// Active les notifications.
        /// </summary>
        public bool EnableNotifications { get; set; }

        /// <summary>
        /// Valide la configuration.
        /// </summary>
        /// <exception cref="ArgumentException">Levée si la configuration est invalide.</exception>
        public void Validate()
        {
            if (ScanIntervalHours <= 0)
            {
                throw new ArgumentException($"L'intervalle de scan doit être positif. Valeur actuelle : {ScanIntervalHours}");
            }

            if (MaxConcurrentDownloads <= 0)
            {
                throw new ArgumentException($"Le nombre maximum de téléchargements simultanés doit être positif. Valeur actuelle : {MaxConcurrentDownloads}");
            }

            if (string.IsNullOrWhiteSpace(ThemeSongFileName))
            {
                throw new ArgumentException("Le nom du fichier de thème musical ne peut pas être vide.");
            }

            if (string.IsNullOrWhiteSpace(ThemeSongBaseUrl))
            {
                throw new ArgumentException("L'URL de base ne peut pas être vide.");
            }
        }

        /// <summary>
        /// Réinitialise la configuration aux valeurs par défaut.
        /// </summary>
        public void ResetToDefaults()
        {
            EnableAutoDownload = true;
            ScanIntervalHours = DefaultScanInterval;
            MaxConcurrentDownloads = DefaultMaxConcurrentDownloads;
            ThemeSongFileName = DefaultThemeSongFileName;
            ThemeSongBaseUrl = "http://tvthemes.plexapp.com";
            SkipExistingFiles = true;
            EnableNotifications = true;
        }
    }
}
