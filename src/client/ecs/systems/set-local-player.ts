import type { World } from "@rbxts/matter";

import { Client, LocalClient } from "shared/ecs/components";
import { SystemPriority } from "shared/ecs/constants/system-priority";

const players: Players = game.GetService("Players");
const { LocalPlayer } = players;

/**
 * 设置本地玩家实体并更新应用状态.
 *
 * @param world -世界实体.
 */
function setLocalPlayer(world: World) {
	for (const [id, client] of world.queryChanged(Client)) {
		if (client.new?.player.UserId === LocalPlayer.UserId) {
			world.insert(id, LocalClient({ player: client.new.player }));
		}
	}
}

export = {
	priority: SystemPriority.CRITICAL,
	system: setLocalPlayer,
};
