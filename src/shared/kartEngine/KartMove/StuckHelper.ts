/**
 * StuckHelper struct的TypeScript实现
 * 与Lua版本完全一致
 */

export class StuckHelper {
    public wallStuckTime: number = 0.0;
    public obstStuckTime: number = 0.0;
    public inStuck: boolean = false;
    public gndStuckTime: number = 0.0; // 这个字段在Initialize中没有设置，但在struct中存在

    constructor() {
        this.Initialize();
    }

    public Initialize(): void {
        this.wallStuckTime = 0.0;
        this.obstStuckTime = 0.0;
        this.inStuck = false;
        this.gndStuckTime = 0.0; // 这个字段在Initialize中没有设置，但在struct中存在
    }
}

export default StuckHelper;
