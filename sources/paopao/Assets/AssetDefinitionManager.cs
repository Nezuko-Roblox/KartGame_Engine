using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AssetDefinitionManager
{
	public AssetDefinitionManager()
	{
		this.assets_ = new List<AssetDefinition>();
	}

	public virtual AssetDefinition CreateAssetDefinition()
	{
		return null;
	}

	public void Initialize(TextAsset assetDefinition)
	{
		if (this.isInitialized_)
		{
			return;
		}
		if (assetDefinition != null)
		{
			StringReader stringReader = new StringReader(assetDefinition.text);
			if (stringReader != null)
			{
				XMLElement xmlelement = new XMLElement();
				xmlelement.parseFromReader(stringReader);
				foreach (object obj in xmlelement.getChildren())
				{
					XMLElement xmlelement2 = (XMLElement)obj;
					AssetDefinition assetDefinition2 = this.CreateAssetDefinition();
					if (assetDefinition2.Initialize(xmlelement2))
					{
						this.assets_.Add(assetDefinition2);
						assetDefinition2.Id = this.assets_.Count - 1;
					}
				}
				stringReader.Close();
			}
		}
		this.isInitialized_ = true;
	}

	public void RestoreLock()
	{
		foreach (AssetDefinition assetDefinition in this.assets_)
		{
			assetDefinition.Lock = assetDefinition.LockType != AssetDefinition.enLockType.NONE;
		}
	}

	public List<int> Refresh()
	{
		List<int> list = new List<int>();
		List<int> list2;
		List<int> list3;
		this.Refresh(out list2, out list3);
		list.AddRange(list2);
		list.AddRange(list3);
		return list;
	}

	public void Refresh(out List<int> quest, out List<int> cash)
	{
		quest = new List<int>();
		cash = new List<int>();
		int num = 0;
		foreach (AssetDefinition assetDefinition in this.assets_)
		{
			if (assetDefinition.Lock)
			{
				switch (assetDefinition.LockType)
				{
				case AssetDefinition.enLockType.NONE:
					assetDefinition.Lock = false;
					break;
				case AssetDefinition.enLockType.CASH:
					if (assetDefinition.ProductIDs != null)
					{
						foreach (string text in assetDefinition.ProductIDs)
						{
							if (FiaStore.Inst.UnlockedProductList != null && FiaStore.Inst.UnlockedProductList.Contains(text))
							{
								assetDefinition.Lock = false;
								cash.Add(num);
							}
						}
					}
					break;
				case AssetDefinition.enLockType.QUEST:
					assetDefinition.Quest.Refresh();
					if (assetDefinition.Quest.IsComplete())
					{
						assetDefinition.Lock = false;
						quest.Add(num);
					}
					break;
				}
			}
			num++;
		}
	}

	public void Clear()
	{
		this.assets_.Clear();
		this.isInitialized_ = false;
	}

	public bool IsInitialized()
	{
		return this.isInitialized_;
	}

	public List<AssetDefinition> GetAssetDefinitionList()
	{
		return this.assets_;
	}

	public AssetDefinition GetAssetDefinition(int idx)
	{
		if (!MathHelper.IsBetweenIE(idx, 0, this.assets_.Count))
		{
			return null;
		}
		return this.assets_[idx];
	}

	public static AssetDefinition GetAssetDefinition(AssetType assetType, int idx)
	{
		AssetDefinitionManager assetDefinitionManager = null;
		if (assetType == AssetType.TRACK)
		{
			assetDefinitionManager = TrackAssetDefinitionManager.Instance;
		}
		else if (assetType == AssetType.KART)
		{
			assetDefinitionManager = KartAssetDefinitionManager.Instance;
		}
		else if (assetType == AssetType.CHARACTER)
		{
			assetDefinitionManager = CharacterAssetDefinitionManager.Instance;
		}
		if (assetDefinitionManager == null)
		{
			return null;
		}
		return assetDefinitionManager.GetAssetDefinition(idx);
	}

	public void GetAssets(int idx, ref List<string> assets)
	{
		if (!MathHelper.IsBetweenIE(idx, 0, this.assets_.Count))
		{
			return;
		}
		this.assets_[idx].GetAssets(ref assets);
	}

	public string GetMainAsset(int idx)
	{
		if (!MathHelper.IsBetweenIE(idx, 0, this.assets_.Count))
		{
			return string.Empty;
		}
		return this.assets_[idx].GetMainAsset();
	}

	public byte GetAssetIndex(string name)
	{
		byte b = 0;
		foreach (AssetDefinition assetDefinition in this.assets_)
		{
			if (assetDefinition.Name == name)
			{
				return b;
			}
			b += 1;
		}
		return byte.MaxValue;
	}

	public void GetRandomAsset(out string output, bool isOnlyUnlocked)
	{
		byte b = 0;
		this.GetRandomAsset(out b, isOnlyUnlocked);
		output = this.GetAssetDefinition((int)b).Name;
	}

	public void GetRandomAsset(out byte assetIdx, bool isOnlyUnlocked)
	{
		this.GetRandomAsset(out assetIdx, isOnlyUnlocked, byte.MaxValue);
	}

	public void GetRandomAsset(out byte assetIdx, bool isOnlyUnlocked, byte except)
	{
		if (isOnlyUnlocked)
		{
			List<int> list = new List<int>();
			int num = 0;
			foreach (AssetDefinition assetDefinition in this.assets_)
			{
				if (!assetDefinition.Lock && num != (int)except)
				{
					list.Add(num);
				}
				num++;
			}
			assetIdx = (byte)list[global::UnityEngine.Random.Range(0, list.Count)];
		}
		else if ((int)except < this.assets_.Count)
		{
			assetIdx = (byte)global::UnityEngine.Random.Range(0, this.assets_.Count);
		}
		else
		{
			assetIdx = (byte)global::UnityEngine.Random.Range(0, this.assets_.Count - 1);
			if (assetIdx >= except)
			{
				assetIdx += 1;
			}
		}
	}

	public bool IsValidIndex(int idx)
	{
		return MathHelper.IsBetweenIE(idx, 0, this.assets_.Count);
	}

	public int GetAssetCount()
	{
		return this.assets_.Count;
	}

	public override string ToString()
	{
		string text = string.Empty;
		foreach (AssetDefinition assetDefinition in this.assets_)
		{
			text = text + assetDefinition.ToString() + "\n";
		}
		return text;
	}

	public virtual int GetMainIconIdx(int idx)
	{
		return idx;
	}

	public void UnlockAll()
	{
		foreach (AssetDefinition assetDefinition in this.assets_)
		{
			assetDefinition.Lock = false;
		}
	}

	public List<int> GetUnlockedQuest()
	{
		List<int> list = new List<int>();
		int num = 0;
		foreach (AssetDefinition assetDefinition in this.assets_)
		{
			if (assetDefinition.Lock && assetDefinition.LockType == AssetDefinition.enLockType.QUEST)
			{
				list.Add(num);
			}
			num++;
		}
		return list;
	}

	public int[] GetAssetIdsByListIndexOrder()
	{
		int count = this.assets_.Count;
		int[] array = new int[count];
		foreach (AssetDefinition assetDefinition in this.assets_)
		{
			if (assetDefinition.ListIdx >= count)
			{
			}
			array[assetDefinition.ListIdx] = assetDefinition.Id;
		}
		return array;
	}

	public const byte NULL_IDX = 255;

	protected List<AssetDefinition> assets_ = new List<AssetDefinition>();

	private bool isInitialized_;
}
