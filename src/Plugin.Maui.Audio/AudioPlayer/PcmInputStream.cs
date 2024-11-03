using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Maui.Audio;

public class PcmInputStream : System.IO.Stream
{
	System.Threading.Channels.Channel<byte[]> channel;
	Memory<byte> cache = Memory<byte>.Empty;
	bool closed = false;

	public PcmInputStream(System.Threading.Channels.Channel<byte[]> channel)
	{
		this.channel = channel;
	}

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length => -1;

	public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

	public override void Flush()
	{
		throw new NotImplementedException();
	}

	int CopyToBuffer(byte[] data, byte[] buffer, int offset, int count)
	{
		var sz = Math.Min(data.Length, count);
		var fromSpan = data.AsSpan<byte>();
		var toSpan = buffer.AsSpan<byte>(offset, sz);
		fromSpan.Slice(0, sz).CopyTo(toSpan);
		if (data.Length == count)
		{
			cache = Memory<byte>.Empty;
			return 0;
		}
		else if (data.Length > count)
		{
			cache = Memory<byte>.Empty;
			return data.Length - count;
		}
		else
		{
			cache = data.AsMemory<byte>(sz);
			return count - data.Length;
		}
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

	async Task<int> ReadFromChannel(byte[] buffer, int offset, int count)
	{
		var (data,haveData) = await ReadNext();
		if (haveData)
		{
			return CopyToBuffer(data, buffer, offset, count);
		}
		else
		{
			closed = true;
			return 0;
		}
	}

	int CopyFromCache(byte[] buffer, int offset, int count)
	{
		if (cache.Length == 0)
		{
			throw new InvalidOperationException("Cache is empty");
		}
		else
		{
			var sz = Math.Min(cache.Length, count);
			var toSpan = buffer.AsSpan<byte>(offset, sz);
			cache.Span.Slice(0, sz).CopyTo(toSpan);
			cache = cache.Slice(sz);
			return sz;
		}
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (cache.Length > 0)
		{
			return CopyFromCache(buffer, offset, count);
		}
		else if (closed)
		{
			return 0;
		}
		else
		{
			return ReadFromChannel(buffer, offset, count).Result;
		}
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