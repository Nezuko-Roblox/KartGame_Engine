using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUILoading : FiaGUILayer
{
	public override void DoInit()
	{
		StageController.Instance.BeginStage();
		this.RegistMonoBehaviour(1024);
		this.mainTex = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		this.fontTex = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		this.mouseManager_ = new MouseManager();
		GUIPanelFactory instance = GUIPanelFactory.Instance;
		int num = 0;
		float[] array = new float[] { 0f, 0f, 0f, 0f, 2f, 2f, 802f, 482f };
		array[2] = (float)Screen.width;
		array[3] = (float)Screen.height;
		this.back_ = instance.CreateByWindowSpace(num, array, this.mainTex, 5, GUIFontCalculator.DEFAULT_GAP);
		this.back_.RegistPanelManager(this.panelManager_);
		this.message_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 300f, 405f, 2f, 1f, 195f, 29f }, this.fontTex, 3, GUIFontCalculator.DEFAULT_GAP);
		this.message_.SubMeshIndex = 1;
		this.message_.RegistPanelManager(this.panelManager_);
		this.message_.Visible = false;
	}

	protected override void BeforePanelUpdate()
	{
		this.mouseManager_.Update();
	}

	private void UpdateFlickering()
	{
		this.mouseManager_.Insert(0, this.back_);
		if (this.message_ != null)
		{
		}
	}

	protected override void AfterPanelUpdate()
	{
		if (this.isReadyToMoveNext_ && this.mouseManager_.GetSelectedKey() != -1)
		{
			if (NativeHelper.buildType == "SKT")
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nexon.kartriderrush.android.core.natives"))
				{
					androidJavaClass.CallStatic<int>("requestGamecenter", new object[] { string.Empty });
				}
			}
			StageController.Instance.ChangeStage(StageType.MAIN);
		}
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.SIMPLE_MESSAGE)
		{
			MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)msg;
			if (monoBehaviourMessage2Param.lparam_ == 0)
			{
				if (NativeHelper.buildType == "SKT")
				{
					using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nexon.kartriderrush.android.core.natives"))
					{
						int num = androidJavaClass.CallStatic<int>("checkLte", new object[0]);
					}
				}
				this.isReadyToMoveNext_ = true;
				base.Invoke("UpdateFlickering", 0.1f);
			}
		}
	}

	private MouseManager mouseManager_;

	private GUIPanelEx back_;

	private GUIPanelEx message_;

	private FiaTexture mainTex;

	private FiaTexture fontTex;

	private bool isReadyToMoveNext_;

	private int i;
}
