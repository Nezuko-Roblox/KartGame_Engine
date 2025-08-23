import type { AnySystem } from "@rbxts/matter";
import { Debugger, Loop, World } from "@rbxts/matter";
import Plasma from "@rbxts/plasma";
import { type Context, HotReloader } from "@rbxts/rewire";

import { $env } from "rbxts-transform-env";
import { Renderable } from "shared/ecs/components";
import type { ClientState } from "shared/ecs/constants/client-state";

const RunService = game.GetService("RunService");
const UserInputService = game.GetService("UserInputService");
const TextChatService = game.GetService("TextChatService");

/**
 * 启动 Matter ECS 框架的核心函数.
 *
 * @template S - 游戏状态对象类型.
 * @param containers - 包含系统模块的容器数组.
 * @param state - 游戏状态对象.
 * @returns 返回一个函数，用于应用插件并返回世界实例.
 */
export function start<S extends object>(
	containers: Array<Instance>,
	state: S,
): (...plugins: Array<(world: World, state: S) => void>) => World {
	const world = new World();
	const production = $env.string("ENV") === "production";

	// 设置调试器
	const myDebugger = new Debugger(Plasma);
	myDebugger.findInstanceFromEntity = (id): Model | undefined => {
		if (!world.contains(id)) {
			return;
		}

		const model = world.get(id, Renderable);

		return model ? model.model : undefined;
	};

	myDebugger.authorize = (player: Player): boolean => {
		const groupId = $env.number("GROUP_ID");
		const studio = RunService.IsStudio();
		if (groupId === undefined) {
			return studio;
		}

		const role = player.GetRoleInGroup(groupId);
		return studio || role === "Admin" || role === "Owner";
	};

	// 创建游戏循环

	const loop = new Loop(world, state, myDebugger.getWidgets());

	// 设置热重载
	const hotReloader = new HotReloader();
	let firstRunSystems: Array<AnySystem> | undefined = new Array<AnySystem>();
	const systemsByModule = new Map<ModuleScript, AnySystem>();

	/**
	 * 加载模块并注册系统.
	 *
	 * @param module_ - 要加载的模块脚本.
	 * @param context - 热重载上下文对象.
	 */
	function loadModule(module_: ModuleScript, context: Context): void {
		const { originalModule } = context;

		const [ok, system] = pcall(require, module_) as LuaTuple<[boolean, AnySystem]>;

		if (!ok) {
			warn("Error when hot-reloading system", module_.Name, system);
			return;
		}

		if (firstRunSystems) {
			firstRunSystems.push(system);
		} else if (systemsByModule.has(originalModule)) {
			loop.replaceSystem(systemsByModule.get(originalModule) as unknown as AnySystem, system);
			myDebugger.replaceSystem(
				systemsByModule.get(originalModule) as unknown as AnySystem,
				system,
			);
		} else {
			loop.scheduleSystem(system);
		}

		systemsByModule.set(originalModule, system);
	}

	/**
	 * 卸载模块并移除系统.
	 *
	 * @param _ - 未使用的模块脚本参数.
	 * @param context - 热重载上下文对象.
	 */
	function unloadModule(_: ModuleScript, context: Context): void {
		if (context.isReloading) {
			return;
		}

		const { originalModule } = context;
		if (systemsByModule.has(originalModule)) {
			loop.evictSystem(systemsByModule.get(originalModule) as unknown as AnySystem);
			systemsByModule.delete(originalModule);
		}
	}

	if (!production) {
		for (const container of containers) {
			hotReloader.scan(container, loadModule, unloadModule);
		}
	}

	loop.scheduleSystems(firstRunSystems);
	firstRunSystems = undefined;

	myDebugger.autoInitialize(loop);

	const events: {
		default: RBXScriptSignal;
		fixed?: RBXScriptSignal;
	} = RunService.IsClient()
		? {
				default: RunService.RenderStepped,
				fixed: RunService.Heartbeat,
			}
		: { default: RunService.Heartbeat };

	loop.begin(events);

	if (RunService.IsClient()) {
		UserInputService.InputBegan.Connect(input => {
			if (input.KeyCode === Enum.KeyCode.F4 && RunService.IsStudio()) {
				myDebugger.toggle();
				(state as ClientState).debugEnabled = !!(
					RunService.IsStudio() && myDebugger.enabled
				);
			} else {
				// 处理其他输入
			}
		});
		let matterOpenCmd = TextChatService.FindFirstChild("TextChatCommands")?.FindFirstChild(
			"MatterOpenCmd",
		) as TextChatCommand | undefined;
		if (matterOpenCmd === undefined) {
			matterOpenCmd = new Instance("TextChatCommand");
			matterOpenCmd.Name = "MatterOpenCmd";
			matterOpenCmd.PrimaryAlias = "/matter";
			matterOpenCmd.SecondaryAlias = "/matterdebug";
			matterOpenCmd.Triggered.Connect(() => {
				myDebugger.toggle();
			});
			matterOpenCmd.Parent = TextChatService.FindFirstChild("TextChatCommands");
		}
	}

	return function (...plugins: Array<(world: World, state: S) => void>): World {
		for (const plugin of plugins) {
			plugin(world, state);
		}

		return world;
	};
}
