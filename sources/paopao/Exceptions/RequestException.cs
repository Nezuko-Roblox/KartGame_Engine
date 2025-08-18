using System;

public class RequestException : Exception
{
	public RequestException()
	{
	}

	public RequestException(string msg)
		: base(msg)
	{
	}

	public RequestException(Exception e)
		: base("RequestException", e)
	{
	}
}
