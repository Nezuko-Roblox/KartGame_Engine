using System;
using UnityEngine;

public class KartManager
{
	public static KartManager Instance
	{
		get
		{
			if (KartManager.instance_ == null)
			{
				KartManager.instance_ = new KartManager();
			}
			return KartManager.instance_;
		}
	}

	public GoKart SetKart(int idx, GoKartBuilder builder, KartBasicController controller, Transform[] wheelPos)
	{
		if (this.goKart_[idx] != null)
		{
			return null;
		}
		this.goKart_[idx] = builder.Build();
		this.goKart_[idx].setReKart(controller, wheelPos);
		if (idx == KartManager.PLAYER_KART_IDX)
		{
			this.goPlayKart_ = (GoPlayKart)this.goKart_[idx];
		}
		this.goKartCount_++;
		return this.goKart_[idx];
	}

	public void InitGoCourse(TextAsset planeInfo_)
	{
		this.goCourse_ = new GoCourse();
		this.goCourse_.Initialize(planeInfo_);
	}

	public void InitGoCourse(TextAsset planeInfo, TextAsset sequenceInfo)
	{
		this.goCourse_ = new GoCourse();
		this.goCourse_.Initialize(planeInfo, sequenceInfo);
	}

	public void InitGoCourse(BinaryAsset planeInfo_)
	{
		this.goCourse_ = new GoCourse();
		this.goCourse_.Initialize(planeInfo_);
	}

	public void InitGoCourse(BinaryAsset planeInfo, BinaryAsset sequenceInfo)
	{
		this.goCourse_ = new GoCourse();
		this.goCourse_.Initialize(planeInfo, sequenceInfo);
	}

	public void InitGameData()
	{
		this.driveStartTime_ = 0f;
		this.driveEndTime_ = 0f;
		for (int i = 0; i < 6; i++)
		{
			this.goKart_[i] = null;
			this.kartState_[i] = KartState.NO_STATE;
		}
		this.goKartCount_ = 0;
		this.result_ = null;
		this.goCourse_ = null;
		this.isPaused_ = false;
	}

	public void ResetForRestarting()
	{
		this.isPaused_ = false;
		this.driveStartTime_ = 0f;
		this.driveEndTime_ = 0f;
		for (int i = 0; i < 6; i++)
		{
			this.goKart_[i] = null;
			this.kartState_[i] = KartState.NO_STATE;
		}
		this.result_ = null;
		this.goCourse_.ResetForRestarting();
		for (int j = 0; j < 6; j++)
		{
			if (this.goKart_[j] != null)
			{
				this.goKart_[j].ResetForRestarting();
			}
		}
		if (this.gameInterface_ != null)
		{
			this.gameInterface_.ResetForRestarting();
		}
	}

	public bool Stuck
	{
		set
		{
			for (int i = 0; i < 6; i++)
			{
				if (this.goKart_[i] != null)
				{
					this.goKart_[i].Stuck = value;
				}
			}
		}
	}

	public float DriveStartTime
	{
		get
		{
			return this.driveStartTime_;
		}
		set
		{
			this.driveStartTime_ = value;
		}
	}

	public float DriveEndTime
	{
		get
		{
			return this.driveEndTime_;
		}
		set
		{
			this.driveEndTime_ = value;
		}
	}

	public float GetPlayTime()
	{
		if ((double)this.driveStartTime_ <= 0.0)
		{
			return 0f;
		}
		float num = Time.time - this.driveStartTime_;
		return (num > 0f) ? num : 0f;
	}

	public int GetGoKartCount()
	{
		return this.goKartCount_;
	}

	public bool IsValidKart(int index)
	{
		return MathHelper.IsBetweenIE(index, 0, 6) && this.goKart_[index] != null;
	}

	public int GetKartIndex(GameObject obj)
	{
		for (int i = 0; i < 6; i++)
		{
			if (this.goKart_[i] != null && this.goKart_[i].m_kart == obj)
			{
				return i;
			}
		}
		return -1;
	}

	public void CheckDirectory()
	{
		FiaUtil.CreateFolderIfNotExist("record");
	}

	public bool IsTestDrive()
	{
		return this.parameter_.driveOption_ == DriveOption.RECORD_AI;
	}

	public static void DidEnterBackground()
	{
		if (KartManager.Instance.parameter_.Stage == StageType.GAME)
		{
			GameStageBase.DriveState[] array = new GameStageBase.DriveState[]
			{
				GameStageBase.DriveState.LOADING,
				GameStageBase.DriveState.READY,
				GameStageBase.DriveState.DRIVING
			};
			GameStageBase gameStageBase = KartManager.Instance.currGameStage_;
			if (gameStageBase != null)
			{
				foreach (GameStageBase.DriveState driveState in array)
				{
					if (driveState == gameStageBase.driveState_ && Time.timeScale > 0f)
					{
						MonoBehaviourExCenter.Instance.SendMessage(0, 1, ((MonoBehaviourMessage1Param<GameStageCommand>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.GAMESTAGE_COMMAND)).Initialize(GameStageCommand.PAUSE));
						break;
					}
				}
			}
		}
	}

	public static void WillEnterForeground()
	{
	}

	public const float KART_SCALE_FACTOR = 0.15f;

	public const int MAX_KART = 6;

	public const float KART_GRAVITY = -49f;

	public static int PLAYER_KART_IDX;

	public static int FIXED_UPDATE_COUNTER;

	public static KartManager instance_;

	public KartState[] kartState_ = new KartState[6];

	public GoPlayKart goPlayKart_;

	public GoKart[] goKart_ = new GoKart[6];

	public GoCourse goCourse_;

	public GameInterface gameInterface_;

	private float driveStartTime_;

	private float driveEndTime_;

	public Parameter parameter_ = new Parameter();

	private int goKartCount_;

	public RaceResult result_;

	public bool isPaused_;

	public GameStageBase currGameStage_;
}
