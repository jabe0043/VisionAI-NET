//-- Jad Jaber, Apr 17 2024

using Maui.FixesAndWorkarounds;
using Microsoft.Extensions.Logging;

namespace MaddVisionAI;

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

				fonts.AddFont("fa-solid-900.ttf", "FA-S");
				fonts.AddFont("fa-regular-400.ttf", "FA-R");
			});


		builder.ConfigureMauiWorkarounds();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

