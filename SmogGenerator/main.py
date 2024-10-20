import json
import random
import time
import uuid
from enum import Enum

import pika
from pyexpat.errors import messages

USERNAME = 'guest'
PASSWORD = 'guest'
PORT = 5672

class Type(Enum):
    TYPE1 = 0
    TYPE2 = 1
    TYPE3 = 2
    TYPE4 = 3


class Station:
    def __init__(self, stationUUID, st_type, value):
        self.stationUUID = stationUUID
        self.type = st_type
        self.value = value

class Message:
    def __init__(self, messageUUID, station):
        self.messageUUID = messageUUID
        self.station = station

    def to_JSON(self):
        return {
            'messageUUID': str(self.messageUUID),
            'stationUUID': str(self.station.stationUUID),
            'type': self.station.type.name,
            'value': self.station.value
        }

station_uuids = ['cf7ede9d-eb8e-426f-a101-999d7425c26e', '4ca91a6d-9c17-4b0c-82c0-4399c2f65adb',
              '3b2ee09e-f753-4124-b403-f862c3628f88', 'bf8ab9d0-8894-4d0a-9616-7e989add1ef9',
              '1b8227fc-7729-4319-b094-1c4afda6d020', '6a5497ae-c8c4-45b0-b233-d9ef1e979837',
              '0d952ffe-faca-4c67-ba28-94cc227860c3', '87090854-7b03-4c57-a49e-0d46a35abb0a',
              'a139eb4f-8017-4c42-8556-f5f1c33ab041', '710a49a9-39c6-45ab-bc5b-e6cbfa6a240b',
              'f959ce51-5e57-4a22-a711-8620324704e4', '8ee1c2a9-cad9-4ffc-af8a-41c66031092e',
              '854d1ecd-6fb8-4120-bcb4-e0d34ba2eede', 'c68ee8f2-6345-465c-a00c-2e99c148d7f4',
              'cdb05bb6-13a6-48c8-9494-0c675eae79d4', 'ab2bf1e4-3c5d-42df-bd6e-04d5ceba2fcb']
stations=[]
connection = None

def initConnection():
    global connection
    credentials = pika.PlainCredentials(USERNAME, PASSWORD)
    connection = pika.BlockingConnection(
        pika.ConnectionParameters(host='localhost', port=PORT, credentials=credentials))

def sendMessage(mess):
    global connection
    channel= connection.channel()
    channel.queue_declare(queue='sensorQueue')
    channel.basic_publish(exchange='', routing_key='sensorQueue', body=json.dumps(mess.to_JSON()))
    channel.close()

def initStations():
    for i in range(len(station_uuids)):
        uuid = station_uuids[i]
        type = Type( i% len(Type))
        station = Station(uuid, type, None)
        stations.append(station)

initStations()
initConnection()

while True:
    sleep_time = random.uniform(1, 5)  # Losowa liczba zmiennoprzecinkowa od 1 do 5
    print(f"Sleeping for {sleep_time:.2f} seconds...")
    time.sleep(sleep_time)
    station = stations[random.randint(0,len(stations)-1)]
    station.value = random.randint(0,30)
    messageUUID = uuid.uuid4()
    message = Message(messageUUID, station)
    sendMessage(message)


