namespace MaddVisionAI;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();

        if(DeviceInfo.Platform == DevicePlatform.MacCatalyst)
        {
            AppSettingsButton.IsEnabled = false;
            AppSettingsButton.IsVisible = false;
        }

        RecognizeTextSwitch.IsToggled = App.RecognizeText;
        SpeechSwitch.IsToggled = App.UseSpeech;
        DisplayDeviceInfo();
        DisplayAppInfo();
    }




    private void DisplayAppInfo()
    {
        string infoString = $"{AppInfo.Name}{Environment.NewLine}" +
            $"Version: {AppInfo.VersionString}{Environment.NewLine}" +
            $"Build: {AppInfo.BuildString}";

        AppInformation.Text = infoString;
    }

    private void DisplayDeviceInfo()
    {
        string infoString =
            $"Manufacturer: {DeviceInfo.Manufacturer}{Environment.NewLine}" +
            $"Model: {DeviceInfo.Model}{Environment.NewLine}" +
            $"Version: {DeviceInfo.Version}{Environment.NewLine}" +
            $"Platform: {DeviceInfo.Platform}{Environment.NewLine}" +
            $"Name: {DeviceInfo.Name}{Environment.NewLine}" +
            $"Idiom: {DeviceInfo.Idiom}{Environment.NewLine}" +
            $"Type: {DeviceInfo.DeviceType}{Environment.NewLine}";
        DeviceInformation.Text = infoString;
    }



    private void AppSettingsButton_Clicked(System.Object sender, System.EventArgs e)
    {
        AppInfo.ShowSettingsUI(); //--Show us the settings ui based on device os. This does not work on macOs (Mac Catalyst) so we set it to invisible on Mac
    }



    private void RecognizeTextSwitch_Toggled(System.Object sender, Microsoft.Maui.Controls.ToggledEventArgs e)
    {
        App.RecognizeText = RecognizeTextSwitch.IsToggled;
    }

    private void SpeechSwitch_Toggled(System.Object sender, Microsoft.Maui.Controls.ToggledEventArgs e)
    {
        App.UseSpeech = SpeechSwitch.IsToggled;
    }
}
