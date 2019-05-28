import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { StateDto } from '../domain/state-dto';

@Injectable()
export class PhiloTableService {

    constructor(private http: HttpClient) {
    }

    public startTable(tableType: string, philosophersCount?: number, simulationTimeSeconds?: number): Promise<string> {
        return this.http.get<string>(`https://${environment.apiServer}/api/table/start/${tableType}`, {
            params: {
                simulationTime: simulationTimeSeconds === undefined ? undefined : simulationTimeSeconds.toString(),
                philosophersCount: philosophersCount === undefined ? undefined : philosophersCount.toString()
            }
        })
            .toPromise();
    }

    public stopTable(tableId: string) {
        return this.http
            .get<string>(`https://${environment.apiServer}/api/table/stop/${tableId}`)
            .toPromise();
    }

    public async getStateDtos() {
        const url = `https://${environment.apiServer}/api/tablestate`;
        const dtos = await this.http.get<StateDto[]>(url).toPromise();
        return dtos.sort((a, b) => new Date(b.startTime).getTime() - new Date(a.startTime).getTime());
    }
}
