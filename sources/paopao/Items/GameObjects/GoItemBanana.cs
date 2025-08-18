using System;
using UnityEngine;

public class GoItemBanana : ItemBasicController
{
	public override void Initialize(ItemParam param)
	{
		ItemBananaParam itemBananaParam = (ItemBananaParam)param;
		this.state_ = GoItemBanana.State.DESTROY;
		this.ignoreTime_ = 2f;
		if (itemBananaParam != null)
		{
			base.Initialize(param);
			this.srcKartIndex_ = itemBananaParam.srcKartIndex_;
			GoKart goKart = KartManager.Instance.goKart_[this.srcKartIndex_];
			GameObject kart = goKart.m_kart;
			GameObject kartBodyObject = goKart.controller_.KartBodyObject;
			if (kartBodyObject.collider != null)
			{
				BoxCollider boxCollider = (BoxCollider)kartBodyObject.collider;
				Vector3 vector = itemBananaParam.pos_ - kart.transform.forward * boxCollider.size.z * 0.5f;
				vector.y += 0.1f;
				RaycastHit raycastHit;
				if (Physics.Raycast(vector, -Vector3.up, out raycastHit, 500f, 256))
				{
					base.transform.localPosition = raycastHit.point;
					TriggerEventController[] componentsInChildren = base.GetComponentsInChildren<TriggerEventController>();
					foreach (TriggerEventController triggerEventController in componentsInChildren)
					{
						triggerEventController.RegisterItemController(this);
					}
					this.state_ = GoItemBanana.State.IGNORE;
				}
			}
			else
			{
				Debug.LogError("no kart collider");
			}
			base.PlayFx(0, this.srcKartIndex_ == KartManager.PLAYER_KART_IDX);
		}
		else
		{
			Debug.LogError("derived param is null");
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

	private void SetEnableItemObject(GoItemBanana.ItemObject obj, bool isEnable)
	{
		this.itemObjects_[(int)obj].SetActiveRecursively(isEnable);
		if (isEnable && this.itemObjects_[(int)obj].animation != null)
		{
			this.itemObjects_[(int)obj].animation.Play();
		}
	}

	private void Update()
	{
		if (this.state_ == GoItemBanana.State.IGNORE)
		{
			this.ignoreTime_ -= Time.deltaTime;
			if (this.ignoreTime_ <= 0f)
			{
				this.state_ = GoItemBanana.State.IDLE;
			}
		}
		else if (this.state_ == GoItemBanana.State.DESTROY)
		{
			base.gameObject.SetActiveRecursively(false);
		}
	}

	public override void OnUserDefinedTriggerEnter(Collider other)
	{
		if (other.name != "kart_body")
		{
			return;
		}
		int num = -1;
		GameObject gameObject = other.transform.parent.gameObject;
		GoKart goKart = null;
		for (int i = 0; i < 6; i++)
		{
			if (KartManager.Instance.goKart_[i] != null && KartManager.Instance.goKart_[i].m_kart == gameObject)
			{
				num = i;
				goKart = KartManager.Instance.goKart_[i];
				break;
			}
		}
		if (goKart.GetType() == typeof(GoNetKart))
		{
			return;
		}
		if (num == -1)
		{
			return;
		}
		if (this.state_ == GoItemBanana.State.IGNORE && this.srcKartIndex_ == num)
		{
			return;
		}
		if (this.state_ == GoItemBanana.State.DESTROY)
		{
			return;
		}
		this.applyItemParam_.senderId_ = this.srcKartIndex_;
		this.toPlayer_.Initialize(GameItem.BANANA, this.applyItemParam_);
		MonoBehaviourExCenter.Instance.SendMessage(0, (num != KartManager.PLAYER_KART_IDX) ? (24 + num) : MonoBehaiourExConst.PLAYER_KART, this.toPlayer_);
		if (KartManager.Instance.parameter_.Stage == StageType.GAME_WIFI && num == KartManager.PLAYER_KART_IDX)
		{
			NetworkManager.Inst.SendPacketToAll(new DestroyBananaPacket((float)this.id_), SendDataMode.RELIABLE);
		}
		this.state_ = GoItemBanana.State.DESTROY;
	}

	private const float IGNORE_TIME = 2f;

	private int srcKartIndex_;

	private float ignoreTime_ = 2f;

	public GoItemBanana.State state_;

	private GameObject[] itemObjects_ = new GameObject[1];

	private ApplyItemParam applyItemParam_;

	private MonoBehaviourMessage2Param<GameItem, ApplyItemParam> toPlayer_;

	public enum State
	{
		IDLE,
		IGNORE,
		DESTROY
	}

	private enum FxType
	{
		FIRE,
		TRAPPED
	}

	private enum ItemObject
	{
		FIRE,
		SIZE
	}
}
