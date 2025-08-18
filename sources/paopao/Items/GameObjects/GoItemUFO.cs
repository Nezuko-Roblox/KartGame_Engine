using System;
using UnityEngine;

public class GoItemUFO : ItemBasicController
{
	public override void Initialize(ItemParam param)
	{
		ItemUFOParam itemUFOParam = (ItemUFOParam)param;
		this.duration_ = 3f;
		if (itemUFOParam != null)
		{
			base.Initialize(param);
			this.srcKartIndex_ = itemUFOParam.srcKartIndex_;
			this.targetKartIndex_ = itemUFOParam.dstKartIndex_;
			this.isAttack_ = itemUFOParam.isAttack_;
			this.srcKartBodyTrans_ = KartManager.Instance.goKart_[this.srcKartIndex_].controller_.KartBodyTransform;
			if (KartManager.Instance.IsValidKart(this.targetKartIndex_))
			{
				GoKart goKart = KartManager.Instance.goKart_[this.targetKartIndex_];
				this.dstKartBodyTrans_ = goKart.controller_.KartBodyTransform;
			}
			for (int i = 0; i < 4; i++)
			{
				this.SetEnableItemObject((GoItemUFO.ItemObject)i, false);
			}
			if (this.isAttack_)
			{
				base.transform.position = this.srcKartBodyTrans_.position;
				base.transform.localRotation = this.srcKartBodyTrans_.rotation;
				this.SetEnableItemObject(GoItemUFO.ItemObject.FIRE, true);
				this.state_ = GoItemUFO.State.FIRE;
				base.PlayFx(0, this.srcKartIndex_ == KartManager.PLAYER_KART_IDX);
			}
			else
			{
				base.transform.position = this.dstKartBodyTrans_.position;
				base.transform.localRotation = this.dstKartBodyTrans_.rotation;
				this.SetEnableItemObject(GoItemUFO.ItemObject.START, true);
				this.state_ = GoItemUFO.State.START;
			}
		}
		else
		{
			this.state_ = GoItemUFO.State.DESTROY;
		}
	}

	private void SetEnableItemObject(GoItemUFO.ItemObject obj, bool isEnable)
	{
		this.itemObjects_[(int)obj].SetActiveRecursively(isEnable);
		if (isEnable && this.itemObjects_[(int)obj].animation != null)
		{
			this.itemObjects_[(int)obj].animation.Play();
		}
	}

	protected override void ObjectSetting()
	{
		string[] array = new string[] { "fire", "start", "play", "end" };
		foreach (object obj in base.transform)
		{
			Transform transform = (Transform)obj;
			int i;
			for (i = 0; i < 4; i++)
			{
				if (array[i] == transform.name)
				{
					this.itemObjects_[i] = transform.gameObject;
					break;
				}
			}
			if (i >= 4)
			{
				Debug.LogError("Invalid gameobject name in ITEM " + FiaUtil.AddSquareBracket(transform.name));
			}
		}
		this.applyItemParam_ = new ApplyItemParam(0);
		this.toPlayer_ = (MonoBehaviourMessage2Param<GameItem, ApplyItemParam>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.APPLY_ITEM);
	}

	private void FixedUpdate()
	{
		if (this.state_ == GoItemUFO.State.FIRE)
		{
			base.transform.position = this.srcKartBodyTrans_.position;
		}
		else if (this.state_ != GoItemUFO.State.DESTROY)
		{
			base.transform.position = this.dstKartBodyTrans_.position;
			base.transform.localRotation = this.dstKartBodyTrans_.rotation;
		}
	}

	private void Update()
	{
		if (this.state_ == GoItemUFO.State.FIRE)
		{
			if (!this.itemObjects_[0].animation.isPlaying)
			{
				this.SetEnableItemObject(GoItemUFO.ItemObject.FIRE, false);
				if (KartManager.Instance.IsValidKart(this.targetKartIndex_))
				{
					this.SetEnableItemObject(GoItemUFO.ItemObject.START, true);
					this.state_ = GoItemUFO.State.START;
				}
				else
				{
					this.state_ = GoItemUFO.State.DESTROY;
				}
			}
		}
		else if (this.state_ == GoItemUFO.State.START)
		{
			if (!this.itemObjects_[1].animation.isPlaying)
			{
				this.SetEnableItemObject(GoItemUFO.ItemObject.START, false);
				this.SetEnableItemObject(GoItemUFO.ItemObject.PLAY, true);
				this.state_ = GoItemUFO.State.PLAY;
				this.applyItemParam_.senderId_ = this.srcKartIndex_;
				this.toPlayer_.Initialize(GameItem.UFO, this.applyItemParam_);
				MonoBehaviourExCenter.Instance.SendMessage(0, 24 + this.targetKartIndex_, this.toPlayer_);
				base.PlayFx(1, this.targetKartIndex_ == KartManager.PLAYER_KART_IDX);
			}
		}
		else if (this.state_ == GoItemUFO.State.PLAY)
		{
			this.duration_ -= Time.deltaTime;
			if (this.duration_ <= 0f || KartManager.Instance.goCourse_.IsKartGoalIn(this.targetKartIndex_))
			{
				this.SetEnableItemObject(GoItemUFO.ItemObject.PLAY, false);
				this.SetEnableItemObject(GoItemUFO.ItemObject.END, true);
				this.state_ = GoItemUFO.State.END;
			}
		}
		else if (this.state_ == GoItemUFO.State.END)
		{
			if (!this.itemObjects_[3].animation.isPlaying)
			{
				this.SetEnableItemObject(GoItemUFO.ItemObject.END, false);
				this.state_ = GoItemUFO.State.DESTROY;
			}
		}
		else if (this.state_ == GoItemUFO.State.DESTROY)
		{
			base.gameObject.SetActiveRecursively(false);
		}
	}

	public const float SPEED_DOWN_DURATION = 3f;

	private int srcKartIndex_;

	private int targetKartIndex_;

	private bool isAttack_;

	private float duration_ = 3f;

	private GoItemUFO.State state_;

	private GameObject[] itemObjects_ = new GameObject[4];

	private Transform srcKartBodyTrans_;

	private Transform dstKartBodyTrans_;

	private ApplyItemParam applyItemParam_;

	private MonoBehaviourMessage2Param<GameItem, ApplyItemParam> toPlayer_;

	private enum State
	{
		FIRE,
		START,
		PLAY,
		END,
		DESTROY
	}

	private enum ItemObject
	{
		FIRE,
		START,
		PLAY,
		END,
		SIZE
	}

	private enum FxType
	{
		FIRE,
		AFFECT
	}
}
