import type { AnyEntity, World } from "@rbxts/matter";

import { ServerTime } from "shared/ecs/components";
import { SystemPriority } from "shared/ecs/constants/system-priority";

const Workspace = game.GetService("Workspace");
/** 服务器时间实体 ID. */
let timeEntity: AnyEntity | undefined;

/**
 * 同步服务器时间到本地世界.
 *
 * @param world - 世界实体.
 */
function syncServerTime(world: World) {
	let currentServerTime = 0;
	const currentTime = tick();
	try {
		currentServerTime = Workspace.GetServerTimeNow();
	} catch {
		// ignore
	}

	if (timeEntity === undefined || !world.contains(timeEntity)) {
		timeEntity = world.spawn(
			ServerTime({ lastSync: currentTime, serverTime: currentServerTime }),
		) as AnyEntity;
	} else {
		world.insert(
			timeEntity,
			ServerTime({ lastSync: currentTime, serverTime: currentServerTime }),
		);
	}
}

export = {
	priority: SystemPriority.CRITICAL,
	system: syncServerTime,
};
