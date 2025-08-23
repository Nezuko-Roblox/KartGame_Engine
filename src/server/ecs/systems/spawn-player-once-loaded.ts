import type { AnyEntity, World } from "@rbxts/matter";
import { useChange } from "@rbxts/matter-hooks";

import { ConfirmLoaded } from "server/ecs/network";
import { Client, PlayerSave, PlayerState } from "shared/ecs/components";
import { PlayerGameState } from "shared/ecs/constants/player-state";
import { SystemPriority } from "shared/ecs/constants/system-priority";
import { ResetTransparency, SetTransparency } from "shared/ecs/utils/transparency";

/**
 * 玩家加载完成后生成系统 - 处理玩家客户端加载确认和透明度控制.
 *
 * @param world - 世界实例.
 */
function spawnPlayerOnceLoaded(world: World) {
	if (useChange([])) {
		ConfirmLoaded.on(player => {
			for (const [id, client] of world.query(Client)) {
				if (
					player.GetAttribute("id") === id &&
					(client.loaded === undefined || !client.loaded)
				) {
					world.insert(id as AnyEntity, client.patch({ loaded: true }));
				}
			}
		});
	}

	for (const [_id, playerState, { loaded, player }] of world.query(
		PlayerState,
		Client,
		PlayerSave,
	)) {
		if (playerState.state === PlayerGameState.Connected) {
			if (loaded === true) {
				if (player.Character) {
					SetTransparency(player.Character, 0);
				}
			} else if (player.Character) {
				ResetTransparency(player.Character);
			}
		}
	}
}

/** 导出玩家加载完成后生成系统，优先级为关键. */
export = {
	priority: SystemPriority.CRITICAL,
	system: spawnPlayerOnceLoaded,
};
