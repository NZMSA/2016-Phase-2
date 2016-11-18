# Integrating Cognative Services 

## 1. Installing the Nuget Package

* Firstly search for and install ```Microsoft.ProjectOxford.Emotion``` in all your projects.

## 2. Calling Cognative Services

* In MainPage.xaml.cs add ```using Microsoft.ProjectOxford.Emotion;``` to the top of your file.
* Next we'll pass the image the user took to get the current emotion.

```
try
            {

                string emotionKey = "88f748eefd944a5d8d337a1765414bba";

                EmotionServiceClient emotionClient = new EmotionServiceClient(emotionKey);

                var emotionResults = await emotionClient.RecognizeAsync(file.GetStream());

                UploadingIndicator.IsRunning = false;

                var temp = emotionResults[0].Scores;
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
                    createdAt = DateTime.Now
                };

                EmotionView.ItemsSource = temp.ToRankedList();

                App.Database.SaveItem(emo);

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
```