using System;
using System.Collections.Generic;
using System.IO;

public class AllTimeRanking : Ranking
{
	public AllTimeRanking(string fbid, GameMode gameMode, int map)
	{
		this.fbid_ = fbid;
		this.gameMode_ = gameMode;
		this.map_ = map;
		this.records_ = new List<Record>();
	}

	public override void FromXML(XMLElement xml)
	{
		this.records_ = new List<Record>();
		try
		{
			this.gameMode_ = (GameMode)int.Parse((string)xml.getAttribute("game_mode"));
			this.fbid_ = (string)xml.getAttribute("fbid");
			this.map_ = int.Parse((string)xml.getAttribute("map"));
			foreach (object obj in xml.getChildren())
			{
				XMLElement xmlelement = (XMLElement)obj;
				this.records_.Add(new Record(xmlelement));
			}
			this.records_.Sort(new Ranking.sortByTime());
		}
		catch (Exception ex)
		{
		}
	}

	public override XMLElement ToXML()
	{
		XMLElement xmlelement = new XMLElement();
		xmlelement.setName("ranking");
		xmlelement.setAttribute("ranking_mode", "alltime");
		xmlelement.setAttribute("game_mode", (int)this.gameMode_);
		xmlelement.setAttribute("fbid", this.fbid_);
		xmlelement.setAttribute("map", this.map_);
		foreach (Record record in this.records_)
		{
			xmlelement.addChild(record.ToXML());
		}
		return xmlelement;
	}

	public override string LocalPath
	{
		get
		{
			string text = FiaUtil.recordPath;
			text = Path.Combine(text, "ranking");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			return Path.Combine(text, string.Format("{0}_{1}_{2}_{3}.xml", new object[]
			{
				this.fbid_,
				"alltime",
				(int)this.gameMode_,
				this.map_
			}));
		}
	}

	public override string ToString()
	{
		string text = string.Format("<AllTimeRanking fbid={0} map={1} records=[", this.fbid_, this.map_);
		foreach (Record record in this.records_)
		{
			text += string.Format("{0},", record.ToString());
		}
		text += "]>";
		return text;
	}

	private int map_;

	private GameMode gameMode_;
}
