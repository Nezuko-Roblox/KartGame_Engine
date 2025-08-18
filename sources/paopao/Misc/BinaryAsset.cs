using System;
using UnityEngine;

public class BinaryAsset : ScriptableObject
{
	public BinaryAsset(byte[] content)
	{
		this.content_ = content;
	}

	public byte[] content_;
}
