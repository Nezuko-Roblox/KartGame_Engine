using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class Ranking : IXMLizable
{
	public List<Record> Records
	{
		get
		{
			return this.records_;
		}
	}

	public abstract string LocalPath { get; }

	public abstract void FromXML(XMLElement xml);

	public abstract XMLElement ToXML();

	public bool Load()
	{
		bool flag;
		try
		{
			XMLElement xmlelement = new XMLElement();
			using (StreamReader streamReader = new StreamReader(this.LocalPath))
			{
				xmlelement.parseString(streamReader.ReadToEnd());
			}
			this.FromXML(xmlelement);
			flag = true;
		}
		catch (Exception ex)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log(ex);
			}
			flag = false;
		}
		return flag;
	}

	public virtual bool Save()
	{
		bool flag;
		try
		{
			using (StreamWriter streamWriter = new StreamWriter(this.LocalPath))
			{
				if (this.Records != null)
				{
					this.Records.Sort(new Ranking.sortByTime());
				}
				streamWriter.Write(this.ToXML().ToString());
				streamWriter.Flush();
			}
			flag = true;
		}
		catch (Exception ex)
		{
			flag = false;
		}
		return flag;
	}

	public int CalcRank()
	{
		int num = 1;
		if (this.Records != null)
		{
			this.Records.Sort(new Ranking.sortByTime());
			foreach (Record record in this.Records)
			{
				if (record.fbid_ == this.fbid_)
				{
					return num;
				}
				num++;
			}
			return 0;
		}
		return 0;
	}

	public Record FindRecordWithID(string fbid)
	{
		return this.Records.Find((Record r) => r.fbid_ == fbid);
	}

	public const float NO_RECORD_TIME = 1200f;

	protected string fbid_;

	public List<Record> records_;

	protected class sortByTime : Comparer<Record>
	{
		public override int Compare(Record x, Record y)
		{
			return x.time_.CompareTo(y.time_);
		}
	}
}
