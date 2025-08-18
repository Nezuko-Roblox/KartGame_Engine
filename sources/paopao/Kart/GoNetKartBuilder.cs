using System;

public class GoNetKartBuilder : GoKartBuilder
{
	public override GoKart Build()
	{
		return new GoSmoothNetKart();
	}
}
