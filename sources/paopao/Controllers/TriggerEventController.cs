using System;
using UnityEngine;

public class TriggerEventController : MonoBehaviour
{
	public void RegisterItemController(ItemBasicController itemController)
	{
		this.itemController_ = itemController;
	}

	private void OnTriggerEnter(Collider hit)
	{
		if (this.itemController_ != null)
		{
			this.itemController_.OnUserDefinedTriggerEnter(hit);
		}
	}

	private ItemBasicController itemController_;
}
