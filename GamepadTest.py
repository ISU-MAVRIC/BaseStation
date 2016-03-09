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
    if abs(joystick.get_axis(axis))>0.2:
        Value = joystick.get_axis(axis)
    else:
        Value = 0.0
    return Value

def getButtonVal():
    return 0


ser = serial.Serial('COM5', 9600)  # open first serial port
print ser.portstr       # check which port was really used
ser.write("hello")      # write a string
#ser.close()



while 1:
    pygame.event.get()


    x = getJoyVal(0)
    y = getJoyVal(1)

    ser.write("The value   :")
    ser.write(str(x))
    time.sleep(0.5)

    #ser.close()