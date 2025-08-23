import type { AnyEntity, World } from "@rbxts/matter";

import { PlayerSave, PlayerSettings } from "../components";
import type { CurrentPlayerSave } from "../constants/player-save";

/**
 * 异步加载玩家数据并在世界中创建玩家实体.
 *
 * @param id - 玩家实体的唯一标识符.
 * @param player - 玩家对象.
 * @param world - Matter ECS 世界实例.
 */
export async function loadPlayer(id: AnyEntity, player: Player, world: World): Promise<void> {
	const saveDefault: CurrentPlayerSave = {
		lastLogin: 0,
		lastSave: 0,
		settings: {
			musicEnabled: true,
			musicVolume: 0.5,
			sfxVolume: 0.5,
		},
		version: 1,
	};
	// let saveJSON: CurrentPlayerSave = { ...saveDefault };
	// try {
	// 	const save = PlayerDataStore.GetAsync<string>("Save");

	// 	const json = HttpService.JSONDecode(save[0] ?? "{}") as CurrentPlayerSave;

	// 	saveJSON = {
	// 		...json,
	// 		lastLogin: os.time(),
	// 		lastSave: json.lastSave,
	// 		version: CurrentSaveVersion,
	// 	};
	// } catch {}

	const { settings } = saveDefault;

	world.insert(
		id,
		PlayerSettings({
			musicEnabled: settings.musicEnabled,
			musicVolume: settings.musicVolume,
			sfxVolume: settings.sfxVolume,
		}),
		PlayerSave({ playerId: player.UserId, save: saveDefault }),
	);
}
