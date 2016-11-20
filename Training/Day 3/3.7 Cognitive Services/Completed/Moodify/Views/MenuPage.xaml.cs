using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Moodify
{
	public partial class MenuPage : ContentPage
	{
		public MenuPage()
		{
			BindingContext = new MenuPageViewModel();
			Title = "Menu";
            Icon = Device.OS == TargetPlatform.iOS ? "menu.png" : null;
            InitializeComponent();
		}
	}
}
