using System;
using System.Globalization;
using UnityEngine;

public class RegistryTrack : RegistryValue
{
	public RegistryTrack()
	{
		this.isDirty_ = false;
		this.isReadRegistry_ = false;
		this.best_ = null;
		this.best_ = new GhostFilenameInfo[2];
		this.raceCounter_ = new int[4];
		this.winCounter_ = new int[4];
	}

	public override void ReadRegistry(string key)
	{
		this.isDirty_ = false;
		this.isReadRegistry_ = true;
		string @string = PlayerPrefs.GetString(key);
		if (@string != null && @string != string.Empty)
		{
			char[] array = new char[] { '/' };
			string[] array2 = @string.Split(array);
			if (array2.Length == 11)
			{
				for (int i = 0; i < 2; i++)
				{
					if (array2[i] != null && array2[i] != string.Empty)
					{
						this.best_[i] = new GhostFilenameInfo(array2[i]);
						if (!this.best_[i].IsValidData())
						{
							this.best_[i] = null;
						}
						if (this.best_[i] != null)
						{
							this.raceCounter_[i] = int.Parse(array2[i + 2], NumberStyles.HexNumber);
							this.raceCounter_[i + 2] = int.Parse(array2[i + 4], NumberStyles.HexNumber);
							this.winCounter_[i] = int.Parse(array2[i + 6], NumberStyles.HexNumber);
							this.winCounter_[i + 2] = int.Parse(array2[i + 8], NumberStyles.HexNumber);
						}
					}
					else
					{
						this.raceCounter_[i] = 0;
						this.raceCounter_[i + 2] = int.Parse(array2[i + 4], NumberStyles.HexNumber);
						this.winCounter_[i] = 0;
						this.winCounter_[i + 2] = int.Parse(array2[i + 8], NumberStyles.HexNumber);
					}
				}
			}
		}
	}

	public override void SaveRegistry(string key)
	{
		if (this.isDirty_)
		{
			string text = string.Empty;
			for (int i = 0; i < 2; i++)
			{
				if (this.best_[i] != null)
				{
					text += this.best_[i].GenerateFilename();
				}
				text += "/";
			}
			for (int j = 0; j < this.raceCounter_.Length; j++)
			{
				text += string.Format("{0:X}/", this.raceCounter_[j]);
			}
			for (int k = 0; k < this.winCounter_.Length; k++)
			{
				text += string.Format("{0:X}/", this.winCounter_[k]);
			}
			PlayerPrefs.SetString(key, text);
			this.isDirty_ = false;
		}
	}

	public void SetBestInfo(int idx, GhostFilenameInfo info)
	{
		this.best_[idx] = info;
		this.isDirty_ = true;
	}

	public GhostFilenameInfo GetBestInfo(int idx)
	{
		return this.best_[idx];
	}

	public int[] RaceCounter
	{
		get
		{
			return this.raceCounter_;
		}
		set
		{
			Array.Copy(value, this.raceCounter_, this.raceCounter_.Length);
			this.isDirty_ = true;
		}
	}

	public int[] WinCounter
	{
		get
		{
			return this.winCounter_;
		}
		set
		{
			Array.Copy(value, this.winCounter_, this.winCounter_.Length);
			this.isDirty_ = true;
		}
	}

	public override void Reset()
	{
		for (int i = 0; i < 2; i++)
		{
			this.best_[i] = null;
		}
		this.isDirty_ = true;
		this.isReadRegistry_ = true;
		for (int j = 0; j < 4; j++)
		{
			this.raceCounter_[j] = 0;
			this.winCounter_[j] = 0;
		}
	}

	public override string ToString()
	{
		return string.Empty;
	}

	private GhostFilenameInfo[] best_;

	private int[] raceCounter_;

	private int[] winCounter_;
}
