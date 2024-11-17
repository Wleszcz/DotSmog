import json
import random
import time
import uuid
import time
from datetime import datetime, timedelta
from enum import Enum
import pika

USERNAME = 'guest'
PASSWORD = 'guest'
PORT = 5672

startDate = datetime(2024, 10, 20)

class Type(Enum):
    TYPE1 = 0
    TYPE2 = 1
    TYPE3 = 2
    TYPE4 = 3

class Station:
    def __init__(self, stationId, st_type, value, last_message):
        self.stationId = stationId
        self.type = st_type
        self.value = value
        self.lastMessage = last_message

class Message:
    def __init__(self, messageUUID, station, dateTime):
        self.messageUUID = messageUUID
        self.station = station
        self.dateTime = dateTime

    def to_JSON(self):
        return {
            'messageUUID': str(self.messageUUID),
            'stationId': str(self.station.stationId),
            'dateTime': str(self.dateTime),
            'type': self.station.type.name,
            'value': self.station.value
        }

station_uuids = ['0x30f8131C921E7dcb2e3763d45B6c893C18401345', '0xC800AFd98f1b4B871b54787149cA3BD3874f53Cf',
                 '0x4E61FbBC81f9E6BA5520b4FdaE779Cbc368F2e47', '0x5aC5504414311a5abe7299F55F7A30eCb010Cd09',
                 '1b8227fc-7729-4319-b094-1c4afda6d020', '6a5497ae-c8c4-45b0-b233-d9ef1e979837',
                 '0d952ffe-faca-4c67-ba28-94cc227860c3', '87090854-7b03-4c57-a49e-0d46a35abb0a',
                 'a139eb4f-8017-4c42-8556-f5f1c33ab041', '710a49a9-39c6-45ab-bc5b-e6cbfa6a240b',
                 'f959ce51-5e57-4a22-a711-8620324704e4', '8ee1c2a9-cad9-4ffc-af8a-41c66031092e',
                 '854d1ecd-6fb8-4120-bcb4-e0d34ba2eede', 'c68ee8f2-6345-465c-a00c-2e99c148d7f4',
                 'cdb05bb6-13a6-48c8-9494-0c675eae79d4', 'ab2bf1e4-3c5d-42df-bd6e-04d5ceba2eecb']

stations = []
connection = None

def initConnection():
    global connection
    credentials = pika.PlainCredentials(USERNAME, PASSWORD)
    connection = pika.BlockingConnection(
        pika.ConnectionParameters(host='localhost', port=PORT, credentials=credentials))


def sendMessage(mess):
    time.sleep(5)
    global connection
    channel = connection.channel()
    channel.queue_declare(queue='sensorQueue')
    channel.basic_publish(exchange='', routing_key='sensorQueue', body=json.dumps(mess.to_JSON()))
    channel.close()

def initStations():
    for i in range(len(station_uuids)):
        uuid = station_uuids[i]
        type = Type(i % len(Type))
        station = Station(uuid, type, None, startDate)
        stations.append(station)

def prepareMessage(station, dateTime):
    messageUUID = uuid.uuid4()
    formatted_datetime = dateTime.strftime('%Y-%m-%dT%H:%M:%S')
    message = Message(messageUUID, station, formatted_datetime)
    print(message.to_JSON())
    return message

def setLastMessage(station):
    if station.type == Type.TYPE1:
        station.lastMessage = station.lastMessage + timedelta(hours=1)
    elif station.type == Type.TYPE2:
        station.lastMessage = station.lastMessage + timedelta(hours=2)
    elif station.type == Type.TYPE3:
        station.lastMessage = station.lastMessage + timedelta(hours=3)
    elif station.type == Type.TYPE4:
        station.lastMessage = station.lastMessage + timedelta(hours=4)

initStations()
initConnection()

lastDate =[startDate, startDate, startDate, startDate]

print("Witaj w generatorze danych :)")
print("Generuj dane dla wszystkich stacji - a")
print("Generuj dane dla stacji numer uuid - s")
print("Zakończ program - q")
user_input = input("Wybierz opcję (a/s): ")
station_found = None

while True:
    if user_input == 'a':
        for station in stations:
            station.value = random.randint(0, 30)
            setLastMessage(station)
            sendMessage(prepareMessage(station, station.lastMessage))
    elif user_input == 's':
        if(station_found == None):
            uuid_input = input("Podaj UUID stacji: ")
            for station in stations:
                if station.stationId == uuid_input:
                    station_found = station
                    break
        if station_found:
            station_found.value = random.randint(0, 30)
            setLastMessage(station_found)
            sendMessage(prepareMessage(station_found, station_found.lastMessage))
        else:
            print("Nie znaleziono stacji o podanym UUID.")
    elif user_input == 'q':
        print("Zatrzymywanie generatora...")
        break
    else:
        print("Niepoprawny wybór, spróbuj ponownie.")
