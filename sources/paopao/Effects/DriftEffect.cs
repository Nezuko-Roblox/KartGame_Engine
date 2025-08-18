using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class DriftEffect : MonoBehaviour
{
	private void Start()
	{
		Vector3[] array = new Vector3[2];
		for (int i = 0; i < 2; i++)
		{
			array[i] = KartManager.Instance.goPlayKart_.GetWheelPos(i + 2) - KartManager.Instance.goPlayKart_.m_kart.transform.position;
		}
		for (int j = 0; j < 2; j++)
		{
			this.driftEffectElemArray_[j] = new DriftEffectElem(array[j], 0f, 0f, (j != 0) ? 0.2f : (-0.2f), DriftEffectElem.EffectType.TYPE1);
		}
		this.mesh_ = base.GetComponent<MeshFilter>().mesh;
	}

	private void FixedUpdate()
	{
		GoPlayKart goPlayKart_ = KartManager.Instance.goPlayKart_;
		bool flag = goPlayKart_.GetKartSpeed() > 50f && goPlayKart_.m_isDrift && goPlayKart_.m_Contact;
		this.UpdateVertex(1, flag);
	}

	private void LateUpdate()
	{
		int num = 0;
		foreach (DriftEffectElem driftEffectElem in this.driftEffectElemArray_)
		{
			num += driftEffectElem.GetSegmentCount();
		}
		this.mesh_.Clear();
		if (num <= 0)
		{
			return;
		}
		int num2 = 0;
		foreach (DriftEffectElem driftEffectElem2 in this.driftEffectElemArray_)
		{
			num2 = driftEffectElem2.UpdateVertexBuffer(ref this.vertices_, ref this.colors_, ref this.uvs_, ref this.triangles_, num2);
		}
		for (int k = num2 * 6; k < 120; k++)
		{
			this.triangles_[k] = 0;
		}
		this.mesh_.vertices = this.vertices_;
		this.mesh_.triangles = this.triangles_;
		this.mesh_.colors = this.colors_;
		this.mesh_.uv = this.uvs_;
		this.mesh_.RecalculateNormals();
	}

	private void UpdateVertex(int tick, bool make)
	{
		Transform transform = KartManager.Instance.goPlayKart_.m_kart.transform;
		Vector3 normalized = transform.forward.normalized;
		Vector3 normalized2 = KartManager.Instance.goPlayKart_.m_KartWLVel.normalized;
		DriftEffectElem driftEffectElem = this.driftEffectElemArray_[0];
		DriftEffectElem driftEffectElem2 = this.driftEffectElemArray_[1];
		Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
		Vector3 lossyScale = transform.lossyScale;
		ref Matrix4x4 ptr = ref localToWorldMatrix;
		int num2;
		int num = (num2 = 0);
		int num4;
		int num3 = (num4 = 0);
		float num5 = ptr[num2, num4];
		localToWorldMatrix[num, num3] = num5 / lossyScale.x;
		ref Matrix4x4 ptr2 = ref localToWorldMatrix;
		int num6 = (num4 = 1);
		int num7 = (num2 = 1);
		num5 = ptr2[num4, num2];
		localToWorldMatrix[num6, num7] = num5 / lossyScale.y;
		ref Matrix4x4 ptr3 = ref localToWorldMatrix;
		int num8 = (num2 = 2);
		int num9 = (num4 = 2);
		num5 = ptr3[num2, num4];
		localToWorldMatrix[num8, num9] = num5 / lossyScale.z;
		Quaternion quaternion = Quaternion.FromToRotation(Vector3.forward, normalized2);
		if (make && Vector3.Dot(normalized, normalized2) > 0f)
		{
			driftEffectElem.AddDriftMark2(KartManager.Instance.goPlayKart_.GetWheelPos(2), quaternion, 1f);
			driftEffectElem2.AddDriftMark2(KartManager.Instance.goPlayKart_.GetWheelPos(3), quaternion, 1f);
		}
		driftEffectElem.UpdateTransform(tick);
		driftEffectElem2.UpdateTransform(tick);
	}

	private DriftEffectElem[] driftEffectElemArray_ = new DriftEffectElem[2];

	private Mesh mesh_;

	private Vector3[] vertices_ = new Vector3[80];

	private Color[] colors_ = new Color[80];

	private Vector2[] uvs_ = new Vector2[80];

	private int[] triangles_ = new int[120];

	public static bool isMakeDriftForced;
}
