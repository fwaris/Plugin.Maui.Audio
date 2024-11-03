namespace Plugin.Maui.Audio;

/// <summary>
/// Provides the ability to play audio.
/// </summary>
public interface IAudioPlayer : IAudio
{
	///<Summary>
	/// Raised when audio playback completes successfully.
	///</Summary>
	event EventHandler PlaybackEnded;

	///<Summary>
	/// Begin playback or resume if paused.
	///</Summary>
	void Play();

	///<Summary>
	/// Pause playback if playing (does not resume).
	///</Summary>
	void Pause();

	///<Summary>
	/// Stop playback and set the current position to the beginning.
	///</Summary>
	void Stop();

	static byte[] WaveFileHeader(long dataLength, int sampleRate, int channels, int bitsPerSample)
	{
		byte[] header = new byte[44];
		int byteRate = sampleRate * channels * bitsPerSample / 8;
		int audioLength = (int)(dataLength + 36);

		header[0] = Convert.ToByte('R'); // RIFF/WAVE header
		header[1] = Convert.ToByte('I'); // (byte)'I'
		header[2] = Convert.ToByte('F');
		header[3] = Convert.ToByte('F');
		header[4] = (byte)(dataLength & 0xff);
		header[5] = (byte)((dataLength >> 8) & 0xff);
		header[6] = (byte)((dataLength >> 16) & 0xff);
		header[7] = (byte)((dataLength >> 24) & 0xff);
		header[8] = Convert.ToByte('W');
		header[9] = Convert.ToByte('A');
		header[10] = Convert.ToByte('V');
		header[11] = Convert.ToByte('E');
		header[12] = Convert.ToByte('f'); // fmt chunk
		header[13] = Convert.ToByte('m');
		header[14] = Convert.ToByte('t');
		header[15] = (byte)' ';
		header[16] = 16; // 4 bytes - size of fmt chunk
		header[17] = 0;
		header[18] = 0;
		header[19] = 0;
		header[20] = 1; // format = 1
		header[21] = 0;
		header[22] = Convert.ToByte(channels);
		header[23] = 0;
		header[24] = (byte)(sampleRate & 0xff);
		header[25] = (byte)((sampleRate >> 8) & 0xff);
		header[26] = (byte)((sampleRate >> 16) & 0xff);
		header[27] = (byte)((sampleRate >> 24) & 0xff);
		header[28] = (byte)(byteRate & 0xff);
		header[29] = (byte)((byteRate >> 8) & 0xff);
		header[30] = (byte)((byteRate >> 16) & 0xff);
		header[31] = (byte)((byteRate >> 24) & 0xff);
		header[32] = (byte)(2 * 16 / 8); // block align
		header[33] = 0;
		header[34] = Convert.ToByte(16); // bits per sample
		header[35] = 0;
		header[36] = Convert.ToByte('d');
		header[37] = Convert.ToByte('a');
		header[38] = Convert.ToByte('t');
		header[39] = Convert.ToByte('a');
		header[40] = (byte)(audioLength & 0xff);
		header[41] = (byte)((audioLength >> 8) & 0xff);
		header[42] = (byte)((audioLength >> 16) & 0xff);
		header[43] = (byte)((audioLength >> 24) & 0xff);
		return header;
	}
}