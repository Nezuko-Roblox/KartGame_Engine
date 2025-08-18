using System;
using System.Collections;
using System.IO;
using System.Text;

public class XMLElement
{
	public XMLElement()
		: this(new Hashtable(), false, true, true)
	{
	}

	public XMLElement(Hashtable entities)
		: this(entities, false, true, true)
	{
	}

	public XMLElement(bool skipLeadingWhitespace)
		: this(new Hashtable(), skipLeadingWhitespace, true, true)
	{
	}

	public XMLElement(Hashtable entities, bool skipLeadingWhitespace)
		: this(entities, skipLeadingWhitespace, true, true)
	{
	}

	public XMLElement(Hashtable entities, bool skipLeadingWhitespace, bool ignoreCase)
		: this(entities, skipLeadingWhitespace, true, ignoreCase)
	{
	}

	protected XMLElement(Hashtable entities, bool skipLeadingWhitespace, bool fillBasicConversionTable, bool ignoreCase)
	{
		this.ignoreWhitespace = skipLeadingWhitespace;
		this.ignoreCase = ignoreCase;
		this.name = null;
		this.contents = string.Empty;
		this.attributes = new Hashtable();
		this.children = new ArrayList();
		this.entities = entities;
		this.lineNr = 0;
		foreach (object obj in entities)
		{
			DictionaryEntry dictionaryEntry = (DictionaryEntry)obj;
			if (dictionaryEntry.Value.GetType() == typeof(string))
			{
				char[] array = ((string)dictionaryEntry.Value).ToCharArray();
				this.entities.Add(dictionaryEntry.Key, array);
			}
		}
		if (fillBasicConversionTable)
		{
			this.entities.Add("amp", new char[] { '&' });
			this.entities.Add("quot", new char[] { '"' });
			this.entities.Add("apos", new char[] { '\'' });
			this.entities.Add("lt", new char[] { '<' });
			this.entities.Add("gt", new char[] { '>' });
		}
	}

	public void addChild(XMLElement child)
	{
		this.children.Add(child);
	}

	public void setAttribute(string name, object value)
	{
		if (this.ignoreCase)
		{
			name = name.ToUpper();
		}
		this.attributes.Add(name, value.ToString());
	}

	public void addProperty(string name, object value)
	{
		this.setAttribute(name, value);
	}

	public void setIntAttribute(string name, int value)
	{
		if (this.ignoreCase)
		{
			name = name.ToUpper();
		}
		this.attributes.Add(name, value.ToString());
	}

	public void addProperty(string key, int value)
	{
		this.setIntAttribute(key, value);
	}

	public void setDoubleAttribute(string name, double value)
	{
		if (this.ignoreCase)
		{
			name = name.ToUpper();
		}
		this.attributes.Add(name, value.ToString());
	}

	public void addProperty(string name, double value)
	{
		this.setDoubleAttribute(name, value);
	}

	public int countChildren()
	{
		return this.children.Count;
	}

	public IEnumerator enumerateAttributeNames()
	{
		return this.attributes.GetEnumerator();
	}

	public IEnumerator enumeratePropertyNames()
	{
		return this.enumerateAttributeNames();
	}

	public IEnumerator enumerateChildren()
	{
		return this.children.GetEnumerator();
	}

	public ArrayList getChildren()
	{
		return (ArrayList)this.children.Clone();
	}

	public string getContents()
	{
		return this.getContent();
	}

	public string getContent()
	{
		return this.contents;
	}

	public int getLineNr()
	{
		return this.lineNr;
	}

	public object getAttribute(string name)
	{
		return this.getAttribute(name, null);
	}

	public object getAttribute(string name, object defaultValue)
	{
		if (this.ignoreCase)
		{
			name = name.ToUpper();
		}
		object obj = this.attributes[name];
		if (obj == null)
		{
			obj = defaultValue;
		}
		return obj;
	}

	public object getAttribute(string name, Hashtable valueSet, string defaultKey, bool allowLiterals)
	{
		if (this.ignoreCase)
		{
			name = name.ToUpper();
		}
		object obj = this.attributes[name];
		if (obj == null)
		{
			obj = defaultKey;
		}
		object obj2 = valueSet[obj];
		if (obj2 == null)
		{
			if (!allowLiterals)
			{
				throw this.invalidValue(name, (string)obj);
			}
			obj2 = obj;
		}
		return obj2;
	}

	public string getStringAttribute(string name)
	{
		return this.getStringAttribute(name, null);
	}

	public string getStringAttribute(string name, string defaultValue)
	{
		return (string)this.getAttribute(name, defaultValue);
	}

	public string getStringAttribute(string name, Hashtable valueSet, string defaultKey, bool allowLiterals)
	{
		return (string)this.getAttribute(name, valueSet, defaultKey, allowLiterals);
	}

	public int getIntAttribute(string name)
	{
		return this.getIntAttribute(name, 0);
	}

	public int getIntAttribute(string name, int defaultValue)
	{
		if (this.ignoreCase)
		{
			name = name.ToUpper();
		}
		string text = (string)this.attributes[name];
		if (text == null)
		{
			return defaultValue;
		}
		int num;
		try
		{
			num = int.Parse(text);
		}
		catch (FormatException ex)
		{
			throw this.invalidValue(ex.ToString() + ": " + name, text);
		}
		return num;
	}

	public int getIntAttribute(string name, Hashtable valueSet, string defaultKey, bool allowLiteralNumbers)
	{
		if (this.ignoreCase)
		{
			name = name.ToUpper();
		}
		object obj = this.attributes[name];
		if (obj == null)
		{
			obj = defaultKey;
		}
		int num;
		try
		{
			num = (int)valueSet[obj];
		}
		catch (InvalidCastException ex)
		{
			throw this.invalidValueSet(ex.ToString() + ": " + name);
		}
		return num;
	}

	public double getDoubleAttribute(string name)
	{
		return this.getDoubleAttribute(name, 0.0);
	}

	public double getDoubleAttribute(string name, double defaultValue)
	{
		if (this.ignoreCase)
		{
			name = name.ToUpper();
		}
		string text = (string)this.attributes[name];
		if (text == null)
		{
			return defaultValue;
		}
		double num;
		try
		{
			num = double.Parse(text);
		}
		catch (FormatException ex)
		{
			throw this.invalidValue(ex.ToString() + ": " + name, text);
		}
		return num;
	}

	public double getDoubleAttribute(string name, Hashtable valueSet, string defaultKey, bool allowLiteralNumbers)
	{
		if (this.ignoreCase)
		{
			name = name.ToUpper();
		}
		object obj = this.attributes[name];
		if (obj == null)
		{
			obj = defaultKey;
		}
		double num;
		try
		{
			num = (double)valueSet[obj];
		}
		catch (InvalidCastException ex)
		{
			throw this.invalidValueSet(ex.ToString() + ": " + name);
		}
		return num;
	}

	public bool getBooleanAttribute(string name, string trueValue, string falseValue, bool defaultValue)
	{
		if (this.ignoreCase)
		{
			name = name.ToUpper();
		}
		object obj = this.attributes[name];
		if (obj == null)
		{
			return defaultValue;
		}
		if (obj.ToString() == trueValue)
		{
			return true;
		}
		if (obj.ToString() == falseValue)
		{
			return false;
		}
		throw this.invalidValue(name, (string)obj);
	}

	public int getIntProperty(string name, Hashtable valueSet, string defaultKey)
	{
		return this.getIntAttribute(name, valueSet, defaultKey, false);
	}

	public string getProperty(string name)
	{
		return this.getStringAttribute(name);
	}

	public string getProperty(string name, string defaultValue)
	{
		return this.getStringAttribute(name, defaultValue);
	}

	public int getProperty(string name, int defaultValue)
	{
		return this.getIntAttribute(name, defaultValue);
	}

	public double getProperty(string name, double defaultValue)
	{
		return this.getDoubleAttribute(name, defaultValue);
	}

	public bool getProperty(string key, string trueValue, string falseValue, bool defaultValue)
	{
		return this.getBooleanAttribute(key, trueValue, falseValue, defaultValue);
	}

	public object getProperty(string name, Hashtable valueSet, string defaultKey)
	{
		return this.getAttribute(name, valueSet, defaultKey, false);
	}

	public string getStringProperty(string name, Hashtable valueSet, string defaultKey)
	{
		return this.getStringAttribute(name, valueSet, defaultKey, false);
	}

	public int getSpecialIntProperty(string name, Hashtable valueSet, string defaultKey)
	{
		return this.getIntAttribute(name, valueSet, defaultKey, true);
	}

	public double getSpecialDoubleProperty(string name, Hashtable valueSet, string defaultKey)
	{
		return this.getDoubleAttribute(name, valueSet, defaultKey, true);
	}

	public string getName()
	{
		return this.name;
	}

	public string getTagName()
	{
		return this.getName();
	}

	public void parseFromReader(TextReader reader)
	{
		this.parseFromReader(reader, 1);
	}

	public void parseFromReader(TextReader reader, int startingLineNr)
	{
		this.name = null;
		this.contents = string.Empty;
		this.attributes = new Hashtable();
		this.children = new ArrayList();
		this.charReadTooMuch = '\0';
		this.reader = reader;
		this.parserLineNr = startingLineNr;
		char c;
		for (;;)
		{
			c = this.scanWhitespace();
			if (c != '<')
			{
				break;
			}
			c = this.readChar();
			if (c != '!' && c != '?')
			{
				goto IL_0080;
			}
			this.skipSpecialTag(0);
		}
		throw this.expectedInput("<");
		IL_0080:
		this.unreadChar(c);
		this.scanElement(this);
	}

	public void parseString(string str)
	{
		this.parseFromReader(new StringReader(str), 1);
	}

	public void parseString(string str, int offset)
	{
		this.parseString(str.Substring(offset));
	}

	public void parseString(string str, int offset, int end)
	{
		this.parseString(str.Substring(offset, end));
	}

	public void parseString(string str, int offset, int end, int startingLineNr)
	{
		str = str.Substring(offset, end);
		this.parseFromReader(new StringReader(str), startingLineNr);
	}

	public void parseCharArray(char[] input, int offset, int end)
	{
		this.parseCharArray(input, offset, end, 1);
	}

	public void parseCharArray(char[] input, int offset, int end, int startingLineNr)
	{
		TextReader textReader = new StringReader(new string(input, offset, end));
		this.parseFromReader(textReader, startingLineNr);
	}

	public void removeChild(XMLElement child)
	{
		this.children.Remove(child);
	}

	public void removeAttribute(string name)
	{
		if (this.ignoreCase)
		{
			name = name.ToUpper();
		}
		this.attributes.Remove(name);
	}

	public void removeProperty(string name)
	{
		this.removeAttribute(name);
	}

	public void removeChild(string name)
	{
		this.removeAttribute(name);
	}

	protected XMLElement createAnotherElement()
	{
		return new XMLElement(this.entities, this.ignoreWhitespace, false, this.ignoreCase);
	}

	public void setContent(string content)
	{
		this.contents = content;
	}

	public void setTagName(string name)
	{
		this.setName(name);
	}

	public void setName(string name)
	{
		this.name = name;
	}

	public override string ToString()
	{
		string text;
		try
		{
			MemoryStream memoryStream = new MemoryStream();
			StreamWriter streamWriter = new StreamWriter(memoryStream);
			this.write(streamWriter);
			streamWriter.Flush();
			byte[] array = memoryStream.ToArray();
			char[] array2 = new char[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = Convert.ToChar(array[i]);
			}
			text = new string(array2);
		}
		catch (IOException ex)
		{
			text = base.ToString() + ":- " + ex.ToString();
		}
		return text;
	}

	public void write(TextWriter writer)
	{
		if (this.name == null)
		{
			this.writeEncoded(writer, this.contents);
			return;
		}
		writer.Write('<');
		writer.Write(this.name);
		if (this.attributes.Count > 0)
		{
			foreach (object obj in this.attributes)
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)obj;
				writer.Write(' ');
				string text = (string)dictionaryEntry.Key;
				string text2 = (string)dictionaryEntry.Value;
				writer.Write(text);
				writer.Write('=');
				writer.Write('"');
				this.writeEncoded(writer, text2);
				writer.Write('"');
			}
		}
		if (this.contents != null && this.contents.Length > 0)
		{
			writer.Write('>');
			this.writeEncoded(writer, this.contents);
			writer.Write('<');
			writer.Write('/');
			writer.Write(this.name);
			writer.Write('>');
		}
		else if (this.children.Count == 0)
		{
			writer.Write('/');
			writer.Write('>');
		}
		else
		{
			writer.Write('>');
			IEnumerator enumerator2 = this.enumerateChildren();
			while (enumerator2.MoveNext())
			{
				object obj2 = enumerator2.Current;
				XMLElement xmlelement = (XMLElement)obj2;
				xmlelement.write(writer);
			}
			writer.Write('<');
			writer.Write('/');
			writer.Write(this.name);
			writer.Write('>');
		}
	}

	protected void writeEncoded(TextWriter writer, string str)
	{
		foreach (char c in str)
		{
			char c2 = c;
			switch (c2)
			{
			case '"':
				writer.Write('&');
				writer.Write('q');
				writer.Write('u');
				writer.Write('o');
				writer.Write('t');
				writer.Write(';');
				break;
			default:
			{
				switch (c2)
				{
				case '<':
					writer.Write('&');
					writer.Write('l');
					writer.Write('t');
					writer.Write(';');
					goto IL_017E;
				case '>':
					writer.Write('&');
					writer.Write('g');
					writer.Write('t');
					writer.Write(';');
					goto IL_017E;
				}
				int num = (int)c;
				if (num < 32 || num > 126)
				{
					writer.Write('&');
					writer.Write('#');
					writer.Write('x');
					writer.Write(Convert.ToString(num, 16));
					writer.Write(';');
				}
				else
				{
					writer.Write(c);
				}
				break;
			}
			case '&':
				writer.Write('&');
				writer.Write('a');
				writer.Write('m');
				writer.Write('p');
				writer.Write(';');
				break;
			case '\'':
				writer.Write('&');
				writer.Write('a');
				writer.Write('p');
				writer.Write('o');
				writer.Write('s');
				writer.Write(';');
				break;
			}
			IL_017E:;
		}
	}

	protected void scanIdentifier(StringBuilder result)
	{
		char c;
		for (;;)
		{
			c = this.readChar();
			if ((c < 'A' || c > 'Z') && (c < 'a' || c > 'z') && (c < '0' || c > '9') && c != '_' && c != '.' && c != ':' && c != '-' && c <= '~')
			{
				break;
			}
			result.Append(c);
		}
		this.unreadChar(c);
	}

	protected char scanWhitespace()
	{
		char c;
		for (;;)
		{
			c = this.readChar();
			char c2 = c;
			switch (c2)
			{
			case '\t':
			case '\n':
			case '\r':
				break;
			default:
				if (c2 != ' ')
				{
					return c;
				}
				break;
			}
		}
		return c;
	}

	protected char scanWhitespace(StringBuilder result)
	{
		char c;
		for (;;)
		{
			c = this.readChar();
			char c2 = c;
			switch (c2)
			{
			case '\t':
			case '\n':
				break;
			default:
				if (c2 != ' ')
				{
					return c;
				}
				break;
			case '\r':
				continue;
			}
			result.Append(c);
		}
		return c;
	}

	protected void scanString(StringBuilder str)
	{
		char c = this.readChar();
		if (c != '\'' && c != '"')
		{
			throw this.expectedInput("' or \"");
		}
		for (;;)
		{
			char c2 = this.readChar();
			if (c2 == c)
			{
				break;
			}
			if (c2 == '&')
			{
				this.resolveEntity(str);
			}
			else
			{
				str.Append(c2);
			}
		}
	}

	protected void scanPCData(StringBuilder data)
	{
		char c;
		for (;;)
		{
			c = this.readChar();
			if (c == '<')
			{
				c = this.readChar();
				if (c != '!')
				{
					break;
				}
				this.checkCDATA(data);
			}
			else if (c == '&')
			{
				this.resolveEntity(data);
			}
			else
			{
				data.Append(c);
			}
		}
		this.unreadChar(c);
	}

	protected bool checkCDATA(StringBuilder buf)
	{
		char c = this.readChar();
		if (c != '[')
		{
			this.unreadChar(c);
			this.skipSpecialTag(0);
			return false;
		}
		if (!this.checkLiteral("CDATA["))
		{
			this.skipSpecialTag(1);
			return false;
		}
		int i = 0;
		while (i < 3)
		{
			c = this.readChar();
			char c2 = c;
			if (c2 != '>')
			{
				if (c2 != ']')
				{
					for (int j = 0; j < i; j++)
					{
						buf.Append(']');
					}
					buf.Append(c);
					i = 0;
				}
				else if (i < 2)
				{
					i++;
				}
				else
				{
					buf.Append(']');
					buf.Append(']');
					i = 0;
				}
			}
			else if (i < 2)
			{
				for (int k = 0; k < i; k++)
				{
					buf.Append(']');
				}
				i = 0;
				buf.Append('>');
			}
			else
			{
				i = 3;
			}
		}
		return true;
	}

	protected void skipComment()
	{
		int i = 2;
		while (i > 0)
		{
			char c = this.readChar();
			if (c == '-')
			{
				i--;
			}
			else
			{
				i = 2;
			}
		}
		if (this.readChar() != '>')
		{
			throw this.expectedInput(">");
		}
	}

	protected void skipSpecialTag(int bracketLevel)
	{
		int i = 1;
		char c = '\0';
		if (bracketLevel == 0)
		{
			char c2 = this.readChar();
			if (c2 == '[')
			{
				bracketLevel++;
			}
			else if (c2 == '-')
			{
				c2 = this.readChar();
				if (c2 == '[')
				{
					bracketLevel++;
				}
				else if (c2 == ']')
				{
					bracketLevel--;
				}
				else if (c2 == '-')
				{
					this.skipComment();
					return;
				}
			}
		}
		while (i > 0)
		{
			char c3 = this.readChar();
			if (c == '\0')
			{
				if (c3 == '"' || c3 == '\'')
				{
					c = c3;
				}
				else if (bracketLevel <= 0)
				{
					if (c3 == '<')
					{
						i++;
					}
					else if (c3 == '>')
					{
						i--;
					}
				}
				if (c3 == '[')
				{
					bracketLevel++;
				}
				else if (c3 == ']')
				{
					bracketLevel--;
				}
			}
			else if (c3 == c)
			{
				c = '\0';
			}
		}
	}

	protected bool checkLiteral(string literal)
	{
		int length = literal.Length;
		for (int i = 0; i < length; i++)
		{
			if (this.readChar() != literal[i])
			{
				return false;
			}
		}
		return true;
	}

	protected char readChar()
	{
		if (this.charReadTooMuch != '\0')
		{
			char c = this.charReadTooMuch;
			this.charReadTooMuch = '\0';
			return c;
		}
		int num = this.reader.Read();
		if (num < 0)
		{
			throw this.unexpectedEndOfData();
		}
		if (num == 10)
		{
			this.parserLineNr++;
			return '\n';
		}
		return (char)num;
	}

	protected void scanElement(XMLElement elt)
	{
		StringBuilder stringBuilder = new StringBuilder();
		this.scanIdentifier(stringBuilder);
		string text = stringBuilder.ToString();
		elt.setName(text);
		char c = this.scanWhitespace();
		while (c != '>' && c != '/')
		{
			stringBuilder.Length = 0;
			this.unreadChar(c);
			this.scanIdentifier(stringBuilder);
			string text2 = stringBuilder.ToString();
			c = this.scanWhitespace();
			if (c != '=')
			{
				throw this.expectedInput("=");
			}
			this.unreadChar(this.scanWhitespace());
			stringBuilder.Length = 0;
			this.scanString(stringBuilder);
			elt.setAttribute(text2, stringBuilder);
			c = this.scanWhitespace();
		}
		if (c == '/')
		{
			c = this.readChar();
			if (c != '>')
			{
				throw this.expectedInput(">");
			}
			return;
		}
		else
		{
			stringBuilder.Length = 0;
			c = this.scanWhitespace(stringBuilder);
			if (c != '<')
			{
				this.unreadChar(c);
				this.scanPCData(stringBuilder);
			}
			else
			{
				for (;;)
				{
					c = this.readChar();
					if (c != '!')
					{
						goto IL_0139;
					}
					if (this.checkCDATA(stringBuilder))
					{
						break;
					}
					c = this.scanWhitespace(stringBuilder);
					if (c != '<')
					{
						goto Block_8;
					}
				}
				this.scanPCData(stringBuilder);
				goto IL_016C;
				Block_8:
				this.unreadChar(c);
				this.scanPCData(stringBuilder);
				goto IL_016C;
				IL_0139:
				if (c != '/' || this.ignoreWhitespace)
				{
					stringBuilder.Length = 0;
				}
				if (c == '/')
				{
					this.unreadChar(c);
				}
			}
			IL_016C:
			if (stringBuilder.Length == 0)
			{
				while (c != '/')
				{
					if (c == '!')
					{
						c = this.readChar();
						if (c != '-')
						{
							throw this.expectedInput("Comment or Element");
						}
						c = this.readChar();
						if (c != '-')
						{
							throw this.expectedInput("Comment or Element");
						}
						this.skipComment();
					}
					else
					{
						this.unreadChar(c);
						XMLElement xmlelement = this.createAnotherElement();
						this.scanElement(xmlelement);
						elt.addChild(xmlelement);
					}
					c = this.scanWhitespace();
					if (c != '<')
					{
						throw this.expectedInput("<");
					}
					c = this.readChar();
				}
				this.unreadChar(c);
			}
			else if (this.ignoreWhitespace)
			{
				elt.setContent(stringBuilder.ToString().Trim());
			}
			else
			{
				elt.setContent(stringBuilder.ToString());
			}
			c = this.readChar();
			if (c != '/')
			{
				throw this.expectedInput("/");
			}
			this.unreadChar(this.scanWhitespace());
			if (!this.checkLiteral(text))
			{
				throw this.expectedInput(text);
			}
			if (this.scanWhitespace() != '>')
			{
				throw this.expectedInput(">");
			}
			return;
		}
	}

	protected void resolveEntity(StringBuilder buf)
	{
		char c = '\0';
		StringBuilder stringBuilder = new StringBuilder();
		for (;;)
		{
			c = this.readChar();
			if (c == ';')
			{
				break;
			}
			stringBuilder.Append(c);
		}
		string text = stringBuilder.ToString();
		if (text[0] == '#')
		{
			try
			{
				if (text[1] == 'x')
				{
					c = (char)Convert.ToInt32(text.Substring(2), 16);
				}
				else
				{
					c = (char)Convert.ToInt32(text.Substring(1), 10);
				}
			}
			catch (FormatException ex)
			{
				throw this.unknownEntity(ex.ToString() + ": " + text);
			}
			buf.Append(c);
		}
		else
		{
			char[] array = (char[])this.entities[text];
			if (array == null)
			{
				throw this.unknownEntity(text);
			}
			buf.Append(array);
		}
	}

	protected void unreadChar(char ch)
	{
		this.charReadTooMuch = ch;
	}

	protected XMLParseException invalidValueSet(string name)
	{
		string text = "Invalid value set (entity name = \"" + name + "\")";
		return new XMLParseException(this.getName(), this.parserLineNr, text);
	}

	protected XMLParseException invalidValue(string name, string value)
	{
		string text = string.Concat(new string[] { "Attribute \"", name, "\" does not contain a valid value (\"", value, "\")" });
		return new XMLParseException(this.getName(), this.parserLineNr, text);
	}

	protected XMLParseException unexpectedEndOfData()
	{
		string text = "Unexpected end of data reached";
		return new XMLParseException(this.getName(), this.parserLineNr, text);
	}

	protected XMLParseException syntaxError(string context)
	{
		string text = "Syntax error while parsing " + context;
		return new XMLParseException(this.getName(), this.parserLineNr, text);
	}

	protected XMLParseException expectedInput(string charSet)
	{
		string text = "Expected: " + charSet;
		return new XMLParseException(this.getName(), this.parserLineNr, text);
	}

	protected XMLParseException unknownEntity(string name)
	{
		string text = "Unknown or invalid entity: &" + name + ";";
		return new XMLParseException(this.getName(), this.parserLineNr, text);
	}

	public string ToDebugString()
	{
		string text = string.Empty;
		text = text + this.name + "\n";
		foreach (object obj in this.attributes)
		{
			DictionaryEntry dictionaryEntry = (DictionaryEntry)obj;
			string text2 = text;
			text = string.Concat(new string[]
			{
				text2,
				dictionaryEntry.Key.ToString(),
				" ",
				dictionaryEntry.Value.ToString(),
				"\n"
			});
		}
		return text;
	}

	public static readonly int NANOXML_MAJOR_VERSION = 2;

	public static readonly int NANOXML_MINOR_VERSION = 2;

	private Hashtable attributes;

	private ArrayList children;

	private string name;

	private string contents;

	private Hashtable entities;

	private int lineNr;

	private bool ignoreCase;

	private bool ignoreWhitespace;

	private char charReadTooMuch;

	private TextReader reader;

	private int parserLineNr;
}
