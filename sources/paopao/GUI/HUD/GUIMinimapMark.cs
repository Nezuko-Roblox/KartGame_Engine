using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUIMinimapMark : MonoBehaviourEx
{
	private void Awake()
	{
		base.gameObject.name = "minimap_mark_" + this.kartIndex_.ToString();
		this.RegistMonoBehaviour(32 + this.kartIndex_);
		this.mesh_ = base.GetComponent<MeshFilter>().mesh;
		this.material_ = new Material(ShaderHelper.noTextureShader_);
		MeshRenderer component = base.GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.material = this.material_;
		}
		base.gameObject.layer = LayerMask.NameToLayer("Minimap");
		if (this.kartIndex_ == KartManager.PLAYER_KART_IDX)
		{
			this.DefaultMeshSettingOwner();
		}
		else
		{
			this.DefaultMeshSettingOther();
		}
	}

	private void Start()
	{
	}

	private void DefaultMeshSettingOwner()
	{
		this.vertices_ = new Vector3[]
		{
			new Vector3(-16.5870285f, 0f, -16.3963757f),
			new Vector3(0.274272f, 0f, 21.5172558f),
			new Vector3(16.5870285f, 0f, -16.3963737f)
		};
		this.uvs_ = new Vector2[]
		{
			Vector2.zero,
			Vector2.zero,
			Vector2.zero
		};
		if (this.kartIndex_ >= GUIMinimapMark.markColor_.Length)
		{
			Debug.LogWarning(" kartIndex >= markColor_.Length");
		}
		int num = Mathf.Clamp(this.kartIndex_, 0, GUIMinimapMark.markColor_.Length - 1);
		this.colors_ = new Color[]
		{
			GUIMinimapMark.markColor_[num],
			GUIMinimapMark.markColor_[num],
			GUIMinimapMark.markColor_[num]
		};
		this.triangles_ = new int[] { 0, 1, 2 };
		this.mesh_.vertices = this.vertices_;
		this.mesh_.triangles = this.triangles_;
		this.mesh_.colors = this.colors_;
		this.mesh_.uv = this.uvs_;
		this.mesh_.RecalculateNormals();
	}

	private void DefaultMeshSettingOther()
	{
		this.vertices_ = new Vector3[]
		{
			new Vector3(6.780695f, 0f, 4.926467f),
			new Vector3(2.589996f, 0f, 7.97118f),
			new Vector3(-2.589998f, 0f, 7.971189f),
			new Vector3(-6.780699f, 0f, 4.926462f),
			new Vector3(-8.381405f, 0f, -1E-07f),
			new Vector3(-6.780698f, 0f, -4.926466f),
			new Vector3(-2.589995f, 0f, -7.971182f),
			new Vector3(2.589997f, 0f, -7.971183f),
			new Vector3(6.780696f, 0f, -4.926465f),
			new Vector3(8.381404f, 0f, 2f)
		};
		this.uvs_ = new Vector2[10];
		this.colors_ = new Color[10];
		if (this.kartIndex_ >= GUIMinimapMark.markColor_.Length)
		{
			Debug.LogWarning(" kartIndex >= markColor_.Length");
		}
		int num = Mathf.Clamp(this.kartIndex_, 0, GUIMinimapMark.markColor_.Length - 1);
		for (int i = 0; i < 10; i++)
		{
			this.uvs_[i] = Vector2.zero;
			this.colors_[i] = GUIMinimapMark.markColor_[num];
		}
		this.triangles_ = new int[24];
		int num2 = 0;
		for (int j = 0; j <= 7; j++)
		{
			this.triangles_[num2] = 0;
			this.triangles_[num2 + 1] = j + 2;
			this.triangles_[num2 + 2] = j + 1;
			num2 += 3;
		}
		this.mesh_.vertices = this.vertices_;
		this.mesh_.triangles = this.triangles_;
		this.mesh_.colors = this.colors_;
		this.mesh_.uv = this.uvs_;
		this.mesh_.RecalculateNormals();
	}

	private void Update()
	{
		Transform transform = KartManager.Instance.goKart_[this.kartIndex_].m_kart.transform;
		this.eulerAngle_.y = transform.localRotation.eulerAngles.y;
		this.rotation_.eulerAngles = this.eulerAngle_;
		base.transform.localRotation = this.rotation_;
		Vector3 localPosition = transform.localPosition;
		this.diffOffset = (float)((this.kartIndex_ != KartManager.PLAYER_KART_IDX) ? this.kartIndex_ : 6);
		localPosition.y = GUIMinimapMark.MINIMAP_OFFSET + this.diffOffset * 3f + 1f;
		base.transform.localPosition = localPosition;
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.SHOW_UI)
		{
			MonoBehaviourMessage1Param<bool> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<bool>)msg;
			if (monoBehaviourMessage1Param != null)
			{
				base.gameObject.SetActiveRecursively(monoBehaviourMessage1Param.param_);
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.PAUSE || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL2)
		{
			base.gameObject.SetActiveRecursively(false);
		}
		else if (msg.type_ == MonoBehaviourMessageType.RESUME)
		{
			base.gameObject.SetActiveRecursively(true);
		}
	}

	private Vector3[] vertices_;

	private Color[] colors_;

	private Vector2[] uvs_;

	private int[] triangles_;

	private Mesh mesh_;

	public int kartIndex_;

	private Material material_;

	private Quaternion rotation_ = Quaternion.identity;

	private Vector3 eulerAngle_ = Vector3.zero;

	public static float MINIMAP_OFFSET = -100f;

	private static Color[] markColor_ = new Color[]
	{
		new Color(0f, 0.5019608f, 0f),
		new Color(0.9019608f, 0.06666667f, 0.06666667f),
		new Color(0.08235294f, 0.443137258f, 0.733333349f),
		new Color(1f, 0.8117647f, 0.117647059f),
		new Color(0.647058845f, 0.0509803928f, 0.8156863f),
		new Color(0.3137255f, 0.9019608f, 0.9372549f)
	};

	private float diffOffset;
}
