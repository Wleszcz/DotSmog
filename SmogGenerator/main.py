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
                 '0xE98e85A65Ed1a6C17977202786c72AE8222EC119', '0xC2dCC02e6498EBB77D83521aFae8749020CfBC7B',
                 '0x4b33Ae4cFa207f196e3f7788745aF76E26A602D9', '0x133FF2cE0C9995E62674E030e362EC614c132Db4',
                 '0x5f897C984f4430df26394d53Cf00BC28F8112Eaf', '0xD567a72C8764e0EBDD5311f8b57DBe9997716E81',
                 '0x812196b8a1FE6B1230926384CF935dFF876C05BE', '0x4978Dc12658Ee8dA8Dc091C12d7f60f1794872Fa',
                 '0x14f03b5B903067efBe8F275f591B475236c7dCA6', '0x26a3a157E8F3B3568b3C316DC382F0F5C22d3Adc',
                 '0xe38918a4eC647A3c9668b83D7cB2072C5c50Bd55', '0x749Eb27aECd9aaDEa0717DB7B4fd2E15Eee4d706']

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
