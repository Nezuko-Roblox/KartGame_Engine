using System;

public class GUIBoosterGauge : MonoBehaviourEx
{
	private void Awake()
	{
		this.RegistMonoBehaviour(15);
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.SHOW_UI)
		{
			MonoBehaviourMessage1Param<bool> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<bool>)msg;
			if (monoBehaviourMessage1Param != null)
			{
				base.gameObject.SetActiveRecursively(monoBehaviourMessage1Param.param_);
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.PAUSE || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL2)
		{
			base.gameObject.SetActiveRecursively(false);
		}
		else if (msg.type_ == MonoBehaviourMessageType.RESUME)
		{
			base.gameObject.SetActiveRecursively(true);
		}
	}
}
