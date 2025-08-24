import { HttpService, ReplicatedStorage } from "@rbxts/services";

import type { AllTables } from "types/configs/all-tables";
import { createAllTables } from "types/configs/all-tables";
import type { ConfigDataProvider as ConfigDataProviderInstance } from "types/interfaces/config-data-provider";

import { loadModuleScript } from "./module-loader";

class ConfigDataProvider implements ConfigDataProviderInstance {
	private _tables?: AllTables;

	public GetConfig<K extends keyof AllTables>(
		tableName: K,
		key: string,
	): AllTables[K] extends { dataMap: Record<string, infer V> } ? V : never {
		const configTable = this.GetTable(tableName);
		// 类型守卫，检查是否有 dataMap
		if ("dataMap" in configTable && typeIs(configTable.dataMap, "table")) {
			const dataMap = configTable.dataMap as Record<string, unknown>;
			const result = dataMap[key];
			if (result !== undefined) {
				return result as AllTables[K] extends { dataMap: Record<string, infer V> }
					? V
					: never;
			}
		}

		error(`Config not found: ${tableName as string}.${key}`);
	}

	public GetTable<K extends keyof AllTables>(tableName: K): AllTables[K] {
		const tables = this.GetTables();
		return tables[tableName];
	}

	public GetTables(): AllTables {
		if (this._tables) {
			return this._tables;
		}

		const loader = (fileName: string): unknown => {
			const configsFolder = ReplicatedStorage.FindFirstChild("configs");
			if (!configsFolder) {
				error("configs folder not found in ReplicatedStorage");
			}

			const jsonConfigsFolder = configsFolder.FindFirstChild("jsonConfigs");
			if (!jsonConfigsFolder) {
				error("jsonConfigs folder not found in configs");
			}

			// 查找对应的配置文件
			const configFile = jsonConfigsFolder.FindFirstChild(fileName);
			if (!configFile) {
				error(`Config file ${fileName}.json not found`);
			}

			// 根据文件类型加载配置
			if (configFile.IsA("ModuleScript")) {
				return loadModuleScript(configFile);
			}

			if (configFile.IsA("StringValue")) {
				return HttpService.JSONDecode(configFile.Value);
			}

			error(`Unknown config file type for ${fileName}`);
		};

		// 使用静态导入的 createAllTables
		this._tables = createAllTables(loader);
		return this._tables;
	}

	public _resetTables(tables: AllTables): void {
		this._tables = tables;
	}
}

export const configDataProvider = new ConfigDataProvider();
