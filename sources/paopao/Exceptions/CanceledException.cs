using System;

public class CanceledException : Exception
{
	public CanceledException()
	{
	}

	public CanceledException(string msg)
		: base(msg)
	{
	}

	public CanceledException(Exception e)
		: base("CanceledException", e)
	{
	}
}
