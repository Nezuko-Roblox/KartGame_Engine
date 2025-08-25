using System;
using UnityEngine;

public class MaterialManager
{
	public static MaterialManager Instance
	{
		get
		{
			if (MaterialManager.instance_ == null)
			{
				MaterialManager.instance_ = new MaterialManager();
			}
			return MaterialManager.instance_;
		}
	}

	public void Clear()
	{
		if (this.material_ != null)
		{
			foreach (Material[] array2 in this.material_)
			{
				if (array2 != null)
				{
					foreach (Material material in array2)
					{
						global::UnityEngine.Object.Destroy(material);
					}
				}
			}
		}
		this.material_ = null;
	}

	public void Initialize()
	{
		this.Clear();
		this.material_ = new Material[1][];
		for (int i = 0; i < 1; i++)
		{
			Texture2D texture2D = (Texture2D)Resources.Load("textures/" + this.MATERIAL_PRESET_INFO[i * 2], typeof(Texture2D));
			if (texture2D != null)
			{
				Shader shader = Shader.Find(this.MATERIAL_PRESET_INFO[i * 2 + 1]);
				if (shader != null)
				{
					this.material_[i] = new Material[1];
					this.material_[i][0] = new Material(shader);
					this.material_[i][0].mainTexture = texture2D;
					global::UnityEngine.Object.DontDestroyOnLoad(this.material_[i][0]);
					global::UnityEngine.Object.DontDestroyOnLoad(texture2D);
				}
				else if (Debug.isDebugBuild)
				{
					Debug.LogError("no shader : " + this.MATERIAL_PRESET_INFO[i * 2 + 1]);
				}
			}
			else if (Debug.isDebugBuild)
			{
				Debug.LogError("no texture resource : " + this.MATERIAL_PRESET_INFO[i * 2]);
			}
		}
	}

	public Material[] GetMaterial(MaterialManager.MaterialPresetType preset)
	{
		return this.material_[(int)preset];
	}

	public static MaterialManager instance_;

	public string[] MATERIAL_PRESET_INFO = new string[] { "kart_texture", "Default" };

	private Material[][] material_;

	public enum MaterialPresetType
	{
		KART_TEXTURE,
		SIZE
	}
}
