using System.Threading.Channels;

namespace Plugin.Maui.Audio;

/// <summary>
/// Implementation of <see cref="IAudioManager"/> providing the ability to create audio playback and recording instances.
/// </summary>
public class AudioManager : IAudioManager
{
	static IAudioManager? currentImplementation;
	
	/// <summary>
	/// Gets the current implementation of the audio manager.
	/// </summary>
	public static IAudioManager Current => currentImplementation ??= new AudioManager();

	/// <inheritdoc cref="IAudioManager.DefaultPlayerOptions"/>
	public AudioPlayerOptions DefaultPlayerOptions { get; set; } = new();

	/// <inheritdoc cref="IAudioManager.DefaultRecorderOptions"/>
	public AudioRecorderOptions DefaultRecorderOptions { get; set; } = new();

	/// <inheritdoc cref="IAudioManager.CreatePlayer(Stream, AudioPlayerOptions)" />
	public IAudioPlayer CreatePlayer(Stream audioStream, AudioPlayerOptions? options = default)
	{
		ArgumentNullException.ThrowIfNull(audioStream);

		return new AudioPlayer(audioStream, options ?? DefaultPlayerOptions);
	}

	/// <inheritdoc cref="IAudioManager.CreatePlayer(string, AudioPlayerOptions)" />
	public IAudioPlayer CreatePlayer(string fileName, AudioPlayerOptions? options = default)
	{
		ArgumentNullException.ThrowIfNull(fileName);

        return new AudioPlayer(fileName, options ?? DefaultPlayerOptions);
    }
	public IAudioPlayer CreatePlayer(System.Threading.Channels.Channel<byte[]> channel, AudioPlayerOptions? options = default)
	{
		ArgumentNullException.ThrowIfNull(channel);

        return new AudioPlayer(channel, options ?? DefaultPlayerOptions);
    }

	/// <inheritdoc cref="IAudioManager.CreateAsyncPlayer(string, AudioPlayerOptions)" />
	public AsyncAudioPlayer CreateAsyncPlayer(Stream audioStream, AudioPlayerOptions? options = default) => new (CreatePlayer(audioStream));

	/// <inheritdoc cref="IAudioManager.CreateAsyncPlayer(string, AudioPlayerOptions)" />
	public AsyncAudioPlayer CreateAsyncPlayer(string fileName, AudioPlayerOptions? options = default) => new (CreatePlayer(fileName));


	public AsyncAudioPlayer CreateAsyncPlayer(Channel<byte[]> channel, AudioPlayerOptions? options = null) => new(CreatePlayer(channel));

	/// <inheritdoc cref="IAudioManager.CreateRecorder(AudioRecorderOptions)" />
	public IAudioRecorder CreateRecorder(AudioRecorderOptions? options = default)
	{
		return new AudioRecorder(options ?? DefaultRecorderOptions);
	}

}
