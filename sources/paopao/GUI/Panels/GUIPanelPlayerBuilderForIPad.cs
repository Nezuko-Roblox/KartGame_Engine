using System;

public class GUIPanelPlayerBuilderForIPad : GUIPanelBuilder
{
	public override GUIPanelEx Build()
	{
		return new GUIIPadPlayerPanel();
	}
}
