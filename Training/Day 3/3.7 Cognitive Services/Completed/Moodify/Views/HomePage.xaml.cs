using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Microsoft.ProjectOxford.Emotion;

namespace Moodify
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private async void TakePicture_Clicked(object sender, System.EventArgs e)
        {

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera avaialble.", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Front,
                Directory = "Moodify",
                Name = $"{DateTime.UtcNow}.jpg",
                CompressionQuality = 92
            });

            if (file == null)
                return;

            try
            {
                string emotionKey = "YOUR-API-KEY";

                EmotionServiceClient emotionClient = new EmotionServiceClient(emotionKey);

                var emotionResults = await emotionClient.RecognizeAsync(file.GetStream());

                UploadingIndicator.IsRunning = false;

                var temp = emotionResults[0].Scores;

                EmotionView.ItemsSource = temp.ToRankedList();

                image.Source = ImageSource.FromStream(() =>
                {
                    var stream = file.GetStream();
                    file.Dispose();
                    return stream;
                });
            }
            catch (Exception ex)
            {
                errorLabel.Text = ex.Message;
            }

        }
    }
}
