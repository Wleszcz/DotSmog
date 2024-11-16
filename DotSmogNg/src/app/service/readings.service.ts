import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Messages, SensorMessage } from '../model/SensorMessage';
import { HttpClient, HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class ReadingsService {

  private apiUrl = 'http://localhost:5275/api/readings'; // URL do API
  private http = inject(HttpClient);

  constructor() {}

  getReadings(type?: string, date?: string, stationUUID?: string): Observable<Messages> {
    let params = new HttpParams();
    
    if (type) params = params.set('type', type);
    if (date) params = params.set('date', date);
    if (stationUUID) params = params.set('stationUUID', stationUUID);

    return this.http.get<Messages>(this.apiUrl, { params });
  }
}
