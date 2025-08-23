import { CmdrClient } from "@rbxts/cmdr";
import type { World } from "@rbxts/matter";

import { LocalClient, PlayerAdmin } from "shared/ecs/components";

import setLocalPlayer from "./set-local-player";

const TextChatService = game.GetService("TextChatService");

/**
 * 为具有管理员权限的玩家启用管理员功能.
 *
 * @param world -世界实体.
 */
function enableAdminFeatures(world: World) {
	for (const [id, _playerAdmin] of world.queryChanged(PlayerAdmin)) {
		if (!world.contains(id)) {
			continue;
		}

		const localClient = world.get(id, LocalClient);
		if (localClient === undefined) {
			continue;
		}

		CmdrClient.SetEnabled(true);
		let cmdrOpenCmd = TextChatService.FindFirstChild("TextChatCommands")?.FindFirstChild(
			"CmdrOpenCmd",
		) as TextChatCommand | undefined;
		if (cmdrOpenCmd === undefined) {
			cmdrOpenCmd = new Instance("TextChatCommand");
			cmdrOpenCmd.Name = "CmdrOpenCmd";
			cmdrOpenCmd.PrimaryAlias = "/cmdr";
			cmdrOpenCmd.SecondaryAlias = "/cmd";
			cmdrOpenCmd.Triggered.Connect(() => {
				CmdrClient.Toggle();
			});
			cmdrOpenCmd.Parent = TextChatService.FindFirstChild("TextChatCommands");
		}

		cmdrOpenCmd.Enabled = true;
	}
}

export = {
	after: [setLocalPlayer],
	system: enableAdminFeatures,
};
