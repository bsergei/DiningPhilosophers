import { Injectable, NgZone } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { StateDto } from '../domain/state-dto';

@Injectable()
export class RealtimeService {
    constructor(private zone: NgZone) {
    }

    public getStateDtoStream(): Observable<StateDto> {
        return Observable.create(observer => {
            const ws = new WebSocket(`${environment.apiServerWsScheme}://${environment.apiServer}/api/realtime`);
            let pingTask: any;

            this.zone.runOutsideAngular(() => {
                pingTask = setInterval(() => ws.send('ping'), 2000);

                ws.onmessage = (msg) => {
                    const obj: StateDto = JSON.parse(msg.data);
                    observer.next(obj);
                };

                ws.onerror = err => {
                    console.error(err);
                    observer.error(err);
                };

                ws.onclose = (ev) => {
                    observer.complete();
                };
            });

            return () => {
                clearInterval(pingTask);
                ws.close();
            };
        });
    }
}
