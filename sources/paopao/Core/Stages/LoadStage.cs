using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadStage : MonoBehaviour
{
	public int CheckPlanePassing(PassPlane plane, Vector3 p0, Vector3 p1)
	{
		Vector3 vector = p1 - p0;
		if (MathHelper.RayFaceIntersect(plane.pt1_, p0, vector) || MathHelper.RayFaceIntersect(plane.pt2_, p0, vector))
		{
			return (Vector3.Dot(vector, plane.normal_) < 0f) ? (-1) : 1;
		}
		return 0;
	}

	protected void Awake()
	{
		this.kartParameter_[0] = null;
		for (int i = 1; i < 6; i++)
		{
			this.kartParameter_[i] = new KartParameter(0, (byte)i, this.CHARACTER_NAME[i], PlayerType.AI, string.Empty);
		}
		Time.timeScale = 1f;
		KartManager.Instance.CheckDirectory();
		FadeInOut.Instance.FadeIn();
		if (!KartAssetDefinitionManager.Instance.IsInitialized())
		{
			KartAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("kartdefinition"));
		}
		if (!CharacterAssetDefinitionManager.Instance.IsInitialized())
		{
			CharacterAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("characterdefinition"));
		}
		if (!TrackAssetDefinitionManager.Instance.IsInitialized())
		{
			TrackAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("trackdefinition"));
		}
		KartOptions.Instance.LoadRegistry();
		if (!MonoBehaviourMessageFactory.Instance.IsInitialized())
		{
			MonoBehaviourMessageFactory.Instance.Initialize();
		}
		this.lock_ = true;
		this.isMakeInsatnce_ = false;
		this.trackName_ = StaticOption.GetTrackName();
		if (Env.SupportsIOSRuntime)
		{
			this.controllerName_ = Enum.GetNames(typeof(iOSControllerType))[0];
			if (KartOptions.Instance.Controller == -1)
			{
				KartOptions.Instance.Controller = 0;
			}
			this.controllerName_ = Enum.GetNames(typeof(iOSControllerType))[KartOptions.Instance.Controller];
		}
		if (Debug.isDebugBuild)
		{
			Debug.Log("LoadStage:controllerName_ = " + this.controllerName_);
		}
		for (int j = 1; j < 6; j++)
		{
			if (KartManager.Instance.parameter_.kart_[j] == null)
			{
				this.kartParameter_[j].type_ = PlayerType.NONE;
			}
			else
			{
				this.kartParameter_[j].type_ = KartManager.Instance.parameter_.kart_[j].type_;
			}
		}
		this.driveOption_ = KartManager.Instance.parameter_.driveOption_;
		this.RecordFileSetting();
		GhostRecordManager.Instance.Initialize((byte)AssetSelection.selection_[2]);
		if (Debug.isDebugBuild)
		{
			Debug.Log(GhostRecordManager.Instance.ToString());
		}
	}

	protected void ResetSelection()
	{
		List<int> list;
		FiaUtil.GetRandomList(0, this.aiFilenames.Count - 1, out list);
		List<int> list2;
		FiaUtil.GetRandomList(0, this.ghostFilenames.Count - 1, out list2);
		int num = 0;
		int num2 = 0;
		for (int i = 1; i < 4; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				this.selectedRecords_[j, i] = 0;
				if (list.Count > 0)
				{
					this.selectedRecords_[j, i] = list[num++ % list.Count];
				}
			}
		}
		for (int k = 0; k < 6; k++)
		{
			this.selectedRecords_[k, 0] = 0;
			if (list2.Count > 0)
			{
				this.selectedRecords_[k, 0] = list2[num2++ % list2.Count];
			}
		}
		if (Debug.isDebugBuild)
		{
			string text = string.Empty;
			for (int l = 0; l < 6; l++)
			{
				for (int m = 0; m < 4; m++)
				{
					text = text + this.selectedRecords_[l, m].ToString() + " ";
				}
				text += "\n";
			}
			Debug.Log(text + this.aiFilenames.Count.ToString());
		}
	}

	protected void RecordFileSetting()
	{
		this.aiFiles.Clear();
		this.ghostFiles.Clear();
		this.aiFilenames.Clear();
		this.ghostFilenames.Clear();
		string name = TrackAssetDefinitionManager.Instance.GetAssetDefinition(AssetSelection.selection_[2]).Name;
		foreach (string text in Directory.GetFiles(FiaUtil.recordPath))
		{
			text = text.Replace("\\", "/");
			string[] array = text.Split(new char[] { '/' });
			string text2 = array[array.Length - 1];
			if (text2.Contains(name) && text2.Contains("ai_") && text2.Contains(".bin"))
			{
				this.aiFiles.Add(text);
			}
			else if (text2.Contains(".krg"))
			{
				GhostFilenameInfo ghostFilenameInfo = new GhostFilenameInfo(text2);
				if ((int)ghostFilenameInfo.trackIdx_ == AssetSelection.selection_[2])
				{
					this.ghostFiles.Add(text);
				}
			}
		}
		foreach (string text3 in this.aiFiles)
		{
			string text4 = text3.Replace("\\", "/");
			string[] array2 = text4.Split(new char[] { '/' });
			this.aiFilenames.Add(array2[array2.Length - 1]);
		}
		foreach (string text5 in this.ghostFiles)
		{
			string text6 = text5.Replace("\\", "/");
			string[] array3 = text6.Split(new char[] { '/' });
			this.ghostFilenames.Add(array3[array3.Length - 1]);
		}
		this.ResetSelection();
	}

	protected void OnGUI()
	{
		if (this.lock_)
		{
			return;
		}
		GUI.depth = 10;
		if (GUI.Button(new Rect(20f, 10f, 150f, 25f), this.trackName_))
		{
			StaticOption.IncreaseTrackIndex();
			this.trackName_ = StaticOption.GetTrackName();
		}
		if (Env.SupportsIOSRuntime)
		{
			if (GUI.Button(new Rect(20f, 50f, 150f, 25f), this.controllerName_))
			{
				int num = (int)Enum.Parse(typeof(iOSControllerType), this.controllerName_, true);
				int num2 = ((!Env.IsIPhone) ? 3 : 0);
				int num3 = ((!Env.IsIPhone) ? 5 : 2);
				num = num2 + (num + 1 - num2) % (num3 - num2 + 1);
				this.controllerName_ = Enum.GetNames(typeof(iOSControllerType))[num];
			}
			this.deadZone_ = GUI.TextField(new Rect(20f, 90f, 150f, 25f), this.deadZone_);
			this.steerMultiplier_ = GUI.TextField(new Rect(20f, 130f, 150f, 25f), this.steerMultiplier_);
		}
		if (GUI.Button(new Rect(350f, 170f, 100f, 25f), KartManager.Instance.parameter_.gameMode_.ToString()))
		{
			GameMode gameMode = KartManager.Instance.parameter_.gameMode_;
			gameMode = (gameMode + 1) % GameMode.SIZE;
			KartManager.Instance.parameter_.gameMode_ = gameMode;
		}
		for (int i = 0; i < 3; i++)
		{
			List<AssetDefinition> list = null;
			switch (i)
			{
			case 0:
				list = CharacterAssetDefinitionManager.Instance.GetAssetDefinitionList();
				break;
			case 1:
				list = KartAssetDefinitionManager.Instance.GetAssetDefinitionList();
				break;
			case 2:
				list = TrackAssetDefinitionManager.Instance.GetAssetDefinitionList();
				break;
			}
			if (GUI.Button(new Rect(20f, (float)(210 + 40 * i), 100f, 25f), list[AssetSelection.selection_[i]].Name))
			{
				AssetSelection.selection_[i] = (AssetSelection.selection_[i] + 1) % list.Count;
				if (i == 2)
				{
					this.RecordFileSetting();
					GhostRecordManager.Instance.Initialize((byte)AssetSelection.selection_[i]);
					if (Debug.isDebugBuild)
					{
						Debug.Log(GhostRecordManager.Instance.ToString());
					}
				}
			}
		}
		if (GUI.Button(new Rect(180f, 10f, 150f, 150f), "GO!!!!"))
		{
			KartManager.Instance.parameter_.track_ = (byte)AssetSelection.selection_[2];
			KartManager.Instance.parameter_.kart_[KartManager.PLAYER_KART_IDX] = new KartParameter(0, (byte)AssetSelection.selection_[0], "Kaiser", PlayerType.PLAYER, string.Empty);
			KartManager.Instance.parameter_.driveOption_ = this.driveOption_;
			bool flag = true;
			if (flag)
			{
				TrackAssetDefinition trackAssetDefinition = (TrackAssetDefinition)TrackAssetDefinitionManager.Instance.GetAssetDefinition((int)KartManager.Instance.parameter_.track_);
				KartManager.Instance.parameter_.maxLap_ = trackAssetDefinition.MaxLap;
				if (KartManager.Instance.parameter_.gameMode_ == GameMode.SINGLE_ITEM)
				{
					List<int> list2;
					FiaUtil.GetRandomList(0, CharacterAssetDefinitionManager.Instance.GetAssetCount() - 1, out list2);
					list2.Remove((int)KartManager.Instance.parameter_.kart_[KartManager.PLAYER_KART_IDX].character_);
					int num4 = 0;
					for (int j = 0; j < 6; j++)
					{
						if (j != KartManager.PLAYER_KART_IDX)
						{
							byte maxValue = byte.MaxValue;
							byte b = (byte)list2[num4++];
							KartAssetDefinitionManager.Instance.GetRandomAsset(out maxValue, false);
							KartManager.Instance.parameter_.kart_[j] = new KartParameter(maxValue, b, this.CHARACTER_NAME[(int)b], PlayerType.AI, string.Empty);
						}
					}
				}
				else if (KartManager.Instance.parameter_.gameMode_ == GameMode.SINGLE_SPEED)
				{
					List<int> list3;
					FiaUtil.GetRandomList(0, this.DEVELOPER_NAME.Length - 1, out list3);
					GhostRecordInfo[] array = null;
					GhostRecordManager.Instance.GetRandomRecord(out array);
					if (array != null)
					{
						for (int k = 0; k < array.Length; k++)
						{
							if (k != KartManager.PLAYER_KART_IDX)
							{
								KartManager.Instance.parameter_.kart_[k] = null;
								if (array[k] != null)
								{
									GhostFilenameInfo filenameInfo_ = array[k].filenameInfo_;
									string text = string.Empty;
									if (array[k].type_ == GhostRecordType.PLAYER)
									{
										text = "YOUR BEST!!";
									}
									else if (array[k].type_ == GhostRecordType.DEVELOPER || array[k].type_ == GhostRecordType.OLDER)
									{
										text = this.DEVELOPER_NAME[list3[k]];
									}
									KartManager.Instance.parameter_.kart_[k] = new KartParameter(filenameInfo_.kartIdx_, filenameInfo_.characterIdx_, text, PlayerType.GHOST, filenameInfo_.filePath_ + ";" + ((array[k].type_ != GhostRecordType.DEVELOPER) ? "0;" : "1;"));
								}
							}
						}
					}
				}
			}
			List<string> list4 = new List<string>();
			TrackAssetDefinitionManager.Instance.GetAssets((int)KartManager.Instance.parameter_.track_, ref list4);
			for (int l = 0; l < 6; l++)
			{
				KartParameter kartParameter = KartManager.Instance.parameter_.kart_[l];
				if (kartParameter != null)
				{
					KartAssetDefinitionManager.Instance.GetAssets((int)kartParameter.body_, ref list4);
					CharacterAssetDefinitionManager.Instance.GetAssets((int)kartParameter.character_, ref list4);
				}
			}
			string text2 = string.Empty;
			foreach (string text3 in list4)
			{
				ResourceLoader.Instance.RequestAssetBundle(text3);
				text2 = text2 + text3 + "\n";
			}
			if (Debug.isDebugBuild)
			{
				Debug.Log("### LOAD RESOURCE ###\n" + text2);
			}
			this.lock_ = true;
			this.isMakeInsatnce_ = true;
		}
	}

	protected void Update()
	{
		if (ResourceLoader.Instance.IsDone() && this.isMakeInsatnce_)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log(ResourceLoader.Instance.ToString());
			}
			FadeInOut.Instance.FadeOut();
			this.isMakeInsatnce_ = false;
		}
		if (FadeInOut.Instance.IsFadeInEnd())
		{
			this.lock_ = false;
			FadeInOut.Instance.ResetFadeState();
		}
		else if (FadeInOut.Instance.IsFadeOutEnd())
		{
			KartManager.Instance.parameter_.Stage = StageType.GAME;
			Application.LoadLevel(this.trackName_);
			base.enabled = false;
			FadeInOut.Instance.ResetFadeState();
		}
	}

	protected void Start()
	{
	}

	protected bool isMakeInsatnce_;

	public TextAsset assetDefinition;

	protected int[] indexArray_ = new int[3];

	protected bool lock_;

	protected string trackName_;

	protected string controllerName_;

	protected string deadZone_ = "0.06";

	protected string steerMultiplier_ = "3.4";

	protected KartParameter[] kartParameter_ = new KartParameter[6];

	protected int[,] selectedRecords_ = new int[6, 4];

	protected DriveOption driveOption_;

	protected List<string> aiFiles = new List<string>();

	protected List<string> ghostFiles = new List<string>();

	protected List<string> aiFilenames = new List<string>();

	protected List<string> ghostFilenames = new List<string>();

	private string[] CHARACTER_NAME = new string[] { "Dao", "Diz", "Ethi", "Boz", "Evie", "Orion" };

	private string[] DEVELOPER_NAME = new string[] { "Han Jeongmin", "Ahn Minu", "Seo Kijun", "Han Daehoon", "Jung Sehoon", "Park Jiyoung", "Ryu Jieun", "Kim Junghyun", "Lee Seungchan" };
}
