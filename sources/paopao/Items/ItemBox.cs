using System;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
	private void Awake()
	{
		base.gameObject.layer = LayerMask.NameToLayer("Item");
		this.meshRenderer_ = base.GetComponent<MeshRenderer>();
		if (base.transform.GetChildCount() > 0)
		{
			Transform child = base.transform.GetChild(0);
			if (child != null)
			{
				this.effect_ = child.gameObject;
			}
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!this.meshRenderer_.enabled)
		{
			return;
		}
		int kartIndex = KartManager.Instance.GetKartIndex(other.transform.parent.gameObject);
		if (!KartManager.Instance.IsValidKart(kartIndex))
		{
			return;
		}
		GameItem gameItem = GameItemManager.Instance.GenerateItem(kartIndex);
		if (gameItem == GameItem.NONE)
		{
			return;
		}
		MonoBehaviourExCenter.Instance.SendMessage(0, 24 + kartIndex, ((MonoBehaviourMessage1Param<GameItem>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.GET_ITEM)).Initialize(gameItem));
		this.meshRenderer_.enabled = false;
		base.Invoke("ItemRegen", 3f);
		if (this.effect_ != null)
		{
			this.effect_.particleEmitter.Emit();
		}
	}

	private void ItemRegen()
	{
		this.meshRenderer_.enabled = true;
	}

	private bool isActive_;

	private MeshRenderer meshRenderer_;

	private GameObject effect_;
}
