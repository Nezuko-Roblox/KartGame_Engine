using System;
using UnityEngine;

public class GUIScrollBar : FiaGUILayer
{
	protected virtual GUIPanelEx3Part CreateGUIPanel(FiaTexture mainTex)
	{
		return null;
	}

	public bool Visible
	{
		get
		{
			return this.scrollBar_.Visible;
		}
		set
		{
			this.scrollBar_.Visible = value;
		}
	}

	public override void Initialize()
	{
		this.panelManager_ = new GUIPanelManager();
		float num = -1000f;
		foreach (Camera camera in Camera.allCameras)
		{
			if (camera.depth > num && string.Compare(camera.tag, "guicam", true) == 0)
			{
				this.cam_ = camera;
				num = camera.depth;
			}
		}
		for (int j = 0; j < 32; j++)
		{
			if (((this.cam_.cullingMask >> j) & 1) != 0)
			{
				base.gameObject.layer = j;
				break;
			}
		}
		this.meshRenderer_ = base.GetComponent<MeshRenderer>();
		this.meshRenderer_.castShadows = false;
		this.meshRenderer_.receiveShadows = false;
		this.panelManager_.SetCamera(this.cam_);
		this.DoInit();
		this.mesh_ = base.GetComponent<MeshFilter>().mesh;
		this.mesh_.Clear();
		this.panelManager_.Update();
		this.panelManager_.UpdateMesh(ref this.mesh_);
		this.initialize_ = true;
	}

	public override void DoInit()
	{
		this.meshRenderer_.material = this.info_.material_;
		this.scrollBar_ = this.CreateGUIPanel(this.info_.texture_);
		this.scrollBar_.RegistPanelManager(this.panelManager_);
		this.Recalculate();
	}

	public virtual void Recalculate()
	{
	}

	public virtual void SetScrollPosition(float minRange, float maxRange, float range, float moveFactor)
	{
	}

	protected override void Start()
	{
	}

	protected override void Update()
	{
	}

	protected GUIPanelEx3Part scrollBar_;

	public ScrollBarInfo info_;

	protected float barLength_;
}
