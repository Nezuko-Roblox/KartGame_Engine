using System;

public class InvalidRecordException : Exception
{
	public InvalidRecordException()
	{
	}

	public InvalidRecordException(string msg)
		: base(msg)
	{
	}

	public InvalidRecordException(Exception e)
		: base("InvalidRecordException", e)
	{
	}
}
