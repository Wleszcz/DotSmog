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
import { MatExpansionModule } from '@angular/material/expansion';
import { provideNativeDateAdapter } from '@angular/material/core';
import { Chart, ChartData, ChartOptions, registerables } from 'chart.js/auto';
import 'chartjs-adapter-date-fns';
import { SensorBalance } from '../../model/SensorBalance';
import { SensorRealTimeData } from '../../model/SensorRealTimeData';
import { Subscription } from 'rxjs';
import { SseService } from '../../service/sse.service';
import { DataService } from '../../service/data.service';

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
    MatExpansionModule,
  ],
  providers: [provideNativeDateAdapter(), DatePipe],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements AfterViewInit {
  displayedColumns: string[] = ['stationId', 'type', 'date', 'value'];
  sensorMessages: SensorMessage[] = [];
  uniquestationIds: string[] = [];
  uniquestationWithDataIds: string[] = [];
  dataSource = new MatTableDataSource<SensorMessage>(this.sensorMessages);
  chart: Chart<'line'> | undefined;
  sensorBalance: SensorBalance | undefined;
  selectedSensorRealTimeData:
    | { lastValue: number; averageValue: number }
    | undefined;
  dateFilter: string = '';
  typeFilter: string = '';
  stationIdFilter: string = '';
  chartData: ChartData<'line'> = { labels: [], datasets: [] };

  public sensorDataMap: {
    [stationId: string]: { lastValue: number; averageValue: number };
  } = {};
  private sseSubscription: Subscription | undefined;
  public realTimeData: any;

  // private readingsService = inject(ReadingsService);
  // private downloadService = inject(DownloadService);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private readingsService: ReadingsService,
    private downloadService: DownloadService,
    private sseService: SseService,
    private dataService: DataService,
    private datePipe: DatePipe
  ) {
    Chart.register(...registerables);
  }

  ngAfterViewInit() {
    this.fetchReadings();
  }

  ngOnInit() {
    this.sseSubscription = this.sseService
      .getServerSentEvent()
      .subscribe((data) => {
        this.realTimeData = data;
        this.updateSensorData(data);
        this.uniquestationWithDataIds = Array.from(
          new Set(
            Object.keys(this.sensorDataMap).filter((key) => {
              const data = this.sensorDataMap[key];
              return (
                data &&
                (data.lastValue !== undefined ||
                  data.averageValue !== undefined)
              );
            })
          )
        );
        console.log('Received real-time data:', data);
      });
    //this.dataSource.filterPredicate = this.applyCustomFilter.bind(this);
  }

  ngOnDestroy() {
    if (this.sseSubscription) {
      this.sseSubscription.unsubscribe();
    }
  }

  fetchReadings(): void {
    console.log(this.dateFilter);
    this.readingsService
      .getReadings(
        this.typeFilter,
        this.convertDate(this.dateFilter),
        this.stationIdFilter
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
          this.uniquestationIds = Array.from(
            new Set(this.sensorMessages.map((msg) => msg.stationId))
          );
          this.updateChartData();
          if (!this.chart) {
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
      stationId: this.stationIdFilter,
    });
  }

  applyCustomFilter(data: SensorMessage, filter: string): boolean {
    const filterObj = JSON.parse(filter);
    return (
      (!filterObj.date || data.dateTime.startsWith(filterObj.date)) &&
      (!filterObj.type || data.type === filterObj.type) &&
      (!filterObj.stationId || data.stationId === filterObj.stationId)
    );
  }

  // downloadCSV(): void {
  //   this.downloadService.downloadCSV(this.dataSource.filteredData);
  // }

  // downloadJSON(): void {
  //   this.downloadService.downloadJSON(this.dataSource.filteredData);
  // }

  downloadCSV(): void {
    const formattedDate = this.datePipe.transform(
      this.dateFilter,
      'yyyy-MM-ddTHH:mm:ss'
    );
    this.dataService
      .exportData(
        'csv',
        this.typeFilter,
        formattedDate ? formattedDate : '',
        this.stationIdFilter
      )
      .subscribe(
        (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = 'data.csv';
          a.click();
          window.URL.revokeObjectURL(url);
        },
        (error) => {
          if (error.status == 404) {
            alert('No data found for the selected filters');
          } else {
            console.error('Error downloading data: ', error);
          }
        }
      );
  }

  downloadJSON(): void {
    const formattedDate = this.datePipe.transform(
      this.dateFilter,
      'yyyy-MM-ddTHH:mm:ss'
    );
    this.dataService
      .exportData(
        'json',
        this.typeFilter,
        formattedDate ? formattedDate : '',
        this.stationIdFilter
      )
      .subscribe(
        (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = 'data.json';
          a.click();
          window.URL.revokeObjectURL(url);
        },
        (error) => {
          if (error.status == 404) {
            alert('No data found for the selected filters');
          } else {
            console.error('Error downloading data: ', error);
          }
        }
      );
  }

  convertDate(date: string): string {
    return new DatePipe('en-US').transform(date, 'MM-dd-yyyy') || '';
  }

  convertStringToDate(date: string): Date {
    return new Date(date);
  }

  updateChartData(): void {
    console.log('data: ', this.dataSource.filteredData);
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

    if (this.chart) {
      this.chart.data = this.chartData;
      this.chart.update('active');
    }
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

  checkStationBalance(stationId: string): void {
    this.readingsService.getBalance(stationId).subscribe({
      next: (response: SensorBalance) => {
        this.sensorBalance = response;
        console.log(`Station ${stationId} has a balance of ${response.value}`);
      },
      error: (error) => {
        console.error('Error fetching balance:', error);
      },
    });
  }

  checkStationRealTimeData(stationId: string): void {
    this.selectedSensorRealTimeData = this.sensorDataMap[stationId];
  }

  private updateSensorData(data: SensorRealTimeData): void {
    const stationId = data.stationId;
    if (!this.sensorDataMap[stationId]) {
      this.sensorDataMap[stationId] = {
        lastValue: 0,
        averageValue: 0,
      };
    }

    const sensorData = this.sensorDataMap[stationId];
    sensorData.lastValue = data.lastValue;
    sensorData.averageValue = data.averageValue;
  }
}
