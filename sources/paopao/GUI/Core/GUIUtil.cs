using System;
using UnityEngine;

public class GUIUtil
{
	public static void Localize(Material[] mats, int i)
	{
		string text = string.Format("i18n/{0}/textures/{1}", iOSUtil.Locale, mats[i].mainTexture.name);
		Texture2D texture2D = (Texture2D)Resources.Load(text);
		if (texture2D != null)
		{
			mats[i].mainTexture = texture2D;
		}
	}

	public static void Localize(Material mat)
	{
		string text = string.Format("i18n/{0}/textures/{1}", iOSUtil.Locale, mat.mainTexture.name);
		Texture2D texture2D = (Texture2D)Resources.Load(text);
		if (texture2D != null)
		{
			mat.mainTexture = texture2D;
		}
	}

	public static void Localize(GUITexture tex)
	{
		Texture texture = GUIUtil.LocalizeTexture(tex.texture);
		if (texture != null)
		{
			tex.texture = GUIUtil.LocalizeTexture(tex.texture);
		}
	}

	public static Texture2D LocalizeTexture(Texture2D tex)
	{
		string text = string.Format("i18n/{0}/textures/{1}", iOSUtil.Locale, tex.name);
		return (Texture2D)Resources.Load(text);
	}

	public static Texture LocalizeTexture(Texture tex)
	{
		string text = string.Format("i18n/{0}/textures/{1}", iOSUtil.Locale, tex.name);
		return (Texture)Resources.Load(text);
	}

	public static Texture2D LoadLocalizedTexture(string name)
	{
		string text = string.Format("i18n/{0}/textures/{1}", iOSUtil.Locale, name);
		return (Texture2D)Resources.Load(text);
	}

	public static Texture2D LoadQuestTexture(int type, int id)
	{
		string text = null;
		switch (type)
		{
		case 0:
			text = "c";
			break;
		case 1:
			text = "k";
			break;
		case 2:
			text = "t";
			break;
		}
		string text2 = string.Format("quest_info_{0}_{1}", text, id);
		return (Texture2D)Resources.Load(string.Format("i18n/{0}/textures/{1}", iOSUtil.Locale, text2));
	}
}
