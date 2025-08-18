using System;

public class ServerException : Exception
{
	public ServerException()
	{
	}

	public ServerException(string msg)
		: base(msg)
	{
	}

	public ServerException(Exception e)
		: base("ServerException", e)
	{
	}
}
