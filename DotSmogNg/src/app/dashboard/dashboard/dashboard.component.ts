import { Component, inject, ViewChild, AfterViewInit } from '@angular/core';
import { ReadingsService } from '../../service/readings.service';
import { Messages, SensorMessage } from '../../model/SensorMessage';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { CommonModule, DatePipe } from '@angular/common';
import { MatSort, Sort, MatSortModule } from '@angular/material/sort';
import { FormsModule } from '@angular/forms';
import { DownloadService } from '../../service/download.service';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';
import { Chart, ChartData, ChartOptions, registerables } from 'chart.js/auto';
import 'chartjs-adapter-date-fns';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    MatPaginatorModule,
    MatTableModule,
    MatSortModule,
    CommonModule,
    FormsModule,
    MatSelectModule,
    MatInputModule,
    MatFormFieldModule,
    MatDatepickerModule,
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements AfterViewInit {
  displayedColumns: string[] = ['stationUUID', 'type', 'date', 'value'];
  sensorMessages: SensorMessage[] = [];
  uniqueStationUUIDs: string[] = [];
  dataSource = new MatTableDataSource<SensorMessage>(this.sensorMessages);
  chart: Chart<'line'> | undefined;

  dateFilter: string = '';
  typeFilter: string = '';
  stationUUIDFilter: string = '';

  private readingsService = inject(ReadingsService);
  private downloadService = inject(DownloadService);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor() {
    Chart.register(...registerables);
  }

  ngAfterViewInit() {
    this.fetchReadings();
  }

  ngOnInit() {
    //this.dataSource.filterPredicate = this.applyCustomFilter.bind(this);
  }

  fetchReadings(): void {
    console.log(this.dateFilter);
    this.readingsService
      .getReadings(
        this.typeFilter,
        this.convertDate(this.dateFilter),
        this.stationUUIDFilter
      )
      .subscribe({
        next: (response: Messages) => {
          this.sensorMessages = response.sensorMessages;
          this.dataSource.data = this.sensorMessages;
          this.dataSource.paginator = this.paginator;
          this.dataSource.sort = this.sort;
          this.sensorMessages.map((msg) => {
            msg.dateTime = this.convertDate(msg.dateTime);
          });
          this.uniqueStationUUIDs = Array.from(
            new Set(this.sensorMessages.map((msg) => msg.stationUUID))
          );
          this.updateChartData();
          if (this.chart) {
            this.chart.update();
          } else {
            this.renderChart();
          }
        },
        error: (error) => {
          console.error('Error fetching readings:', error);
        },
      });
  }

  filterData(): void {
    this.dataSource.filter = JSON.stringify({
      date: this.dateFilter,
      type: this.typeFilter,
      stationUUID: this.stationUUIDFilter,
    });
  }

  applyCustomFilter(data: SensorMessage, filter: string): boolean {
    const filterObj = JSON.parse(filter);
    return (
      (!filterObj.date || data.dateTime.startsWith(filterObj.date)) &&
      (!filterObj.type || data.type === filterObj.type) &&
      (!filterObj.stationUUID || data.stationUUID === filterObj.stationUUID)
    );
  }

  downloadCSV(): void {
    this.downloadService.downloadCSV(this.dataSource.filteredData);
  }

  downloadJSON(): void {
    this.downloadService.downloadJSON(this.dataSource.filteredData);
  }

  convertDate(date: string): string {
    return new DatePipe('en-US').transform(date, 'MM-dd-yyyy') || '';
  }

  convertStringToDate(date: string): Date {
    return new Date(date);
  }

  chartData: ChartData<'line'> = { labels: [], datasets: [] };

  updateChartData(): void {
    const filteredData = this.dataSource.filteredData;
    this.chartData = {
      labels: filteredData.map((data) => data.dateTime),
      datasets: [
        {
          label: 'Sensor Value',
          data: filteredData.map((data) => data.value),
          borderColor: '#42A5F5',
          fill: false,
        },
      ],
    };
  }

  renderChart(): void {
    this.updateChartData();
    if (this.chart === undefined) {
      this.chart = new Chart<'line'>('linechart', {
        type: 'line',
        data: this.chartData,
        options: {},
      });
    }
  }
}
