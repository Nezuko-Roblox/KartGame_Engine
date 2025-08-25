using System;

public class InfoStage : MonoBehaviourStage
{
	protected override void Start()
	{
		base.Start();
		this.RegistMonoBehaviour(518);
	}

	public override void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
	{
		if (msg.type_ == MonoBehaviourMessageType.CHANGE_SCENE)
		{
			base.ReadyToChangeScene();
		}
	}
}
