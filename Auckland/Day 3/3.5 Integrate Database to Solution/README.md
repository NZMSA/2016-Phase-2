# 3. 5. [WIP] Integrate Database to Solution 

# NOTE THIS IS A WORK IN PROGRESS
## Introduction
So now that we have a database attached to our server (web app), we now want our client application (xamarin application) to do GET and POST requests to the database.

As our server is hosted as a web application we could just use a HTTP request. However there exists a managed client SDK package for Mobile Apps (`Microsoft.Azure.Mobile.Client`) that we can use to work with our server. 

## Resources
### Bootcamp Content
* [Slide Deck](http://link.com)

## 1. Postman requests
TBC photos
Lets first see how our data looks like by making a GET request to https://MOBILE_APP_URL.azurewebsites.net (replace `MOBILE_APP_URL` with your server name, for this demo its "mywebsite").
TBC

Here we can see it matches well with what we designed for our server
TBC

Now with a POST request, the field namess of need to match with what we got from our GET Request (values dont!).
(Note the body-content type is `JSON` because xx)
TBC

Now we see that it has added the new entry to our database.
TBC

## 2. Xamarin

### 2.1 Referencing Azure Mobile Services
At the earlier sections, we would have already added it to our Nuget Packages. If not

- For Visual Studio: Right-click your project, click Manage NuGet Packages, search for the `Microsoft.Azure.Mobile.Client` package, then click Install.
- For Xamarin Studio: Right-click your project, click Add > Add NuGet Packages, search for the `Microsoft.Azure.Mobile.Client` package, and then click Add Package.

If we want to use this SDK we add the following using statement
```C#
using Microsoft.WindowsAzure.MobileServices;
``` 

### 2.2 Creating Model Classes
Lets now create model class `Timeline` to represent the tables in our database. 
So in `Moodify (Portable)`, create a folder named `DataModels` and then create a `Timeline.cs` file with,

```C#
public class Timeline
{
    [JsonProperty(PropertyName = "Id")]
    public int ID { get; set; }

    [JsonProperty(PropertyName = "anger")]
    public double Anger { get; set; }

    [JsonProperty(PropertyName = "contempt")]
    public double Contempt { get; set; }

    [JsonProperty(PropertyName = "disgust")]
    public double Disgust { get; set; }

    [JsonProperty(PropertyName = "fear")]
    public double Fear { get; set; }
    
    [JsonProperty(PropertyName = "happiness")]
    public double Happiness { get; set; }
    
    [JsonProperty(PropertyName = "neutral")]
    public double Neutral { get; set; }
    
    [JsonProperty(PropertyName = "sadness")]
    public double Sadness { get; set; }
    
    [JsonProperty(PropertyName = "surprise")]
    public double Surprise { get; set; }
    
    [JsonProperty(PropertyName = "createdAt")]
    public DateTime Date { get; set; }
    
    [JsonProperty(PropertyName = "lat")]
    public double Lat { get; set; }
    
    [JsonProperty(PropertyName = "lon")]
    public double Lon { get; set; }
}
``` 

- `JsonPropertyAttribute` is used to define the PropertyName mapping between the client type and the table 
- Important that they match the field names that we got from our postman request (else it wont map properly)
- Our field names for our client types can then be renamed if we want (like the field `date`)
- All client types must contain a field member mapped to `Id` (default a string). The `Id` is required to perform CRUD operations and for offline sync (not discussed) 

### 2.3 Initalize the Azure Mobile Client
Lets now create a singleton class named `AzureManager` that will look after our interactions with our web server. Add this to the class
(NOTE: replace `MOBILE_APP_URL` with your server name, for this demo its "https://hellotheretest.azurewebsites.net/")


So in `Moodify (Portable)`, create a `AzureManager.cs` file with,

```C#
public class AzureManager
    {

        private static AzureManager instance;
        private MobileServiceClient client;

        private AzureManager()
        {
            this.client = new MobileServiceClient("MOBILE_APP_URL");
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null) {
                    instance = new AzureManager();
                }

                return instance;
            }
        }
    }
``` 

Now if we want to access our `MobileServiceClient` in an activity we can add the following line,
```C#
    MobileServiceClient client = AzureManager.AzureManagerInstance.AzureClient;
``` 


### 2.4 Creating a table references
For this demo we will consider a database table a `table`, so all code that accesses (READ) or modifies (CREATE, UPDATE) the table calls functions on a `MobileServiceTable` object. 
These can be obtained by calling the `GetTable` on our `MobileServiceClient` object.

Lets add our `timelineTable` field to our `AzureManager` activity 
```C#
    private IMobileServiceTable<Timeline> timelineTable;
``` 

And then the following line at the end of our `private AzureManager()` function
```C#
    this.timelineTable = this.client.GetTable<Timeline>();
```

This grabs a reference to the data in our `Timeline` table in our backend and maps it to our client side model defined earlier.
We can then use this table to actually get data, get filtered data, get a timeline by id, create new timeline, edit timeline and much more.

### 2.5 Grabbing timeline data
To retrieve information about the table, we can invoke a `ToListAsync()` method call, this is asynchronous and allows us to do LINQ querys.

Lets create a `GetTimelines` method in our `AzureManager` activity 
```C#
    public async Task<List<Timeline>> GetTimelines() {
        return await this.timelineTable.ToListAsync();
    }
``` 

Lets create a button in our `HomePage.xaml` file after our other button
```xaml
      <Button Text="See Timeline" TextColor="White" BackgroundColor="Red" Clicked="ViewTimeline_Clicked" />
``` 

Now to can call our `GetTimelines` function, we can add the following method in our `HomePage.xaml.cs` class
```C#
    
        private async void ViewTimeline_Clicked(Object sender)
        {
            List<Timeline> timelines = await AzureManager.AzureManagerInstance.GetTimelines();
            
            ListView lb = new ListView();
            lb.ItemsSource = timelines;

        }
``` 

[MORE INFO] A LINQ query we may want to achieve is if we want to filter the data to only return high happiness songs. We could do this by the following line, this grabs the timelines if it has a happiness of 0.5 or higher
```C#
    public async Task<List<Timeline>> GetHappyTimelines() {
        return await timelineTable.Where(timeline => timeline.Happiness > 0.5).ToListAsync();
    }
``` 

### 2.6 Posting timeline data
To post a new timeline entry to our backend, we can invoke a `InsertAsync(timeline)` method call, where `timeline` is a Timeline object.

Lets create a `AddTimeline` method in our `AzureManager` activity 

```C#
    public async Task AddTimeline(Timeline timeline) {
        await this.timelineTable.InsertAsync(timeline);
    }
``` 

NOTE: If a unique `Id` is not included in the `timeline` object when we insert it, the server generates one for us.


Now to can call our `AddTimeline` function, we can do the following in our `HomePageXaml.cs` class at the end of the `TakePicture_Clicked` method so that each response from cognitive services is uploaded
```C#
    TBC
``` 

### 2.6 [More Info] Updating and deleting timeline data
To edit an existing timeline entry in our backend, we can invoke a `UpdateAsync(timeline)` method call, where `timeline` is a Timeline object. 
The `Id` of the timeline object needs to match the one we want to edit as the backend uses the `id` field to identify which row to update. This applies to delete as well.
Timeline entries that we retrieve by `ToListAsync()`, will have all the object's corresponding `Id` attached and the object returned by the result of `InsertAsync()` will also have its `Id` attached.

Lets create a `UpdateTimeline` method in our `AzureManager` activity 
```C#
    public async Task UpdateTimeline(Timeline timeline) {
        await this.timelineTable.UpdateAsync(timeline);
    }
``` 

NOTE: If no `Id` is present, an `ArgumentException` is raised.


To edit an existing timeline entry in our backend, we can invoke a `DeleteAsync(timeline)` method call, where `timeline` is a Timeline object. 
Likewise information concerning `Id` applies to delete as well.

Lets create a `DeleteTimeline` method in our `AzureManager` activity 
```C#
    public async Task DeleteTimeline(Timeline timeline) {
        await this.timelineTable.DeleteAsync(timeline);
    }
``` 
## 3. Bot Framework
:D TBC

### Extra Learning Resources
* [Using App Service with Xamarin by Microsoft](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-dotnet-how-to-use-client-library/)
* [Using App Service with Xamarin by Xamarin](https://blog.xamarin.com/getting-started-azure-mobile-apps-easy-tables/)
