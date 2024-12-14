export interface SensorMessage {
  messageUUID: string;
  stationId: string;
  dateTime: string;
  type: string;
  value: number;
}

export interface Messages {
  sensorMessages: SensorMessage[];
}
