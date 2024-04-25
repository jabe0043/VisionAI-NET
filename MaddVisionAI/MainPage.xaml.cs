
//-- Jad Jaber, Apr 17 2024

using Azure.AI.Vision.ImageAnalysis;
using Azure;
using System.Text;
using System.IO;



namespace MaddVisionAI;

public partial class MainPage : ContentPage
{

	public MainPage()
	{
		InitializeComponent();

        //-- Double loading indicators on apple devices
        if(DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.MacCatalyst || DeviceInfo.Platform == DevicePlatform.macOS)
        {
            TheActivityIndicator.Scale = 2.0;
        }
	}


    #region Web Image Fields
    //private const string BASEURL = "https://source.unsplash.com/random/1080*1080/";
    private const string BASEURL = "https://source.unsplash.com/random/1080*1080/?text"; //-- for testing retrieval of images with text
    private HttpClient httpClient = new();

    #endregion


    #region Azure API Constants
    private const string APIKEY = "8a338daded84402eaf06f4090ff7b604";
    private const string ENDPOINT = "https://jad-visionai.cognitiveservices.azure.com/";

    #endregion


    #region Azure AI Vision Fields

    private ImageAnalysisOptions opts = new ImageAnalysisOptions()
    {
        Language = "en-US",
        GenderNeutralCaption = true
    };

    //-- visual features enum 
    private VisualFeatures visualFeatures =
        VisualFeatures.Caption |
        VisualFeatures.Objects |
        VisualFeatures.Read |
        VisualFeatures.People |
        VisualFeatures.SmartCrops |
        VisualFeatures.DenseCaptions |
        VisualFeatures.Tags;

    //-- Endpoint URI and API Key
    private ImageAnalysisClient visionClient = new ImageAnalysisClient(
        new Uri(ENDPOINT),
        new AzureKeyCredential(APIKEY)
        );

    #endregion


    #region Recognized Text Fields
    private string recognizedImageText = string.Empty;

    #endregion


    #region Speech Fields
    private CancellationTokenSource cts = null; //-- Used to cancel speech

    #endregion

    //-- EVENT HANDLERS
    private async void SettingsButton_Clicked(System.Object sender, System.EventArgs e)
    {
		await Navigation.PushAsync(new SettingsPage());
    }


    private void TheResults_PropertyChanged(System.Object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (!App.UseSpeech || e.PropertyName != "Text") //-- if not using speech or no text, return
            return;

        if(!string.IsNullOrEmpty(TheResults.Text)) //-- if the text property is not null or empty;
        {
            CancelSpeak();
            Speak();
        }
    }


    private async void WebImageButton_Clicked(System.Object sender, System.EventArgs e)
    {
        TheActivityIndicator.IsRunning = true;

        Uri webImageUri = new Uri($"{BASEURL}");
        try
        {
            using (var response = await httpClient.GetStreamAsync(webImageUri))
            {
                var memoryStream = new MemoryStream();
                await response.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                try
                {
                    TheImage.Source = ImageSource.FromStream(() => memoryStream);
                    memoryStream.Position = 0;

                    //-- Analyze and process image
                    var result = await GetImageDescription(memoryStream);
                    ProcessImageResults(result);
                }
                catch(Exception ex)
                {
                    TheResults.Text = "Failed to load the image" + ex.Message;
                }
            }
        }
        catch(Exception ex)
        {
            TheResults.Text = ex.Message;
        }

        TheActivityIndicator.IsRunning = false;
    }



    private async void ImageButton_Clicked(System.Object sender, System.EventArgs e)
    {
        var file = await MediaPicker.PickPhotoAsync(new MediaPickerOptions {
            Title = "Please choose an image",
        });

        if (file != null) //-- null if user cancelled
        {
            TheActivityIndicator.IsRunning = true;

            try
            {
                var stream = await file.OpenReadAsync();

                //-- Analyze and process image
                var result = await GetImageDescription(stream);
                ProcessImageResults(result);
                TheImage.Source = ImageSource.FromStream(() => stream);

            } catch(Exception ex)
            {
                TheResults.Text = ex.Message;
            }

            TheActivityIndicator.IsRunning = false;
        }
    }



    private async void CameraButton_Clicked(System.Object sender, System.EventArgs e)
    {
        if (!MediaPicker.IsCaptureSupported) //-- If device does not support camera;
        {
            await DisplayAlert("No Camera", ":( No camera available", "OK");
            return;
        }
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

        if(status != PermissionStatus.Granted)
        {
            await DisplayAlert("Camera Permission", ":( App camera permission was deniied", "OK");
            status = await Permissions.RequestAsync<Permissions.Camera>();
            if(status != PermissionStatus.Granted)
            {
                return;
            }
        }

        FileResult file = await MediaPicker.Default.CapturePhotoAsync();

        if(file != null) //-- user did not cancel;
        {
            TheActivityIndicator.IsRunning = true;
            try
            {
                //Save the image
                string localFilePath = Path.Combine(FileSystem.CacheDirectory, file.FileName);
                var cameraStream = await file.OpenReadAsync();
                FileStream localFileStream = File.OpenWrite(localFilePath); //-- Saved

                await cameraStream.CopyToAsync(localFileStream);
                cameraStream.Position = 0;

                //-- Analyze and process image
                var result = await GetImageDescription(cameraStream);
                ProcessImageResults(result);

                TheImage.Source = ImageSource.FromStream(() => cameraStream);
            }
            catch (Exception ex)
            {
                TheResults.Text = ex.Message;
            }



            TheActivityIndicator.IsRunning = false;

        }
    }





    private async void TextButton_Clicked(System.Object sender, System.EventArgs e)
    {
        await Clipboard.Default.SetTextAsync(recognizedImageText); //-- Copy the recognized text to clipboard
        await DisplayAlert("Recognized Text", recognizedImageText, "OK");
    }



    #region Image Processing Methods

    private async Task<ImageAnalysisResult> GetImageDescription(Stream imageStream)
    {
        TheResults.Text = string.Empty;

        MemoryStream memoryStream = new();
        await imageStream.CopyToAsync(memoryStream);
        imageStream.Position = 0;
        memoryStream.Position = 0;

        BinaryData imageData = BinaryData.FromStream(memoryStream);

        var result = visionClient.Analyze(imageData, visualFeatures);

        return result;
    }


    //-- Extract the visualFeatures that we want
    private async void ProcessImageResults(ImageAnalysisResult result)
    {
        ResetTextButton(); //-- reset the btn to disabled because we need to analyse the image to see if there's text

        const string DELIMITER = ", ";

        StringBuilder sb = new();

        StringBuilder recognizedText = new(); //-- Used if there are words in an image;

        float conf = float.Parse(result.Caption.Confidence.ToString()); //-- "0.7667" string, 76.67 (+0.5 rounding)= 77.17 --> rounded 77
        int confidence = Convert.ToInt32(conf * 100.0f + 0.5f); //--  77 int

        sb.Append($"'{result.Caption.Text}'{Environment.NewLine}Confidence: {confidence}%{Environment.NewLine}");

        sb.Append("Tags: ");

        foreach(DetectedTag tag in result.Tags.Values) //-- Adding each tag as a string to sb;
        {
            sb.Append($"{tag.Name}{DELIMITER}"); 
        }

        if (result.Tags.Values.Any()) 
        {
            sb.Remove(sb.Length - DELIMITER.Length, DELIMITER.Length); 
        }


        sb.Append($"{Environment.NewLine}");

        TheResults.Text = sb.ToString();

        if (App.RecognizeText)
        {
            foreach(DetectedTextBlock block in result.Read.Blocks)
            {
                foreach (DetectedTextLine line in block.Lines)
                {
                    recognizedText.Append($"{line.Text}{Environment.NewLine}");
                }
            }

            if(result.Read.Blocks.Count > 0)
            {
                TextButton.IsEnabled = true;
                TextButton.BackgroundColor = Colors.Red;
                recognizedImageText = recognizedText.ToString();
            }
        }
    }


    //-- Disable the text button (the last clipboard icon in app tabs)
    private void ResetTextButton()
    {
        TextButton.IsEnabled = false;
        TextButton.BackgroundColor = Colors.Grey;
        recognizedImageText = string.Empty;
    }

    #endregion



    #region Speech Methods

    private async void Speak()
    {
        try
        {
            cts = new CancellationTokenSource();
            await TextToSpeech.SpeakAsync(TheResults.Text, cts.Token);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Speech Error", ex.Message, "OK");
            App.UseSpeech = false;
        }
    }




    private void CancelSpeak()
    {
        if (cts == null)
            return;

        cts.Cancel(); //-- if not null (else)
        cts = null;
    }


    #endregion

}


