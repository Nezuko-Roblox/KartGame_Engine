using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class FiaUtil
{
	public static ArrayList CurrencyCharacterList
	{
		get
		{
			if (FiaUtil.currencyCharacterList_ == null)
			{
				JSONObject jsonobject = new JSONObject(((TextAsset)Resources.Load("currency")).text);
				if (jsonobject != null)
				{
					FiaUtil.currencyCharacterList_ = new ArrayList();
					foreach (object obj in jsonobject.list)
					{
						JSONObject jsonobject2 = (JSONObject)obj;
						ArrayList arrayList = new ArrayList();
						foreach (object obj2 in jsonobject2.list)
						{
							JSONObject jsonobject3 = (JSONObject)obj2;
							arrayList.Add(jsonobject3.str);
						}
						FiaUtil.currencyCharacterList_.Add(arrayList);
					}
				}
			}
			return FiaUtil.currencyCharacterList_;
		}
	}

	public static void LoadResourceFile(string filename, out LinkedList<string> stringList)
	{
		stringList = new LinkedList<string>();
		StringReader stringReader = new StringReader(((TextAsset)Resources.Load(filename)).text);
		if (stringReader != null)
		{
			string text = string.Empty;
			while ((text = stringReader.ReadLine()) != null)
			{
				if (text != string.Empty && text.Substring(0, 2) != "//")
				{
					stringList.AddLast(text);
				}
			}
			stringReader.Close();
		}
	}

	public static string docPath
	{
		get
		{
			foreach (RuntimePlatform runtimePlatform in FiaUtil.desktopPlatforms)
			{
				if (Application.platform == runtimePlatform)
				{
					return Path.Combine(Path.GetDirectoryName(Application.dataPath), "Documents");
				}
			}
			return Application.persistentDataPath;
		}
	}

	public static string recordPath
	{
		get
		{
			return Path.Combine(FiaUtil.docPath, "record");
		}
	}

	public static StreamWriter CreateStreamWriter(string path)
	{
		string directoryName = Path.GetDirectoryName(path);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		return new StreamWriter(path);
	}

	public static string AddSquareBracket(string str)
	{
		return " [ " + str + " ] ";
	}

	public static string AddSquareBracket(float t)
	{
		return " [ " + t.ToString() + " ] ";
	}

	public static string AddSquareBracket(bool t)
	{
		return " [ " + t.ToString() + " ] ";
	}

	public static void AttachChild(ref GameObject parent, ref GameObject child)
	{
		Transform transform = parent.transform;
		Transform transform2 = child.transform;
		FiaUtil.AttachChild(ref transform, ref transform2);
	}

	public static void AttachChild(ref Transform parent, ref Transform child)
	{
		Vector3 localPosition = child.localPosition;
		Quaternion localRotation = child.localRotation;
		Vector3 localScale = child.localScale;
		child.parent = parent;
		child.localPosition = localPosition;
		child.localRotation = localRotation;
		child.localScale = localScale;
	}

	public static void AttachKartNCharacter(ref Transform root, ref Transform kartBody, ref Transform character)
	{
		Transform[] array = new Transform[kartBody.transform.GetChildCount()];
		int num = 0;
		int num2 = -1;
		foreach (object obj in kartBody.transform)
		{
			Transform transform = (Transform)obj;
			array[num] = transform;
			if (transform.name == "seat")
			{
				num2 = num;
			}
			num++;
		}
		for (int i = 0; i < array.Length; i++)
		{
			FiaUtil.AttachChild(ref root, ref array[i]);
			if (i == num2)
			{
				FiaUtil.AttachChild(ref array[i], ref character);
			}
		}
		kartBody.DetachChildren();
	}

	public static void AttachKartNCharacterEx(ref Transform root, ref Transform kartBody, ref Transform character)
	{
		FiaUtil.AttachChild(ref root, ref kartBody);
		Transform transform = null;
		foreach (object obj in kartBody.transform)
		{
			Transform transform2 = (Transform)obj;
			if (transform2.name == "seat")
			{
				transform = transform2;
				break;
			}
		}
		FiaUtil.AttachChild(ref transform, ref character);
	}

	public static string GetModelingName(byte character)
	{
		List<string> list = new List<string>();
		CharacterAssetDefinitionManager.Instance.GetAssets((int)character, ref list);
		string text = string.Empty;
		foreach (string text2 in list)
		{
			if (!text2.Contains("_ani"))
			{
				text = text2;
				break;
			}
		}
		return text;
	}

	public static GameObject GenerateKart(byte kartIdx)
	{
		string mainAsset = KartAssetDefinitionManager.Instance.GetMainAsset((int)kartIdx);
		GameObject gameObjectMainAsset = ResourceLoader.Instance.GetGameObjectMainAsset(mainAsset, true);
		if (gameObjectMainAsset == null)
		{
			return null;
		}
		MeshRenderer[] componentsInChildren = gameObjectMainAsset.GetComponentsInChildren<MeshRenderer>();
		Material[] material = MaterialManager.Instance.GetMaterial(MaterialManager.MaterialPresetType.KART_TEXTURE);
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			if (meshRenderer.sharedMaterials.Length > 0 && meshRenderer.sharedMaterials[0] == null)
			{
				meshRenderer.sharedMaterials = material;
			}
		}
		return gameObjectMainAsset;
	}

	public static void GenerateBone(Transform[] boneTransform, string modelingName, string boneAssetName, out Transform[] bones)
	{
		string[] content = ((StringHolder)ResourceLoader.Instance.GetAsset(modelingName, boneAssetName)).content;
		string text = string.Empty;
		foreach (string text2 in content)
		{
			text = text + text2 + "\n";
		}
		List<Transform> list = new List<Transform>();
		foreach (string text3 in content)
		{
			foreach (Transform transform in boneTransform)
			{
				if (text3 == transform.name)
				{
					list.Add(transform);
				}
			}
		}
		bones = list.ToArray();
	}

	public static GameObject GenerateNoAniCharacter(byte characterIdx)
	{
		List<string> list = new List<string>();
		CharacterAssetDefinitionManager.Instance.GetAssets((int)characterIdx, ref list);
		string text = string.Empty;
		foreach (string text2 in list)
		{
			if (text2.Contains("_noani"))
			{
				text = text2;
				break;
			}
		}
		if (text == string.Empty)
		{
			return null;
		}
		GameObject gameObjectMainAsset = ResourceLoader.Instance.GetGameObjectMainAsset(text, true);
		if (gameObjectMainAsset != null)
		{
			MeshRenderer[] componentsInChildren = gameObjectMainAsset.GetComponentsInChildren<MeshRenderer>();
			Material[] material = MaterialManager.Instance.GetMaterial(MaterialManager.MaterialPresetType.KART_TEXTURE);
			foreach (MeshRenderer meshRenderer in componentsInChildren)
			{
				meshRenderer.sharedMaterials = material;
			}
			return gameObjectMainAsset;
		}
		return null;
	}

	public static GameObject GenerateCharacter(byte characterIdx)
	{
		List<string> list = new List<string>();
		CharacterAssetDefinitionManager.Instance.GetAssets((int)characterIdx, ref list);
		string text = string.Empty;
		string text2 = string.Empty;
		foreach (string text3 in list)
		{
			if (text3.Contains("_ani"))
			{
				text = text3;
			}
			else if (!text3.Contains("_noani"))
			{
				text2 = text3;
			}
		}
		GameObject gameObject = (GameObject)ResourceLoader.Instance.GetMainAsset(text);
		GameObject gameObject2 = null;
		if (gameObject != null)
		{
			gameObject2 = (GameObject)global::UnityEngine.Object.Instantiate(gameObject);
		}
		Transform[] componentsInChildren = gameObject2.GetComponentsInChildren<Transform>();
		Transform[] array = null;
		Transform[] array2 = null;
		FiaUtil.GenerateBone(componentsInChildren, text2, "body_bonenames", out array);
		FiaUtil.GenerateBone(componentsInChildren, text2, "face_bonenames", out array2);
		string[] array3 = new string[] { "body", "face" };
		Transform[][] array4 = new Transform[][] { array, array2 };
		int num = array3.Length;
		for (int i = 0; i < num; i++)
		{
			GameObject gameObjectAsset = ResourceLoader.Instance.GetGameObjectAsset(text2, array3[i], true);
			if (gameObjectAsset != null)
			{
				SkinnedMeshRenderer component = gameObjectAsset.GetComponent<SkinnedMeshRenderer>();
				if (component != null)
				{
					component.bones = array4[i];
					component.updateWhenOffscreen = true;
				}
				FiaUtil.AttachChild(ref gameObject2, ref gameObjectAsset);
			}
		}
		return gameObject2;
	}

	public static GameObject GetChildGameObject(GameObject obj, string childname)
	{
		foreach (object obj2 in obj.transform)
		{
			Transform transform = (Transform)obj2;
			if (transform.name == childname)
			{
				return transform.gameObject;
			}
		}
		return null;
	}

	public static string GetFileTitle(string filename)
	{
		if (filename == null || filename == string.Empty)
		{
			return string.Empty;
		}
		char[] array = new char[] { '\\', '/', '.' };
		string[] array2 = filename.Split(array, StringSplitOptions.RemoveEmptyEntries);
		if (array2.Length == 1)
		{
			return array2[0];
		}
		return array2[array2.Length - 2];
	}

	public static string GetFileName(string filename)
	{
		if (filename == null || filename == string.Empty)
		{
			return string.Empty;
		}
		char[] array = new char[] { '\\', '/' };
		string[] array2 = filename.Split(array, StringSplitOptions.RemoveEmptyEntries);
		return array2[array2.Length - 1];
	}

	public static void CreateFolderIfNotExist(string folder)
	{
		string docPath = FiaUtil.docPath;
		if (!Directory.Exists(FiaUtil.docPath))
		{
			Directory.CreateDirectory(FiaUtil.docPath);
		}
		string text = Path.Combine(docPath, folder);
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
	}

	public static void GetRandomList(int start, int end, out List<int> randomList)
	{
		randomList = new List<int>(end - start + 1);
		for (int i = start; i <= end; i++)
		{
			if (i == start)
			{
				randomList.Add(i);
			}
			else
			{
				randomList.Insert(global::UnityEngine.Random.Range(0, i - start + 1), i);
			}
		}
	}

	public static float GetRandom(float t1, float t2)
	{
		if (t1 == t2)
		{
			return t1;
		}
		return global::UnityEngine.Random.Range(Mathf.Min(t1, t2), Mathf.Max(t1, t2));
	}

	public static string GenerateUserName(string name)
	{
		string text = string.Empty;
		for (int i = 0; i < name.Length; i++)
		{
			if ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.:,(*!?')/;- ".IndexOf(name[i]) < 0)
			{
				text += '?';
			}
			else
			{
				text += name[i];
			}
		}
		if (text.Length > 10)
		{
			text = text.Substring(0, 9);
			text += "=";
		}
		return text;
	}

	public static string GenerateUserNameEx(string strNew, string strOld)
	{
		if (strNew == strOld)
		{
			return strOld;
		}
		if (strNew.Length > 10)
		{
			strNew = strNew.Substring(0, 10);
		}
		string text = string.Empty;
		for (int i = 0; i < strNew.Length; i++)
		{
			if ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789- ".IndexOf(strNew[i]) >= 0)
			{
				text += strNew[i];
			}
		}
		return text;
	}

	public static ushort ReadUShort(byte[] input, uint offset)
	{
		return (ushort)(((int)input[(int)((UIntPtr)(offset + 1U))] << 8) | (int)input[(int)((UIntPtr)offset)]);
	}

	public static uint Checksum8Update(byte[] input, uint length)
	{
		uint num = 0U;
		uint num2 = 0U;
		while (length >> 17 > 0U)
		{
			length -= 131072U;
			for (uint num3 = 65536U; num3 > 0U; num3 -= 1U)
			{
				num += (uint)FiaUtil.ReadUShort(input, num2);
				num2 += 2U;
			}
			num = (num >> 16) + (num & 65535U);
			num = (uint)((ushort)(num + (num >> 16)));
		}
		for (uint num3 = length >> 1; num3 > 0U; num3 -= 1U)
		{
			num += (uint)FiaUtil.ReadUShort(input, num2);
			num2 += 2U;
		}
		if ((length & 1U) != 0U)
		{
			num += (uint)input[(int)((UIntPtr)num2)];
		}
		num = (num >> 16) + (num & 65535U);
		num += num >> 16;
		return (uint)((ushort)num);
	}

	public static byte CalcChecksum8(byte[] data, uint dataLength)
	{
		uint num = FiaUtil.Checksum8Update(data, dataLength);
		num = (num >> 8) + (num & 255U);
		num += num >> 8;
		return (byte)(~(byte)num);
	}

	private static ArrayList currencyCharacterList_ = null;

	private static RuntimePlatform[] desktopPlatforms = new RuntimePlatform[]
	{
		RuntimePlatform.OSXEditor,
		RuntimePlatform.OSXPlayer,
		RuntimePlatform.WindowsEditor,
		RuntimePlatform.WindowsPlayer
	};
}
