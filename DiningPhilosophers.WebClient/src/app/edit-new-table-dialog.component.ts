import { Component, Inject } from '@angular/core';
import {MatDialog, MatDialogRef, MAT_DIALOG_DATA} from '@angular/material';

export interface NewTableData {
  tableType: string;
  count: number;
  simulationTime: number;
}

@Component({
    selector: 'app-start-new-table-dialog-component',
    templateUrl: 'edit-new-table-dialog.component.html',
})
export class EditNewTableDialogComponent {

    public tableTypes: string[] = [
        'Problem',
        'Dijkstra',
        'Arbitrary',
        'Agile',
        'ChandyMisra'
    ];

    constructor(
        public dialogRef: MatDialogRef<EditNewTableDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: NewTableData) { }

    cancel(): void {
        this.dialogRef.close();
    }
}
