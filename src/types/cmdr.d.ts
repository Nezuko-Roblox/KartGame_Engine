import type { CommandContext } from "@rbxts/cmdr";
import type { World } from "@rbxts/matter";

declare module "@rbxts/cmdr" {
	interface CommandContextWithWorld extends CommandContext {
		world?: World;
	}
}
