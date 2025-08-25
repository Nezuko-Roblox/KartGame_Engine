using System;
using System.Collections.Generic;
using UnityEngine;

public class GameInterface
{
	public GameInterface()
	{
		this.action_.Reset();
		this.itemSlotInterface_ = new ItemSlotInterfaceIPad();
	}

	public void Initialize(GameStageBase stage)
	{
		this.gameStage_ = stage;
		if (this.itemSlotInterface_ != null)
		{
			this.itemSlotInterface_.Initialize();
		}
		GUIMinimapMark minimapMark_ = this.gameStage_.minimapMark_;
		if (minimapMark_ != null)
		{
			for (int i = 0; i < 6; i++)
			{
				if (KartManager.Instance.goKart_[i] != null)
				{
					minimapMark_.kartIndex_ = i;
					GUIMinimapMark guiminimapMark = (GUIMinimapMark)global::UnityEngine.Object.Instantiate(minimapMark_);
					guiminimapMark.transform.parent = this.gameStage_.transform;
				}
			}
		}
	}

	public void Update(float tick)
	{
		if (this.itemSlotInterface_ != null)
		{
			this.itemSlotInterface_.Update(tick);
		}
		if (this.action_.isSet_)
		{
			if (!this.action_.isPlaying_ && this.action_.startTick_ <= tick)
			{
				this.action_.anim_.SetActiveRecursively(true);
				this.action_.anim_.animation.Play();
				this.action_.isPlaying_ = true;
			}
			if (this.action_.isPlaying_ && this.action_.endTick_ <= tick)
			{
				this.StopAction();
			}
		}
	}

	public void SetItemSlotCnt(int cnt)
	{
		if (this.itemSlotInterface_ != null)
		{
			this.itemSlotInterface_.SetItemSlotCnt(cnt);
		}
	}

	public int GetItemSlotCnt()
	{
		if (this.itemSlotInterface_ != null)
		{
			return this.itemSlotInterface_.GetItemSlotCnt();
		}
		return 0;
	}

	public bool AddItemSlotItem(int item)
	{
		return this.itemSlotInterface_ != null && this.itemSlotInterface_.AddItemSlotItem(item);
	}

	public int GetFirstItemSlot()
	{
		if (this.itemSlotInterface_ != null)
		{
			return this.itemSlotInterface_.GetFirstItemSlot();
		}
		return -1;
	}

	public int UseItemSlotItem()
	{
		if (this.itemSlotInterface_ != null)
		{
			return this.itemSlotInterface_.UseItemSlotItem();
		}
		return -1;
	}

	public void GetItemSlotItemInfo(out int[] itemVec)
	{
		itemVec = null;
		if (this.itemSlotInterface_ != null)
		{
			this.itemSlotInterface_.GetItemSlotItemInfo(out itemVec);
		}
	}

	public bool CanChangeSlotItems()
	{
		return this.itemSlotInterface_ != null && this.itemSlotInterface_.CanChangeSlotItems();
	}

	public void ChangeSlotItems(int tick)
	{
		if (this.itemSlotInterface_ != null)
		{
			this.itemSlotInterface_.ChangeSlotItems(tick);
		}
	}

	public int GetSlotChangerNum()
	{
		if (this.itemSlotInterface_ != null)
		{
			return this.itemSlotInterface_.GetSlotChangerNum();
		}
		return 0;
	}

	public void SetSlotChangerNum(int num)
	{
		if (this.itemSlotInterface_ != null)
		{
			this.itemSlotInterface_.SetSlotChangerNum(num);
		}
	}

	public void AddAction(string name, GameObject obj)
	{
		if (obj.animation == null)
		{
			return;
		}
		obj.animation.playAutomatically = false;
		obj.SetActiveRecursively(false);
		this.actionList_.Add(name, obj);
	}

	public void PlayAction(string name, float tick)
	{
		this.PlayAction(name, tick, true, false);
	}

	public void PlayAction(string name, float tick, bool isAutoEnd, bool isForced)
	{
		if (!isForced && this.action_.startTick_ != 0f && this.action_.name_ == name)
		{
			return;
		}
		this.StopAction();
		if (this.actionList_.ContainsKey(name))
		{
			this.action_.name_ = name;
			this.action_.isSet_ = true;
			this.action_.startTick_ = tick;
			this.action_.anim_ = this.actionList_[name];
			this.action_.anim_.animation.wrapMode = ((!isAutoEnd) ? WrapMode.ClampForever : WrapMode.Once);
			this.action_.endTick_ = ((!isAutoEnd) ? float.MaxValue : (this.action_.startTick_ + this.action_.anim_.animation.clip.length));
		}
	}

	public GameObject GetAction(string name)
	{
		if (this.actionList_.ContainsKey(name))
		{
			return this.actionList_[name];
		}
		return null;
	}

	public void StopAction()
	{
		if (this.action_.anim_ != null)
		{
			if (this.action_.anim_.animation.isPlaying)
			{
				this.action_.anim_.animation.Stop();
			}
			this.action_.anim_.SetActiveRecursively(false);
		}
		this.action_.Reset();
	}

	public void ShowBlackBar(bool isSmooth, bool isShowOnlyBlackBar)
	{
		if (isShowOnlyBlackBar)
		{
			MonoBehaviourMessage1Param<bool> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_UI);
			monoBehaviourMessage1Param.Initialize(false);
			MonoBehaviourExCenter.Instance.BroadcastMessage(0, monoBehaviourMessage1Param);
		}
		BlackBarMessage blackBarMessage = (BlackBarMessage)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.BLACK_BAR);
		blackBarMessage.Initialize(true, isSmooth, isShowOnlyBlackBar);
		MonoBehaviourExCenter.Instance.SendMessage(0, 14, blackBarMessage);
	}

	public void HideBlackBar(bool isSmooth, bool isShowOnlyBlackBar)
	{
		BlackBarMessage blackBarMessage = (BlackBarMessage)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.BLACK_BAR);
		blackBarMessage.Initialize(false, isSmooth, isShowOnlyBlackBar);
		MonoBehaviourExCenter.Instance.SendMessage(0, 14, blackBarMessage);
	}

	public void ResetForRestarting()
	{
		this.StopAction();
		this.HideBlackBar(false, false);
		if (this.itemSlotInterface_ != null)
		{
			this.itemSlotInterface_.ResetForRestarting();
		}
	}

	private ItemSlotInterface itemSlotInterface_;

	private GameInterface.Action action_ = default(GameInterface.Action);

	private GameStageBase gameStage_;

	private Dictionary<string, GameObject> actionList_ = new Dictionary<string, GameObject>();

	public struct Action
	{
		public void Reset()
		{
			this.anim_ = null;
			this.name_ = string.Empty;
			this.startTick_ = 0f;
			this.endTick_ = 0f;
			this.isSet_ = false;
			this.isPlaying_ = false;
		}

		public override string ToString()
		{
			string empty = string.Empty;
			string text = empty;
			return string.Concat(new string[]
			{
				text,
				FiaUtil.AddSquareBracket(this.name_),
				" ",
				FiaUtil.AddSquareBracket(this.startTick_),
				" ",
				FiaUtil.AddSquareBracket(this.endTick_),
				" "
			});
		}

		public GameObject anim_;

		public string name_;

		public float startTick_;

		public float endTick_;

		public bool isSet_;

		public bool isPlaying_;
	}
}
