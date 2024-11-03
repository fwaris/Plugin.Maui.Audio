using System;
using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Channels;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Networking.Connectivity;
using Windows.Storage.Streams;

namespace Plugin.Maui.Audio;
class PcmAudioStream : IRandomAccessStream
{
	readonly Channel<byte[]> channel;
	ulong lastSeek;
	ulong lastBytes;
	bool canWrite = true;
	IBuffer cache;
	uint cacheBytesWritten = 0;

	public PcmAudioStream(Channel<byte[]> channel, uint bufferSize = 4096*2*2)
	{
		this.channel = channel;
		cache = new Windows.Storage.Streams.Buffer(bufferSize);
	}
	public bool CanRead => true;

	public bool CanWrite => true;

	public ulong Position => lastSeek;

	public ulong Size { get => this.Position; set => throw new NotImplementedException(); }

	public IRandomAccessStream CloneStream()
	{
		Debug.WriteLine("cloned pcm stream");
		return new PcmAudioStream(this.channel,this.cache.Capacity);
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
	/// <summary>
	/// Copy a single data item from the channel into the buffer.
	/// If the buffer is not large enough, the remaining data is stored in the cache.
	/// </summary>
	/// <param name="buffer"></param>
	/// <param name="count"></param>
	/// <param name="offset"></param>
	/// <returns></returns>
	async Task<uint> CopyFromChannelIntoBuffer(IBuffer buffer, uint count, uint offset)
	{		
		var data = await channel.Reader.ReadAsync();
		uint dataSz = (uint)data.Length;
		uint cpSz = Math.Min(dataSz, count);
		int rem = (int)dataSz - (int)cpSz;
		data.CopyTo(0,buffer,offset, (int)cpSz);
		if (rem > 0)
		{
			data.CopyTo((int)cpSz, cache, 0u, rem);
			cache.Length = (uint)rem;
			cacheBytesWritten = 0;
		}
		return cpSz;
	}

	/// <summary>
	/// Copy the remaining data in the cache to the buffer.
	/// When all data is copied, the cache is cleared.
	/// </summary>
	/// <param name="buffer"></param>
	/// <param name="count"></param>
	/// <returns></returns>
	uint CopyCacheToBuffer(IBuffer buffer, uint count)
	{
		uint cpSz = Math.Min(cache.Length - cacheBytesWritten, count);
		cache.CopyTo(cacheBytesWritten, buffer, 0u, cpSz);
		cacheBytesWritten += cpSz;
		if (cacheBytesWritten == cache.Length)
		{
			cacheBytesWritten = 0;
			cache.Length = 0;
		}
		return cpSz;	
	}

	IAsyncOperationWithProgress<IBuffer, uint> IInputStream.ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
	{
		return AsyncInfo.Run<IBuffer, uint>(async (cancellationToken, progress) =>
		{
			if (cancellationToken.IsCancellationRequested)
			{
				Debug.WriteLine($"pcm read cancelled");
				return buffer;
			}
			uint bytesWritten = 0;
			if (cache.Length > 0)
			{
				bytesWritten += CopyCacheToBuffer(buffer, count);
				Debug.WriteLine($"pcm read cache {bytesWritten}");
			}
			while (bytesWritten < count)
			{
				bytesWritten += await CopyFromChannelIntoBuffer(buffer, count - bytesWritten, bytesWritten);
				Debug.WriteLine($"pcm read channel {bytesWritten} count: {count} cap:{buffer.Capacity}");
			}
			progress.Report(bytesWritten);
			Debug.WriteLine($"pcm read done {bytesWritten} count: {count} cap:{buffer.Capacity}");
			return buffer;
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

