export function loadModuleScript(module: ModuleScript): unknown {
	// roblox-ts 会将这个编译为正确的 require 调用
	// eslint-disable-next-line ts/no-require-imports -- 动态模块加载必须使用 require
	return require(module);
}
