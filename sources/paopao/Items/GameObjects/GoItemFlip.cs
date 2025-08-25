using System;
using UnityEngine;

public class GoItemFlip : ItemBasicController
{
	public override void Initialize(ItemParam param)
	{
		ItemFlipParam itemFlipParam = (ItemFlipParam)param;
		if (itemFlipParam != null)
		{
			base.Initialize(param);
			this.srcKartIndex_ = itemFlipParam.srcKartIndex_;
			Array.Copy(itemFlipParam.dstKartIndex_, this.targetKartIndex_, itemFlipParam.dstKartIndex_.Length);
			this.kartTrans_ = KartManager.Instance.goKart_[this.srcKartIndex_].controller_.KartBodyTransform;
			for (int i = 0; i < 1; i++)
			{
				this.SetEnableItemObject((GoItemFlip.ItemObject)i, false);
			}
			this.state_ = GoItemFlip.State.FIRE;
			base.transform.position = this.kartTrans_.position;
			base.transform.localRotation = this.kartTrans_.rotation;
			this.SetEnableItemObject(GoItemFlip.ItemObject.FIRE, true);
			base.PlayFx(0, this.srcKartIndex_ == KartManager.PLAYER_KART_IDX);
		}
		else
		{
			this.state_ = GoItemFlip.State.DESTROY;
		}
	}

	private void SetEnableItemObject(GoItemFlip.ItemObject obj, bool isEnable)
	{
		this.itemObjects_[(int)obj].SetActiveRecursively(isEnable);
		if (isEnable && this.itemObjects_[(int)obj].animation != null)
		{
			this.itemObjects_[(int)obj].animation.Play();
		}
	}

	protected override void ObjectSetting()
	{
		string[] array = new string[] { "fire" };
		foreach (object obj in base.transform)
		{
			Transform transform = (Transform)obj;
			int i;
			for (i = 0; i < 1; i++)
			{
				if (array[i] == transform.name)
				{
					this.itemObjects_[i] = transform.gameObject;
					break;
				}
			}
			if (i >= 1)
			{
				Debug.LogError("Invalid gameobject name in ITEM " + FiaUtil.AddSquareBracket(transform.name));
			}
		}
		this.applyItemParam_ = new ApplyItemParam(0);
		this.toPlayer_ = (MonoBehaviourMessage2Param<GameItem, ApplyItemParam>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.APPLY_ITEM);
	}

	private void FixedUpdate()
	{
		if (this.state_ == GoItemFlip.State.FIRE)
		{
			base.transform.position = this.kartTrans_.position;
			base.transform.localRotation = this.kartTrans_.rotation;
		}
		else if (this.state_ != GoItemFlip.State.DESTROY)
		{
			base.transform.position = this.kartTrans_.position;
		}
	}

	private void Update()
	{
		if (this.state_ == GoItemFlip.State.FIRE)
		{
			if (!this.itemObjects_[0].animation.isPlaying)
			{
				this.SetEnableItemObject(GoItemFlip.ItemObject.FIRE, false);
				this.applyItemParam_.senderId_ = this.srcKartIndex_;
				for (int i = 0; i < 6; i++)
				{
					if (this.targetKartIndex_[i])
					{
						this.toPlayer_.Initialize(GameItem.FLIP, this.applyItemParam_);
						MonoBehaviourExCenter.Instance.SendMessage(0, 24 + i, this.toPlayer_);
					}
				}
				this.state_ = GoItemFlip.State.DESTROY;
			}
		}
		else if (this.state_ == GoItemFlip.State.DESTROY)
		{
			base.gameObject.SetActiveRecursively(false);
		}
	}

	public const float FLIP_DURATION = 5f;

	public const float FLIP_READY_TIME = 1f;

	private int srcKartIndex_;

	private bool[] targetKartIndex_ = new bool[6];

	private GoItemFlip.State state_;

	private GameObject[] itemObjects_ = new GameObject[1];

	private Transform kartTrans_;

	private ApplyItemParam applyItemParam_;

	private MonoBehaviourMessage2Param<GameItem, ApplyItemParam> toPlayer_;

	private enum State
	{
		FIRE,
		DESTROY
	}

	private enum ItemObject
	{
		FIRE,
		SIZE
	}

	private enum FxType
	{
		FIRE
	}
}
