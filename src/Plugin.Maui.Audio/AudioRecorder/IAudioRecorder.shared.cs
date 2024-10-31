
namespace Plugin.Maui.Audio;

/// <summary>
/// Provides the ability to record audio.
/// </summary>
public interface IAudioRecorder
{
	///<Summary>
	/// Gets whether the device is capable of recording audio.
	///</Summary>
	bool CanRecordAudio { get; }

	///<Summary>
	/// Gets whether the recorder is currently recording audio.
	///</Summary>
	bool IsRecording { get; }

	///<Summary>
	/// Start recording audio to disk in a randomly generated file.
	///</Summary>
	Task StartAsync();

	///<Summary>
	/// Start recording audio to disk in the supplied <paramref name="filePath"/>.
	///</Summary>
	///<param name="filePath">The path on disk to store the recording.</param>
	Task StartAsync(string filePath);

	///<Summary>
	/// Start recording audio to disk in a randomly generated file. AudioRecordingOptions are only supported on Android and iOS.
	///</Summary>
	///<param name="options">The audio recording options.</param>
	public Task StartAsync(AudioRecordingOptions options) => Task.CompletedTask;

	///<Summary>
	/// Start recording audio to disk in the supplied <paramref name="filePath"/>.
	///</Summary>
	///<param name="filePath">The path on disk to store the recording. AudioRecordingOptions are only supported on Android and iOS. If options are used, read the audio stream and write your own file. The default header might not match your options.</param>
	///<param name="options">The audio recording options.</param>
	public Task StartAsync(string filePath, AudioRecordingOptions options) => Task.CompletedTask;


	///<Summary>
	/// Post raw PCM audio chunks to  <paramref name="channel"/>.
	///</Summary>
	///<param name="channel">channel to receive audio chunks</param>
	public Task StartPcmAsync(System.Threading.Channels.Channel<byte[]> channel);

	///<Summary>
	/// Post raw PCM audio chunks to  <paramref name="channel"/>.
	///</Summary>
	///<param name="channel">channel to receive audio chunks</param>
	///<param name="options">The audio recording options.</param>
	public Task StartPcmAsync(System.Threading.Channels.Channel<byte[]> channel, AudioRecordingOptions options);

	///<Summary>
	/// Stop recording. Use this method when using <see cref="StartPcmAsync(System.Threading.Channels.Channel{byte[]}, AudioRecordingOptions)"/>
	///</Summary>
	public Task StopPcmAsync();

	///<Summary>
	/// Stop recording and return the <see cref="IAudioSource"/> instance with the recording data.
	///</Summary>
	Task<IAudioSource> StopAsync();

	static byte[] StreamWaveFileHeader(int sampleRate, int bitsPerSample, int channels)
	{
		int byteRate = sampleRate * channels * (bitsPerSample / 8);
		int blockAlign = channels * (bitsPerSample / 8);
		int subChunk2Size = 0; // 0 for streaming
		int chunkSize = 36 + subChunk2Size;

		byte[] header = new byte[44];

		// ChunkID
		header[0] = (byte)'R';
		header[1] = (byte)'I';
		header[2] = (byte)'F';
		header[3] = (byte)'F';

		// ChunkSize
		BitConverter.GetBytes(chunkSize).CopyTo(header, 4);

		// Format
		header[8] = (byte)'W';
		header[9] = (byte)'A';
		header[10] = (byte)'V';
		header[11] = (byte)'E';

		// Subchunk1ID
		header[12] = (byte)'f';
		header[13] = (byte)'m';
		header[14] = (byte)'t';
		header[15] = (byte)' ';

		// Subchunk1Size (16 for PCM)
		BitConverter.GetBytes(16).CopyTo(header, 16);

		// AudioFormat (1 for PCM)
		BitConverter.GetBytes((short)1).CopyTo(header, 20);

		// NumChannels
		BitConverter.GetBytes((short)channels).CopyTo(header, 22);

		// SampleRate
		BitConverter.GetBytes(sampleRate).CopyTo(header, 24);

		// ByteRate
		BitConverter.GetBytes(byteRate).CopyTo(header, 28);

		// BlockAlign
		BitConverter.GetBytes((short)blockAlign).CopyTo(header, 32);

		// BitsPerSample
		BitConverter.GetBytes((short)bitsPerSample).CopyTo(header, 34);

		// Subchunk2ID
		header[36] = (byte)'d';
		header[37] = (byte)'a';
		header[38] = (byte)'t';
		header[39] = (byte)'a';

		// Subchunk2Size
		BitConverter.GetBytes(subChunk2Size).CopyTo(header, 40);

		return header;
	}
}