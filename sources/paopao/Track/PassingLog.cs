using System;
using System.Collections.Generic;

public class PassingLog
{
	public static PassingLog Instance
	{
		get
		{
			if (PassingLog.instance_ == null)
			{
				PassingLog.instance_ = new PassingLog();
			}
			return PassingLog.instance_;
		}
	}

	public void AddElem(PassingLogElem elem)
	{
		this.elems_.Add(elem);
	}

	public void Clear()
	{
		this.elems_.Clear();
	}

	public override string ToString()
	{
		string text = string.Empty;
		text += "\n";
		foreach (PassingLogElem passingLogElem in this.elems_)
		{
			text += passingLogElem.ToString();
			text += "\n";
		}
		return text;
	}

	public static PassingLog instance_;

	private List<PassingLogElem> elems_ = new List<PassingLogElem>();
}
