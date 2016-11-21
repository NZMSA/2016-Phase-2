# 2.4 State Services

## Introduction

"A key to good bot design is to
* make the web service stateless so that it can be scaled
* make it track context of a conversation."

Thus the Bot Framework has a service for storing bot state. This is powerful in any bot application, so we can personalize the experience or allows tracking of things like number of times they said a particular word.

For this tutorial we are going to work on the base `Weather Bot` project

The completed code of the tutorial for `MessagesController.cs` has been provided in the folder as this is the only file that has changed.

## Resources
### Bootcamp Content
* [Video - Waiting](http://link.com)

## 1. Setup State Client

To access the state client its pretty easy and we can do it if we have an `Activity` object (which is a parameter passed in when the bot framework recieves a message)

In `MessagesController.cs`, add this line of code after the line `ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));`

```C#
StateClient stateClient = activity.GetStateClient();
```
## 2. Grab users data
To access bot data of a user, we can invoke the state client by providing the `ChannelId` (which is the channel of the activity ie skype) and the `Id` of the user which we can gather from `activity.From`.

```C#
BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
```

## 3. Get/Set users property data
Now lets test setting data and retrieving data of the calling user.

After the code from above, add the following code,
```C#
    var userMessage = activity.Text;

    string endOutput = "Hello";

    // calculate something for us to return
    if (userData.GetProperty<bool>("SentGreeting"))
    {
        endOutput = "Hello again";
    }
    else
    {
        userData.SetProperty<bool>("SentGreeting", true);
        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
    }

    // return our reply to the user
    Activity infoReply = activity.CreateReply(endOutput);

    await connector.Conversations.ReplyToActivityAsync(infoReply);
```

What is happening is we have a boolean property called `SentGreeting`, 
* When the user first talks to the bot this property does not exist thus will be default value of `false`.
* This property is then set as `true` by `userData.SetProperty<bool>("SentGreeting", true);`
* Then we sync this updated `userData` information by calling upon the state client `await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);`
* NOTE: If we didnt sync up, the changes will not be reflected if we asked for the user data again

* Thus if we have already greeted the bot now says "Hello again" whilst if its the first time the bot says "Hello"

## 4. Clear users property data
Now lets start adding commands to our bot, so that it would recognize phrases to do certain actions. `Help` is a very common command.

These commands we dont want them to be recognized as weather requests so we introduced a bool variable called `isWeatherRequest`, and set that to false at places where its not a weather request

Add the following code before the line  `// return our reply to the user`
```C#
bool isWeatherRequest = true;

if (userMessage.ToLower().Contains("clear"))
{
    endOutput = "User data cleared";
    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
    isWeatherRequest = false;
}
```

So whenever `clear` is sent as a message, we delete user data by telling the state client. Thus the boolean property called `SentGreeting` would not exist anymore!

So the next time we message, the bot would say `Hello`.

## 5. Setting a home preference
Now for more useful use of the bot state service, we want to set a home city so that whenever we message `home` we can the weather of that city.

To set up our home city, we'd ask the user to send a message like `set home Auckland`.

Add the following code after the if block statement `if (userMessage.ToLower().Contains("clear")) { .... }`
```C#
    if (userMessage.Length > 9)
    {
        if (userMessage.ToLower().Substring(0, 8).Equals("set home"))
        {
            string homeCity = userMessage.Substring(9);
            userData.SetProperty<string>("HomeCity", homeCity);
            await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
            endOutput = homeCity;
            isWeatherRequest = false;
        }
    }
```

* We first check if the length of the message is greater than 9 (this is to avoid array excpetion errors when we substring)
* We use 9 because `set home` is 8 letters plus a space ` ` would then be 9 letters, plus the city name would then make it greater than 9 letters.
* `Substring(0, 8)` we grab the first 8 letters of the message and check if it equals to `set home`
* We grab the city by grabbing the 10th letter (after the space so index of 9) to the end of the string `string homeCity = userMessage.Substring(9);`
* We then the property of `HomeCity` to the `homecity` variable, in this case the property is of type `string`
* Then we sync this updated `userData` information by calling upon the state client `await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);`

* Now the user data has a property `HomeCity` that contains their designated city

## 6. Requesting weather for home
Now lets add a command for `home`, such that the it would request the weather details of our set home city.

Add the following code after the before if block statement `if (userMessage.Length > 9) { .... }`
```C#
if (userMessage.ToLower().Equals("home"))
{
    string homecity = userData.GetProperty<string>("HomeCity");
    if (homecity == null)
    {
        endOutput = "Home City not assigned";
        isWeatherRequest = false;
    }
    else
    {
        activity.Text = homecity;
    }
}
```

* If the `HomeCity` property doesnt exist in case for string types itd be defaulted to null at which we give the user an error message
* If `homeCity` has been set we assign it to `activity.Text`, as this is used as the "city" in our weather api request ` string x = await client.GetStringAsync(new Uri("http://api.openweathermap.org/data/2.5/weather?q=" + activity.Text + "&units=metric&APPID=440e3d0ee33a977c5e2fff6bc12448ee"));`

Now when we have a homecity set and tell the bot `home` the bot will give us weather of that city.

### Extra Learning Resources
* [Bot State Service](https://docs.botframework.com/en-us/csharp/builder/sdkreference/stateapi.html)

