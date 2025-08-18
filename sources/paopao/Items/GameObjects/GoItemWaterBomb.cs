using System;
using UnityEngine;

public class GoItemWaterBomb : ItemBasicController
{
	public override void Initialize(ItemParam param)
	{
		ItemWaterBombParam itemWaterBombParam = (ItemWaterBombParam)param;
		if (itemWaterBombParam != null)
		{
			base.Initialize(param);
			this.srcKartIndex_ = itemWaterBombParam.srcKartIndex_;
			this.targetPlane_ = -1;
			this.targetPosition_ = KartManager.Instance.goCourse_.GetCourseFromKart(this.srcKartIndex_, this.fireDistance_, out this.targetPlane_);
			TriggerEventController[] componentsInChildren = base.GetComponentsInChildren<TriggerEventController>();
			foreach (TriggerEventController triggerEventController in componentsInChildren)
			{
				triggerEventController.RegisterItemController(this);
			}
			for (int j = 0; j < 3; j++)
			{
				this.SetEnableItemObject((GoItemWaterBomb.ItemObject)j, false);
			}
			this.kartBodyTransform_ = KartManager.Instance.goKart_[this.srcKartIndex_].controller_.KartBodyTransform;
			base.transform.position = this.kartBodyTransform_.position;
			base.transform.localRotation = this.kartBodyTransform_.rotation;
			this.SetEnableItemObject(GoItemWaterBomb.ItemObject.FIRE, true);
			this.state_ = GoItemWaterBomb.State.FIRE;
			int latestPassingPlane = KartManager.Instance.goCourse_.GetLatestPassingPlane();
			if (this.srcKartIndex_ == KartManager.PLAYER_KART_IDX || MathHelper.IsBetweenII(this.targetPlane_ - latestPassingPlane, -1, 3))
			{
				base.PlayFx(0, true);
			}
		}
		else
		{
			this.state_ = GoItemWaterBomb.State.DESTORY;
		}
	}

	protected override void ObjectSetting()
	{
		string[] array = new string[] { "fire", "fall", "explore" };
		foreach (object obj in base.transform)
		{
			Transform transform = (Transform)obj;
			int i;
			for (i = 0; i < 3; i++)
			{
				if (array[i] == transform.name)
				{
					this.itemObjects_[i] = transform.gameObject;
					break;
				}
			}
			if (i >= 3)
			{
				Debug.LogError("Invalid gameobject name in ITEM " + FiaUtil.AddSquareBracket(transform.name));
			}
		}
		this.applyItemParam_ = new ApplyItemParam(0);
		this.toPlayer_ = (MonoBehaviourMessage2Param<GameItem, ApplyItemParam>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.APPLY_ITEM);
	}

	private void SetEnableItemObject(GoItemWaterBomb.ItemObject obj, bool isEnable)
	{
		this.itemObjects_[(int)obj].SetActiveRecursively(isEnable);
		if (isEnable)
		{
			this.itemObjects_[(int)obj].animation.Play();
		}
	}

	private void FixedUpdate()
	{
		if (this.state_ == GoItemWaterBomb.State.FIRE && this.kartBodyTransform_ != null)
		{
			base.transform.position = this.kartBodyTransform_.position;
			base.transform.localRotation = this.kartBodyTransform_.rotation;
		}
	}

	private void Update()
	{
		switch (this.state_)
		{
		case GoItemWaterBomb.State.FIRE:
			if (!this.itemObjects_[0].animation.isPlaying)
			{
				this.SetEnableItemObject(GoItemWaterBomb.ItemObject.FIRE, false);
				this.SetEnableItemObject(GoItemWaterBomb.ItemObject.FALL, true);
				this.state_ = GoItemWaterBomb.State.FALL;
				base.transform.position = this.targetPosition_;
				base.transform.localRotation = Quaternion.identity;
			}
			break;
		case GoItemWaterBomb.State.FALL:
			if (!this.itemObjects_[1].animation.isPlaying)
			{
				this.SetEnableItemObject(GoItemWaterBomb.ItemObject.FALL, false);
				this.SetEnableItemObject(GoItemWaterBomb.ItemObject.EXPLORE, true);
				this.state_ = GoItemWaterBomb.State.EXPLORE;
				int latestPassingPlane = KartManager.Instance.goCourse_.GetLatestPassingPlane();
				if (MathHelper.IsBetweenII(this.targetPlane_ - latestPassingPlane, -1, 3))
				{
					base.PlayFx(1, true);
				}
			}
			break;
		case GoItemWaterBomb.State.EXPLORE:
			if (!this.itemObjects_[2].animation.isPlaying)
			{
				this.SetEnableItemObject(GoItemWaterBomb.ItemObject.EXPLORE, false);
				this.state_ = GoItemWaterBomb.State.DESTORY;
			}
			break;
		case GoItemWaterBomb.State.DESTORY:
			base.gameObject.SetActiveRecursively(false);
			break;
		}
	}

	public override void OnUserDefinedTriggerEnter(Collider hit)
	{
		if (this.state_ != GoItemWaterBomb.State.EXPLORE)
		{
			return;
		}
		if (hit.name != "kart_body")
		{
			return;
		}
		GameObject gameObject = hit.transform.parent.gameObject;
		int num = 1 << gameObject.layer;
		if ((num & 24576) != 0)
		{
			int kartIndex = KartManager.Instance.GetKartIndex(gameObject);
			if (KartManager.Instance.IsValidKart(kartIndex))
			{
				this.applyItemParam_.senderId_ = this.srcKartIndex_;
				this.toPlayer_.Initialize(GameItem.WATER_BOMB, this.applyItemParam_);
				MonoBehaviourExCenter.Instance.SendMessage(0, 24 + kartIndex, this.toPlayer_);
			}
		}
	}

	private int srcKartIndex_;

	private float fireDistance_ = 200f;

	private GoItemWaterBomb.State state_;

	private GameObject[] itemObjects_ = new GameObject[3];

	private Vector3 targetPosition_;

	private Quaternion targetRotation_;

	private Transform kartBodyTransform_;

	private ApplyItemParam applyItemParam_;

	private MonoBehaviourMessage2Param<GameItem, ApplyItemParam> toPlayer_;

	private int targetPlane_ = -1;

	private enum State
	{
		NONE,
		FIRE,
		FALL,
		EXPLORE,
		DESTORY
	}

	private enum ItemObject
	{
		FIRE,
		FALL,
		EXPLORE,
		SIZE
	}

	private enum FxType
	{
		FIRE,
		EXPLORE
	}
}
