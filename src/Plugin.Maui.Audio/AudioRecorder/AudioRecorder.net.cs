using System.Threading.Channels;

namespace Plugin.Maui.Audio;

partial class AudioRecorder : IAudioRecorder
{
	public AudioRecorder(AudioRecorderOptions options)
	{
	}

	public bool CanRecordAudio => false;

	public bool IsRecording => false;

	public Task StartPcmAsync(Channel<byte[]> channel) => StartPcmAsync(channel, DefaultAudioRecordingOptions.DefaultPcmOptions);

	public Task StartPcmAsync(Channel<byte[]> channel, AudioRecordingOptions options) => Task.CompletedTask;
	public Task StopPcmAsync() => Task.CompletedTask;

	public Task StartAsync(AudioRecordingOptions options) => Task.CompletedTask;

	public Task StartAsync(string filePath, AudioRecordingOptions options) => Task.CompletedTask;

	public Task StartAsync() => Task.CompletedTask;

	public Task StartAsync(string filePath) => Task.CompletedTask;

	public Task<IAudioSource> StopAsync() => Task.FromResult<IAudioSource>(new EmptyAudioSource());
}