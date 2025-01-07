using Microsoft.Extensions.Logging;

namespace CrossPlatformProject2
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Orbitron-Regular.ttf", "Orbitron");
                    //fonts.AddFont("Orbitron-Bold.ttf", "OrbitronBold");//registered the two fonts
                    fonts.AddFont("RockBoulder.ttf", "OrbitronBold");//registered the two fonts
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
