import { Component, OnInit, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { PhiloTable } from '../domain/philo-table';
import { MatTableDataSource, MatSort, MatTable } from '@angular/material';

interface DetailsData {
  id?: number;
  pickUpForksTime?: number;
  putDownForksTime?: number;
  eatSpaghettiTime?: number;
  thinkTime?: number;
  totalCycles?: number;
}

@Component({
  selector: 'app-philosophers-details',
  templateUrl: './philosophers-details.component.html',
  styleUrls: ['./philosophers-details.component.scss']
})
export class PhilosophersDetailsComponent implements OnInit , OnChanges {

  @Input() selection: PhiloTable[];

  displayedColumns = [
    'id',
    'pickUpForksTime',
    'putDownForksTime',
    'eatSpaghettiTime',
    'thinkTime',
    'totalCycles'
  ];

  dataSource = new MatTableDataSource<DetailsData>([]);
  total: DetailsData;

  @ViewChild('pSort') sort: MatSort;
  @ViewChild('pTable') table: MatTable<DetailsData>;

  constructor() {
  }

  ngOnInit() {
    this.dataSource.sort = this.sort;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes && changes.selection !== undefined) {
      if (this.selection && this.selection.length > 0) {
        // this.dataSource.data = this.selection[0].stateDtos;
        const details = this.getDetailsData();
        this.dataSource = new MatTableDataSource<DetailsData>(this.getDetailsData());
        this.total = this.getTotal(details);
      } else {
        // this.dataSource.data = [];
        this.dataSource = new MatTableDataSource<DetailsData>([]);
        this.total = {};
      }
      this.dataSource.sort = this.sort;
      this.table.renderRows();
    }
  }

  private getDetailsData(): DetailsData[] {
    return this.selection[0].stateDtos.map(s => {
      return {
        id: s.philosopherId,
        eatSpaghettiTime: s.totalStats.eatSpaghettiTime / s.totalStats.totalTime,
        pickUpForksTime: s.totalStats.pickUpForksTime / s.totalStats.totalTime,
        putDownForksTime: s.totalStats.putDownForksTime / s.totalStats.totalTime,
        thinkTime: s.totalStats.thinkTime / s.totalStats.totalTime,
        totalCycles: s.totalStats.totalCycles
      };
    });
  }

  private getTotal(details: DetailsData[]) {
    const total = {
      eatSpaghettiTime: 0,
      pickUpForksTime: 0,
      putDownForksTime: 0,
      thinkTime: 0,
      totalCycles: 0
    };

    for (const d of details) {
      total.eatSpaghettiTime += d.eatSpaghettiTime;
      total.pickUpForksTime += d.pickUpForksTime;
      total.putDownForksTime += d.putDownForksTime;
      total.thinkTime += d.thinkTime;
      total.totalCycles += d.totalCycles;
    }

    total.eatSpaghettiTime /= details.length;
    total.pickUpForksTime /= details.length;
    total.putDownForksTime /= details.length;
    total.thinkTime /= details.length;

    return total;
  }
}
