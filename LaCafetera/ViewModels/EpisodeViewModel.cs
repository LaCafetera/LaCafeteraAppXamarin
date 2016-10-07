using System;
namespace LaCafetera
{
	public class EpisodeViewModel
	{
		public EpisodeViewModel()
		{
		}

		public String ID { get; set; }
		public String Title { get; set; }
		public String Duration { get; set; }
		public String PublishedAt { get; set; }
		public String ImageURL { get; set; }
		public String URL { get; set; }
	}
}
