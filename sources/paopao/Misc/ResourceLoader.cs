if (1 << collider.gameObject.layer == 65536)
		{
			int num;
			int num2;
			if (collider.name.Contains("s_"))
			{
				num = int.Parse(collider.name.Substring(2));
				num2 = 1;
			}
			else
			{
				num = int.Parse(collider.name);
				num2 = 0;
			}
			MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.ENTER_USER_SECTION);
			monoBehaviourMessage2Param.Initialize(num2, num);
			base.SendMessage(1, monoBehaviourMessage2Param);
		}wo'z'ji'jusing System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceLoader
{
	public static ResourceLoader Instance
	{
		get
		{
			if (ResourceLoader.instance_ == null)
			{
				ResourceLoader.instance_ = new ResourceLoader();
			}
			return ResourceLoader.instance_;
		}
	}

	public void Initialize()
	{
		if (this.request_ != null)
		{
			foreach (WWW www in this.request_.Values)
			{
				if (www.assetBundle != null)
				{
					www.assetBundle.Unload(false);
				}
			}
			this.request_.Clear();
		}
		else
		{
			this.request_ = new Dictionary<string, WWW>();
		}
		if (this.removeKey_ != null)
		{
			this.removeKey_.Clear();
		}
		else
		{
			this.removeKey_ = new LinkedList<string>();
		}
	}

	public void RequestAssetBundle(string requestname)
	{
		if (requestname == null)
		{
			Debug.LogError("[ResourceLoader:RequestAssetBundle] requestname == null");
			return;
		}
		if (this.request_ == null)
		{
			Debug.LogError("[ResourceLoader:RequestAssetBundle] requestname == null");
			return;
		}
		if (this.request_.ContainsKey(requestname))
		{
			return;
		}
		string text = string.Empty;
		string text2 = "/!/assets/android/";
		text = string.Concat(new string[]
		{
			"file://",
			Application.dataPath,
			text2,
			requestname,
			".assetbundle"
		});
		text = "jar:" + text;
		this.request_.Add(requestname, new WWW(text));
	}

	public bool IsDone()
	{
		bool flag = true;
		if (this.request_ == null)
		{
			Debug.LogError("[ResourceLoader:IsDone] requestname == null");
			return false;
		}
		foreach (KeyValuePair<string, WWW> keyValuePair in this.request_)
		{
			if (!keyValuePair.Value.isDone)
			{
				flag = false;
			}
			else if (keyValuePair.Value.error != null && this.removeKey_ != null)
			{
				this.removeKey_.AddLast(keyValuePair.Key);
			}
		}
		this.RemoveRequestError();
		return flag;
	}

	private void RemoveRequestError()
	{
		if (this.request_ == null)
		{
			Debug.LogError("[ResourceLoader:RemoveRequestError] requestname == null");
			return;
		}
		if (this.removeKey_ == null)
		{
			Debug.LogError("[ResourceLoader:RemoveRequestError] removeKey_ == null");
			return;
		}
		if (this.removeKey_.Count == 0)
		{
			return;
		}
		for (LinkedListNode<string> linkedListNode = this.removeKey_.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			this.request_.Remove(linkedListNode.Value);
		}
		this.removeKey_.Clear();
	}

	public global::UnityEngine.Object GetMainAsset(string key)
	{
		if (this.request_ == null)
		{
			Debug.LogError("[ResourceLoader:GetMainAsset] requestname == null");
			return null;
		}
		if (this.removeKey_ == null)
		{
			Debug.LogError("[ResourceLoader:GetMainAsset] removeKey_ == null");
			return null;
		}
		if (this.request_.ContainsKey(key) && this.request_[key].isDone)
		{
			return this.request_[key].assetBundle.mainAsset;
		}
		return null;
	}

	public bool IsExistAsset(string key, string subkey)
	{
		if (this.request_ == null)
		{
			Debug.LogError("[ResourceLoader:IsExistAsset] requestname == null");
			return false;
		}
		if (this.removeKey_ == null)
		{
			Debug.LogError("[ResourceLoader:IsExistAsset] removeKey_ == null");
			return false;
		}
		return this.request_.ContainsKey(key) && this.request_[key].isDone && this.request_[key].assetBundle.Contains(subkey);
	}

	public global::UnityEngine.Object GetAsset(string key, string subkey)
	{
		if (this.request_ == null)
		{
			Debug.LogError("[ResourceLoader:GetAsset2] requestname == null");
			return null;
		}
		if (this.removeKey_ == null)
		{
			Debug.LogError("[ResourceLoader:GetAsset2] removeKey_ == null");
			return null;
		}
		if (this.request_.ContainsKey(key))
		{
			if (this.request_[key].isDone)
			{
				return this.request_[key].assetBundle.Load(subkey);
			}
		}
		return null;
	}

	public global::UnityEngine.Object GetAsset(string key, string subkey, Type type)
	{
		if (this.request_ == null)
		{
			Debug.LogError("[ResourceLoader:GetAsset3] requestname == null");
			return null;
		}
		if (this.removeKey_ == null)
		{
			Debug.LogError("[ResourceLoader:GetAsset3] removeKey_ == null");
			return null;
		}
		if (this.request_.ContainsKey(key))
		{
			if (this.request_[key].isDone)
			{
				return this.request_[key].assetBundle.Load(subkey, type);
			}
		}
		return null;
	}

	public GameObject CreateGameObject(GameObject obj)
	{
		if (this.request_ == null)
		{
			Debug.LogError("[ResourceLoader:CreateGameObject] requestname == null");
			return null;
		}
		if (this.removeKey_ == null)
		{
			Debug.LogError("[ResourceLoader:CreateGameObject] removeKey_ == null");
			return null;
		}
		if (obj == null)
		{
			return null;
		}
		GameObject gameObject = (GameObject)global::UnityEngine.Object.Instantiate(obj);
		gameObject.name = obj.name;
		return gameObject;
	}

	public GameObject GetGameObjectAsset(string key, string subkey, bool isNew)
	{
		if (this.request_ == null)
		{
			Debug.LogError("[ResourceLoader:GetGameObjectAsset] requestname == null");
			return null;
		}
		if (this.removeKey_ == null)
		{
			Debug.LogError("[ResourceLoader:GetGameObjectAsset] removeKey_ == null");
			return null;
		}
		if (this.request_.ContainsKey(key) && this.request_[key].isDone)
		{
			GameObject gameObject = (GameObject)this.request_[key].assetBundle.Load(subkey, typeof(GameObject));
			return (!(gameObject != null) || !isNew) ? gameObject : this.CreateGameObject(gameObject);
		}
		return null;
	}

	public GameObject GetGameObjectMainAsset(string key, bool isNew)
	{
		if (this.request_ == null)
		{
			Debug.LogError("[ResourceLoader:GetGameObjectMainAsset] requestname == null");
			return null;
		}
		if (this.removeKey_ == null)
		{
			Debug.LogError("[ResourceLoader:GetGameObjectMainAsset] removeKey_ == null");
			return null;
		}
		if (this.request_.ContainsKey(key) && this.request_[key].isDone)
		{
			GameObject gameObject = (GameObject)this.request_[key].assetBundle.mainAsset;
			return (!(gameObject != null) || !isNew) ? gameObject : this.CreateGameObject(gameObject);
		}
		return null;
	}

	public override string ToString()
	{
		string text = "### ResourceLoade ###\n";
		if (this.request_ == null)
		{
			text += "request_ == null\n";
		}
		else
		{
			foreach (KeyValuePair<string, WWW> keyValuePair in this.request_)
			{
				string text2 = text;
				text = string.Concat(new object[]
				{
					text2,
					keyValuePair.Key,
					" ",
					keyValuePair.Value.isDone,
					"\n"
				});
			}
		}
		return text;
	}

	public void DebugAsset(string assetname)
	{
		if (this.request_ == null)
		{
			Debug.LogError("[ResourceLoader:DebugAsset] requestname == null");
			return;
		}
		if (this.removeKey_ == null)
		{
			Debug.LogError("[ResourceLoader:DebugAsset] removeKey_ == null");
			return;
		}
		if (!this.request_.ContainsKey(assetname))
		{
			return;
		}
		WWW www = this.request_[assetname];
		if (!www.isDone)
		{
			return;
		}
		if (www.assetBundle.mainAsset != null)
		{
			global::UnityEngine.Object mainAsset = www.assetBundle.mainAsset;
		}
		global::UnityEngine.Object[] array = www.assetBundle.LoadAll();
		if (Debug.isDebugBuild)
		{
			string text = string.Empty;
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2,
					array[i].name,
					" ( ",
					array[i].GetType().ToString(),
					" )\n"
				});
			}
		}
	}

	public static ResourceLoader instance_;

	private Dictionary<string, WWW> request_ = new Dictionary<string, WWW>();

	private LinkedList<string> removeKey_ = new LinkedList<string>();
}
