# Integrating Cognitive Services 

## 1. Installing the Nuget Package

* Firstly search for and install ```Microsoft.ProjectOxford.Emotion``` in all your projects.

## 2. Calling Cognitive Services

* In MainPage.xaml.cs add ```using Microsoft.ProjectOxford.Emotion;``` to the top of your file.
* Set up UI

```
<Label x:Name="errorLabel" />
<Grid>
	<Grid.RowDefinitions>
		<RowDefinition Height="50*" />
		<RowDefinition Height="50*" />
	</Grid.RowDefinitions>
	<ListView x:Name="EmotionView" HasUnevenRows="True" Grid.Row="0" SeparatorVisibility="None">
		<ListView.ItemTemplate>
			<DataTemplate>
				<ViewCell>
					<Grid>
						<Grid.RowDefinitions>
							<RowDefinition Height="Auto" />
                        </Grid.RowDefinitions>
                        <Grid.ColumnDefinitions>
							<ColumnDefinition Width="50*" />
                            <ColumnDefinition Width="50*" />
                        </Grid.ColumnDefinitions>
                        <Label Grid.Column="0" Text="{Binding Key}"/>
			            <Label Grid.Column="1" Text="{Binding Value}"/>
                    </Grid>
                </ViewCell>
            </DataTemplate>
        </ListView.ItemTemplate>
	</ListView>
	<ActivityIndicator x:Name="UploadingIndicator" Color="Red" IsRunning="false" Grid.Row="0" />
</Grid>
```

* Next we'll pass the image the user took to get the current emotion.

```
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
```