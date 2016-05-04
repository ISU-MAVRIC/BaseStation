__author__ = 'Danny'

import time
import pygame
import serial

pygame.init()
pygame.joystick.init()
done = False

joystick = pygame.joystick.Joystick(0)
joystick.init()

def getJoyVal( axis ):
    "this function returns the value of a joystick, given an axis and a joystick number"
    if abs(joystick.get_axis(axis))>0.1:
        Value = int(round(127*joystick.get_axis(axis)))+127
    else:
        Value = 127
    #map (-1 to 1) to (0.10 to 0.20) for PWM
    #pwm = chr(Value)
    #fpwm = '{:4.4}'.format(pwm)     #for sending direct pwm value
    return Value

def getButtonVal():
    return 0

ser = serial.Serial('COM9', 9600)  # open first serial port
#print ser.portstr       # check which port was really used
ser.write("<@@@>")      # write a string
#ser.close()



while 1:
    pygame.event.get()
    my_bytes = bytearray()
    my_bytes.append(60)
    my_bytes.append(getJoyVal(1))
    my_bytes.append(getJoyVal(3))
    my_bytes.append(40)
    my_bytes.append(62)
    x1 = getJoyVal(0)
    y1 = getJoyVal(1)
    x2 = getJoyVal(4)
    y2 = getJoyVal(3)
    #z = '<'+chr(y1)+chr(y2)+'a'+'>'
    #z = bytes(0x3c)+bytes(0x40)+bytes(0x56)+bytes(0x55)+bytes(0x3E)
    #z=[float(x1),float(y1),float(x2),float(y2)]




    #ser.write("<")
    #ser.write(str(z))
    print my_bytes
    ser.write(my_bytes)
    time.sleep(.1)

    #ser.close()