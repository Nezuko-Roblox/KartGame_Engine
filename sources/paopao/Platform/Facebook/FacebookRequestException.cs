using System;

public class FacebookRequestException : RequestException
{
	public FacebookRequestException()
	{
	}

	public FacebookRequestException(string msg)
		: base(msg)
	{
	}

	public FacebookRequestException(Exception e)
		: base(e)
	{
	}
}
