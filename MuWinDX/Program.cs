using Client.Main;

#if DEBUG
// Opcional: Sobrescribir la ruta de datos para desarrollo
// PlatformPathResolver.DataPathUrl = "http://tu-servidor-local/Data.zip";
#endif

using var game = new MuGame();
game.Run();
