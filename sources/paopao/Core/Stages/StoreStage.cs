using System;
using UnityEngine;

public class StoreStage : MonoBehaviourStage
{
	protected override void Start()
	{
		base.Start();
		if (PlayerPrefs.GetInt("RESTORED_PURCHASE", 0) == 0)
		{
			PlayerPrefs.SetInt("RESTORED_PURCHASE", 1);
			MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.RESTORE_PURCHASES_POPUP_MESSAGE);
			MonoBehaviourExCenter.Instance.SendMessage(0, 271, monoBehaviourMessage1Param.Initialize(1));
		}
	}

	public override void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
	{
		if (msg.type_ == MonoBehaviourMessageType.CHANGE_SCENE)
		{
			base.ReadyToChangeScene();
		}
	}
}
