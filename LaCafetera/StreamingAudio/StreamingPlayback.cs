using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

#if __IOS__
using AudioToolbox;
#endif

#if __ANDROID__
// TODO: Android-specific libraries
#endif

namespace StreamingAudio
{
	/// <summary>
	/// A Class to hold the AudioBuffer with all setting together
	/// </summary>
	internal class AudioBuffer
	{
		public IntPtr Buffer { get; set; }

#if __IOS__
		public List<AudioStreamPacketDescription> PacketDescriptions { get; set; }
#endif

#if __ANDROID__
		// TODO: Android-specific code
#endif

		public int CurrentOffset { get; set; }

		public bool IsInUse { get; set; }
	}

	/// <summary>
	/// Wrapper around OutputQueue and AudioFileStream to allow streaming of various filetypes
	/// </summary>
	public class StreamingPlayback : IDisposable
	{
		public event EventHandler Finished;
#if __IOS__
		public event Action<OutputAudioQueue> OutputReady;

		// the AudioToolbox decoder
		AudioFileStream fileStream;
#endif

#if __ANDROID__
		// TODO: Android-specific code
#endif
		int bufferSize = 128 * 1024;
		List<AudioBuffer> outputBuffers;
		AudioBuffer currentBuffer;
		// Maximum buffers
		int maxBufferCount = 4;
		// Keep track of all queued up buffers, so that we know that the playback finished
		int queuedBufferCount = 0;
		// Current Filestream Position - if we don't keep track we don't know when to push the last uncompleted buffer
		long currentByteCount = 0;
		//Used to trigger a dump of the last buffer.
		bool lastPacket;

#if __IOS__
		public OutputAudioQueue OutputQueue;
#endif

#if __ANDROID__
		// TODO: Android-specific code
#endif

		public bool Started  { get; private set; }

		public float Volume {
#if __IOS__
			get {
				return OutputQueue.Volume;
			}

			set {
				OutputQueue.Volume = value;
			}
#endif

#if __ANDROID__
			// TODO: Android-specific code
			get
			{
				return 0;
			}

			set
			{
			}
#endif
		}

		/// <summary>
		/// Defines the size forearch buffer, when using a slow source use more buffers with lower buffersizes
		/// </summary>
		public int BufferSize {
			get {
				return bufferSize;
			}

			set {
				bufferSize = value;
			}
		}

		/// <summary>
		/// Defines the maximum Number of Buffers to use, the count can only change after Reset is called or the
		/// StreamingPlayback is freshly instantiated
		/// </summary>
		public int MaxBufferCount {
			get {
				return maxBufferCount;
			}

			set {
				maxBufferCount = value;
			}
		}

		public StreamingPlayback ()
		{
#if __IOS__
			fileStream = new AudioFileStream (AudioFileType.MP3);
			fileStream.PacketDecoded += AudioPacketDecoded;
			fileStream.PropertyFound += AudioPropertyFound;
#endif

#if __ANDROID__
			// TODO: Android-specific code
#endif

		}

		public void Reset ()
		{
#if __IOS__
			if (fileStream != null) {
				fileStream.Close ();
				fileStream = new AudioFileStream (AudioFileType.MP3);
				currentByteCount = 0;
				fileStream.PacketDecoded += AudioPacketDecoded;
				fileStream.PropertyFound += AudioPropertyFound;
			}
#endif

#if __ANDROID__
			// TODO: Android-specific code
#endif
		}

		public void ResetOutputQueue ()
		{
#if __IOS__
			if (OutputQueue != null) {
				OutputQueue.Stop (true);
				OutputQueue.Reset ();
				foreach (AudioBuffer buf in outputBuffers) {
					buf.PacketDescriptions.Clear ();
					OutputQueue.FreeBuffer (buf.Buffer);
				}
				outputBuffers = null;
				OutputQueue.Dispose ();
			}
#endif

#if __ANDROID__
			// TODO: Android-specific code
#endif
		}

		/// <summary>
		/// Stops the OutputQueue
		/// </summary>
		public void Pause ()
		{
#if __IOS__
			OutputQueue.Pause ();
#endif

#if __ANDROID__
			// TODO: Android-specific code
#endif
			Started = false;
		}

		/// <summary>
		/// Starts the OutputQueue
		/// </summary>
		public void Play ()
		{
#if __IOS__
			OutputQueue.Start ();
#endif

#if __ANDROID__
			// TODO: Android-specific code
#endif
			Started = true;
		}

		/// <summary>
		/// Main method to kick off the streaming, just send the bytes to this method
		/// </summary>
		public void ParseBytes (byte[] buffer, int count, bool discontinuity, bool lastPacket)
		{
			this.lastPacket = lastPacket;
#if __IOS__
			fileStream.ParseBytes (buffer, 0, count, discontinuity);
#endif

#if __ANDROID__
			// TODO: Android-specific code
#endif
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		/// <summary>
		/// Cleaning up all the native Resource
		/// </summary>
		protected virtual void Dispose (bool disposing)
		{
			if (disposing)
			{
#if __IOS__
				if (OutputQueue != null)
					OutputQueue.Stop(true);

				if (outputBuffers != null) {
					foreach (var b in outputBuffers)
						OutputQueue.FreeBuffer (b.Buffer);

					outputBuffers.Clear ();
					outputBuffers = null;
				}

				if (fileStream != null) {
					fileStream.Close ();
					fileStream = null;
				}

				if (OutputQueue != null) {
					OutputQueue.Dispose ();
					OutputQueue = null;
				}
#endif

#if __ANDROID__
				// TODO: Android-specific code
#endif
			}
		}


#if __IOS__
		/// <summary>
		/// Saving the decoded Packets to our active Buffer, if the Buffer is full queue it into the OutputQueue
		/// and wait until another buffer gets freed up
		/// </summary>
		void AudioPacketDecoded (object sender, PacketReceivedEventArgs args)
		{
			foreach (var p in args.PacketDescriptions) {
				currentByteCount += p.DataByteSize;

				AudioStreamPacketDescription pd = p;

				int left = bufferSize - currentBuffer.CurrentOffset;
				if (left < pd.DataByteSize) {
					EnqueueBuffer ();
					WaitForBuffer ();
				}

				AudioQueue.FillAudioData (currentBuffer.Buffer, currentBuffer.CurrentOffset, args.InputData, (int)pd.StartOffset, pd.DataByteSize);
				// Set new offset for this packet
				pd.StartOffset = currentBuffer.CurrentOffset;
				// Add the packet to our Buffer
				currentBuffer.PacketDescriptions.Add (pd);
				// Add the Size so that we know how much is in the buffer
				currentBuffer.CurrentOffset += pd.DataByteSize;
			}

			if ((fileStream != null && currentByteCount == fileStream.DataByteCount) || lastPacket)
				EnqueueBuffer ();
		}
#endif

		/// <summary>
		/// Flush the current buffer and close the whole thing up
		/// </summary>
		public void FlushAndClose ()
		{
#if __IOS__
			if (OutputQueue != null) {
				EnqueueBuffer ();
				OutputQueue.Flush ();
			}
#endif

#if __ANDROID__
			// TODO: Android-specific code
#endif

			Dispose ();
		}

		/// <summary>
		/// Enqueue the active buffer to the OutputQueue
		/// </summary>
		void EnqueueBuffer ()
		{
			currentBuffer.IsInUse = true;
#if __IOS__
			OutputQueue.EnqueueBuffer (currentBuffer.Buffer, currentBuffer.CurrentOffset, currentBuffer.PacketDescriptions.ToArray ());
#endif

#if __ANDROID__
			// TODO: Android-specific code
#endif
			queuedBufferCount++;
			StartQueueIfNeeded ();
		}

		/// <summary>
		/// Wait until a buffer is freed up
		/// </summary>
		void WaitForBuffer ()
		{
			int curIndex = outputBuffers.IndexOf (currentBuffer);
			currentBuffer = outputBuffers [curIndex < outputBuffers.Count - 1 ? curIndex + 1 : 0];

			lock (currentBuffer) {
				while (currentBuffer.IsInUse)
					Monitor.Wait (currentBuffer);
			}
		}

		void StartQueueIfNeeded ()
		{
			if (Started)
				return;

			Play ();
		}

#if __IOS__
		/// <summary>
		/// When a AudioProperty in the fed packets is found this callback is called
		/// </summary>
		void AudioPropertyFound (object sender, PropertyFoundEventArgs args)
		{
			if (args.Property == AudioFileStreamProperty.ReadyToProducePackets) {
				Started = false;

				if (OutputQueue != null)
					OutputQueue.Dispose ();

				OutputQueue = new OutputAudioQueue (fileStream.StreamBasicDescription);
				if (OutputReady != null)
					OutputReady (OutputQueue);

				currentByteCount = 0;
				OutputQueue.BufferCompleted += HandleBufferCompleted;
				outputBuffers = new List<AudioBuffer> ();

				for (int i = 0; i < MaxBufferCount; i++) {
					IntPtr outBuffer;
					OutputQueue.AllocateBuffer (BufferSize, out outBuffer);
					outputBuffers.Add (new AudioBuffer () {
						Buffer = outBuffer,
						PacketDescriptions = new List<AudioStreamPacketDescription> ()
					});
				}

				currentBuffer = outputBuffers.First ();

				OutputQueue.MagicCookie = fileStream.MagicCookie;
			}
		}

		/// <summary>
		/// Is called when a buffer is completly read and can be freed up
		/// </summary>
		void HandleBufferCompleted (object sender, BufferCompletedEventArgs e)
		{
			queuedBufferCount--;
			IntPtr buf = e.IntPtrBuffer;

			foreach (var buffer in outputBuffers) {
				if (buffer.Buffer != buf)
					continue;

				// free Buffer
				buffer.PacketDescriptions.Clear ();
				buffer.CurrentOffset = 0;
				lock (buffer) {
					buffer.IsInUse = false;
					Monitor.Pulse (buffer);
				}
			}

			if (queuedBufferCount == 0 && Finished != null)
				Finished (this, new EventArgs ());
		}
#endif
	}
}
