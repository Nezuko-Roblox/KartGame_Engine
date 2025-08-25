using System;
using UnityEngine;

public class Skidmarks : MonoBehaviour
{
	private void Awake()
	{
		this.skidmarks = new markSection[256];
		for (int i = 0; i < 256; i++)
		{
			this.skidmarks[i] = new markSection();
		}
		if (base.GetComponent<MeshFilter>().mesh == null)
		{
			base.GetComponent<MeshFilter>().mesh = new Mesh();
		}
		this.mesh_ = base.GetComponent<MeshFilter>().mesh;
		for (int j = 0; j < 1536; j++)
		{
			this.triangles_[j] = 0;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public int AddSkidMark(Vector3 pos, Vector3 normal, float intensity, int lastIndex)
	{
		if (intensity > 1f)
		{
			intensity = 1f;
		}
		if (intensity < 0f)
		{
			return -1;
		}
		markSection markSection = this.skidmarks[this.numMarks % 256];
		markSection.pos = pos + normal * this.groundOffset;
		markSection.normal = normal;
		markSection.intensity = intensity;
		markSection.lastIndex = lastIndex;
		if (lastIndex != -1)
		{
			markSection markSection2 = this.skidmarks[lastIndex % 256];
			Vector3 vector = markSection.pos - markSection2.pos;
			Vector3 normalized = Vector3.Cross(vector, normal).normalized;
			markSection.posl = markSection.pos + normalized * this.markWidth * 0.5f;
			markSection.posr = markSection.pos - normalized * this.markWidth * 0.5f;
			markSection.tangent = new Vector4(normalized.x, normalized.y, normalized.z, 1f);
			if (markSection2.lastIndex == -1)
			{
				markSection2.tangent = markSection.tangent;
				markSection2.posl = markSection.pos + normalized * this.markWidth * 0.5f;
				markSection2.posr = markSection.pos - normalized * this.markWidth * 0.5f;
			}
		}
		this.numMarks++;
		this.updated = true;
		return this.numMarks - 1;
	}

	private void LateUpdate()
	{
		if (!this.updated)
		{
			return;
		}
		this.updated = false;
		this.mesh_.Clear();
		int num = 0;
		int num2 = 0;
		while (num2 < this.numMarks && num2 < 256)
		{
			if (this.skidmarks[num2].lastIndex != -1 && this.skidmarks[num2].lastIndex > this.numMarks - 256)
			{
				num++;
			}
			num2++;
		}
		num = 0;
		int num3 = 0;
		while (num3 < this.numMarks && num3 < 256)
		{
			if (this.skidmarks[num3].lastIndex != -1 && this.skidmarks[num3].lastIndex > this.numMarks - 256)
			{
				markSection markSection = this.skidmarks[num3];
				markSection markSection2 = this.skidmarks[markSection.lastIndex % 256];
				this.vertices_[num * 4] = markSection2.posl;
				this.vertices_[num * 4 + 1] = markSection2.posr;
				this.vertices_[num * 4 + 2] = markSection.posl;
				this.vertices_[num * 4 + 3] = markSection.posr;
				this.normals_[num * 4] = markSection2.normal;
				this.normals_[num * 4 + 1] = markSection2.normal;
				this.normals_[num * 4 + 2] = markSection.normal;
				this.normals_[num * 4 + 3] = markSection.normal;
				this.tangents_[num * 4] = markSection2.tangent;
				this.tangents_[num * 4 + 1] = markSection2.tangent;
				this.tangents_[num * 4 + 2] = markSection.tangent;
				this.tangents_[num * 4 + 3] = markSection.tangent;
				this.colors_[num * 4] = new Color(0f, 0f, 0f, markSection2.intensity);
				this.colors_[num * 4 + 1] = new Color(0f, 0f, 0f, markSection2.intensity);
				this.colors_[num * 4 + 2] = new Color(0f, 0f, 0f, markSection.intensity);
				this.colors_[num * 4 + 3] = new Color(0f, 0f, 0f, markSection.intensity);
				this.uvs_[num * 4] = new Vector2(0f, 0f);
				this.uvs_[num * 4 + 1] = new Vector2(1f, 0f);
				this.uvs_[num * 4 + 2] = new Vector2(0f, 1f);
				this.uvs_[num * 4 + 3] = new Vector2(1f, 1f);
				this.triangles_[num * 6] = num * 4;
				this.triangles_[num * 6 + 2] = num * 4 + 1;
				this.triangles_[num * 6 + 1] = num * 4 + 2;
				this.triangles_[num * 6 + 3] = num * 4 + 2;
				this.triangles_[num * 6 + 5] = num * 4 + 1;
				this.triangles_[num * 6 + 4] = num * 4 + 3;
				num++;
			}
			num3++;
		}
		this.mesh_.vertices = this.vertices_;
		this.mesh_.normals = this.normals_;
		this.mesh_.tangents = this.tangents_;
		this.mesh_.triangles = this.triangles_;
		this.mesh_.colors = this.colors_;
		this.mesh_.uv = this.uvs_;
	}

	protected const int maxMarks = 256;

	protected float markWidth = 0.275f;

	protected float groundOffset = 0.02f;

	protected float minDistance = 0.1f;

	private int indexShift;

	private int numMarks;

	private markSection[] skidmarks;

	private bool updated;

	private Mesh mesh_;

	private Vector3[] vertices_ = new Vector3[1024];

	private Vector3[] normals_ = new Vector3[1024];

	private Vector4[] tangents_ = new Vector4[1024];

	private Color[] colors_ = new Color[1024];

	private Vector2[] uvs_ = new Vector2[1024];

	private int[] triangles_ = new int[1536];
}
