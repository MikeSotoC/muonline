using System;
using System.IO;

namespace Client.Main
{
    /// <summary>
    /// Resuelve rutas de archivos y almacenamiento según la plataforma.
    /// Centraliza la lógica de paths para Windows, Android, iOS, Linux y macOS.
    /// </summary>
    public static class PlatformPathResolver
    {
        private static string _dataPath;
        private static string _configDirectory;
        private static string _cacheDirectory;
        private static string _logsDirectory;

        /// <summary>
        /// Ruta base para datos del juego (Data folder).
        /// Windows: AppContext.BaseDirectory/Data
        /// Android: Environment.GetFolderPath(Environment.SpecialFolder.Personal)/Data
        /// iOS: Environment.GetFolderPath(Environment.SpecialFolder.Personal)/Data
        /// Linux/macOS: AppContext.BaseDirectory/Data
        /// </summary>
        public static string DataPath
        {
            get
            {
                if (_dataPath == null)
                {
                    InitializePaths();
                }
                return _dataPath;
            }
        }

        /// <summary>
        /// Ruta para archivos de configuración (appsettings.json, etc.).
        /// Windows: AppContext.BaseDirectory
        /// Android: Environment.GetFolderPath(Environment.SpecialFolder.Personal)
        /// iOS: Environment.GetFolderPath(Environment.SpecialFolder.Personal)
        /// Linux/macOS: AppContext.BaseDirectory
        /// </summary>
        public static string ConfigDirectory
        {
            get
            {
                if (_configDirectory == null)
                {
                    InitializePaths();
                }
                return _configDirectory;
            }
        }

        /// <summary>
        /// Ruta para caché temporal (texturas, datos descargados, etc.).
        /// Windows: Path.Combine(AppContext.BaseDirectory, "Cache")
        /// Android: Context.CacheDir.Path o Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        /// iOS: Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        /// Linux/macOS: Path.Combine(AppContext.BaseDirectory, "Cache")
        /// </summary>
        public static string CacheDirectory
        {
            get
            {
                if (_cacheDirectory == null)
                {
                    InitializePaths();
                }
                return _cacheDirectory;
            }
        }

        /// <summary>
        /// Ruta para archivos de log.
        /// Windows: Path.Combine(AppContext.BaseDirectory, "Logs")
        /// Android: Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)/Logs
        /// iOS: Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)/Logs
        /// Linux/macOS: Path.Combine(AppContext.BaseDirectory, "Logs")
        /// </summary>
        public static string LogsDirectory
        {
            get
            {
                if (_logsDirectory == null)
                {
                    InitializePaths();
                }
                return _logsDirectory;
            }
        }

        /// <summary>
        /// URL base para descarga de datos (Data.zip).
        /// Configurable según el servidor.
        /// </summary>
        public static string DataPathUrl { get; set; } = "http://192.168.55.220/Data.zip";

        /// <summary>
        /// URL por defecto para descarga de datos oficiales.
        /// </summary>
        public static string DefaultDataPathUrl { get; set; } = "https://full-wkr.mu.webzen.co.kr/muweb/full/MU_Red_1_20_61_Full.zip";

        /// <summary>
        /// Inicializa todas las rutas según la plataforma.
        /// </summary>
        private static void InitializePaths()
        {
#if __ANDROID__
            // Android: Usar almacenamiento interno de la aplicación
            string personalPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            _dataPath = Path.Combine(personalPath, "Data");
            _configDirectory = personalPath;
            _cacheDirectory = Path.Combine(personalPath, "Cache");
            _logsDirectory = Path.Combine(personalPath, "Logs");
#elif __IOS__
            // iOS: Usar almacenamiento interno de la aplicación
            string personalPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            
            _dataPath = Path.Combine(personalPath, "Data");
            _configDirectory = personalPath;
            _cacheDirectory = Path.Combine(libraryPath, "Cache");
            _logsDirectory = Path.Combine(libraryPath, "Logs");
#else
            // Windows, Linux, macOS: Usar directorio base de la aplicación
            string baseDirectory = AppContext.BaseDirectory;
            
            _dataPath = Path.Combine(baseDirectory, "Data");
            _configDirectory = baseDirectory;
            _cacheDirectory = Path.Combine(baseDirectory, "Cache");
            _logsDirectory = Path.Combine(baseDirectory, "Logs");
#endif

            // Asegurar que los directorios existan
            EnsureDirectoryExists(_dataPath);
            EnsureDirectoryExists(_configDirectory);
            EnsureDirectoryExists(_cacheDirectory);
            EnsureDirectoryExists(_logsDirectory);
        }

        /// <summary>
        /// Crea un directorio si no existe.
        /// </summary>
        private static void EnsureDirectoryExists(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PlatformPathResolver] Error creando directorio '{path}': {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Obtiene la ruta completa para un archivo en el directorio de datos.
        /// </summary>
        public static string GetDataFilePath(string relativePath)
        {
            return Path.Combine(DataPath, relativePath);
        }

        /// <summary>
        /// Obtiene la ruta completa para un archivo de configuración.
        /// </summary>
        public static string GetConfigFilePath(string fileName)
        {
            return Path.Combine(ConfigDirectory, fileName);
        }

        /// <summary>
        /// Obtiene la ruta completa para un archivo en caché.
        /// </summary>
        public static string GetCacheFilePath(string relativePath)
        {
            return Path.Combine(CacheDirectory, relativePath);
        }

        /// <summary>
        /// Obtiene la ruta completa para un archivo de log.
        /// </summary>
        public static string GetLogFilePath(string fileName)
        {
            return Path.Combine(LogsDirectory, fileName);
        }

        /// <summary>
        /// Reinicia las rutas (útil para testing o cambios dinámicos).
        /// </summary>
        public static void Reset()
        {
            _dataPath = null;
            _configDirectory = null;
            _cacheDirectory = null;
            _logsDirectory = null;
        }
    }
}
