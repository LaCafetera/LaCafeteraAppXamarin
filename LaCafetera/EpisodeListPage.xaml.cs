using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.IO;
using System.Threading.Tasks;

using Xamarin.Forms;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LaCafetera
{
	public partial class EpisodeListPage : ContentPage
	{
		public EpisodeListPage()
		{
			InitializeComponent();

			Episodes = new ObservableCollection<EpisodeViewModel>();
			this.lstEpisodes.ItemsSource = Episodes;
		}

		ObservableCollection<EpisodeViewModel> Episodes { get; set; }

		override protected void OnAppearing()
		{
			base.OnAppearing();

			LoadEpisodesAsync();
		}

		private async void LoadEpisodesAsync()
		{
			string url = "https://api.spreaker.com/v2/shows/1060718/episodes?limit=5";
			List<EpisodeViewModel> episodes = await FetchEpisodeListAsync(url);
			foreach (EpisodeViewModel episode in episodes)
			{
				Episodes.Add(episode);
			}
		}

		private async Task<List<EpisodeViewModel>> FetchEpisodeListAsync(string url)
		{
			List<EpisodeViewModel> episodes = new List<EpisodeViewModel>();
					
			// Create an HTTP web request using the URL:
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
			request.ContentType = "application/json";
			request.Method = "GET";

			// Send the request to the server and wait for the response:
			using (WebResponse response = await request.GetResponseAsync())
			{
				// Get a stream representation of the HTTP web response:
				using (Stream stream = response.GetResponseStream())
				using (StreamReader streamReader = new StreamReader(stream))
				using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
				{
					JObject jsonResponse = (JObject)JToken.ReadFrom(jsonReader);
					//string stResponse = jsonResponse.ToString();
					//Console.WriteLine(stResponse);
					JToken responseVal;
					if (jsonResponse.TryGetValue("response", out responseVal))
					{
						JToken items;
						if (((JObject)responseVal).TryGetValue("items", out items) && (items.Type == JTokenType.Array))
						{
							int itemsCount = ((JArray)items).Count;
							foreach (JObject jsonEpisode in ((JArray)items).Children<JObject>())
							{
								EpisodeViewModel episode = new EpisodeViewModel();
								episode.ID = jsonEpisode["episode_id"].ToString();
								episode.Title = jsonEpisode["title"].ToString();
								episode.Duration = jsonEpisode["duration"].ToString();
								episode.PublishedAt = jsonEpisode["published_at"].ToString();
								episode.ImageURL = jsonEpisode["image_url"].ToString();
								episode.URL = "https://api.spreaker.com/listen/episode/"+episode.ID+"/http";

								episodes.Add(episode);
							}
						}
						JToken nextURL;
						if (((JObject)responseVal).TryGetValue("next_url", out nextURL))
						{
							// TODO: use this URL for loading following episodes
							//string NextURLString = nextURL.ToString();
						}
					}
				}
			}

			return episodes;
		}

	}
}
