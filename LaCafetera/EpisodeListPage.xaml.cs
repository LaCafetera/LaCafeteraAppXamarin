using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;

namespace LaCafetera
{
	public partial class EpisodeListPage : ContentPage
	{
		public EpisodeListPage()
		{
			InitializeComponent();

			Episodes = new ObservableCollection<EpisodeViewModel>();

			for (int i = 0; i < 10; ++i)
			{
				EpisodeViewModel episode = new EpisodeViewModel();
				episode.ID = i.ToString();
				episode.Title = "Episode " + i;
				episode.URL = "<URL>";
				Episodes.Add(episode);
			}

			this.lstEpisodes.ItemsSource = Episodes;
		}

		ObservableCollection<EpisodeViewModel> Episodes { get; set; }

	}
}
