import { Injectable } from '@angular/core';
import { SensorMessage } from '../model/SensorMessage';

@Injectable({
  providedIn: 'root',
})
export class DownloadService {
  constructor() {}

  downloadCSV(data: SensorMessage[]): void {
    const csvData = this.convertToCSV(data);
    const blob = new Blob([csvData], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.setAttribute('download', `sensor_data_${Date.now()}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  downloadJSON(data: SensorMessage[]): void {
    const jsonData = JSON.stringify(data, null, 2);
    const blob = new Blob([jsonData], {
      type: 'application/json;charset=utf-8;',
    });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.setAttribute('download', `sensor_data_${Date.now()}.json`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  private convertToCSV(data: SensorMessage[]): string {
    const headers = ['Date', 'Sensor Type', 'Sensor Instance', 'Value'];
    const rows = data.map((message) => [
      message.dateTime,
      message.type,
      message.stationId,
      message.value,
    ]);

    const csvContent = [
      headers.join(','),
      ...rows.map((row) => row.join(',')),
    ].join('\n');

    return csvContent;
  }
}
