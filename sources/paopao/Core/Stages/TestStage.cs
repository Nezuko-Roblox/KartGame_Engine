using System;
using System.Collections.Generic;
using UnityEngine;

public class TestStage : MonoBehaviour
{
	private void Awake()
	{
		FadeInOut.Instance.FadeIn();
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (ResourceLoader.Instance.IsDone() && this.isMakeInsatnce_)
		{
			this.GenerateCharacter();
			this.isMakeInsatnce_ = false;
		}
		if (FadeInOut.Instance.IsFadeInEnd())
		{
			ResourceLoader.Instance.RequestAssetBundle("forest_i01");
			ResourceLoader.Instance.RequestAssetBundle("practice0");
			ResourceLoader.Instance.RequestAssetBundle("dao");
			ResourceLoader.Instance.RequestAssetBundle("bone");
			FadeInOut.Instance.ResetFadeState();
			this.isMakeInsatnce_ = true;
		}
		else if (FadeInOut.Instance.IsFadeOutEnd())
		{
			Application.LoadLevel(1);
			base.enabled = false;
			FadeInOut.Instance.ResetFadeState();
		}
	}

	private void AnalysisCharacter()
	{
		if (this.completeCharacter_ == null)
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		foreach (object obj in this.completeCharacter_.transform)
		{
			Transform transform = (Transform)obj;
			if (!transform.name.ToLower().Contains("bip"))
			{
				list.Add(transform.gameObject);
			}
		}
		string text = string.Empty;
		foreach (GameObject gameObject in list)
		{
			SkinnedMeshRenderer component = gameObject.GetComponent<SkinnedMeshRenderer>();
			if (component != null && component.bones != null)
			{
				foreach (Transform transform2 in component.bones)
				{
					text = text + transform2.name + "\n";
				}
			}
		}
	}

	private void GenerateCharacter()
	{
		string text = "dao";
		GameObject gameObject = (GameObject)ResourceLoader.Instance.GetMainAsset("bone");
		GameObject gameObject2 = null;
		if (gameObject != null)
		{
			gameObject2 = (GameObject)global::UnityEngine.Object.Instantiate(gameObject);
		}
		Transform[] componentsInChildren = gameObject2.GetComponentsInChildren<Transform>();
		string[] array = new string[] { "dao", "dao_cartoon", "dao_face" };
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObjectAsset = ResourceLoader.Instance.GetGameObjectAsset(text, array[i], true);
			string[] content = ((StringHolder)ResourceLoader.Instance.GetAsset(text, array[i] + "_bonenames")).content;
			if (gameObjectAsset != null)
			{
				SkinnedMeshRenderer component = gameObjectAsset.GetComponent<SkinnedMeshRenderer>();
				if (component != null)
				{
					List<Transform> list = new List<Transform>();
					foreach (string text2 in content)
					{
						foreach (Transform transform in componentsInChildren)
						{
							if (text2 == transform.name)
							{
								list.Add(transform);
							}
						}
					}
					component.bones = list.ToArray();
				}
				FiaUtil.AttachChild(ref gameObject2, ref gameObjectAsset);
			}
		}
	}

	private bool isMakeInsatnce_;

	public GameObject bone_;

	public GameObject meshOnly_;

	public GameObject completeCharacter_;
}
