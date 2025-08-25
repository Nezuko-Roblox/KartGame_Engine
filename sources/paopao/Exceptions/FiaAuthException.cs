using System;

public class FiaAuthException : Exception
{
	public FiaAuthException()
	{
	}

	public FiaAuthException(string msg)
		: base(msg)
	{
	}

	public FiaAuthException(Exception e)
		: base("FiaAuthException", e)
	{
	}
}
