# 2.5 Bot Cards

## Introduction


## Resources
### Bootcamp Content
* [Video - Waiting](http://link.com)


## 1. Give appropriate message
In `2.4 Bot State Services` we introduced a boolean variable called `isWeatherRequest`, we will use that so we dont get multiple replies from our bot as we introduce cards it can start to look cluttering and for the command `clear` we are not seeking a weather request.

Now lets surround our info reply with an if statement, so that if its not a weather request reply with the desired message. `else` While when it is a weather request do the weather api call (surround the weather stuff in an `else` statement). 

So your replying code should look something like this, 
```C#
if (!isWeatherRequest)
{
    // return our reply to the user
    Activity infoReply = activity.CreateReply(endOutput);

    await connector.Conversations.ReplyToActivityAsync(infoReply);

}
else
{

    WeatherObject.RootObject rootObject;

    HttpClient client = new HttpClient();
    string x = await client.GetStringAsync(new Uri("http://api.openweathermap.org/data/2.5/weather?q=" + activity.Text + "&units=metric&APPID=440e3d0ee33a977c5e2fff6bc12448ee"));

    rootObject = JsonConvert.DeserializeObject<WeatherObject.RootObject>(x);

    string cityName = rootObject.name;
    string temp = rootObject.main.temp + "°C";
    string pressure = rootObject.main.pressure + "hPa";
    string humidity = rootObject.main.humidity + "%";
    string wind = rootObject.wind.deg + "°";

    // return our reply to the user
    Activity reply = activity.CreateReply($"Current weather for {cityName} is {temp}, pressure {pressure}, humidity {humidity}, and wind speeds of {wind}");
    await connector.Conversations.ReplyToActivityAsync(reply);
}
```

So we wont really be seeing our `Hello` and `Hello again` messages anymore, but by now you should have a fair understanding of how bot state services work.

### Extra Learning Resources
* [Bot Cards](https://docs.botframework.com/en-us/csharp/builder/sdkreference/attachments.html)