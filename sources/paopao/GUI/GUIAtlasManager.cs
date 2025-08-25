using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIAtlasManager
{
	public static GUIAtlas CreateAtlas(string name, string[] keys, Texture2D[] textures, Shader shader)
	{
		if (GUIAtlasManager.atlas_.ContainsKey(name))
		{
			return GUIAtlasManager.atlas_[name];
		}
		GUIAtlas guiatlas = new GUIAtlas();
		guiatlas.Generate(keys, textures, shader);
		GUIAtlasManager.atlas_.Add(name, guiatlas);
		return guiatlas;
	}

	public static GUIAtlas GetAtlas(string name)
	{
		if (!GUIAtlasManager.atlas_.ContainsKey(name))
		{
			return null;
		}
		return GUIAtlasManager.atlas_[name];
	}

	public static void RemoveAtlas(string name)
	{
		GUIAtlasManager.atlas_.Remove(name);
	}

	public static void PrintDebug()
	{
		string text = string.Empty;
		foreach (KeyValuePair<string, GUIAtlas> keyValuePair in GUIAtlasManager.atlas_)
		{
			string text2 = text;
			text = string.Concat(new string[]
			{
				text2,
				keyValuePair.Key.ToString(),
				" ",
				keyValuePair.Value.ToString(),
				"\n"
			});
		}
	}

	public static int GetAtlasCount()
	{
		return GUIAtlasManager.atlas_.Count;
	}

	private static Dictionary<string, GUIAtlas> atlas_ = new Dictionary<string, GUIAtlas>();
}
