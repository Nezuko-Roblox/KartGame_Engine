/**
 * GoKartBuilder abstract class的TypeScript实现
 */

import { GoKart } from "./GoKart";

export abstract class GoKartBuilder {
    constructor() {
        // 基类构造函数
    }

    // 抽象方法 - 子类必须实现
    public abstract Build(): GoKart;
}

export default GoKartBuilder;
