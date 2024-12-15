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
  unfilteredSensorMessages: SensorMessage[] = [];
  uniquestationIds: string[] = [];
  uniquestationWithDataIds: string[] = [];
  dataSource = new MatTableDataSource<any>(this.sensorMessages);
  chart: Chart<'line'> | undefined;
  charts: { [key: string]: Chart } = {};
  sensorBalance: SensorBalance | undefined;
  selectedSensorRealTimeData:
    | { lastValue: number; averageValue: number }
    | undefined;
  dateFilter: string = '';
  typeFilter: string = '';
  stationIdFilter: string = '';
  stationIdBalanceFilter: string = '';
  stationIdRealTimeDataFilter: string = '';
  chartData: ChartData<'line'> = { labels: [], datasets: [] };

  public sensorDataMap: {
    [stationId: string]: { lastValue: number; averageValue: number };
  } = {};
  private sseSubscription: Subscription | undefined;
  public realTimeData: any;

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
    this.readingsService.getReadings().subscribe({
      next: (response: Messages) => {
        this.unfilteredSensorMessages = response.sensorMessages;
        this.unfilteredSensorMessages.map((msg) => {
          msg.dateTime = this.convertDate(msg.dateTime);
        });
      },
      error: (error) => {
        console.error('Error fetching readings:', error);
      },
    });

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
      });
  }

  ngOnDestroy() {
    if (this.sseSubscription) {
      this.sseSubscription.unsubscribe();
    }
  }

  fetchReadings(): void {
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
          this.dataSource.sortingDataAccessor = (item, property) => {
            switch (property) {
              case 'date':
                return new Date(item.dateTime);
              default:
                return item[property];
            }
          };
          this.dataSource.paginator = this.paginator;
          this.dataSource.sort = this.sort;
          this.uniquestationIds = Array.from(
            new Set(this.unfilteredSensorMessages.map((msg) => msg.stationId))
          );
          if (Object.keys(this.charts).length === 0) {
            this.renderCharts();
          }
          if (!this.chart) {
            this.renderChart();
          }
          this.updateCharts();
          this.updateChartData();
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
    return new DatePipe('en-US').transform(date, 'MM-dd-yyyy HH:mm:ss') || '';
  }

  convertStringToDate(date: string): Date {
    return new Date(date);
  }

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

    if (this.chart) {
      this.chart.data = this.chartData;
      this.chart.update();
    }
  }

  updateCharts(): void {
    const types = ['TYPE1', 'TYPE2', 'TYPE3', 'TYPE4'];
    types.forEach((type) => {
      if (this.charts[type]) {
        const filteredData = Object.values(
          this.unfilteredSensorMessages
        ).filter((data) => data.type === type);
        const labels = filteredData.map((data) => data.dateTime);
        const data = filteredData.map((data) => data.value);

        this.charts[type].data.labels = labels;
        this.charts[type].data.datasets[0].data = data;
        this.charts[type].update();
      }
    });
  }

  renderCharts(): void {
    const types = ['TYPE1', 'TYPE2', 'TYPE3', 'TYPE4'];
    types.forEach((type) => {
      const ctx = document.getElementById(
        `linechart-${type}`
      ) as HTMLCanvasElement;
      this.charts[type] = new Chart(ctx, {
        type: 'line',
        data: {
          labels: [],
          datasets: [
            {
              label: `Sensor Value - ${type}`,
              data: [],
              borderColor: '#42A5F5',
              fill: false,
            },
          ],
        },
      });
    });
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
