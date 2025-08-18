using System;
using UnityEngine;

public class FiaGUILayer : MonoBehaviourEx
{
	protected virtual void CameraSetting()
	{
		if (base.gameObject.layer == 0)
		{
			Transform transform = base.transform;
			while (transform.parent != null)
			{
				this.cam_ = transform.parent.camera;
				if (this.cam_ != null)
				{
					break;
				}
				transform = transform.parent;
			}
			if (this.cam_ == null)
			{
			}
			for (int i = 0; i < 32; i++)
			{
				if (((this.cam_.cullingMask >> i) & 1) != 0)
				{
					base.gameObject.layer = i;
					break;
				}
			}
		}
		else
		{
			int num = 1 << base.gameObject.layer;
			foreach (Camera camera in Camera.allCameras)
			{
				if ((camera.cullingMask & num) != 0)
				{
					this.cam_ = camera;
					break;
				}
			}
		}
	}

	public virtual void Initialize()
	{
		this.panelManager_ = new GUIPanelManager();
		this.CameraSetting();
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

	public virtual void DoInit()
	{
	}

	protected virtual void Start()
	{
		MeshRenderer component = base.GetComponent<MeshRenderer>();
		if (component != null)
		{
			foreach (Material material in component.materials)
			{
				if (material.mainTexture != null)
				{
					string text = string.Format("i18n/{0}/{1}", iOSUtil.Locale, material.mainTexture.name);
					Texture2D texture2D = (Texture2D)Resources.Load(text);
					if (texture2D != null)
					{
						material.mainTexture = texture2D;
					}
				}
			}
		}
		this.Initialize();
	}

	public bool IsInitialized()
	{
		return this.initialize_;
	}

	protected virtual void BeforePanelUpdate()
	{
	}

	protected virtual void AfterPanelUpdate()
	{
	}

	protected virtual void FirstUpdate()
	{
	}

	protected virtual void Update()
	{
		if (this.isFirstUpdate_)
		{
			this.FirstUpdate();
			this.isFirstUpdate_ = false;
		}
		if (!StageController.Instance.Lock)
		{
			this.BeforePanelUpdate();
		}
		if (this.panelManager_ != null && this.panelManager_.Update())
		{
			this.panelManager_.UpdateMesh(ref this.mesh_);
		}
		if (!StageController.Instance.Lock)
		{
			this.AfterPanelUpdate();
		}
	}

	protected Camera cam_;

	protected MeshRenderer meshRenderer_;

	protected GUIPanelManager panelManager_;

	protected Mesh mesh_;

	protected bool initialize_;

	protected bool isFirstUpdate_ = true;
}
