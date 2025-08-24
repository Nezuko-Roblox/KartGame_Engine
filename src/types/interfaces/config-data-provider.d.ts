import type { AllTables } from "types/configs/all-tables";

export interface ConfigDataProvider {
	GetConfig<K extends keyof AllTables>(
		tableName: K,
		key: string,
	): AllTables[K] extends { dataMap: Record<string, infer V> } ? V : never;
	GetTable<K extends keyof AllTables>(tableName: K): AllTables[K];
}
