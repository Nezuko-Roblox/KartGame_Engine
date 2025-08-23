import type { AnyComponent, AnyEntity, World } from "@rbxts/matter";
import { log } from "@rbxts/matter";

import { Replication } from "server/ecs/network";
import { Client } from "shared/ecs/components";
import { REPLICATED_COMPONENTS, REPLICATED_PLAYER_ONLY } from "shared/ecs/components/replicated";
import type { ComponentNames } from "shared/ecs/components/serde";

const Players = game.GetService("Players");
/** 变更日志类型定义 - 用于跟踪组件数据变化. */
type Changelog = Map<string, Map<ComponentNames, { data: AnyComponent }>>;
/** 全局变更记录映射. */
const changes = new Map<string, Changelog>();

/**
 * 复制系统 - 将组件变化同步到客户端.
 *
 * @param world - Matter ECS 世界实例.
 */
function replication(world: World) {
	for (const [playerId, client] of world.queryChanged(Client)) {
		if (!world.contains(playerId) || client.new === undefined) {
			continue;
		}

		const { loaded, player } = client.new;
		if (loaded === undefined || !loaded) {
			continue;
		}

		const playerPayload: Changelog = new Map<
			string,
			Map<ComponentNames, { data: AnyComponent }>
		>();
		for (const [entityId, entityData] of world) {
			const key = tostring(entityId);
			const entityPayload = new Map<ComponentNames, { data: AnyComponent }>();

			for (const [component, componentInstance] of entityData) {
				if (
					REPLICATED_COMPONENTS.has(component) ||
					(playerId === entityId && REPLICATED_PLAYER_ONLY.has(component))
				) {
					entityPayload.set(tostring(component) as ComponentNames, {
						data: componentInstance,
					});
				}
			}

			playerPayload.set(key, entityPayload);
		}

		log("Setting initial replication", playerId, player.Name, playerPayload);
		changes.set(tostring(player.UserId), playerPayload);
	}

	for (const component of REPLICATED_COMPONENTS) {
		for (const [entityId, record] of world.queryChanged(component)) {
			const key = tostring(entityId);
			const name = tostring(component) as ComponentNames;
			log("Replicated component changed", key, name, record.new);
			for (const player of Players.GetPlayers()) {
				const playerId = tostring(player.UserId);
				if (!changes.has(playerId)) {
					changes.set(playerId, new Map());
				}

				const playerChanges = changes.get(playerId);
				if (playerChanges !== undefined && world.contains(entityId as AnyEntity)) {
					if (!playerChanges.has(key)) {
						playerChanges.set(key, new Map());
					}

					playerChanges
						.get(key)
						?.set(name, { data: record.new as unknown as AnyComponent });
				}
			}
		}
	}

	for (const component of REPLICATED_PLAYER_ONLY) {
		for (const [entityId, record] of world.queryChanged(component)) {
			const key = tostring(entityId);
			const name = tostring(component) as ComponentNames;
			if (!world.contains(entityId as AnyEntity)) {
				continue;
			}

			const client = world.get(entityId, Client);
			if (client !== undefined) {
				const { player } = client;
				const playerId = tostring(player.UserId);
				if (!changes.has(playerId)) {
					changes.set(playerId, new Map());
				}

				const playerChanges = changes.get(playerId);
				if (playerChanges !== undefined && world.contains(entityId as AnyEntity)) {
					if (!playerChanges.has(key)) {
						playerChanges.set(key, new Map());
					}

					playerChanges
						.get(key)
						?.set(name, { data: record.new as unknown as AnyComponent });
				}

				log("Player only component changed", key, name, entityId);
			}
		}
	}

	for (const [, { loaded, player }] of world.query(Client)) {
		if (loaded === undefined || !loaded) {
			continue;
		}

		const currentChangelog = changes.get(tostring(player.UserId));
		if (currentChangelog !== undefined) {
			log("Sending replication changes to player", player.Name, currentChangelog);
			Replication.fire(player, currentChangelog);
			changes.delete(tostring(player.UserId));
		}
	}
}

/** 导出复制系统，优先级最高. */
export = {
	priority: math.huge,
	system: replication,
};
