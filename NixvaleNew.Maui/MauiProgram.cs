using Microsoft.Extensions.Logging;
using NixvaleNew.Maui.Services;
using NixvaleNew.Maui.ViewModels;

namespace NixvaleNew.Maui;

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
				fonts.AddFont("Consolas.ttf", "Consolas");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Register services
		builder.Services.AddSingleton<MauiDebugLogger>();
		builder.Services.AddSingleton<NixvaleService>();
		
		// Register view models
		builder.Services.AddSingleton<MainViewModel>();

		// Register pages
		builder.Services.AddSingleton<MainPage>();

		return builder.Build();
	}
}
