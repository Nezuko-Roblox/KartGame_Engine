import type { AnyEntity, World } from "@rbxts/matter";
import { useEvent } from "@rbxts/matter";
import promiseR15 from "@rbxts/promise-character";

import { Client, PlayerModel, PlayerState } from "shared/ecs/components";
import { PlayerGameState } from "shared/ecs/constants/player-state";

import addPlayers from "./add-players";

const Players = game.GetService("Players");

/**
 * 重生玩家系统 - 处理玩家角色的生成、死亡和重生.
 *
 * @param world - Matter.js ECS 世界实例.
 */
function respawnPlayers(world: World) {
	for (const [id, playerState] of world.queryChanged(PlayerState)) {
		if (!playerState.new) {
			continue;
		}

		if (!world.contains(id as AnyEntity)) {
			continue;
		}

		const client = world.get(id, Client);
		if (client) {
			switch (playerState.new.state) {
				case PlayerGameState.Connected:
				case PlayerGameState.Reviving:
				case PlayerGameState.Spawning: {
					// Prevents respawn from running twice.
					if (playerState.old?.state === PlayerGameState.Connected) {
						continue;
					}

					if (playerState.old?.state !== playerState.new.state) {
						world.remove(id as AnyEntity, PlayerModel);
						(async () => {
							let loaded = false;
							while (!loaded) {
								try {
									if (client.player.Character !== undefined) {
										client.player.Character.Destroy();
										client.player.Character = undefined;
									}

									client.player.LoadCharacter();
									print("Spawning character", client.player.Name);
									loaded = true;
								} catch (err) {
									warn("Error loading character", err);
									if (
										!Players.GetPlayers().find(
											player => player === client.player,
										)
									) {
										// Player has disconnected, don't try to respawn
										break;
									}
								}
							}
						})();
					}

					break;
				}
				case PlayerGameState.Dead: {
					// No respawn logic needed for Dead state
					break;
				}
				case PlayerGameState.Disconnected: {
					// No respawn logic needed for Disconnected state
					break;
				}
				case PlayerGameState.Playing: {
					// No respawn logic needed for Playing state
					break;
				}
				case PlayerGameState.Ready: {
					// No respawn logic needed for Ready state
					break;
				}
				case PlayerGameState.Revived: {
					// No respawn logic needed for Revived state
					break;
				}
				default: {
					// Optionally handle unexpected states
					break;
				}
			}
		}
	}

	for (const [id, { player }, playerState] of world.query(Client, PlayerState)) {
		// Add PlayerModel component to connect players to their character
		for (const [, character] of useEvent(player, "CharacterAppearanceLoaded")) {
			(async () => {
				let model;
				try {
					model = await promiseR15(character);
				} catch (err) {
					warn("Error loading character, reloading!", err);
					player.LoadCharacter();
					return;
				}

				model.Humanoid.BreakJointsOnDeath = true;
				world.insert(
					id as AnyEntity,
					playerState.patch({
						state:
							playerState.state === PlayerGameState.Reviving
								? PlayerGameState.Revived
								: PlayerGameState.Playing,
					}),
				);
				world.insert(
					id as AnyEntity,
					PlayerModel({
						character: model,
						humanoid: model.Humanoid,
					}),
				);
				print("Character added", player.Name);
			})();
		}
	}

	for (const [id, { character, humanoid }, playerState] of world.query(
		PlayerModel,
		PlayerState,
	)) {
		// Make the PlayerModel component despawn on death to notify other systems
		for (const [, deathCount] of useEvent(humanoid, "Died")) {
			print("Character died", humanoid.DisplayName, deathCount);
			world.remove(id as AnyEntity, PlayerModel);
			world.insert(id as AnyEntity, playerState.patch({ state: PlayerGameState.Reviving }));
		}

		for (const [, health] of useEvent(humanoid, "HealthChanged")) {
			if (health <= 0) {
				for (const part of character.GetChildren()) {
					if (part.Name !== "Humanoid") {
						part.Destroy();
					}
				}
			}
		}
	}
}

/** 导出重生玩家系统，在添加玩家系统之后执行. */
export = {
	after: [addPlayers],
	system: respawnPlayers,
};
