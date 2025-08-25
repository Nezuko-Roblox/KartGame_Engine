using System;

public class ByteArrayResponse : Response
{
	public ByteArrayResponse(byte[] bytes)
	{
		this.bytes_ = bytes;
	}

	public byte[] Bytes
	{
		get
		{
			return this.bytes_;
		}
	}

	private byte[] bytes_;
}
