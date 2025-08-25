using System;
using UnityEngine;

public class GUIAtlas
{
	public void Generate(string[] names, Texture2D[] textures, Shader shader)
	{
		Texture2D texture2D = new Texture2D(1024, 1024);
		int num = names.Length;
		this.names_ = new string[num];
		Array.Copy(names, this.names_, num);
		this.orgRect_ = new Rect[num];
		for (int i = 0; i < num; i++)
		{
			this.orgRect_[i] = new Rect(0f, 0f, (float)textures[i].width, (float)textures[i].height);
		}
		this.uvs_ = texture2D.PackTextures(textures, 0);
		this.material_ = new Material(shader);
		texture2D.wrapMode = TextureWrapMode.Clamp;
		texture2D.filterMode = FilterMode.Point;
		this.material_.mainTexture = texture2D;
	}

	public override string ToString()
	{
		string text = string.Empty;
		text = text + this.names_.Length.ToString() + "\n";
		for (int i = 0; i < this.names_.Length; i++)
		{
			string text2 = text;
			text = string.Concat(new string[]
			{
				text2,
				this.names_[i],
				" ",
				this.uvs_[i].ToString(),
				"\n"
			});
		}
		return text;
	}

	public int GetIdx(string name)
	{
		return Array.IndexOf<string>(this.names_, name);
	}

	public Material material_;

	public string[] names_;

	public Rect[] uvs_;

	public Rect[] orgRect_;
}
