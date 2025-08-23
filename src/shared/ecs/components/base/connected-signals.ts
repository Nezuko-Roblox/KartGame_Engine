import { component } from "@rbxts/matter";

/** 追踪 RbxScriptConnections 和其他信号以便正确释放的方法. */
export const ConnectedSignals = component<{ signals: Array<RBXScriptConnection> }>(
	"ConnectedSignals",
	{
		signals: [],
	},
);
export type ConnectedSignals = ReturnType<typeof ConnectedSignals>;
