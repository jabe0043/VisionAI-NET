//-- Jad Jaber, Apr 17 2024

namespace MaddVisionAI;

public partial class App : Application
{
    //-- GLOBAL PROPERTIES
	public static bool UseSpeech { get; set; } = true;
	public static bool RecognizeText { get; set; } = true;

	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}


    protected override void OnStart()
    {
        base.OnStart();
        UseSpeech = Preferences.Default.Get("UseSpeech", false);
        RecognizeText = Preferences.Default.Get("RecognizeText", true);
    }


    protected override void OnSleep()
    {
        base.OnSleep();
        Preferences.Default.Set("UseSpeech", UseSpeech);
        Preferences.Default.Set("RecognizeText", RecognizeText);
    }
}

