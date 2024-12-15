import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class DataService {
  private apiUrl = 'http://localhost:5275/api/data';

  constructor(private http: HttpClient) {}

  getData(
    stationType?: string,
    date?: string,
    stationId?: string,
    limit?: string,
    sortBy?: string,
    sortOrder?: string
  ): Observable<any> {
    let params = new HttpParams();
    if (stationType) params = params.set('stationType', stationType);
    if (date) params = params.set('date', date);
    if (stationId) params = params.set('stationId', stationId);
    if (limit) params = params.set('limit', limit);
    if (sortBy) params = params.set('sortBy', sortBy);
    if (sortOrder) params = params.set('sortOrder', sortOrder);

    return this.http.get<any>(this.apiUrl, { params });
  }

  exportData(
    exportType: string,
    stationType?: string,
    date?: string,
    stationId?: string,
    limit?: string,
    sortBy?: string,
    sortOrder?: string
  ): Observable<Blob> {
    let params = new HttpParams();
    if (stationType) params = params.set('stationType', stationType);
    if (date) params = params.set('date', date);
    if (stationId) params = params.set('stationId', stationId);
    if (limit) params = params.set('limit', limit);
    if (sortBy) params = params.set('sortBy', sortBy);
    if (sortOrder) params = params.set('sortOrder', sortOrder);
    params = params.set('export', exportType);

    return this.http.get(this.apiUrl, { params, responseType: 'blob' });
  }
}
