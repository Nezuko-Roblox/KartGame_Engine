using System;
using System.Collections;

public class JSONObject
{
	public JSONObject(JSONObject.Type t)
	{
		this.type = t;
		if (t != JSONObject.Type.OBJECT)
		{
			if (t == JSONObject.Type.ARRAY)
			{
				this.list = new ArrayList();
			}
		}
		else
		{
			this.list = new ArrayList();
			this.keys = new ArrayList();
		}
	}

	public JSONObject(bool b)
	{
		this.type = JSONObject.Type.BOOL;
		this.b = b;
	}

	public JSONObject(float f)
	{
		this.type = JSONObject.Type.NUMBER;
		this.n = (double)f;
	}

	public JSONObject()
	{
		this.type = JSONObject.Type.NULL;
	}

	public JSONObject(string str)
	{
		str = str.Trim();
		if (str.Length > 0)
		{
			if (str == "true")
			{
				this.type = JSONObject.Type.BOOL;
				this.b = true;
			}
			else if (str == "false")
			{
				this.type = JSONObject.Type.BOOL;
				this.b = false;
			}
			else if (str == "null")
			{
				this.type = JSONObject.Type.NULL;
			}
			else if (str[0] == '"')
			{
				this.type = JSONObject.Type.STRING;
				this.str = str.Substring(1, str.Length - 2);
			}
			else
			{
				try
				{
					this.n = Convert.ToDouble(str);
					this.type = JSONObject.Type.NUMBER;
				}
				catch (FormatException)
				{
					int num = 0;
					char c = str[0];
					if (c != '[')
					{
						if (c != '{')
						{
							this.type = JSONObject.Type.NULL;
							throw new JSONFormattingException("Improper JSON formatting at " + str);
						}
						this.type = JSONObject.Type.OBJECT;
						this.keys = new ArrayList();
						this.list = new ArrayList();
					}
					else
					{
						this.type = JSONObject.Type.ARRAY;
						this.list = new ArrayList();
					}
					int num2 = 0;
					bool flag = false;
					for (int i = 1; i < str.Length; i++)
					{
						if (str[i] == '"')
						{
							flag = !flag;
						}
						if (str[i] == '[' || str[i] == '{')
						{
							num2++;
						}
						if (num2 == 0 && !flag)
						{
							if (str[i] == ':')
							{
								this.keys.Add(str.Substring(num + 2, i - num - 3));
								num = i;
							}
							if (str[i] == ',')
							{
								this.list.Add(new JSONObject(str.Substring(num + 1, i - num - 1)));
								num = i;
							}
							if (str[i] == ']' || str[i] == '}')
							{
								this.list.Add(new JSONObject(str.Substring(num + 1, i - num - 1)));
							}
						}
						if (str[i] == ']' || str[i] == '}')
						{
							num2--;
						}
					}
				}
			}
		}
		else
		{
			this.type = JSONObject.Type.NULL;
		}
	}

	public static JSONObject StringType(string str)
	{
		return new JSONObject
		{
			type = JSONObject.Type.STRING,
			str = str
		};
	}

	public string print()
	{
		string text = string.Empty;
		switch (this.type)
		{
		case JSONObject.Type.STRING:
			text = "\"" + this.str + "\"";
			break;
		case JSONObject.Type.NUMBER:
			text += this.n;
			break;
		case JSONObject.Type.OBJECT:
		{
			text = "{";
			for (int i = 0; i < this.list.Count; i++)
			{
				string text2 = (string)this.keys[i];
				text = text + "\"" + text2 + "\":";
				JSONObject jsonobject = (JSONObject)this.list[i];
				text = text + jsonobject.print() + ",";
			}
			text = text.Substring(0, text.Length - 1);
			text += "}";
			break;
		}
		case JSONObject.Type.ARRAY:
			text = "[";
			foreach (object obj in this.list)
			{
				JSONObject jsonobject2 = (JSONObject)obj;
				text = text + jsonobject2.print() + ",";
			}
			text = text.Substring(0, text.Length - 1);
			text += "]";
			break;
		case JSONObject.Type.BOOL:
			text += this.b;
			break;
		case JSONObject.Type.NULL:
			text = "null";
			break;
		}
		return text;
	}

	public static implicit operator bool(JSONObject j)
	{
		return j != null;
	}

	public ArrayList keys;

	public JSONObject parent;

	public JSONObject.Type type;

	public ArrayList list;

	public string str;

	public double n;

	public bool b;

	public enum Type
	{
		STRING,
		NUMBER,
		OBJECT,
		ARRAY,
		BOOL,
		NULL
	}
}
