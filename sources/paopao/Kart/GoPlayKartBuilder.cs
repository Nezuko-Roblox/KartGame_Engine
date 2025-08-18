using System;

public class GoPlayKartBuilder : GoKartBuilder
{
	public override GoKart Build()
	{
		return new GoPlayKart();
	}
}
