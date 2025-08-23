import type { AnyEntity, World } from "@rbxts/matter";
import { useEvent } from "@rbxts/matter";

import { $env } from "rbxts-transform-env";
import { Client, PlayerAdmin, PlayerState } from "shared/ecs/components";
import { PlayerGameState } from "shared/ecs/constants/player-state";
import { SystemPriority } from "shared/ecs/constants/system-priority";
import { loadPlayer } from "shared/ecs/utils/load-player";

const RunService = game.GetService("RunService");
const Players = game.GetService("Players");

/**
 * 添加玩家系统 - 处理玩家连接和断开连接事件.
 *
 * @param world - Matter ECS 世界实例.
 */
function addPlayers(world: World): void {
	for (const player of Players.GetPlayers()) {
		if (player.GetAttribute("id") === undefined) {
			const playerId = world.spawn(
				Client({
					player,
				}),
				PlayerState({
					state: PlayerGameState.Connected,
				}),
			);
			(async (): Promise<void> => {
				loadPlayer(playerId as AnyEntity, player, world);
				const groupId = $env.number("GROUP_ID");
				const studio = RunService.IsStudio();
				if (studio) {
					world.insert(playerId as AnyEntity, PlayerAdmin());
					return;
				}

				if (groupId !== undefined) {
					const role = player.GetRoleInGroup(groupId);
					if (role === "Admin" || role === "Owner") {
						world.insert(playerId as AnyEntity, PlayerAdmin());
					}
				}
			})();

			print("Spawning player", player.Name, "with entity", playerId);
			player.SetAttribute("id", playerId);
		}
	}

	for (const [, player] of useEvent(Players, "PlayerRemoving")) {
		// Upon player disconnection, first set the player state to disconnected
		const playerId = player.GetAttribute("id") as AnyEntity | undefined;
		if (playerId === undefined) {
			continue;
		}

		if (world.contains(playerId)) {
			print("Disconnecting player", player.Name);
			world.insert(playerId, PlayerState({ state: PlayerGameState.Disconnected }));
		} else {
			// If the player has not been assigned an entity, then they have not
			// been fully initialized and we can just despawn them
			print("Despawning player", player.Name);
			world.despawn(playerId);
		}
	}
}

/** 导出添加玩家系统，优先级为关键. */
export = {
	priority: SystemPriority.CRITICAL,
	system: addPlayers,
};
