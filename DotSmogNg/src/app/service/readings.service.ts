import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Messages, SensorMessage } from '../model/SensorMessage';
import { SensorBalance } from '../model/SensorBalance';
import { HttpClient, HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class ReadingsService {
  private apiUrl = 'http://localhost:5275/api'; // URL do API
  private http = inject(HttpClient);

  constructor() {}

  getReadings(
    type?: string,
    date?: string,
    stationId?: string
  ): Observable<Messages> {
    let params = new HttpParams();

    if (type) params = params.set('type', type);
    if (date) params = params.set('date', date);
    if (stationId) params = params.set('stationId', stationId);

    return this.http.get<Messages>(this.apiUrl + '/readings', { params });
  }

  getBalance(accountId: string): Observable<SensorBalance> {
    return this.http.get<SensorBalance>(
      this.apiUrl + '/balance' + `/${accountId}`
    );
  }
}
