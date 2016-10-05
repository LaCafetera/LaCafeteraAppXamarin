using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace LaCafetera
{
	public partial class EpisodeListPage : ContentPage
	{
		public EpisodeListPage()
		{
			InitializeComponent();

			List<EpisodeViewModel> episodes = new List<EpisodeViewModel>();
			EpisodeViewModel episode;
			for (int i = 0; i < 10; ++i)
			{
				episode = new EpisodeViewModel();
				episode.ID =  i.ToString();
				episode.Title = "Episode " + i;
				episode.URL = "<URL>";
				episodes.Add(episode);
			}

			this.lstEpisodes.ItemsSource = episodes;
		}
	}
}
