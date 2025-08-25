using System;
using System.Collections.Generic;
using UnityEngine;

public class AssetDefinition
{
	public AssetDefinition(AssetType type)
	{
		this.type_ = type;
	}

	public virtual bool Initialize(XMLElement elem)
	{
		if (elem == null)
		{
			return false;
		}
		if ((this.name_ = elem.getStringAttribute("name", string.Empty)) == string.Empty)
		{
			return false;
		}
		this.listIdx_ = elem.getIntAttribute("list_index", -1);
		this.lockType_ = (AssetDefinition.enLockType)elem.getIntAttribute("lock", 0);
		if (this.lockType_ == AssetDefinition.enLockType.QUEST)
		{
			string stringAttribute = elem.getStringAttribute("quest", string.Empty);
			if (stringAttribute == string.Empty)
			{
				return false;
			}
			this.quest_ = QuestBuilderManager.Instance.Build(stringAttribute);
			if (this.quest_ == null)
			{
				return false;
			}
			string stringAttribute2 = elem.getStringAttribute("quest_contents", string.Empty);
			if (stringAttribute2 == string.Empty)
			{
				return false;
			}
			char[] array = new char[] { ' ', '\t' };
			string[] array2 = stringAttribute2.Split(array, StringSplitOptions.RemoveEmptyEntries);
			if (array2.Length != 4)
			{
				return false;
			}
			this.questContents_ = RectHelper.CreateRect(array2);
			this.questTitle_ = elem.getIntAttribute("quest_title", -1);
			if (this.questTitle_ == -1)
			{
				return false;
			}
		}
		else if (this.lockType_ == AssetDefinition.enLockType.CASH)
		{
			string stringAttribute3 = elem.getStringAttribute("productIDs", string.Empty);
			char[] array3 = new char[] { ' ', '\t' };
			this.productIDs_ = stringAttribute3.Split(array3, StringSplitOptions.RemoveEmptyEntries);
			return true;
		}
		return true;
	}

	public virtual string GetMainAsset()
	{
		return string.Empty;
	}

	public virtual void GetAssets(ref List<string> assetbundleList)
	{
	}

	public AssetDefinition.enLockType LockType
	{
		get
		{
			return this.lockType_;
		}
	}

	public QuestBase Quest
	{
		get
		{
			return this.quest_;
		}
		set
		{
			this.quest_ = value;
		}
	}

	public Rect QuestContents
	{
		get
		{
			return this.questContents_;
		}
	}

	public int QuestTitle
	{
		get
		{
			return this.questTitle_;
		}
	}

	public string Name
	{
		get
		{
			return this.name_;
		}
	}

	public bool Lock
	{
		get
		{
			return this.lock_;
		}
		set
		{
			this.lock_ = value;
		}
	}

	public string[] ProductIDs
	{
		get
		{
			return this.productIDs_;
		}
		set
		{
			this.productIDs_ = value;
		}
	}

	public int ProductID
	{
		get
		{
			if (this.productIDs_ == null)
			{
				return -1;
			}
			string text = this.productIDs_[0];
			if (text == null)
			{
				return -1;
			}
			if (text.CompareTo("p1") == 0)
			{
				return 0;
			}
			if (text.CompareTo("p2") == 0)
			{
				return 1;
			}
			if (text.CompareTo("p3") == 0)
			{
				return 2;
			}
			if (text.CompareTo("p4") == 0)
			{
				return 3;
			}
			if (text.CompareTo("p5") == 0)
			{
				return 4;
			}
			if (text.CompareTo("p6") == 0)
			{
				return 5;
			}
			if (text.CompareTo("p7") == 0)
			{
				return 6;
			}
			if (text.CompareTo("p8") == 0)
			{
				return 7;
			}
			if (text.CompareTo("p9") == 0)
			{
				return 8;
			}
			return -1;
		}
	}

	public string ProductIDString
	{
		get
		{
			if (this.productIDs_ == null)
			{
				return string.Empty;
			}
			return this.productIDs_[0];
		}
	}

	public int Id
	{
		get
		{
			return this.id_;
		}
		set
		{
			this.id_ = value;
		}
	}

	public int ListIdx
	{
		get
		{
			return (this.listIdx_ < 0) ? this.id_ : this.listIdx_;
		}
	}

	public AssetType type_;

	protected string name_;

	protected string[] productIDs_;

	private AssetDefinition.enLockType lockType_;

	private bool lock_ = true;

	private QuestBase quest_;

	private Rect questContents_;

	private int questTitle_;

	private int id_ = -1;

	private int listIdx_ = -1;

	public enum enLockType
	{
		NONE,
		CASH,
		QUEST
	}
}
