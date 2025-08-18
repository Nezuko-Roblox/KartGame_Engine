using System;
using UnityEngine;

public class TestKartShadow : MonoBehaviour
{
	private void Start()
	{
		this.kartTransform_ = KartManager.Instance.goPlayKart_.m_kart.transform;
		this.size_ = ((BoxCollider)KartManager.Instance.goPlayKart_.m_kart.collider).size;
		float num = -this.size_.x * 0.5f;
		float num2 = this.size_.x * 0.5f;
		float num3 = this.size_.y * 0.5f;
		float num4 = -this.size_.y * 0.5f;
		this.shadowStartPoint_[0] = new Vector3(num * this.xScale_, 0.5f, num3 * this.yScale_);
		this.shadowStartPoint_[1] = new Vector3(num2 * this.xScale_, 0.5f, num3 * this.yScale_);
		this.shadowStartPoint_[2] = new Vector3(num * this.xScale_, 0.5f, num4 * this.yScale_);
		this.shadowStartPoint_[3] = new Vector3(num2 * this.xScale_, 0.5f, num4 * this.yScale_);
		this.shadowEndPoint_[0] = new Vector3(num * 1.5f, -20f, num3 * 1.5f);
		this.shadowEndPoint_[1] = new Vector3(num2 * 1.5f, -20f, num3 * 1.5f);
		this.shadowEndPoint_[2] = new Vector3(num * 1.5f, -20f, num4 * 1.5f);
		this.shadowEndPoint_[3] = new Vector3(num2 * 1.5f, -20f, num4 * 1.5f);
	}

	private void Update()
	{
		Mesh mesh = base.GetComponent<MeshFilter>().mesh;
		mesh.Clear();
		if (this.kartTransform_ == null)
		{
			return;
		}
		Vector3[] array = new Vector3[4];
		Vector3 vector = Vector3.zero;
		for (int i = 0; i < 4; i++)
		{
			Vector3 vector2 = this.kartTransform_.localToWorldMatrix.MultiplyPoint(this.shadowStartPoint_[i]);
			Vector3 vector3 = this.kartTransform_.localToWorldMatrix.MultiplyPoint(this.shadowEndPoint_[i]);
			RaycastHit raycastHit;
			if (!Physics.Linecast(vector2, vector3, out raycastHit, 256))
			{
				return;
			}
			array[i] = raycastHit.point + raycastHit.normal * 0.025f;
			vector += array[i];
		}
		if ((array[0] - array[2]).sqrMagnitude > 25f)
		{
			return;
		}
		if ((array[1] - array[2]).sqrMagnitude > 25f)
		{
			return;
		}
		vector *= 0.25f;
		for (int j = 0; j < 4; j++)
		{
			this.transformedVertices_[j] = vector + (array[j] - vector) * 1.25f;
		}
		mesh.vertices = this.transformedVertices_;
		mesh.triangles = this.triangles_;
		mesh.colors = this.colors_;
		mesh.uv = this.uvs_;
		mesh.RecalculateNormals();
	}

	private Mesh mesh_;

	private MeshRenderer meshRenderer_;

	private Material material_;

	private Vector3[] shadowStartPoint_ = new Vector3[4];

	private Vector3[] shadowEndPoint_ = new Vector3[4];

	private Transform kartTransform_;

	public float xScale_ = 1.2f;

	public float yScale_ = 1.22f;

	public Vector2 size_;

	public int kartIndex_;

	private Vector3[] transformedVertices_ = new Vector3[4];

	private Color[] colors_ = new Color[]
	{
		Color.white,
		Color.white,
		Color.white,
		Color.white
	};

	private Vector2[] uvs_ = new Vector2[]
	{
		new Vector2(0f, 1f),
		new Vector2(1f, 1f),
		new Vector2(0f, 0f),
		new Vector2(1f, 0f)
	};

	private int[] triangles_ = new int[] { 0, 1, 3, 0, 3, 2 };
}
