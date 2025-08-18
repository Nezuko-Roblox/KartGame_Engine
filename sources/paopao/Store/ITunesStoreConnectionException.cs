using System;

public class ITunesStoreConnectionException : Exception
{
	public ITunesStoreConnectionException()
	{
	}

	public ITunesStoreConnectionException(string msg)
		: base(msg)
	{
	}

	public ITunesStoreConnectionException(Exception e)
		: base("ITunesStoreConnectionException", e)
	{
	}
}
