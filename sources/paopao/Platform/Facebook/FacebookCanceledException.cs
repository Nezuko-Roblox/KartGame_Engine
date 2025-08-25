using System;

public class FacebookCanceledException : Exception
{
	public FacebookCanceledException()
	{
	}

	public FacebookCanceledException(string msg)
		: base(msg)
	{
	}

	public FacebookCanceledException(Exception e)
		: base("FacebookCanceledException", e)
	{
	}
}
