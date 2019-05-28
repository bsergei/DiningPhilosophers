import { Component, OnInit, Input, OnChanges, SimpleChanges } from '@angular/core';
import { PhiloTable } from '../domain/philo-table';

@Component({
  selector: 'app-philosophers-chart',
  templateUrl: './philosophers-chart.component.html',
  styleUrls: ['./philosophers-chart.component.scss']
})
export class PhilosophersChartComponent implements OnInit, OnChanges {

  @Input() selection: PhiloTable[];

  @Input() type: string;

  public multi = [];

  constructor() { }

  ngOnInit() {
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes && (changes.selection !== undefined || changes.type !== undefined)) {
      this.multi = [];
      if (this.selection && this.selection.length > 0) {
        const stateDtos = this.selection[0].stateDtos;
        const sortedStateDtos = stateDtos.sort((a, b) => a.philosopherId - b.philosopherId);
        for (const stateDto of sortedStateDtos) {
          const chartData = {
            name: stateDto.philosopherId.toString(),
            series: [
            ]
          };

          this.multi.push(chartData);

          const ts = stateDto.totalStats;

          switch (this.type) {
            case 'events':
              {
                chartData.series.push({
                  name: 'Pick Up Forks',
                  value: ts.pickUpForksTime / ts.totalTime * 100.0
                });
                chartData.series.push({
                  name: 'Eat Spaghetti',
                  value: ts.eatSpaghettiTime / ts.totalTime * 100.0
                });
                chartData.series.push({
                  name: 'Put Down Forks',
                  value: ts.putDownForksTime / ts.totalTime * 100.0
                });
                chartData.series.push({
                  name: 'Think',
                  value: ts.thinkTime / ts.totalTime * 100.0
                });
              }
              break;
            case 'cycles':
              {
                chartData.series.push({
                  name: 'Total Cycles',
                  value: ts.totalCycles
                });
              }
              break;
          }
        }
      }
    }
  }
}
