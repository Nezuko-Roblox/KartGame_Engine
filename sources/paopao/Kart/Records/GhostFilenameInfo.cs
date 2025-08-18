using System;

public class GhostFilenameInfo
{
	public GhostFilenameInfo()
	{
		this.version_ = 1;
		this.trackIdx_ = 0;
		this.kartIdx_ = 0;
		this.characterIdx_ = 0;
		this.finishTime_ = 0f;
		this.serialzedTime_ = 0L;
		this.filePath_ = string.Empty;
	}

	public GhostFilenameInfo(string filepath)
	{
		string text = FiaUtil.GetFileTitle(filepath);
		text = text.Replace('-', '/');
		byte[] array = Convert.FromBase64String(text);
		this.version_ = array[0];
		this.trackIdx_ = array[1];
		this.kartIdx_ = array[2];
		this.characterIdx_ = array[3];
		this.finishTime_ = BitConverter.ToSingle(array, 4);
		this.serialzedTime_ = BitConverter.ToInt64(array, 8);
		this.filePath_ = FiaUtil.GetFileName(filepath);
	}

	public bool IsValidData()
	{
		return !(this.filePath_ == string.Empty) && this.version_ == 1 && TrackAssetDefinitionManager.Instance.IsValidIndex((int)this.trackIdx_) && KartAssetDefinitionManager.Instance.IsValidIndex((int)this.kartIdx_) && CharacterAssetDefinitionManager.Instance.IsValidIndex((int)this.characterIdx_) && MathHelper.IsBetweenIE(this.finishTime_, 0f, 600f);
	}

	public string GenerateFilename()
	{
		byte[] array = new byte[16];
		array[0] = this.version_;
		array[1] = this.trackIdx_;
		array[2] = this.kartIdx_;
		array[3] = this.characterIdx_;
		Array.Copy(BitConverter.GetBytes(this.finishTime_), 0, array, 4, 4);
		Array.Copy(BitConverter.GetBytes(this.serialzedTime_), 0, array, 8, 8);
		string text = Convert.ToBase64String(array) + ".krg";
		return text.Replace('/', '-');
	}

	public byte version_;

	public byte trackIdx_;

	public byte kartIdx_;

	public byte characterIdx_;

	public float finishTime_;

	public long serialzedTime_;

	public string filePath_;
}
