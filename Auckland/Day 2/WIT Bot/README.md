# WIT Bot

## 1. Get Compuer Vision API Key

* Navigate to https://www.microsoft.com/cognitive-services/en-us/computer-vision-api.
* At the top click 'Get started for free'.
* sign in and grab your API key for the computer vision API.

## 2. Adding Computer Vision NuGet Package

* Create a new bot project.
* Right click on your solution and click 'Manage NuGet Packages'.
* Search for 'Microsoft.ProjectOxford.Vision' and install it.

## 3. Sending Image to be analysed

Add the following line of code.

```
VisionServiceClient VisionServiceClient = new VisionServiceClient("YOUR API KEY");

AnalysisResult analysisResult = await VisionServiceClient.DescribeAsync(activity.Attachments[0].ContentUrl, 3);

Activity reply = activity.CreateReply($"{analysisResult.Description.Captions[0].Text}");
```