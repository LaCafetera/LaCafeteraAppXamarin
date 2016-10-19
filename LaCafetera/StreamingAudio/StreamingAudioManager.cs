using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Threading;

#if __IOS__
using AudioToolbox;
using AVFoundation;
#endif

#if __ANDROID__
// TODO: Android-specific libraries
#endif

namespace StreamingAudio
{
	public enum PlayerOption
	{
		Stream = 0,
		StreamAndSave
	}


	public class StreamingAudioManager
	{
		public delegate void DownloadProgressDelegate(float progress);

		StreamingPlayback player;

		public PlayerOption currentPlayerOption { get; private set; }

		public string currentStreamURL { get; private set; }

		public DownloadProgressDelegate downloadProgressDelegate { get; set; }

		public event EventHandler<StreamingAudioErrorEventArgs> ErrorOccurred;

		public StreamingAudioManager()
		{
		}

		public void Play(string streamURL, PlayerOption playerOption)
		{
			bool audioSessionReady = false;
#if __IOS__
			audioSessionReady = AVAudioSession.SharedInstance() != null;
#endif

#if __ANDROID__
			// TODO: Android-specific code
#endif

			if (!audioSessionReady)
			{
				string description = "Unable to get AVAudioSession.SharedInstance";
				Debug.WriteLine(description);
				RaiseErrorOccurredEvent(description);
			}
			else {
				if (player != null)
				{
					player.Pause();
					player.Reset();
					//player.ResetOutputQueue();
					//player.FlushAndClose();
					//player.Dispose();
					player = null;
				}
				currentStreamURL = streamURL;
				currentPlayerOption = playerOption;
				StartPlayback(streamURL);
			}
		}

		void StartPlayback(string SourceUrl)
		{
			try
			{
				var request = (HttpWebRequest)WebRequest.Create(SourceUrl);
				request.BeginGetResponse(StreamDownloadedHandler, request);
			}
			catch (Exception e)
			{
				string description = "Error creating the WebRequest.";
				Debug.WriteLine(description);
				RaiseErrorOccurredEvent(description, e);
			}
		}

		void StreamDownloadedHandler(IAsyncResult result)
		{
			var buffer = new byte[8192];
			int l = 0;
			int inputStreamLength;
			double sampleRate = 0;

			Stream inputStream;
#if __IOS__
			AudioQueueTimeline timeline = null;
#endif

#if __ANDROID__
			// TODO: Android-specific code
#endif

			var request = result.AsyncState as HttpWebRequest;
			try
			{
				var response = request.EndGetResponse(result);
				var responseStream = response.GetResponseStream();

				if (currentPlayerOption == PlayerOption.StreamAndSave)
					inputStream = GetQueueStream(responseStream);
				else
					inputStream = responseStream;

				using (player = new StreamingPlayback())
				{
#if __IOS__
					player.OutputReady += delegate
					{
						timeline = player.OutputQueue.CreateTimeline();
						sampleRate = player.OutputQueue.SampleRate;
					};
#endif

#if __ANDROID__
					// TODO: Android-specific code
#endif

					/*
										InvokeOnMainThread(delegate
										{
											if (updatingTimer != null)
												updatingTimer.Invalidate();

											updatingTimer = NSTimer.CreateRepeatingScheduledTimer(0.5, (timer) => RepeatingAction(timeline, sampleRate));
										});
					*/
					while ((inputStreamLength = inputStream.Read(buffer, 0, buffer.Length)) != 0 && player != null)
					{
						l += inputStreamLength;
						player.ParseBytes(buffer, inputStreamLength, false, l == (int)response.ContentLength);
						float progress = l / (float)response.ContentLength;
						//Debug.WriteLine("progress: " + progress);
						if (downloadProgressDelegate != null)
						{
							downloadProgressDelegate(progress);
						}
					}
				}

			}
			catch (Exception e)
			{
				string description = "Error fetching response stream.";
				Debug.WriteLine(description);
				RaiseErrorOccurredEvent(description, e);
			}
		}


		Stream GetQueueStream(Stream responseStream)
		{
			var queueStream = new QueueStream(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/copy.mp3");
			var t = new Thread((x) =>
			{
				var tbuf = new byte[8192];
				int count;

				while ((count = responseStream.Read(tbuf, 0, tbuf.Length)) != 0)
					queueStream.Push(tbuf, 0, count);

			});
			t.Start();
			return queueStream;
		}

		void RaiseErrorOccurredEvent(string description, Exception exception=null)
		{
			StreamingAudioErrorEventArgs error =
				new StreamingAudioErrorEventArgs
				{
					Description = description,
					Exception = exception
				};
			RaiseErrorOccurredEvent(error);
		}

		void RaiseErrorOccurredEvent(StreamingAudioErrorEventArgs error)
		{
			if (ErrorOccurred != null)
			{
				ErrorOccurred(this, error);
			}
		}

	}
}
