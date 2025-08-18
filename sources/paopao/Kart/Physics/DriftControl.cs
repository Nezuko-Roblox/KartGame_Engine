using System;

public struct DriftControl
{
	public void Initialize()
	{
		this.slipMode = false;
		this.slipTime = 0f;
		this.forceSlip = false;
		this.trigger = false;
		this.triggerTime = 0f;
	}

	public override string ToString()
	{
		return string.Concat(new string[]
		{
			this.slipMode.ToString(),
			" ",
			this.slipTime.ToString(),
			" ",
			this.forceSlip.ToString(),
			" ",
			this.trigger.ToString(),
			" ",
			this.triggerTime.ToString()
		});
	}

	public bool slipMode;

	public float slipTime;

	public bool forceSlip;

	public bool trigger;

	public float triggerTime;
}
