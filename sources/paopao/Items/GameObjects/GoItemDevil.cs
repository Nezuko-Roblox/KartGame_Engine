using System;
using UnityEngine;

public class GoItemDevil : ItemBasicController
{
	public override void Initialize(ItemParam param)
	{
		ItemDevilParam itemDevilParam = (ItemDevilParam)param;
		if (itemDevilParam != null)
		{
			base.Initialize(param);
			this.srcKartIndex_ = itemDevilParam.srcKartIndex_;
			Array.Copy(itemDevilParam.dstKartIndex_, this.targetKartIndex_, itemDevilParam.dstKartIndex_.Length);
			this.kartTrans_ = KartManager.Instance.goKart_[this.srcKartIndex_].controller_.KartBodyTransform;
			for (int i = 0; i < 3; i++)
			{
				this.SetEnableItemObject((GoItemDevil.ItemObject)i, false);
			}
			this.state_ = GoItemDevil.State.USE;
			Vector3 position = this.kartTrans_.position;
			Vector3 vector = new Vector3(position.x, position.y + 1.8f, position.z);
			base.transform.position = vector;
			base.transform.localRotation = this.kartTrans_.rotation;
			this.SetEnableItemObject(GoItemDevil.ItemObject.USE, true);
			base.PlayFx(0, this.srcKartIndex_ == KartManager.PLAYER_KART_IDX);
		}
		else
		{
			this.state_ = GoItemDevil.State.DESTROY;
		}
	}

	private void SetEnableItemObject(GoItemDevil.ItemObject obj, bool isEnable)
	{
		this.itemObjects_[(int)obj].SetActiveRecursively(isEnable);
		if (isEnable && this.itemObjects_[(int)obj].animation != null)
		{
			this.itemObjects_[(int)obj].animation.Play();
		}
	}

	protected override void ObjectSetting()
	{
		string[] array = new string[] { "use", "start", "loop" };
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

	private void FixedUpdate()
	{
		if (this.state_ == GoItemDevil.State.USE)
		{
			Vector3 position = this.kartTrans_.position;
			Vector3 vector = new Vector3(position.x, position.y + 1.8f, position.z);
			base.transform.position = vector;
			base.transform.localRotation = this.kartTrans_.rotation;
		}
		else if (this.state_ != GoItemDevil.State.DESTROY)
		{
			base.transform.position = this.kartTrans_.position;
		}
	}

	private void Update()
	{
		if (this.state_ == GoItemDevil.State.USE)
		{
			if (!this.itemObjects_[0].animation.isPlaying)
			{
				this.SetEnableItemObject(GoItemDevil.ItemObject.USE, false);
				this.applyItemParam_.senderId_ = this.srcKartIndex_;
				for (int i = 0; i < 6; i++)
				{
					if (this.targetKartIndex_[i])
					{
						this.toPlayer_.Initialize(GameItem.DEVIL, this.applyItemParam_);
						MonoBehaviourExCenter.Instance.SendMessage(0, 24 + i, this.toPlayer_);
					}
				}
				this.state_ = GoItemDevil.State.DESTROY;
			}
		}
		else if (this.state_ == GoItemDevil.State.DESTROY)
		{
			base.gameObject.SetActiveRecursively(false);
		}
	}

	public const float DEVIL_DURATION = 5f;

	public const float DEVIL_READY_TIME = 1f;

	private int srcKartIndex_;

	private bool[] targetKartIndex_ = new bool[6];

	private GoItemDevil.State state_;

	private GameObject[] itemObjects_ = new GameObject[3];

	private Transform kartTrans_;

	private ApplyItemParam applyItemParam_;

	private MonoBehaviourMessage2Param<GameItem, ApplyItemParam> toPlayer_;

	private enum State
	{
		USE,
		START,
		LOOP,
		DESTROY
	}

	private enum ItemObject
	{
		USE,
		START,
		LOOP,
		SIZE
	}

	private enum FxType
	{
		USE,
		START,
		LOOP
	}
}
