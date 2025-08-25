using System;

public class MainMenuStage : MonoBehaviourStage
{
	protected override void Start()
	{
		base.Start();
		this.RegistMonoBehaviour(516);
	}

	public override void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
	{
		if (msg.type_ == MonoBehaviourMessageType.CHANGE_SCENE)
		{
			base.ReadyToChangeScene();
		}
	}
}
