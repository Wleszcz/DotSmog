export interface SensorMessage {
  messageUUID: string;
  stationUUID: string;
  dateTime: string;
  type: string;
  value: number;
}

export interface Messages {
  sensorMessages: SensorMessage[];
}
