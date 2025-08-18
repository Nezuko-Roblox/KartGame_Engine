using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUISpeed : MonoBehaviour
{
	private void Start()
	{
		this.meshRenderer_ = base.GetComponent<MeshRenderer>();
		if (this.meshRenderer_ != null)
		{
			FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
			this.calc_ = GUIFontCalculator.CreateByWindowSpace(fiaTexture, new Vector2(19f, 23f), new Vector2(0f, 88f));
		}
		this.mainCam_ = CameraManager.Instance.mainCam_;
		this.panel_ = GUIPanel.CreateByWindowSpace(80f, 0f, 99f, 23f, 1);
		this.InitMeshData();
	}

	private void Update()
	{
	}

	private void InitMeshData()
	{
		this.vertices_ = new Vector3[this.panelCount_ * 4];
		this.panel_.GetVertices(ref this.vertices_, 0);
		this.colors_ = new Color[this.panelCount_ * 4];
		for (int i = 0; i < this.panelCount_ * 4; i++)
		{
			this.colors_[i] = Color.white;
		}
		this.uvs_ = new Vector2[this.panelCount_ * 4];
		this.calc_.GetUV(0, ref this.uvs_, 0);
		int[] array = new int[] { 0, 1, 2, 1, 3, 2 };
		this.triangles_ = new int[this.panelCount_ * 6];
		for (int j = 0; j < this.panelCount_; j++)
		{
			for (int k = 0; k < 6; k++)
			{
				this.triangles_[j * 6 + k] = array[k] + j * 4;
			}
		}
	}

	private void LateUpdate()
	{
		if (this.mainCam_ == null)
		{
			return;
		}
		Mesh mesh = base.GetComponent<MeshFilter>().mesh;
		mesh.Clear();
		Vector3[] array = new Vector3[this.panelCount_ * 4];
		for (int i = 0; i < this.panelCount_ * 4; i++)
		{
			array[i] = this.vertices_[i];
			Vector3[] array2 = array;
			int num = i;
			array2[num].z = array2[num].z + this.mainCam_.nearClipPlane;
			array[i] = this.mainCam_.ScreenToWorldPoint(array[i]);
		}
		mesh.vertices = array;
		mesh.triangles = this.triangles_;
		mesh.colors = this.colors_;
		mesh.uv = this.uvs_;
		mesh.RecalculateNormals();
	}

	private GUIFontCalculator calc_;

	private MeshRenderer meshRenderer_;

	private Camera mainCam_;

	private GUIPanel panel_;

	private int panelCount_ = 1;

	private Vector3[] vertices_;

	private Color[] colors_;

	private Vector2[] uvs_;

	private int[] triangles_;
}
