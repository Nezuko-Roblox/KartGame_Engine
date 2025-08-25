using System;
using UnityEngine;

public class GarageStage : BaseWaitRoomStage
{
	protected override void Start()
	{
		base.Start();
		this.RegistMonoBehaviour(517);
	}

	public override void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
	{
		if (msg.type_ == MonoBehaviourMessageType.CHANGE_SCENE)
		{
			base.ReadyToChangeScene();
		}
	}

	protected override void SetReady(bool _ready)
	{
	}

	protected override void Ready()
	{
	}

	protected override void LoadResources(GameParamPacket p)
	{
		if (Debug.isDebugBuild)
		{
			Debug.Log("Ignore load resouces.");
		}
	}

	protected override void UpdateStatus(GameParamPacket p)
	{
		if (Debug.isDebugBuild)
		{
			Debug.Log("Ignore update status");
		}
	}
}
