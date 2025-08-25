using System;

public class FacebookAuthException : Exception
{
	public FacebookAuthException()
	{
	}

	public FacebookAuthException(string msg)
		: base(msg)
	{
	}

	public FacebookAuthException(Exception e)
		: base("FacebookAuthException", e)
	{
	}
}
