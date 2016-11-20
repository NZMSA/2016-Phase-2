## Building an app
If you've been having troubles building the base project try this example. Otherwise head over the our [day 3 training](https://github.com/NZMSA/2016-Phase-2/tree/master/Training/Day%203) on expanding the base project.

Our objective by the end of this is to have an app that takes a picture, send this to cognitive services and display some text on screen saying whether you're happy or sad. 

Create a new project with the Templates > Visual C# > Android > Blank App 

First we will download the following nugets. 
- Microsoft.BCL.Build
- Microsoft.ProjectOxford

Nugets are libraries of code that we can utilize to save time. The *BCL.Build* package helps with building packages whereas *ProjectOxford* contains contains code that we will use to help us use Congitive Services.

### Adding some UI Elements

This will be a very simple one page app with 3 main elements. An ImageView to show the picture we've taken. A TextView to display whether we're happy or sad and finally Button for us to take a picture. You can do this yourself or simply copy the following code to `Resource/Layout/Main.xml`
```sh
<?xml version="1.0" encoding="utf-8"?>
<LinearLayout xmlns:android="http://schemas.android.com/apk/res/android"
    android:orientation="vertical"
    android:layout_width="fill_parent"
    android:layout_height="fill_parent">
    <ImageView
    android:src="@android:drawable/ic_menu_gallery"
    android:layout_width="fill_parent"
    android:layout_height="300.0dp"
    android:id="@+id/imageView1"
    android:adjustViewBounds="true" />
    <TextView
    android:textAppearance="?android:attr/textAppearanceLarge"
    android:layout_width="fill_parent"
    android:layout_height="wrap_content"
    android:id="@+id/resultText"
    android:textAlignment="center" />
    <Button
    android:id="@+id/GetPictureButton"
    android:layout_width="fill_parent"
    android:layout_height="wrap_content"
    android:text="Take Picture" />
</LinearLayout>
```
Build this solution.

### Connecting up the camera. 
Xamarin contains some very good instructions on how to take a picture [using the camera here](https://developer.xamarin.com/recipes/android/other_ux/camera_intent/take_a_picture_and_save_using_camera_app/). 
Most of the the steps outlined above will be inside our MainActivity class unless specified otherwise.
Your final code should look something like this.
```
[Activity(Label = "AndroidApp", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
public class MainActivity : Activity
{
    public static File _file;
    public static File _dir;
    public static Bitmap _bitmap; 
    private ImageView _imageView;
    private Button _pictureButton;
    private TextView _resultTextView; 
    private bool _isCaptureMode = true;

    private void CreateDirectoryForPictures()
    {
        _dir = new File(
            Android.OS.Environment.GetExternalStoragePublicDirectory(
                Android.OS.Environment.DirectoryPictures), "CameraAppDemo");
        if (!_dir.Exists())
        {
            _dir.Mkdirs();
        }
    }

    private bool IsThereAnAppToTakePictures()
    {
        Intent intent = new Intent(MediaStore.ActionImageCapture);
        IList<ResolveInfo> availableActivities =
            PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
        return availableActivities != null && availableActivities.Count > 0;
    }

    protected override void OnCreate(Bundle bundle)
    {
        base.OnCreate(bundle);

        SetContentView(Resource.Layout.Main);

        if (IsThereAnAppToTakePictures())
        {
            CreateDirectoryForPictures();

            _pictureButton = FindViewById<Button>(Resource.Id.GetPictureButton);
            _pictureButton.Click += OnActionClick;

            _imageView = FindViewById<ImageView>(Resource.Id.imageView1);

            _resultTextView = FindViewById<TextView>(Resource.Id.resultText); 
        }
    }

    private void OnActionClick(object sender, EventArgs eventArgs)
    {
        if (_isCaptureMode == true)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            _file = new Java.IO.File(_dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(_file));
            StartActivityForResult(intent, 0);
        }
        else
        {
            _imageView.SetImageBitmap(null);
            if (_bitmap != null)
            {
                _bitmap.Recycle();
                _bitmap.Dispose();
                _bitmap = null;
            }
            _pictureButton.Text = "Take Picture";
            _resultTextView.Text = "";
            _isCaptureMode = true;
        }
    }

    protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        try
        {
            //Get the bitmap with the right rotation
            _bitmap = BitmapHelpers.GetAndRotateBitmap(_file.Path);

            //Resize the picture to be under 4MB (Emotion API limitation and better for Android memory)
            _bitmap = Bitmap.CreateScaledBitmap(_bitmap, 2000, (int)(2000*_bitmap.Height/_bitmap.Width), false);

            //Display the image
            _imageView.SetImageBitmap(_bitmap);

            //Loading message
            _resultTextView.Text = "Loading...";

            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                //Get a stream
                _bitmap.Compress(Bitmap.CompressFormat.Jpeg, 90, stream);
                stream.Seek(0, System.IO.SeekOrigin.Begin);

                //Get and display the happiness score
                float result = await Core.GetAverageHappinessScore(stream);
                _resultTextView.Text = Core.GetHappinessMessage(result);
            }
        }
        catch (Exception ex)
        {
            _resultTextView.Text = ex.Message;
        }
        finally
        {
            _pictureButton.Text = "Reset";
            _isCaptureMode = false;
        }
    }
}
```

We will also need to reference the following namespaces
```
using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Widget;
using Java.IO;
```
#### Handling the image
[The Xamarin doc](https://developer.xamarin.com/recipes/android/other_ux/camera_intent/take_a_picture_and_save_using_camera_app/) mentioned a helper class and we will use this to help resize and orientate our image. 
To do this let's create a new class within our solution and name it `Bitmaphelper` and copy and paste the following. Much of this was has already be written for us [here](https://developer.xamarin.com/recipes/android/other_ux/camera_intent/take_a_picture_and_save_using_camera_app/)
```
using Android.Graphics;
using Android.Media;

namespace AndroidApp
{
    public static class BitmapHelpers
    {
        public static Bitmap GetAndRotateBitmap(string fileName)
        {
            Bitmap bitmap = BitmapFactory.DecodeFile(fileName);

            // Images are being saved in landscape, so rotate them back to portrait if they were taken in portrait
            // See https://forums.xamarin.com/discussion/5409/photo-being-saved-in-landscape-not-portrait
            // See http://developer.android.com/reference/android/media/ExifInterface.html
            using (Matrix mtx = new Matrix())
            {
                if (Android.OS.Build.Product.Contains("Emulator"))
                {
                    mtx.PreRotate(90);
                }
                else
                {
                    ExifInterface exif = new ExifInterface(fileName);
                    var orientation = (Orientation)exif.GetAttributeInt(ExifInterface.TagOrientation, (int)Orientation.Normal);

                    //TODO : handle FlipHorizontal, FlipVertical, Transpose and Transverse
                    switch (orientation)
                    {
                        case Orientation.Rotate90:
                            mtx.PreRotate(90);
                            break;
                        case Orientation.Rotate180:
                            mtx.PreRotate(180);
                            break;
                        case Orientation.Rotate270:
                            mtx.PreRotate(270);
                            break;
                        case Orientation.Normal:
                            // Normal, do nothing
                            break;
                        default:
                            break;
                    }
                }

                if (mtx != null)
                    bitmap = Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, mtx, false);
            }

            return bitmap;
        }
    }
}
```

If you were to build your project right now, you would see some errors becuase some thing are still missing.

#### Calling our Emotion API
Create a new project inside your solution and just name it `SharedProject` ( Templates > Visual C# > Shared Project template). Create a new class named `Core`

This is the part where we will using the nuget package we installed earlier so reference the following as we will be using the Emotion API.
```
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
```
Here we are calling the Emotion API through the nuget pacakge we installed earlier
Make sure you've replaced the `emotionKey`
```
private static async Task<Emotion[]> GetHappiness(Stream stream)
{
    string emotionKey = "YourKeyHere";
    EmotionServiceClient emotionClient = new EmotionServiceClient(emotionKey);
    var emotionResults = await emotionClient.RecognizeAsync(stream);
    if (emotionResults == null || emotionResults.Count() == 0)
    {
        throw new Exception("Can't detect face");
    }
    return emotionResults;
}
```
Finally we will need to process the result retrieved
```
//Average happiness calculation in case of multiple people
public static async Task<float> GetAverageHappinessScore(Stream stream)
{
    Emotion[] emotionResults = await GetHappiness(stream);

    float score = 0;
    foreach (var emotionResult in emotionResults)
    {
        score = score + emotionResult.Scores.Happiness;
    }

    return score / emotionResults.Count();
}

public static string GetHappinessMessage(float score)
{
    score = score * 100;
    double result = Math.Round(score, 2);

    if (score >= 50)
        return "You are "+ result + " % Happy";
    else
        return result "You are " + "% Sad";
}
```

Note: Please remember to refrence this SharedProject inside your solution, the reason we have created a shared project is to expose some good coding etiquette.

##### Build and run your completed project 

