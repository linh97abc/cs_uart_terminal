import serial
import time

ser = serial.Serial("COM2")

while True:
    data = ser.read()

    if data:
        print(data)
        time.sleep(0.01)
        # if data == b'\n': ser.write(b'\n>> ')
        # else: ser.write(data);
