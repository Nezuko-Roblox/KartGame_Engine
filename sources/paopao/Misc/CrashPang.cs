using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class CrashPang : MonoBehaviour
{
	private void Start()
	{
		this.InitDefaultVertex();
		this.InitTransformList();
		this.mesh_ = base.GetComponent<MeshFilter>().mesh;
	}

	private void InitTransformList()
	{
		this.transformList_.Add(Matrix4x4.identity);
		this.transformList_.Add(Matrix4x4.Scale(new Vector3(3f, 3f, 3f)));
	}

	private void InitDefaultVertex()
	{
		float num = 1f;
		float num2 = 1f;
		for (int i = 0; i < 4; i++)
		{
			this.defaultVertices_[i].x = num * (float)((i % 2 != 0) ? 1 : (-1));
			this.defaultVertices_[i].y = 0f;
			this.defaultVertices_[i].z = num2 * (float)((i / 2 != 0) ? (-1) : 1);
			this.defaultVertices_[i].w = 1f;
		}
	}

	public void Run()
	{
		if (this.isRun_)
		{
			return;
		}
		this.isRun_ = true;
		this.currentFrameIdx = 0;
	}

	private void Update()
	{
		this.mesh_.Clear();
		if (!this.isRun_)
		{
			return;
		}
		if (this.currentFrameIdx >= this.transformList_.Count)
		{
			this.isRun_ = false;
			return;
		}
		Matrix4x4 matrix4x = (Matrix4x4)this.transformList_[this.currentFrameIdx];
		for (int i = 0; i < 4; i++)
		{
			this.transformedVertices_[i] = matrix4x * this.defaultVertices_[i];
		}
		this.mesh_.vertices = this.transformedVertices_;
		this.mesh_.triangles = this.triangles_;
		this.mesh_.colors = this.colors_;
		this.mesh_.uv = this.uvs_;
		this.mesh_.RecalculateNormals();
		this.currentFrameIdx++;
	}

	private ArrayList transformList_ = new ArrayList();

	private Vector4[] defaultVertices_ = new Vector4[4];

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

	private int currentFrameIdx;

	private bool isRun_;

	private Mesh mesh_;
}
