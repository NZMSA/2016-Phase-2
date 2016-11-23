using Plugin.Media;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Microsoft.ProjectOxford.Emotion;
using Moodify.DataModels;
using Plugin.Settings;
using Microsoft.WindowsAzure.MobileServices;

namespace Moodify
{
	public partial class HomePage : ContentPage
	{

        bool authenticated = false;

        public HomePage()
		{
			InitializeComponent();
		}

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            string userId = CrossSettings.Current.GetValueOrDefault("user", "");
            string token = CrossSettings.Current.GetValueOrDefault("token", "");

            if (!token.Equals("") && !userId.Equals(""))
            {
                MobileServiceUser user = new MobileServiceUser(userId);
                user.MobileServiceAuthenticationToken = token;

                AzureManager.AzureManagerInstance.AzureClient.CurrentUser = user;

                authenticated = true;
            }

            if (authenticated == true)
                this.loginButton.IsVisible = false;
        }

        async void loginButton_Clicked(object sender, EventArgs e)
        {
            if (App.Authenticator != null)
                authenticated = await App.Authenticator.Authenticate();

            if (authenticated == true)
            {
                this.loginButton.IsVisible = false;
                CrossSettings.Current.AddOrUpdateValue("user", AzureManager.AzureManagerInstance.AzureClient.CurrentUser.UserId);
                CrossSettings.Current.AddOrUpdateValue("token", AzureManager.AzureManagerInstance.AzureClient.CurrentUser.MobileServiceAuthenticationToken);
            }
        }

        private async void TakePicture_Clicked(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera avaliable.", "OK");
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
                UploadingIndicator.IsRunning = true;

                string emotionKey = "88f748eefd944a5d8d337a1765414bba";

                EmotionServiceClient emotionClient = new EmotionServiceClient(emotionKey);

                var result = await emotionClient.RecognizeAsync(file.GetStream());

                UploadingIndicator.IsRunning = false;
               
                EmotionView.ItemsSource = result[0].Scores.ToRankedList();

                var temp = result[0].Scores;

                Timeline emo = new Timeline()
                {
                    Anger = temp.Anger,
                    Contempt = temp.Contempt,
                    Disgust = temp.Disgust,
                    Fear = temp.Fear,
                    Happiness = temp.Happiness,
                    Neutral = temp.Neutral,
                    Sadness = temp.Sadness,
                    Surprise = temp.Surprise,
                    Date = DateTime.Now
                };

                await AzureManager.AzureManagerInstance.AddTimeline(emo);

            }
            catch (Exception ex)
            {
                errorLabel.Text = ex.Message;
            }

            //image.Source = ImageSource.FromStream(() =>
            //{
            //    var stream = file.GetStream();
            //    file.Dispose();
            //    return stream;
            //});
        }

        private async void ViewTimeline_Clicked(Object sender, EventArgs e)
        {

            List<Timeline> timelines = await AzureManager.AzureManagerInstance.GetTimelines();

            TimelineList.ItemsSource = timelines;

        }
    }
}
