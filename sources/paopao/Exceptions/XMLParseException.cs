using System;

public class XMLParseException : SystemException
{
	public XMLParseException(string name, string message)
		: base("XML Parse Exception during parsing of " + ((name != null) ? ("a " + name + " element") : "the XML definition") + ": " + message)
	{
		this.lineNr = XMLParseException.NO_LINE;
	}

	public XMLParseException(string name, int lineNr, string message)
		: base(string.Concat(new object[]
		{
			"XML Parse Exception during parsing of ",
			(name != null) ? ("a " + name + " element") : "the XML definition",
			" at line ",
			lineNr,
			": ",
			message
		}))
	{
		this.lineNr = lineNr;
	}

	public int getLineNr()
	{
		return this.lineNr;
	}

	public static readonly int NO_LINE = -1;

	private int lineNr;
}
