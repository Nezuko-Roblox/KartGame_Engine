import GoKartBuilder from "../KartMove/GoKartBuilder";
import { GoPlayKart } from "../KartMove/GoPlayKart";

export class GoPlayKartBuilder extends GoKartBuilder {
    constructor() {
        super();
    }

    public Build(): GoPlayKart {
        return new GoPlayKart();
    }
}

export default GoPlayKartBuilder;
