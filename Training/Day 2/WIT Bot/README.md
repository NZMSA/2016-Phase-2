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

First add the namespace of the newly added NuGet package.

```
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
```

Add the following line of code.

```
VisionServiceClient VisionServiceClient = new VisionServiceClient("81ca643d8b1d46d8a2c953c9afc3c147");

AnalysisResult analysisResult = await VisionServiceClient.DescribeAsync(activity.Attachments[0].ContentUrl, 3);

Activity reply = activity.CreateReply($"{analysisResult.Description.Captions[0].Text}");
```