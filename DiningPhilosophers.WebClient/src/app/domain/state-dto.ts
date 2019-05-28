import { TotalStats } from './total-stats';

// C#: DiningPhilosophers.Sim.Model.StateDto
export class StateDto {
    public tableId: string;

    public tableType: string;

    public philosopherId: number;

    public isDeadlockDetected: boolean;

    public startTime: string;

    public endTime?: string;

    public totalStats: TotalStats;
}
