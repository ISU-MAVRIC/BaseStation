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
    #map (-1 to 1) to (0.10 to 0.20) for PWM
    pwm = (0.05*Value)+0.15
    fpwm = '{:4.4}'.format(pwm)
    return fpwm

def getButtonVal():
    return 0


ser = serial.Serial('COM5', 9600)  # open first serial port
print ser.portstr       # check which port was really used
ser.write("hello")      # write a string
#ser.close()



while 1:
    pygame.event.get()


    x1 = getJoyVal(0)
    y1 = getJoyVal(1)
    x2 = getJoyVal(4)
    y2 = getJoyVal(3)
    z=[float(x1),float(y1),float(x2),float(y2)]




    ser.write("The value   :")
    ser.write(str(z))
    print(z)
    time.sleep(1)

    #ser.close()