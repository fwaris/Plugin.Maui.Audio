using System;
using System.Diagnostics;
using System.Net.Mime;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Channels;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace Plugin.Maui.Audio;
class PcmAudioStream : IRandomAccessStream
{
	readonly Channel<byte[]> channel;
	ulong lastSeek;
	ulong lastBytes;
	bool canWrite = true;

	public PcmAudioStream(Channel<byte[]> channel)
	{
		this.channel = channel;
	}
	public bool CanRead => true;

	public bool CanWrite => true;

	public ulong Position => lastSeek;

	public ulong Size { get => this.Position; set => throw new NotImplementedException(); }

	public IRandomAccessStream CloneStream()
	{
		return this;
	}
	public void Dispose()
	{
		//the channel may be reused multiple times so don't close here
	}

	public IAsyncOperation<bool> FlushAsync()
	{
		return Task.FromResult(true).AsAsyncOperation();
	}

	public IInputStream GetInputStreamAt(ulong position)
	{
		return this;
	}

	public IOutputStream GetOutputStreamAt(ulong position)
	{
		return this; // throw new NotImplementedException();
	}
	public void Seek(ulong position)
	{
		//only write to the end of the stream
		if (position >= this.Position)
		{
			canWrite = true;
			this.lastSeek = position;	
			Debug.WriteLine($"seek {position}");
		}
		else
		{
			canWrite = false;
		}
	}
	IAsyncOperation<bool> IOutputStream.FlushAsync()
	{
		return AsyncInfo.FromResult(true);
	}
	IAsyncOperationWithProgress<IBuffer, uint> IInputStream.ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
	{
		return AsyncInfo.Run<IBuffer, uint>(async (cancellationToken, progress) =>
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return new byte[0].AsBuffer();
			}			
			byte[] data = await channel.Reader.ReadAsync(cancellationToken);
			this.lastSeek += (ulong)data.Length;
			Debug.WriteLine($"read {data.Length}");
			progress.Report((uint)this.lastSeek);
			return data.AsBuffer();
		});
	}
	IAsyncOperationWithProgress<uint, uint> IOutputStream.WriteAsync(IBuffer buffer)
	{
		return AsyncInfo.Run<uint, uint>((cancellationToken, progress) => WriteBufferAsync(buffer, progress, cancellationToken));
	}
	Task<uint> WriteBufferAsync(IBuffer buffer, IProgress<uint> progress, System.Threading.CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromResult(0u);
		}
		if (!canWrite)
		{
			return Task.FromResult(buffer.Length); //ignore write to earlier positions
		}
		byte[] data = buffer.ToArray();
		bool succ = channel.Writer.TryWrite(data);
		if (succ) { this.lastSeek += (ulong)data.Length; }
		progress.Report((uint)this.Position);
		return Task.FromResult((uint)data.Length);
	}
}

