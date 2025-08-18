using System;
using UnityEngine;

public class MemoryCheckStagePrefab : MonoBehaviourStage
{
	private void changeShaders(GameObject obj)
	{
		if (obj != null)
		{
			Shader shader = Shader.Find("Default");
			if (obj.renderer != null && obj.renderer.materials != null)
			{
				foreach (Material material in obj.renderer.materials)
				{
					if (material != null)
					{
						material.shader = shader;
					}
				}
			}
			if (obj.transform != null)
			{
				foreach (object obj2 in obj.transform)
				{
					Transform transform = (Transform)obj2;
					if (transform != null && transform.gameObject != null)
					{
						this.changeShaders(transform.gameObject);
					}
				}
			}
		}
	}

	protected override void Awake()
	{
		Debug.Log("Awake Start " + Time.time + "\n");
		this.tracks = new GameObject("Tracks");
		this.karts = new GameObject("Karts");
		this.characters = new GameObject("Characters");
		int num = this.trackPrefab.Length;
		int num2 = this.kartPrefab.Length;
		int num3 = this.characterPrefab.Length;
		num2 = 1;
		num3 = 1;
		this.track = new GameObject[num];
		for (int i = 0; i < num; i++)
		{
			if (!(this.trackPrefab[i] == null))
			{
				this.track[i] = (GameObject)global::UnityEngine.Object.Instantiate(this.trackPrefab[i]);
				this.track[i].transform.parent = this.tracks.transform;
				this.changeShaders(this.track[i]);
			}
		}
		this.kart = new GameObject[num2];
		for (int j = 0; j < num2; j++)
		{
			if (!(this.kartPrefab[j] == null))
			{
				this.kart[j] = (GameObject)global::UnityEngine.Object.Instantiate(this.kartPrefab[j]);
				this.kart[j].transform.parent = this.karts.transform;
				this.changeShaders(this.kart[j]);
			}
		}
		this.character = new GameObject[num3];
		for (int k = 0; k < num3; k++)
		{
			if (!(this.characterPrefab[k] == null))
			{
				this.character[k] = (GameObject)global::UnityEngine.Object.Instantiate(this.characterPrefab[k]);
				this.character[k].transform.parent = this.characters.transform;
				this.changeShaders(this.character[k]);
			}
		}
		Debug.Log("Awake End " + Time.time + "\n");
	}

	private void Update()
	{
	}

	public GameObject[] trackPrefab;

	public GameObject[] kartPrefab;

	public GameObject[] characterPrefab;

	private GameObject tracks;

	private GameObject karts;

	private GameObject characters;

	private GameObject[] track;

	private GameObject[] kart;

	private GameObject[] character;
}
