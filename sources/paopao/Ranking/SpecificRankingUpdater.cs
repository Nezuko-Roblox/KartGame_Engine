using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecificRankingUpdater : RankingUpdater
{
	public SpecificRankingUpdater(string fbid, Dictionary<string, string> friends, string authToken, int map, GameMode gameMode)
		: base(fbid, friends, authToken)
	{
		this.map_ = map;
		this.gameMode_ = gameMode;
	}

	public override Request BuildRequest()
	{
		GetRequest getRequest = new GetRequest("http://s.kartriderrush.com/server/specificranking.php");
		getRequest.AddField("fbid", this.fbid_);
		getRequest.AddField("friends", this.friends_.Keys);
		getRequest.AddField("auth_token", this.authToken_);
		getRequest.AddField("map", this.map_);
		getRequest.AddField("game_mode", (int)this.gameMode_);
		return getRequest;
	}

	public override IEnumerator Update()
	{
		base.Error = null;
		base.State = StartableState.RUNNING;
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
				MonthlyRanking ranking = new MonthlyRanking(this.fbid_, this.gameMode_, this.map_);
				ranking.Delete();
				base.Save(base.ParseXML(req.XMLResponse.XML));
				this.UpdateYearMonth(req.XMLResponse.XML);
				base.State = StartableState.WAITING;
			}
			catch (Exception ex2)
			{
				Exception ex = ex2;
				base.Error = new UnknownException(ex);
			}
		}
		else
		{
			base.Error = req.XMLResponse.Error;
		}
		if (this.State != StartableState.WAITING)
		{
			base.State = StartableState.FAILED;
		}
		if (this.Error != null)
		{
			throw this.Error;
		}
		yield break;
	}

	public void UpdateFromXML(XMLElement xml)
	{
		Dictionary<string, Record> dictionary = new Dictionary<string, Record>();
		foreach (string text in this.friends_.Keys)
		{
			float num = 1200f;
			if (text == this.fbid_)
			{
				num += 1f;
			}
			if (!dictionary.ContainsKey(text))
			{
				Record record = new Record(text, this.friends_[text], this.gameMode_, num, this.map_);
				dictionary.Add(text, record);
			}
		}
		foreach (object obj in xml.getChildren())
		{
			XMLElement xmlelement = (XMLElement)obj;
			Record record2 = new Record(xmlelement);
			dictionary[record2.fbid_].time_ = record2.time_;
		}
		MonthlyRanking monthlyRanking = new MonthlyRanking(this.fbid_, this.gameMode_, this.map_);
		foreach (Record record3 in dictionary.Values)
		{
			monthlyRanking.Records.Add(record3);
		}
		monthlyRanking.Save();
	}

	public override void UpdateYearMonth(XMLElement xml)
	{
		if (xml.getName() == "records")
		{
			string text = (string)xml.getAttribute("yearmonth");
			string @string = PlayerPrefs.GetString("YEARMONTH_LAST", string.Empty);
			if (text != @string)
			{
				PlayerPrefs.SetString("YEARMONTH_LAST", text);
				PlayerPrefs.SetInt("REFRESH_RANKING", 1);
				if (@string != string.Empty)
				{
					PlayerPrefs.SetInt("YEARMONTH_RESET", 1);
				}
			}
		}
	}

	protected int map_;

	protected GameMode gameMode_;
}
