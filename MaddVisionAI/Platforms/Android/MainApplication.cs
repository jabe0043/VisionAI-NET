//-- Jad Jaber, Apr 17 2024

using Android.App;
using Android.Runtime;

namespace MaddVisionAI;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

