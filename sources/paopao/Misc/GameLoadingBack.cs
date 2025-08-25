using System;
using UnityEngine;

public class GameLoadingBack : FiaGUILayer
{
	public static GameLoadingStage Instance
	{
		get
		{
			return GameLoadingBack.instance_;
		}
		set
		{
			GameLoadingBack.instance_ = value;
		}
	}

	protected override void Start()
	{
	}

	public override void DoInit()
	{
		base.DoInit();
		this.RegistMonoBehaviour(519);
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		FiaTexture fiaTexture2 = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		FiaTexture fiaTexture3 = new FiaTexture(this.meshRenderer_.materials[2].mainTexture);
		FiaTexture fiaTexture4 = new FiaTexture(this.meshRenderer_.materials[3].mainTexture);
		GUIPanelFactory instance = GUIPanelFactory.Instance;
		int num = 0;
		float[] array = new float[] { 0f, 0f, 0f, 0f, 2f, 2f, 34f, 34f };
		array[0] = 742f * (float)Screen.width / 800f;
		array[1] = 422f * (float)Screen.height / 480f;
		array[2] = 774f * (float)Screen.width / 800f;
		array[3] = 454f * (float)Screen.height / 480f;
		this.image_ = instance.CreateByWindowSpace(num, array, fiaTexture, 3, new Vector3(2f, 2f, 16f));
		this.image_.SubMeshIndex = 0;
		GUIPanelFactory instance2 = GUIPanelFactory.Instance;
		int num2 = 0;
		float[] array2 = new float[] { 0f, 0f, 0f, 0f, 2f, 2f, 802f, 482f };
		array2[2] = (float)Screen.width;
		array2[3] = (float)Screen.height;
		this.bg_ = instance2.CreateByWindowSpace(num2, array2, fiaTexture2, 5, GUIFontCalculator.DEFAULT_GAP);
		this.bg_.SubMeshIndex = 1;
		int num3 = global::UnityEngine.Random.Range(0, 13);
		GUIPanelEx guipanelEx = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[6], fiaTexture3, 5, GUIFontCalculator.DEFAULT_GAP);
		if (num3 < 6)
		{
			GUIPanelFactory instance3 = GUIPanelFactory.Instance;
			int num4 = 0;
			float[] array3 = new float[] { 0f, 0f, 0f, 0f, 2f, 2f, 510f, 68f };
			array3[0] = 146f * (float)Screen.width / 800f;
			array3[1] = 100f * (float)Screen.height / 480f;
			array3[2] = 654f * (float)Screen.width / 800f;
			array3[3] = 166f * (float)Screen.height / 480f;
			this.desc1_ = instance3.CreateByWindowSpace(num4, array3, fiaTexture3, 2, new Vector3(2f, 2f, 106f));
			this.desc1_.SetUV(num3);
			this.desc1_.SubMeshIndex = 2;
			GUIPanelFactory instance4 = GUIPanelFactory.Instance;
			int num5 = 0;
			float[] array4 = new float[] { 0f, 0f, 0f, 0f, 2f, 410f, 510f, 582f };
			array4[0] = 146f * (float)Screen.width / 800f;
			array4[1] = 180f * (float)Screen.height / 480f;
			array4[2] = 654f * (float)Screen.width / 800f;
			array4[3] = 352f * (float)Screen.height / 480f;
			this.desc2_ = instance4.CreateByWindowSpace(num5, array4, fiaTexture3, 2, new Vector3(2f, 2f, 103f));
			this.desc2_.SetUV(num3);
			this.desc2_.SubMeshIndex = 2;
			guipanelEx.SubMeshIndex = 3;
		}
		else
		{
			GUIPanelFactory instance5 = GUIPanelFactory.Instance;
			int num6 = 0;
			float[] array5 = new float[] { 0f, 0f, 0f, 0f, 2f, 2f, 510f, 24f };
			array5[0] = 146f * (float)Screen.width / 800f;
			array5[1] = 122f * (float)Screen.height / 480f;
			array5[2] = 654f * (float)Screen.width / 800f;
			array5[3] = 144f * (float)Screen.height / 480f;
			this.desc1_ = instance5.CreateByWindowSpace(num6, array5, fiaTexture4, 2, GUIFontCalculator.DEFAULT_GAP);
			this.desc1_.SubMeshIndex = 3;
			GUIPanelFactory instance6 = GUIPanelFactory.Instance;
			int num7 = 0;
			float[] array6 = new float[] { 0f, 0f, 0f, 0f, 2f, 26f, 510f, 162f };
			array6[0] = 146f * (float)Screen.width / 800f;
			array6[1] = 180f * (float)Screen.height / 480f;
			array6[2] = 654f * (float)Screen.width / 800f;
			array6[3] = 316f * (float)Screen.height / 480f;
			this.desc2_ = instance6.CreateByWindowSpace(num7, array6, fiaTexture4, 2, new Vector3(2f, 2f, 107f));
			this.desc2_.SubMeshIndex = 3;
			this.desc2_.SetUV(num3 - 6);
			guipanelEx.SubMeshIndex = 2;
		}
		this.panelManager_.RegistGUIInterface(this.image_);
		this.panelManager_.RegistGUIInterface(this.bg_);
		this.panelManager_.RegistGUIInterface(this.desc1_);
		this.panelManager_.RegistGUIInterface(this.desc2_);
		this.panelManager_.RegistGUIInterface(guipanelEx);
	}

	public static GameLoadingStage instance_;

	private GUIPanelEx bg_;

	private GUIPanelEx desc1_;

	private GUIPanelEx desc2_;

	private GUIPanelEx image_;
}
