using System;

public class GUIPanelPlayerMarkBuilderForIPad : GUIPanelBuilder
{
	public override GUIPanelEx Build()
	{
		return new GUIIPadPlayerMark();
	}
}
