using System;
using System.Collections.Generic;

public class Parameter
{
	public Parameter()
	{
		for (int i = 0; i < 6; i++)
		{
			this.kart_[i] = null;
		}
	}

	public StageType GaragePrevStage
	{
		get
		{
			return this.garagePrevStage_;
		}
	}

	public StageType PrevStage
	{
		get
		{
			return this.prevStage_;
		}
	}

	public StageType Stage
	{
		get
		{
			return this.stage_;
		}
		set
		{
			if (this.prevStage_ != StageType.LOADING_DEFAULT && this.prevStage_ != StageType.STORE && this.prevStage_ != StageType.GARAGE)
			{
				this.garagePrevStage_ = this.prevStage_;
			}
			this.prevStage_ = this.stage_;
			this.stage_ = value;
		}
	}

	public override string ToString()
	{
		string text = string.Empty;
		text = text + this.track_.ToString() + "\n";
		text = text + this.level_ + "\n";
		text = text + this.maxLap_.ToString() + "\n";
		text = text + this.driveOption_.ToString() + "\n";
		text = text + this.gameMode_.ToString() + "\n";
		for (int i = 0; i < 6; i++)
		{
			if (this.kart_[i] != null)
			{
				text = text + FiaUtil.AddSquareBracket((float)i) + this.kart_[i].ToString() + "\n";
			}
			else
			{
				text = text + FiaUtil.AddSquareBracket((float)i) + "<null>\n";
			}
		}
		return text;
	}

	public KartParameter[] kart_ = new KartParameter[6];

	public byte track_;

	public string level_ = "level/level1";

	public int maxLap_ = 1;

	public DriveOption driveOption_;

	public GameMode gameMode_ = GameMode.SINGLE_SPEED;

	public List<int> startPositions_;

	private StageType stage_ = StageType.MAIN;

	private StageType prevStage_ = StageType.MAIN;

	private StageType garagePrevStage_ = StageType.MAIN;
}
