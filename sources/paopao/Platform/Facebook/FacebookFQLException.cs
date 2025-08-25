using System;

public class FacebookFQLException : Exception
{
	public FacebookFQLException()
	{
	}

	public FacebookFQLException(string msg)
		: base(msg)
	{
	}

	public FacebookFQLException(Exception e)
		: base("FacebookFQLException", e)
	{
	}
}
