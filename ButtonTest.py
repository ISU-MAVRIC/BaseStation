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
        Value = 255-(int(round(127*joystick.get_axis(axis)))+127)
    else:
        Value = 127
    #map (-1 to 1) to (0.10 to 0.20) for PWM
    #pwm = chr(Value)
    #fpwm = '{:4.4}'.format(pwm)     #for sending direct pwm value
    return Value

def getButtonVal(button):
    x = joystick.get_button(button)
    return x

def camSelect():
        counter = 0
        if(getButtonValue(6))
            if counter = 0
                cam_select = 0
            if counter = 1
                cam_select = 127
            if counter = 2
                cam_select = 255
    return cam_select

while 1:
    my_bytes = bytearray()
    my_bytes.append(60)
    my_bytes.append(getJoyVal(3))
    my_bytes.append(getJoyVal(1))
    my_bytes.append(camSelect())
    my_bytes.append(62)
    pygame.event.get()
    x = getButtonVal(6)

    print x
    time.sleep(.3)

    #ser.close()