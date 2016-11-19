# Weather Bot

## 1. Get OpenWather API Key

* Navigate to http://openweathermap.org/appid.
* Scroll down until you see 'How to get API key (APPID)'.
* Click 'Sign up' to create an account and grab yourself an API key.
* Save your API key somewhere safe.

## 2. Making a get request to OpenWather

* Create a new bot project.
* Open 'MessagesController.cs' (found inside 'Controllers').

To call OpenWather we need to initialise a new instance of HttpClient. Add the following line within the ```public async Task<HttpResponseMessage> Post([FromBody]Activity activity)``` method.

```
HttpClient client = new HttpClient();
```

The URL we'll be making to grab the current weather based is...

```
http://api.openweathermap.org/data/2.5/weather?q=[%%% CITY %%%]&units=metric&APPID=[%%% API KEY %%%]"
```

Remember to replace ```[%%% API KEY %%%]``` with your API key.

* To make the request and get a response we'll next add the following line.

```
string x = await client.GetStringAsync(new Uri("http://api.openweathermap.org/data/2.5/weather?q=" + activity.Text + "&units=metric&APPID=440e3d0ee33a977c5e2fff6bc12448ee"));
```

## 3. Create model to bind the response to

* Create a new class and call it 'WeatherObject.cs'.
* Based on the response given by OpenWather.

```
 public class WeatherObject
    {
        public class Coord
        {
            public double lon { get; set; }
            public double lat { get; set; }
        }

        public class Weather
        {
            public int id { get; set; }
            public string main { get; set; }
            public string description { get; set; }
            public string icon { get; set; }
        }

        public class Main
        {
            public double temp { get; set; }
            public int pressure { get; set; }
            public int humidity { get; set; }
            public int temp_min { get; set; }
            public double temp_max { get; set; }
        }

        public class Wind
        {
            public double speed { get; set; }
            public int deg { get; set; }
        }

        public class Clouds
        {
            public int all { get; set; }
        }

        public class Sys
        {
            public int type { get; set; }
            public int id { get; set; }
            public double message { get; set; }
            public string country { get; set; }
            public int sunrise { get; set; }
            public int sunset { get; set; }
        }

        public class RootObject
        {
            public Coord coord { get; set; }
            public List<Weather> weather { get; set; }
            public string @base { get; set; }
            public Main main { get; set; }
            public Wind wind { get; set; }
            public Clouds clouds { get; set; }
            public int dt { get; set; }
            public Sys sys { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public int cod { get; set; }
        }
    }
```

## 4. Deserialising the reponse

* Create a new instance of the new weather model by adding ```WeatherObject.RootObject rootObject;```
* Next we'll deseralise the response we got from OpenWeather and assign it to the new weather object. Add ```rootObject = JsonConvert.DeserializeObject<WeatherObject.RootObject>(x);```

## 5. Send the data back to the user

To send back data to the user add the following lines.

```
string cityName = rootObject.name;
string temp = rootObject.main.temp + "°C";
string pressure = rootObject.main.pressure + "hPa";
string humidity = rootObject.main.humidity + "%";
string wind = rootObject.wind.deg + "°";

Activity reply = activity.CreateReply($"Current weather for {cityName} is {temp}, pressure {pressure}, humidity {humidity}, and wind speeds of {wind}");
await connector.Conversations.ReplyToActivityAsync(reply);
```