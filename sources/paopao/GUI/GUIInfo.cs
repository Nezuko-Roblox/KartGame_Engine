using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIInfo : FiaGUILayer
{
	public override void DoInit()
	{
		StageController.Instance.BeginStage();
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		FiaTexture fiaTexture2 = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		this.mouseManager_ = new MouseManager();
		for (int i = 0; i < 4; i++)
		{
			GUIPanelEx[] array = this.buttons_;
			int num = i;
			GUIPanelFactory instance = GUIPanelFactory.Instance;
			int num2 = 0;
			float[] array2 = new float[] { 12f, 0f, 0f, 0f, 582f, 2f, 742f, 98f };
			array2[1] = (float)(78 + i * 100);
			array2[2] = array2[0] + array2[6] - array2[4];
			array2[3] = array2[1] + array2[7] - array2[5];
			array2[0] = array2[0] * (float)Screen.width / 800f;
			array2[1] = array2[1] * (float)Screen.height / 480f;
			array2[2] = array2[2] * (float)Screen.width / 800f;
			array2[3] = array2[3] * (float)Screen.height / 480f;
			array[num] = instance.CreateByWindowSpace(num2, array2, fiaTexture, 3, GUIFontCalculator.Y2_GAP);
			this.buttons_[i].RegistPanelManager(this.panelManager_);
			this.mouseManager_.Insert(i, this.buttons_[i]);
			GUIPanelEx[] array3 = this.buttonNames_;
			int num3 = i;
			GUIPanelFactory instance2 = GUIPanelFactory.Instance;
			int num4 = 0;
			float[] array4 = new float[] { 45f, 0f, 0f, 0f, 744f, 2f, 838f, 29f };
			array4[1] = (float)(110 + i * 100);
			array4[2] = array4[0] + array4[6] - array4[4];
			array4[3] = array4[1] + array4[7] - array4[5];
			array4[0] = array4[0] * (float)Screen.width / 800f;
			array4[1] = array4[1] * (float)Screen.height / 480f;
			array4[2] = array4[2] * (float)Screen.width / 800f;
			array4[3] = array4[3] * (float)Screen.height / 480f;
			array3[num3] = instance2.CreateByWindowSpace(num4, array4, fiaTexture, 3, GUIFontCalculator.Y2_GAP);
			this.buttonNames_[i].SetUV(i);
			this.buttonNames_[i].RegistPanelManager(this.panelManager_);
		}
		this.programVersion_ = string.Format("ver {0}", KartOptions.Instance.ProgramVersion);
		this.version_ = new GUIString(new Vector2(788f * (float)Screen.width / 800f, 35f * (float)Screen.height / 480f), 2, 15, GUIString.Alignment.RIGHT, 2, fiaTexture2);
		this.version_.SubMeshIndex = 1;
		this.panelManager_.RegistGUIInterface(this.version_);
		this.version_.SetColor(FiaColor.black);
		this.version_.SetString(this.programVersion_);
		this.main_ = new GUIButton(0, new float[]
		{
			12f, 6f, 190f, 62f, 583f, 233f, 709f, 582f, 349f, 772f,
			372f
		}, fiaTexture, 3, GUIFontCalculator.Y2_GAP);
		this.panelManager_.RegistGUIInterface(this.main_);
		this.mouseManager_.Insert(4, this.main_);
		this.mask_.SetActiveRecursively(false);
	}

	protected override void FirstUpdate()
	{
		this.selectedId_ = 0;
		for (int i = 0; i < 4; i++)
		{
			this.buttons_[i].SetUV((this.selectedId_ != i) ? 0 : 1);
			this.infos_[i].SetActiveRecursively(this.selectedId_ == i);
		}
	}

	protected override void BeforePanelUpdate()
	{
		this.mouseManager_.Update();
		int selectedKey = this.mouseManager_.GetSelectedKey();
		if (selectedKey != -1 && selectedKey != this.selectedId_ && MathHelper.IsBetweenIE(selectedKey, 0, 4))
		{
			this.infos_[this.selectedId_].SetActiveRecursively(false);
			this.selectedId_ = selectedKey;
			this.infos_[this.selectedId_].SetActiveRecursively(true);
			StageController.Instance.PlaySound(StageController.FxType.CLICK);
		}
		this.mask_.SetActiveRecursively(this.selectedId_ != 0);
		for (int i = 0; i < 4; i++)
		{
			this.buttons_[i].SetUV((this.selectedId_ != i) ? 0 : 1);
		}
		this.main_.SetPushed(this.mouseManager_.GetPushedKey() == 4);
	}

	protected override void AfterPanelUpdate()
	{
		int num = this.mouseManager_.GetSelectedKey();
		if (this.backButtonSelected)
		{
			this.backButtonSelected = false;
			num = 4;
		}
		if (num == 4)
		{
			this.ChangeToMain(string.Empty);
		}
	}

	public void ChangeToMain(string msg)
	{
		StageController.Instance.ChangeStage(StageType.MAIN);
		StageController.Instance.PlaySound(StageController.FxType.CLICK);
	}

	public void BackButtonAction(string msg)
	{
		this.backButtonSelected = true;
	}

	private const int BUTTON_NO = 4;

	private const int MOUSE_NOTIFIER_ID_MAIN = 4;

	private MouseManager mouseManager_;

	private GUIPanelEx[] buttons_ = new GUIPanelEx[4];

	private GUIPanelEx[] buttonNames_ = new GUIPanelEx[4];

	private GUIButton main_;

	public GameObject[] infos_;

	public GameObject mask_;

	private string programVersion_;

	private GUIString version_;

	private int selectedId_ = -1;

	private bool backButtonSelected;
}
