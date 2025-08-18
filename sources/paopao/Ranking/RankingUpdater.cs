using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankingUpdater : IStartable
{
	public RankingUpdater(string fbid, Dictionary<string, string> friends, string authToken)
	{
		this.fbid_ = fbid;
		this.friends_ = friends;
		this.authToken_ = authToken;
	}

	public StartableState State
	{
		get
		{
			return this.state_;
		}
		set
		{
			this.state_ = value;
		}
	}

	public Exception Error
	{
		get
		{
			return this.error_;
		}
		set
		{
			this.error_ = value;
		}
	}

	public virtual Request BuildRequest()
	{
		GetRequest getRequest = new GetRequest("http://s.kartriderrush.com/server/ranking.php");
		getRequest.AddField("fbid", this.fbid_);
		getRequest.AddField("friends", this.friends_.Keys);
		getRequest.AddField("auth_token", this.authToken_);
		return getRequest;
	}

	protected List<Dictionary<int, Dictionary<string, Record>>> ParseXML(XMLElement xml)
	{
		List<Dictionary<int, Dictionary<string, Record>>> list = new List<Dictionary<int, Dictionary<string, Record>>>();
		list.Add(new Dictionary<int, Dictionary<string, Record>>());
		list.Add(new Dictionary<int, Dictionary<string, Record>>());
		int assetCount = TrackAssetDefinitionManager.Instance.GetAssetCount();
		for (int i = 0; i < assetCount; i++)
		{
			list[0].Add(i, this.CreateDefaultRankingDict(GameMode.SINGLE_ITEM, i));
			list[1].Add(i, this.CreateDefaultRankingDict(GameMode.SINGLE_SPEED, i));
		}
		foreach (object obj in xml.getChildren())
		{
			XMLElement xmlelement = (XMLElement)obj;
			Record record = new Record(xmlelement);
			int gameMode_ = (int)record.gameMode_;
			if (!list[gameMode_].ContainsKey(record.map_))
			{
				list[0].Add(record.map_, this.CreateDefaultRankingDict(GameMode.SINGLE_ITEM, record.map_));
				list[1].Add(record.map_, this.CreateDefaultRankingDict(GameMode.SINGLE_SPEED, record.map_));
			}
			if (this.friends_.ContainsKey(record.fbid_))
			{
				list[(int)record.gameMode_][record.map_][record.fbid_].time_ = record.time_;
			}
		}
		return list;
	}

	protected Dictionary<string, Record> CreateDefaultRankingDict(GameMode mode, int map)
	{
		Dictionary<string, Record> dictionary = new Dictionary<string, Record>();
		foreach (string text in this.friends_.Keys)
		{
			float num = 1200f;
			if (text == this.fbid_)
			{
				num += 1f;
			}
			dictionary.Add(text, new Record(text, this.friends_[text], mode, num, map));
		}
		return dictionary;
	}

	protected void Save(List<Dictionary<int, Dictionary<string, Record>>> records)
	{
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < TrackAssetDefinitionManager.Instance.GetAssetCount(); j++)
			{
				MonthlyRanking monthlyRanking = new MonthlyRanking(this.fbid_, (GameMode)i, j);
				foreach (Record record in records[i][j].Values)
				{
					monthlyRanking.Records.Add(record);
				}
				monthlyRanking.Save();
			}
		}
	}

	public virtual IEnumerator Update()
	{
		this.Error = null;
		this.State = StartableState.RUNNING;
		Request req = this.BuildRequest();
		IEnumerator r = req.Run();
		while (r.MoveNext())
		{
			object obj = r.Current;
			yield return obj;
		}
		if (req.XMLResponse.Error == null)
		{
			try
			{
				MonthlyRanking.DeleteAll(this.fbid_);
				this.Save(this.ParseXML(req.XMLResponse.XML));
				this.UpdateYearMonth(req.XMLResponse.XML);
				this.State = StartableState.WAITING;
			}
			catch (Exception ex2)
			{
				Exception ex = ex2;
				this.Error = new UnknownException(ex);
			}
		}
		else
		{
			this.Error = req.XMLResponse.Error;
		}
		if (this.State != StartableState.WAITING)
		{
			this.State = StartableState.FAILED;
		}
		if (this.Error != null)
		{
			throw this.Error;
		}
		RankingParameter.UpdateRanking = false;
		yield break;
	}

	public IEnumerator Update(RankingUpdaterDelegate del)
	{
		IEnumerator e = this.Update();
		while (e.MoveNext())
		{
			object obj = e.Current;
			yield return obj;
		}
		del(this);
		yield break;
	}

	public virtual void UpdateYearMonth(XMLElement xml)
	{
		if (xml.getName() == "records")
		{
			string text = (string)xml.getAttribute("yearmonth");
			string @string = PlayerPrefs.GetString("YEARMONTH_LAST", string.Empty);
			if (@string != string.Empty && text != @string)
			{
				PlayerPrefs.SetString("YEARMONTH_LAST", text);
				PlayerPrefs.SetInt("YEARMONTH_RESET", 1);
			}
		}
	}

	protected string fbid_;

	private List<List<string>> rank_;

	protected Dictionary<string, string> friends_;

	protected string authToken_;

	protected Exception error_;

	protected StartableState state_;

	protected class sortByTime : Comparer<Record>
	{
		public override int Compare(Record x, Record y)
		{
			return (x.time_ <= y.time_) ? (-1) : 1;
		}
	}
}
