using System;
using UnityEngine;

public class GUIController : MonoBehaviour
{
	private void Awake()
	{
		if (this.keys_.Length == this.textures_.Length)
		{
			GUIAtlasManager.CreateAtlas("ingame", this.keys_, this.textures_, this.shader_);
		}
	}

	private void Start()
	{
		GUIAtlas atlas = GUIAtlasManager.GetAtlas("ingame");
		if (!Debug.isDebugBuild || atlas != null)
		{
		}
	}

	private void Update()
	{
	}

	public string[] keys_;

	public Texture2D[] textures_;

	public Shader shader_;
}
