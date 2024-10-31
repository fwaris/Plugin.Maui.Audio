﻿namespace Plugin.Maui.Audio;

partial class AudioPlayer : IAudioPlayer
{
	public AudioPlayer(Stream audioStream, AudioPlayerOptions audioPlayerOptions) { }

	public AudioPlayer(string fileName, AudioPlayerOptions audioPlayerOptions) { }

	public AudioPlayer(System.Threading.Channels.Channel<byte[]> channel, AudioPlayerOptions audioPlayerOptions) { }

	protected virtual void Dispose(bool disposing) { }

	public double Duration { get; }

	public double CurrentPosition { get; }

	public double Volume { get; set; }

	public double Balance { get; set; }

	public bool IsPlaying { get; }

	public bool Loop { get; set; }

	public bool CanSeek { get; }

	public void Play() { }

	public void Pause() { }

	public void Stop() { }

	public void SetSpeed(double speed) { }

	public void Seek(double position) { }

	public double Speed { get; }

	public double MinimumSpeed { get; }

	public double MaximumSpeed { get; }

	public bool CanSetSpeed { get; }
}