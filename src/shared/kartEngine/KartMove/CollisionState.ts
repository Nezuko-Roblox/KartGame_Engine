/**
 * CollisionState struct的TypeScript实现
 * 与Lua版本完全一致，用于处理碰撞状态
 */

export class CollisionState {
    public kartCollide: boolean = false;
    public kartCollideVel: number = 0.0;
    public kartCollideDominant: boolean = false;
    public shock: boolean = false;
    public hop: boolean = false;
    public unmovingTime: number = 0.0;
    public shockVel: number = 0.0;

    constructor() {
        this.Initialize();
    }

    public Initialize(): void {
        this.kartCollide = false;
        this.kartCollideVel = 0.0;
        this.kartCollideDominant = false;
        this.shock = false;
        this.hop = false;
        this.unmovingTime = 0.0;
        this.shockVel = 0.0;
    }

    // 静态工厂方法
    static create(): CollisionState {
        return new CollisionState();
    }
}

export default CollisionState;