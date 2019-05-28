import { Component, NgZone, ChangeDetectorRef } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { SelectionModel } from '@angular/cdk/collections';
import { PhiloTable } from './domain/philo-table';
import { PhiloTableService } from './services/philo-table.service';
import { StateDto } from './domain/state-dto';
import { Subscription } from 'rxjs';
import { buffer, debounceTime, share } from 'rxjs/operators';
import { RealtimeService } from './services/realtime-service';
import { MatDialog } from '@angular/material';
import { EditNewTableDialogComponent, NewTableData } from './edit-new-table-dialog.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  providers: [
    PhiloTableService,
    RealtimeService
  ]
})
export class AppComponent {

  public dataSource = new MatTableDataSource<PhiloTable>([]);

  public selection = new SelectionModel<PhiloTable>(false, []);

  public isRefreshing: boolean;

  public isSelectedRunning: boolean;

  public get philoTables() {
    return this.philoTablesField;
  }

  public set philoTables(value: PhiloTable[]) {
    this.philoTablesField = value;
    this.dataSource = new MatTableDataSource<PhiloTable>(this.philoTablesField);
    this.selection.clear();
  }

  public displayedColumns = [
    'id',
    'startTime',
    'simulationTimeSeconds',
    'type',
    'count',
    'isRunning',
    'isDeadlock',
    'totalCycles'
  ];

  private philoTablesField: PhiloTable[] = [];
  private realtimeSubscription: Subscription;

  private newPhiloTable: NewTableData = {
    tableType: 'Problem',
    count: 10,
    simulationTime: 10
  };

  constructor(
    private philoTableService: PhiloTableService,
    private realtimeService: RealtimeService,
    private dialog: MatDialog,
    private zone: NgZone,
    private ref: ChangeDetectorRef) {
    this.refresh();

    this.selection.onChange.subscribe(r => {
      this.updateIsSelectedRunning();
    });
  }

  public addNewPhiloTable(): void {
    const dialogRef = this.dialog.open(EditNewTableDialogComponent, {
      width: '250px',
      data: { ...this.newPhiloTable }
    });

    dialogRef.afterClosed().subscribe(async result => {
      if (result) {
        this.newPhiloTable = result;

        const tableId = await this.philoTableService.startTable(
          this.newPhiloTable.tableType,
          this.newPhiloTable.count,
          this.newPhiloTable.simulationTime);

        const newPt: PhiloTable = {
          startTime: new Date(),
          id: tableId,
          count: this.newPhiloTable.count,
          isRunning: true,
          type: this.newPhiloTable.tableType,
          isDeadlock: false,
          totalCycles: 0,
          stateDtos: []
        };
        this.philoTables = [newPt, ...this.philoTables];
        this.selection.select(newPt);
      }
    });
  }

  public async refresh() {
    this.isRefreshing = true;
    try {
      const statusDtos = await this.philoTableService.getStateDtos();

      const groups = statusDtos.reduce<{ [tableId: string]: StateDto[] }>(
        (tableGroups, s) => {
          const tableId = s.tableId;
          tableGroups[tableId] = tableGroups[tableId] || [];
          tableGroups[tableId].push(s);
          return tableGroups;
        }, {});

      const result: PhiloTable[] = [];
      for (const tableId of Object.getOwnPropertyNames(groups)) {
        const phStates = groups[tableId];
        const pt = this.createPhiloTable(tableId, phStates);
        result.push(pt);
      }

      this.subscribeRealtime();

      this.philoTables = result;
    } finally {
      this.isRefreshing = false;
    }
  }

  public async stopPhiloTable() {
    const s = this.selection.selected;
    if (s && s.length === 1) {
      const selection = this.selection.selected[0];
      await this.philoTableService.stopTable(selection.id);
      const idx = this.philoTables.findIndex(v => v.id === selection.id);
      if (idx >= 0) {
        selection.isRunning = false;
        this.philoTables[idx].isRunning = false;
      }
    }
  }

  public updateIsSelectedRunning() {
    this.isSelectedRunning = !this.selection.selected
      || this.selection.selected.length === 0
      || !this.selection.selected[0].isRunning;
  }

  private subscribeRealtime() {
    if (this.realtimeSubscription) {
      this.realtimeSubscription.unsubscribe();
    }

    const stream = this.realtimeService.getStateDtoStream().pipe(share());
    this.realtimeSubscription = stream.pipe(
      buffer(stream.pipe(debounceTime(500))))
      .subscribe(ss => {
        for (const s of ss) {
          let pt = this.philoTables.find(v => v.id === s.tableId);
          if (pt) {
            const phIndex = pt.stateDtos.findIndex(v => v.philosopherId === s.philosopherId);
            if (phIndex >= 0) {
              pt.stateDtos[phIndex] = s;
            } else {
              pt.stateDtos.push(s);
            }
            this.actualize(pt);
          } else {
            pt = this.createPhiloTable(s.tableId, [s]);
            this.philoTables = [pt, ...this.philoTables];
          }
        }
        this.zone.run(() => {
          this.ref.detectChanges();
        });
      });
  }

  private createPhiloTable(tableId: string, stateDtos?: StateDto[]) {
    const pt: PhiloTable = {
      startTime: new Date(),
      id: tableId,
      stateDtos: stateDtos || [],
      count: 0,
      isDeadlock: false,
      isRunning: false,
      totalCycles: 0,
      type: ''
    };
    this.actualize(pt);
    return pt;
  }

  private actualize(pt: PhiloTable) {
    const ss = pt.stateDtos || [];
    pt.isRunning = ss.every(v => !v.endTime);
    pt.isDeadlock = ss.some(v => v.isDeadlockDetected);
    pt.type = ss.length === 0 ? '' : ss[0].tableType;
    pt.count = ss.length;
    pt.totalCycles = ss.reduce((sum, s) => {
      sum = sum + s.totalStats.totalCycles;
      return sum;
    }, 0);
    if (ss.length === 0) {
      pt.simulationTimeSeconds = undefined;
    } else {
      const s = ss[0];
      pt.startTime = new Date(s.startTime);
      if (s.endTime) {
        pt.simulationTimeSeconds = (new Date(s.endTime).getTime() - new Date(s.startTime).getTime()) / 1000;
      } else {
        pt.simulationTimeSeconds = (new Date().getTime() - new Date(s.startTime).getTime()) / 1000;
      }
    }

    if (this.selection.selected
      && this.selection.selected.length === 1
      && this.selection.selected[0] === pt) {
      this.selection.deselect(pt);
      this.selection.select(pt);
    }
  }
}
