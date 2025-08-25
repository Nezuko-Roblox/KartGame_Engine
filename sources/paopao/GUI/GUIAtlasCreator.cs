using System;
using UnityEngine;

public class GUIAtlasCreator : MonoBehaviourEx
{
	private void Awake()
	{
		this.RegistMonoBehaviour(256 + GUIAtlasManager.GetAtlasCount());
		GUIAtlasManager.CreateAtlas(this.atlasName_, this.keys_, this.textures_, Shader.Find("fontshader"));
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.CHANGE_SCENE)
		{
			GUIAtlasManager.RemoveAtlas(this.atlasName_);
		}
	}

	public string[] keys_;

	public Texture2D[] textures_;

	public string atlasName_ = "trackList";
}
