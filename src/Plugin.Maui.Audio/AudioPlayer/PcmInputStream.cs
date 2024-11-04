using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Maui.Audio;

public class PcmInputStream : System.IO.Stream
{
	int threshold = 4096 * 2 * 2 * 2 * 2;
	System.Threading.Channels.Channel<byte[]> channel;
	Memory<byte> cache = Memory<byte>.Empty;
	bool closed = false;

	public PcmInputStream(System.Threading.Channels.Channel<byte[]> channel)
	{
		this.channel = channel;
	}

	public override bool CanRead => true;

	public override bool CanSeek
	{
		get {
			//Debug.WriteLine("pcm: CanSeek");
			return false;
		}
	}

	public override bool CanWrite => false;

	public override long Length
	{
		get
		{
			//Debug.WriteLine("pcm: Length");
			return  -1;
		}
	}			

	public override long Position { get { Debug.WriteLine("pcm: Position"); throw new NotImplementedException(); } set => throw new NotImplementedException(); }

	public override void Flush()
	{
		throw new NotImplementedException();
	}

	int CopyToBuffer(Memory<byte> data, byte[] buffer, int offset, int count)
	{
		var sz = Math.Min(data.Length, count);
		var toSpan = buffer.AsSpan<byte>(offset, sz);
		data.Span.CopyTo(toSpan);
		cache = data.Slice(sz);
		return sz;
	}

	async Task<Tuple<byte[],bool>> ReadNext()
	{
		bool haveData = await channel.Reader.WaitToReadAsync();
		if (!haveData)
		{
			return Tuple.Create(Array.Empty<byte>(),false);
		}
		else
		{
			channel.Reader.TryRead(out var data);
			return Tuple.Create(data!, true);
		}
	}

	int totalBytes = 0;
	async Task<int> WriteToBuffer(byte[] buffer, int offset, int count)
	{
		int bytesWritten = 0;

		//empty cache first
		while (cache.Length > 0 && bytesWritten < count && bytesWritten < threshold)
		{
			bytesWritten = CopyToBuffer(cache, buffer, offset + bytesWritten, count - bytesWritten);
		}

		byte[] data = Array.Empty<byte>();
		bool haveData = true;

		while (haveData && bytesWritten < count && bytesWritten < threshold )
		{
			(data, haveData) = await ReadNext();
			if (haveData)
			{
				bytesWritten += CopyToBuffer(data, buffer, offset + bytesWritten, count - bytesWritten);
			}
		}
		totalBytes += bytesWritten;
		Debug.WriteLine($"pcm: written {bytesWritten} bytes; total : {totalBytes}");
		return bytesWritten;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return WriteToBuffer(buffer, offset, count).Result;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotImplementedException();
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotImplementedException();
	}

}