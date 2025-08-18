using System;
using UnityEngine;

public class XMLResponse : Response
{
	public XMLResponse(string resp)
	{
		this.xml_ = new XMLElement();
		try
		{
			this.xml_.parseString(resp);
		}
		catch (Exception ex)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log(ex.ToString());
			}
			base.Error = new XMLParsingException(ex);
		}
		if (this.xml_ != null)
		{
			if (!(this.xml_.getName() == "error"))
			{
				if (!(this.xml_.getName() == "error_response"))
				{
					return;
				}
			}
			try
			{
				if (this.xml_.getName() == "error")
				{
					base.Error = this.ParseFIAError(this.xml_);
				}
				else if (this.xml_.getName() == "error_response")
				{
					base.Error = this.ParseFacebookError(this.xml_);
				}
			}
			catch (Exception ex2)
			{
				base.Error = new UnknownException(ex2);
			}
			if (base.Error == null)
			{
				base.Error = new UnknownException();
			}
		}
	}

	private Exception ParseFIAError(XMLElement xml)
	{
		string text = (string)xml.getAttribute("message");
		object[] array = new object[] { text };
		Type type = ExceptionFactory.FiaExceptionFromTypeName((string)xml.getAttribute("type"));
		if (type != null)
		{
			return (Exception)Activator.CreateInstance(type, array);
		}
		return new UnknownException(text);
	}

	private Exception ParseFacebookError(XMLElement xml)
	{
		foreach (object obj in xml.getChildren())
		{
			XMLElement xmlelement = (XMLElement)obj;
			if (xmlelement.getName() == "error_code")
			{
				int num = int.Parse(xmlelement.getContent());
				if (num == 190)
				{
					return new FacebookAuthException("Invalid OAuth 2.0 Access Token");
				}
				if (num == 601)
				{
					return new FacebookFQLException("Invalid query.");
				}
			}
		}
		return new UnknownException();
	}

	public XMLElement XML
	{
		get
		{
			return this.xml_;
		}
	}

	private XMLElement xml_;
}
