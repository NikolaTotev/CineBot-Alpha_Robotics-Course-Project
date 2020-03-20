import custom_sensor_driver.py
import json
import atexit

sensorData = []
shouldRead = True

i2c = busio.I2C(board.SCL, board.SDA)
sensor = MMA8451RAW(i2c)


def startSensor():
    while shouldRead:
        x, y, z = sensor.acceleration
        time.sleep(0.01)
        sensorData.append((x, y, z))
    serializeData()


def serializeData():
    print("Serializing data...")
    with open('/sensorData.json', 'w') as path:
        json.dump(sensorData, path, sort_keys=True)


atexit.register(serializeData())
