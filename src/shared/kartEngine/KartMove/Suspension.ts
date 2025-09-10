/**
 * Suspension struct的TypeScript实现
 * 与Lua版本完全一致
 */

import { Vector2, UnityVector3 } from "../KartShared/UnityMath";

export class Suspension {
    public wheelOff: Vector2[] = [];
    public wheelContact: boolean[] = [];
    public wheelContactN: UnityVector3[] = [];
    public maxTravel: number = 1.5; // 大幅增加悬挂行程，让车体离地更高
    public travel: number[] = []; // 改为空数组，在Initialize中设置1基数组
    public deltaTravel: number[] = []; // 改为空数组，在Initialize中设置1基数组
    
    // 添加遗漏的contactN字段
    public contactN: UnityVector3 = UnityVector3.zero;

    constructor() {
        this.Initialize();
    }

    private InitVector2(v: Vector2, x: number, y: number): void {
        v.X = x;
        v.Y = y;
    }

    public Initialize(): void {
        // 初始化为1基数组，与Lua版本一致
        this.wheelOff = [];
        for (let i = 1; i <= 4; i++) {
            this.wheelOff[i] = new Vector2();
            // 注意：这里的计算需要用 (i-1) 来匹配Lua的逻辑
            const xVal = ((i - 1) % 2 !== 0) ? 1.0 : -1.0;
            const yVal = (math.floor((i - 1) / 2) !== 0) ? -1.0 : 1.0;
            this.InitVector2(this.wheelOff[i]!, xVal, yVal);
        }
        
        this.wheelContact = [];
        for (let i = 1; i <= 4; i++) {
            this.wheelContact[i] = false;
        }
        
        this.wheelContactN = [];
        for (let i = 1; i <= 4; i++) {
            this.wheelContactN[i] = new UnityVector3();
        }
        
        this.maxTravel = 1.5; // 大幅增加悬挂行程，让车体离地更高
        
        // 初始化travel和deltaTravel为1基数组
        this.travel = [];
        this.deltaTravel = [];
        for (let i = 1; i <= 4; i++) {
            this.travel[i] = 1.5;
            this.deltaTravel[i] = 0.0;
        }
        
        // 添加遗漏的contactN字段
        this.contactN = new UnityVector3(0, 0, 0);
    }
}

export default Suspension;
