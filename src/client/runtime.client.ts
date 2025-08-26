import { Flamework, Modding } from "@flamework/core";
import type { Logger } from "@rbxts/log";
import Log from "@rbxts/log";

import { GAME_NAME } from "shared/constants";
import { setupLogger } from "shared/functions/setup-logger";
import { initItemSystem } from "shared/ecs/items";

import { createApp, reactConfig } from "./ui/react-config";

function start(): void {
	reactConfig();
	setupLogger();

	Log.Info(`${GAME_NAME} client version: ${game.PlaceVersion}`);

	Modding.registerDependency<Logger>(ctor => Log.ForContext(ctor));

	Flamework.addPaths("src/client/controllers");

	Log.Info("Flamework ignite!");
	Flamework.ignite();

	// 初始化道具系统
	Log.Info("Initializing item system...");
	initItemSystem();

	createApp().catch(() => {
		Log.Fatal("Failed to create React app!");
	});
}

start();
