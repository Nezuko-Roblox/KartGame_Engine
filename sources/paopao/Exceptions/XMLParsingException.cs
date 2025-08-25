using System;

public class XMLParsingException : Exception
{
	public XMLParsingException()
	{
	}

	public XMLParsingException(string msg)
		: base(msg)
	{
	}

	public XMLParsingException(Exception e)
		: base("XMLParsingException", e)
	{
	}
}
