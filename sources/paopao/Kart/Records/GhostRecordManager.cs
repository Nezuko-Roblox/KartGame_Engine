using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GhostRecordManager
{
	public static GhostRecordManager Instance
	{
		get
		{
			if (GhostRecordManager.instance_ == null)
			{
				GhostRecordManager.instance_ = new GhostRecordManager();
			}
			return GhostRecordManager.instance_;
		}
	}

	public byte GetSelectedTrack()
	{
		return this.selectedTrack_;
	}

	public void Initialize(byte trackIndex)
	{
		this.list_.Clear();
		this.selectedTrack_ = trackIndex;
		string[] files = Directory.GetFiles(FiaUtil.recordPath, "*.krg");
		for (int i = 0; i < files.Length; i++)
		{
			GhostFilenameInfo ghostFilenameInfo = new GhostFilenameInfo(files[i]);
			if (ghostFilenameInfo.version_ == 1 && ghostFilenameInfo.trackIdx_ == this.selectedTrack_)
			{
				this.list_.Add(new GhostRecordInfo(ghostFilenameInfo, GhostRecordType.OLDER));
			}
		}
		if (this.developerList_.Count == 0)
		{
			TextAsset textAsset = (TextAsset)Resources.Load("record/developer");
			if (textAsset != null)
			{
				StringReader stringReader = new StringReader(textAsset.text);
				if (stringReader != null)
				{
					string text;
					while ((text = stringReader.ReadLine()) != null)
					{
						if (text != string.Empty)
						{
							this.developerList_.Add(new GhostRecordInfo(text, GhostRecordType.DEVELOPER));
						}
					}
					stringReader.Close();
				}
			}
		}
		if (this.developerList_.Count > 0)
		{
			foreach (GhostRecordInfo ghostRecordInfo in this.developerList_)
			{
				if (ghostRecordInfo.filenameInfo_.trackIdx_ == trackIndex)
				{
					this.list_.Add(ghostRecordInfo);
				}
			}
		}
	}

	private int GetRandomGhost(ref List<GhostRecordInfo> l, out GhostRecordInfo ghostRecord, int total)
	{
		ghostRecord = null;
		if (l.Count > 0)
		{
			int num = global::UnityEngine.Random.Range(0, total);
			int num2 = 0;
			foreach (GhostRecordInfo ghostRecordInfo in l)
			{
				ghostRecord = ghostRecordInfo;
				if (ghostRecordInfo.randomValue_ > 0)
				{
					num -= ghostRecordInfo.randomValue_;
					if (num <= 0)
					{
						return num2;
					}
				}
				num2++;
			}
			return num2;
		}
		return -1;
	}

	public void GetRandomRecord(out GhostRecordInfo[] ghostRecordInfo)
	{
		ghostRecordInfo = new GhostRecordInfo[6];
		for (int i = 0; i < 6; i++)
		{
			ghostRecordInfo[i] = null;
		}
		GhostFilenameInfo bestInfo = KartOptions.Instance.GetBestInfo(this.selectedTrack_, (int)KartManager.Instance.parameter_.gameMode_);
		if (bestInfo != null)
		{
			ghostRecordInfo[1] = new GhostRecordInfo(bestInfo, GhostRecordType.PLAYER);
		}
		if (this.list_.Count <= 2)
		{
			for (int j = 0; j < this.list_.Count; j++)
			{
				ghostRecordInfo[j + 1] = this.list_[j];
			}
			return;
		}
		List<GhostRecordInfo> list = new List<GhostRecordInfo>();
		List<GhostRecordInfo> list2 = new List<GhostRecordInfo>();
		int num = 0;
		int num2 = 0;
		float num3 = 0f;
		if (bestInfo != null)
		{
			num3 = bestInfo.finishTime_;
		}
		foreach (GhostRecordInfo ghostRecordInfo2 in this.list_)
		{
			int num4 = ghostRecordInfo2.GenerateRandomValue(num3);
			if (ghostRecordInfo2.filenameInfo_.finishTime_ > num3)
			{
				list2.Add(ghostRecordInfo2);
				num2 += num4;
			}
			else
			{
				list.Add(ghostRecordInfo2);
				num += num4;
			}
		}
		if (list.Count > 0)
		{
			int randomGhost = this.GetRandomGhost(ref list, out ghostRecordInfo[2], num);
			if (list2.Count <= 0 && list.Count > 1)
			{
				list.RemoveAt(randomGhost);
				num -= ghostRecordInfo[2].randomValue_;
				this.GetRandomGhost(ref list, out ghostRecordInfo[3], num);
				return;
			}
		}
		if (list2.Count > 0)
		{
			int randomGhost2 = this.GetRandomGhost(ref list2, out ghostRecordInfo[3], num2);
			if (list.Count <= 0 && list2.Count > 1)
			{
				list2.RemoveAt(randomGhost2);
				num2 -= ghostRecordInfo[3].randomValue_;
				this.GetRandomGhost(ref list2, out ghostRecordInfo[2], num2);
				return;
			}
		}
	}

	public override string ToString()
	{
		string text = string.Empty;
		text = text + "track : " + this.selectedTrack_.ToString() + "\n";
		foreach (GhostRecordInfo ghostRecordInfo in this.list_)
		{
			text += ghostRecordInfo.filenameInfo_.filePath_;
			text += "\n";
		}
		return text;
	}

	public static GhostRecordManager instance_;

	private List<GhostRecordInfo> list_ = new List<GhostRecordInfo>();

	private List<GhostRecordInfo> developerList_ = new List<GhostRecordInfo>();

	private byte selectedTrack_;
}
