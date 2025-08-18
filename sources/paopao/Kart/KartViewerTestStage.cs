using System;
using UnityEngine;

public class KartViewerTestStage : MonoBehaviour
{
	private void Start()
	{
		TrackAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("trackdefinition"));
		KartAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("kartdefinition"));
		CharacterAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("characterdefinition"));
		MaterialManager.Instance.Initialize();
		GUIKartViewer.Instance.ChangeKartCharacter(0, 0);
		GUIKartViewer.Instance.Show();
		Debug.Log("Started");
	}

	private void Update()
	{
	}
}
