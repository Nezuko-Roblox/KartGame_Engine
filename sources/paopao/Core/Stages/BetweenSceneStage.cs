using System;
using System.Collections;
using UnityEngine;

public class BetweenSceneStage : FiaGUILayer
{
	public static BetweenSceneStage Instance
	{
		get
		{
			return BetweenSceneStage.instance_;
		}
		set
		{
			BetweenSceneStage.instance_ = value;
		}
	}

	private void Awake()
	{
		BetweenSceneStage.Instance = this;
	}

	protected override void Start()
	{
		base.Start();
		if (GUIKartViewer.IsInstantiated())
		{
			GUIKartViewer.Instance.Hide();
		}
		StageController.Instance.StartFade(new Color(0f, 0f, 0f, 0f), 0f);
		base.InvokeRepeating("ProgressAnimation", 0.1f, 0.1f);
		base.StartCoroutine(this.LoadNextStage());
	}

	public override void DoInit()
	{
		base.DoInit();
		this.RegistMonoBehaviour(519);
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		GUIPanelFactory instance = GUIPanelFactory.Instance;
		int num = 0;
		float[] array = new float[] { 0f, 0f, 2f, 2f, 34f, 34f };
		array[0] = (float)(Screen.width / 2 - 16);
		array[1] = (float)(Screen.height / 2 - 16);
		this.image_ = instance.CreateByWindowSpace(num, array, fiaTexture, 2, new Vector3(2f, 2f, 16f));
		this.panelManager_.RegistGUIInterface(this.image_);
	}

	public void ProgressAnimation()
	{
		int uv = this.image_.UV;
		this.image_.UV = (uv + 1) % 12;
		if (this.panelManager_.Update())
		{
			this.panelManager_.UpdateMesh(ref this.mesh_);
		}
	}

	private IEnumerator LoadNextStage()
	{
		KartManager.Instance.parameter_.Stage = GameLoadingStageStaticVariable.prevStage_;
		KartManager.Instance.parameter_.Stage = GameLoadingStageStaticVariable.nextStage_;
		StageController.Instance.SetBgm(KartManager.Instance.parameter_.Stage);
		yield return Application.LoadLevelAdditiveAsync(KartDefine.STAGE_SCNE_NAME[(int)GameLoadingStageStaticVariable.nextStage_]);
		base.CancelInvoke("ProgressAnimation");
		global::UnityEngine.Object.Destroy(this.cam_.gameObject);
		yield break;
	}

	public static BetweenSceneStage instance_;

	private GUIPanelEx image_;
}
