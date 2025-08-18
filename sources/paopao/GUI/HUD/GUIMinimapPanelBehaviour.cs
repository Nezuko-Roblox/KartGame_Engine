using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUIMinimapPanelBehaviour : MonoBehaviourEx
{
	private void Awake()
	{
		this.RegistMonoBehaviour(17);
	}

	private void Start()
	{
		this.guiManager_ = new GUIPanelManager();
		this.guiManager_.SetCamera(CameraManager.Instance.guiCam_);
		this.panel_ = new GUIMinimapPanel(new float[] { 576f, 391f, 768f, 199f });
		this.guiManager_.RegistGUIInterface(this.panel_);
		base.gameObject.layer = LayerMask.NameToLayer("Gui");
		this.material_ = new Material(ShaderHelper.noTextureShader_);
		MeshRenderer component = base.GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.material = this.material_;
		}
		this.mesh_ = base.GetComponent<MeshFilter>().mesh;
		this.guiManager_.Update();
		this.guiManager_.UpdateMesh(ref this.mesh_);
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

	private Material material_;

	private GUIPanelManager guiManager_;

	private GUIMinimapPanel panel_;

	private Mesh mesh_;
}
