using System;

internal interface IXMLizable
{
	XMLElement ToXML();

	void FromXML(XMLElement xml);
}
