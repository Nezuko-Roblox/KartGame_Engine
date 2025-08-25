using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIKartViewer : MonoBehaviour
{
	public static GUIKartViewer Instance
	{
		get
		{
			if (GUIKartViewer.instance_ == null)
			{
				GameObject gameObject = (GameObject)global::UnityEngine.Object.Instantiate(Resources.Load("Prefabs/kart_viewer"));
				if (gameObject != null)
				{
					GUIKartViewer.instance_ = gameObject.GetComponent<GUIKartViewer>();
					if (GUIKartViewer.instance_ != null)
					{
						global::UnityEngine.Object.DontDestroyOnLoad(GUIKartViewer.instance_);
						gameObject.SetActiveRecursively(false);
						GameObject gameObject2 = gameObject.transform.GetChild(0).gameObject;
						gameObject2.camera.pixelRect = new Rect((float)Screen.width - (float)Screen.width * 0.4f, 0f, (float)Screen.width * 0.4f, (float)Screen.height * 0.8f);
						GUIKartViewer.instance_.kartBodyRoot_ = gameObject.transform;
					}
				}
			}
			return GUIKartViewer.instance_;
		}
	}

	public static bool IsInstantiated()
	{
		return GUIKartViewer.instance_ != null;
	}

	public void Hide()
	{
		if (GUIKartViewer.instance_ != null)
		{
			GUIKartViewer.instance_.gameObject.SetActiveRecursively(false);
			this.StopAnimation();
		}
	}

	public void Show()
	{
		if (GUIKartViewer.instance_ != null)
		{
			GUIKartViewer.instance_.gameObject.SetActiveRecursively(true);
			this.PlayAnimation();
		}
	}

	public void ChangeKartCharacter(byte kart, byte character)
	{
		if (GUIKartViewer.instance_ == null)
		{
			return;
		}
		if (kart == this.kart_ && this.character_ == character)
		{
			return;
		}
		List<string> list = new List<string>();
		KartAssetDefinitionManager.Instance.GetAssets((int)kart, ref list);
		CharacterAssetDefinitionManager.Instance.GetAssets((int)character, ref list);
		foreach (string text in list)
		{
			ResourceLoader.Instance.RequestAssetBundle(text);
		}
		this.requenstKart_ = kart;
		this.requenstCharacter_ = character;
	}

	private void PlayAnimation()
	{
		if (this.characterTransform_ != null)
		{
			this.characterTransform_.animation.animateOnlyIfVisible = false;
			this.characterTransform_.animation.Play("idle");
		}
	}

	private void StopAnimation()
	{
		if (this.characterTransform_ != null)
		{
			this.characterTransform_.animation.Stop();
		}
	}

	private void Update()
	{
		if (this.requenstKart_ != 255 && this.requenstCharacter_ != 255 && ResourceLoader.Instance.IsDone())
		{
			if (this.characterTransform_ != null)
			{
				this.characterTransform_.parent = null;
				this.characterTransform_.localPosition = Vector3.zero;
				this.characterTransform_.localRotation = Quaternion.identity;
				this.StopAnimation();
			}
			if (this.kart_ != this.requenstKart_)
			{
				this.kart_ = this.requenstKart_;
				if (this.kartBodyTransform_ != null)
				{
					global::UnityEngine.Object.DestroyImmediate(this.kartBodyTransform_.gameObject);
					this.kartBodyTransform_ = null;
				}
				this.kartBodyTransform_ = FiaUtil.GenerateKart(this.kart_).transform;
				this.kartBodyTransform_.position = new Vector3(-0.1480141f, 0.369704f, 0f);
				foreach (object obj in this.kartBodyTransform_)
				{
					Transform transform = (Transform)obj;
					if (transform.name == "port0")
					{
						Transform transform2 = transform.Find("booster");
						if (transform2 != null)
						{
							transform2.gameObject.SetActiveRecursively(false);
						}
					}
				}
			}
			if (this.character_ != this.requenstCharacter_)
			{
				this.character_ = this.requenstCharacter_;
				if (this.characterTransform_ != null)
				{
					global::UnityEngine.Object.DestroyImmediate(this.characterTransform_.gameObject);
					this.characterTransform_ = null;
				}
				this.characterTransform_ = FiaUtil.GenerateCharacter(this.character_).transform;
			}
			FiaUtil.AttachKartNCharacterEx(ref this.kartBodyRoot_, ref this.kartBodyTransform_, ref this.characterTransform_);
			Transform[] componentsInChildren = this.kartBodyRoot_.GetComponentsInChildren<Transform>();
			foreach (Transform transform3 in componentsInChildren)
			{
				transform3.gameObject.layer = 10;
			}
			this.PlayAnimation();
			this.requenstCharacter_ = byte.MaxValue;
			this.requenstKart_ = byte.MaxValue;
		}
	}

	public void Refresh()
	{
		this.Update();
	}

	private byte kart_ = byte.MaxValue;

	private byte character_ = byte.MaxValue;

	private byte requenstKart_ = byte.MaxValue;

	private byte requenstCharacter_ = byte.MaxValue;

	private Transform kartBodyRoot_;

	public static GUIKartViewer instance_;

	private Transform kartBodyTransform_;

	private Transform characterTransform_;
}
