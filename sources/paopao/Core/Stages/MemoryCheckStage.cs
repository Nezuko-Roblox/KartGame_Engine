using System;
using System.Collections.Generic;
using UnityEngine;

public class MemoryCheckStage : MonoBehaviourStage
{
	protected override void Awake()
	{
		Debug.Log("Awake Start " + Time.time + "\n");
		TrackAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("trackdefinition"));
		KartAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("kartdefinition"));
		CharacterAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("characterdefinition"));
		List<string> list = new List<string>();
		for (int i = 0; i < 0; i++)
		{
			TrackAssetDefinitionManager.Instance.GetAssets((int)((byte)i), ref list);
		}
		for (int j = 0; j < 0; j++)
		{
			KartAssetDefinitionManager.Instance.GetAssets((int)((byte)j), ref list);
		}
		for (int k = 0; k < 2; k++)
		{
			CharacterAssetDefinitionManager.Instance.GetAssets((int)((byte)k), ref list);
		}
		MaterialManager.Instance.Initialize();
		ResourceLoader.Instance.Initialize();
		foreach (string text in list)
		{
			ResourceLoader.Instance.RequestAssetBundle(text);
		}
		this.isInstantiated = false;
		Debug.Log("Awake End " + Time.time + "\n");
	}

	private void Update()
	{
		if (!this.isInstantiated && ResourceLoader.Instance.IsDone())
		{
			this.tracks = new GameObject("Tracks");
			this.karts = new GameObject("Karts");
			this.characters = new GameObject("Characters");
			this.track = new GameObject[0];
			for (int i = 0; i < 0; i++)
			{
				string mainAsset = TrackAssetDefinitionManager.Instance.GetMainAsset((int)((byte)i));
				this.track[i] = ResourceLoader.Instance.GetGameObjectMainAsset(mainAsset, true);
				this.track[i].transform.parent = this.tracks.transform;
			}
			this.kart = new GameObject[0];
			for (int j = 0; j < 0; j++)
			{
				this.kart[j] = FiaUtil.GenerateKart((byte)j);
				this.kart[j].transform.parent = this.karts.transform;
			}
			this.character = new GameObject[2];
			for (int k = 0; k < 2; k++)
			{
				this.character[k] = FiaUtil.GenerateCharacter((byte)k);
				this.character[k].transform.parent = this.characters.transform;
			}
			this.isInstantiated = true;
		}
	}

	private const int trackLength = 0;

	private const int kartLength = 0;

	private const int characterLength = 2;

	private GameObject tracks;

	private GameObject karts;

	private GameObject characters;

	private GameObject[] track;

	private GameObject[] kart;

	private GameObject[] character;

	private bool isInstantiated;
}
