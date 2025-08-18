using System;

public class MonoBehaviourStage : MonoBehaviourEx
{
	protected virtual void Awake()
	{
	}

	protected virtual void Start()
	{
	}

	public override void RegistMonoBehaviour(int id)
	{
		base.RegistMonoBehaviour(id);
		StageController.Instance.RegistMonoBehaviour(id, this);
	}

	public void ReadyToChangeScene()
	{
		StageController.Instance.ReadyToChangeScene(this.id_);
	}

	protected AudioSourceEx bgm_;
}
