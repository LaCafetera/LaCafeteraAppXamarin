using System;

namespace StreamingAudio
{
	public class StreamingAudioErrorEventArgs : EventArgs
	{
		public string Description { get; set; }
		public System.Exception Exception { get; set; }
	}
}

