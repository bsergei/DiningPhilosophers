import { StateDto } from './state-dto';

export class PhiloTable {
    startTime: Date;
    id: string;
    type: string;
    count: number;
    isRunning: boolean;
    isDeadlock: boolean;
    totalCycles: number;
    simulationTimeSeconds?: number;
    stateDtos: StateDto[];
}
