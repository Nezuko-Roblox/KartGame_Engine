using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Record : IStartable, IXMLizable
{
	public Record(string fbid, string fbname, GameMode gameMode, float time, int map)
	{
		Debug.Log(string.Concat(new object[] { "record__________Record______time_:", time, " map:", map, " game_mode:", gameMode, " fbid:", fbid }));
		this.fbid_ = fbid;
		this.fbname_ = fbname;
		this.gameMode_ = gameMode;
		this.time_ = time;
		this.map_ = map;
	}

	public Record(XMLElement xml)
	{
		this.FromXML(xml);
	}

	public Record()
	{
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

	public IEnumerator Upload(string authToken, Dictionary<string, string> friends)
	{
		this.State = StartableState.RUNNING;
		PostRequest req = this.BuildRequest();
		req.AddField("auth_token", authToken);
		req.AddField("friends", friends.Keys);
		IEnumerator r = req.Run();
		while (r.MoveNext())
		{
			object obj = r.Current;
			yield return obj;
		}
		XMLResponse response = req.XMLResponse;
		if (req.Error != null || response.Error != null)
		{
			this.State = StartableState.FAILED;
			this.Error = response.Error;
			throw this.Error;
		}
		SpecificRankingUpdater updater = new SpecificRankingUpdater(this.fbid_, friends, authToken, this.map_, this.gameMode_);
		updater.UpdateFromXML(response.XML);
		updater.UpdateYearMonth(response.XML);
		this.State = StartableState.WAITING;
		yield break;
	}

	public PostRequest BuildRequest()
	{
		PostRequest postRequest = new PostRequest("http://s.kartriderrush.com/server/record.php");
		postRequest.AddField("fbid", this.fbid_);
		postRequest.AddField("game_mode", (int)this.gameMode_);
		postRequest.AddField("time", this.time_);
		postRequest.AddField("map", this.map_);
		return postRequest;
	}

	public XMLElement ToXML()
	{
		XMLElement xmlelement = new XMLElement();
		xmlelement.setName("record");
		xmlelement.setAttribute("fbid", this.fbid_);
		xmlelement.setAttribute("fbname", this.fbname_);
		xmlelement.setAttribute("time", this.time_);
		xmlelement.setAttribute("map", this.map_);
		xmlelement.setAttribute("game_mode", (int)this.gameMode_);
		return xmlelement;
	}

	public void FromXML(XMLElement xml)
	{
		this.fbid_ = (string)xml.getAttribute("fbid");
		this.fbname_ = (string)xml.getAttribute("fbname");
		this.time_ = float.Parse((string)xml.getAttribute("time"));
		this.map_ = int.Parse((string)xml.getAttribute("map"));
		this.gameMode_ = (GameMode)int.Parse((string)xml.getAttribute("game_mode"));
	}

	public override string ToString()
	{
		return string.Format("<Record fbid={0} fbname={1} map={2} time={3} game_mode={4}", new object[] { this.fbid_, this.fbname_, this.map_, this.time_, this.gameMode_ });
	}

	public string fbid_;

	public string fbname_;

	public float time_;

	public int map_;

	public GameMode gameMode_;

	public string yearMonth_;

	protected Exception error_;

	protected StartableState state_;
}
