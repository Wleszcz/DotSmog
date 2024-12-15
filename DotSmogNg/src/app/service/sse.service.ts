import { Injectable, NgZone } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SseService {
  private sseUrl = 'http://localhost:5275/sse';

  constructor(private zone: NgZone) {}

  getServerSentEvent(): Observable<any> {
    return new Observable((observer) => {
      const eventSource = new EventSource(this.sseUrl);

      eventSource.onmessage = (event) => {
        this.zone.run(() => {
          observer.next(JSON.parse(event.data));
        });
      };

      eventSource.onerror = (error) => {
        this.zone.run(() => {
          observer.error(error);
        });
      };

      return () => {
        eventSource.close();
      };
    });
  }
}
