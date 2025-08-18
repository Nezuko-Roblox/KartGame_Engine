using System;
using UnityEngine;

public class GoItemWaterFly : ItemBasicController
{
	public override void Initialize(ItemParam param)
	{
		ItemWaterFlyParam itemWaterFlyParam = (ItemWaterFlyParam)param;
		this.lifeTime_ = 2f;
		this.stayingTime_ = 0.4f;
		this.tick_ = 0f;
		this.flyState_ = 0;
		if (itemWaterFlyParam != null)
		{
			base.Initialize(param);
			this.srcKartIndex_ = itemWaterFlyParam.srcKartIndex_;
			this.targetKartIndex_ = itemWaterFlyParam.targetKartIndex_;
			this.srcKartTrans_ = KartManager.Instance.goKart_[this.srcKartIndex_].m_kart.transform;
			this.dstKartTrans_ = KartManager.Instance.goKart_[this.targetKartIndex_].m_kart.transform;
			for (int i = 0; i < 2; i++)
			{
				this.SetEnableItemObject((GoItemWaterFly.ItemObject)i, false);
			}
			base.transform.position = this.srcKartTrans_.localPosition;
			base.transform.localRotation = this.srcKartTrans_.localRotation;
			this.SetEnableItemObject(GoItemWaterFly.ItemObject.FLY, true);
			this.state_ = GoItemWaterFly.State.FLY;
			base.PlayFx(0, this.srcKartIndex_ == KartManager.PLAYER_KART_IDX || this.targetKartIndex_ == KartManager.PLAYER_KART_IDX);
		}
		else
		{
			this.state_ = GoItemWaterFly.State.DESTROY;
		}
	}

	protected override void ObjectSetting()
	{
		string[] array = new string[] { "fly", "explore" };
		foreach (object obj in base.transform)
		{
			Transform transform = (Transform)obj;
			int i;
			for (i = 0; i < 2; i++)
			{
				if (array[i] == transform.name)
				{
					this.itemObjects_[i] = transform.gameObject;
					break;
				}
			}
			if (i >= 2)
			{
				Debug.LogError("Invalid gameobject name in ITEM " + FiaUtil.AddSquareBracket(transform.name));
			}
		}
		this.applyItemParam_ = new ApplyItemParam(0);
		this.toPlayer_ = (MonoBehaviourMessage2Param<GameItem, ApplyItemParam>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.APPLY_ITEM);
	}

	private void SetEnableItemObject(GoItemWaterFly.ItemObject obj, bool isEnable)
	{
		this.itemObjects_[(int)obj].SetActiveRecursively(isEnable);
		if (isEnable)
		{
			this.itemObjects_[(int)obj].animation.Play();
		}
	}

	private bool IsEndOfAnimation(GoItemWaterFly.ItemObject obj)
	{
		GameObject gameObject = this.itemObjects_[(int)obj];
		return gameObject == null || !gameObject.active || gameObject.animation == null || !gameObject.animation.isPlaying;
	}

	private void Update()
	{
		this.tick_ += Time.deltaTime;
		if (this.state_ == GoItemWaterFly.State.FLY)
		{
			if (this.flyState_ == 0)
			{
				if (this.tick_ >= this.stayingTime_)
				{
					this.ctrlPtr[0] = base.transform.position;
					Vector3 vector = base.transform.forward;
					vector.y = 0f;
					vector.Normalize();
					this.ctrlPtr[1] = this.ctrlPtr[0] + vector * 200f;
					vector = this.dstKartTrans_.forward;
					vector.y = 0f;
					vector.Normalize();
					this.ctrlPtr[2] = this.dstKartTrans_.position + vector * 50f;
					this.ctrlPtr[3] = this.dstKartTrans_.position;
					this.tick_ -= this.stayingTime_;
					this.lifeTime_ -= this.stayingTime_;
					this.flyState_ = 1;
				}
				else
				{
					float num = Mathf.Clamp((this.stayingTime_ - this.tick_) * 2f, 0f, 1f);
					float num2 = (1f - num * num * num * num) * 2f;
					base.transform.position = this.srcKartTrans_.position + new Vector3(0f, num2, 0f);
				}
			}
			if (this.flyState_ == 1)
			{
				float num3 = this.tick_ / this.lifeTime_;
				Vector3 forward = this.dstKartTrans_.forward;
				forward.y = 0f;
				forward.Normalize();
				this.ctrlPtr[2] = this.dstKartTrans_.position + forward * 50f;
				this.ctrlPtr[3] = this.dstKartTrans_.position;
				Vector3 vector2 = MathHelper.BEZ3(num3 * num3, this.ctrlPtr[0], this.ctrlPtr[1], this.ctrlPtr[2], this.ctrlPtr[3]);
				float num4 = 5f;
				float num5 = -4f * num4 / this.lifeTime_ / this.lifeTime_ * this.tick_ * (this.tick_ - this.lifeTime_);
				base.transform.position = vector2 + new Vector3(0f, num5, 0f);
			}
			if (this.tick_ >= this.lifeTime_)
			{
				this.SetEnableItemObject(GoItemWaterFly.ItemObject.FLY, false);
				this.SetEnableItemObject(GoItemWaterFly.ItemObject.EXPLORE, true);
				base.transform.position = this.dstKartTrans_.position;
				this.applyItemParam_.senderId_ = this.srcKartIndex_;
				this.toPlayer_.Initialize(GameItem.WATER_FLY, this.applyItemParam_);
				MonoBehaviourExCenter.Instance.SendMessage(0, 24 + this.targetKartIndex_, this.toPlayer_);
				this.state_ = GoItemWaterFly.State.EXPLORE;
			}
		}
		else if (this.state_ == GoItemWaterFly.State.EXPLORE)
		{
			if (this.IsEndOfAnimation(GoItemWaterFly.ItemObject.EXPLORE))
			{
				this.SetEnableItemObject(GoItemWaterFly.ItemObject.EXPLORE, false);
				this.state_ = GoItemWaterFly.State.DESTROY;
			}
		}
		else if (this.state_ == GoItemWaterFly.State.DESTROY)
		{
			base.gameObject.SetActiveRecursively(false);
		}
	}

	private int srcKartIndex_;

	private int targetKartIndex_;

	private float lifeTime_ = 2f;

	private float stayingTime_ = 0.4f;

	private float tick_;

	private int flyState_;

	private Transform srcKartTrans_;

	private Transform dstKartTrans_;

	private Vector3[] ctrlPtr = new Vector3[4];

	private GoItemWaterFly.State state_;

	private GameObject[] itemObjects_ = new GameObject[2];

	private ApplyItemParam applyItemParam_;

	private MonoBehaviourMessage2Param<GameItem, ApplyItemParam> toPlayer_;

	private enum State
	{
		NONE,
		FLY,
		EXPLORE,
		DESTROY
	}

	private enum ItemObject
	{
		FLY,
		EXPLORE,
		SIZE
	}

	private enum FxType
	{
		FIRE
	}
}
