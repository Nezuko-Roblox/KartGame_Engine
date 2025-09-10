export class AdBoost {
    public validTrigger: boolean = false;
    public validTime: number = 0.0;
    public useLeftTime: number = 0.0;

    constructor() {
        this.Initialize();
    }

    public Initialize(): void {
        this.validTrigger = false;
        this.validTime = 0.0;
        this.useLeftTime = 0.0;
    }
}

export default AdBoost;