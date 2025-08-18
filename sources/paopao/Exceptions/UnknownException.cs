using System;

public class UnknownException : Exception
{
	public UnknownException()
	{
	}

	public UnknownException(string msg)
		: base(msg)
	{
	}

	public UnknownException(Exception e)
		: base("UnknownException", e)
	{
	}
}
