using Xamarin.Forms;
using System;

using StreamingAudio;

namespace LaCafetera
{
	public partial class LaCafeteraPage : ContentPage
	{
		StreamingAudioManager streamingAudioManager;

		public LaCafeteraPage()
		{
			InitializeComponent();
			statusLabel.Text = "";
			streamingAudioManager = new StreamingAudioManager();
			streamingAudioManager.downloadProgressDelegate = new StreamingAudioManager.DownloadProgressDelegate(this.OnDownloadProgress);
			streamingAudioManager.ErrorOccurred += HandleStreamingAudioError;
		}

		void OnPlay(object sender, EventArgs e)
		{
			string streamURL = streamURLEntry.Text;
			streamingAudioManager.Play(streamURL, PlayerOption.Stream);
		}

		void OnDownloadProgress(float progress)
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				statusLabel.Text = "downloaded " + (((int)(progress * 10000)) / 100.0f) + "%";
			});
		}

		void HandleStreamingAudioError(object sender, StreamingAudioErrorEventArgs error)
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				//statusLabel.Text = error.Description;
				statusLabel.Text += "\n" + error.Description;
			});
		}
	}
}
