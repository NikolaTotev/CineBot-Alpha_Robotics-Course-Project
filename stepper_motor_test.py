from time import sleep
import RPi.GPIO as GPIO

DIR = 23
STEP = 24
CW =1
CCW = 0
SPR = 500
GPIO.setmode(GPIO.BCM)
GPIO.setup(DIR,GPIO.OUT)
GPIO.setup(STEP, GPIO.OUT)
GPIO.output(DIR, CW)

step_count = SPR
delay = 0.001
waitdelay = 2
counter = 10

counter=1
while counter>0:
    sleep(waitdelay)

    GPIO.output(DIR, CW)
    for x in range(step_count+2000):
        print("STEP CW")
        GPIO.output(STEP, GPIO.HIGH)
        sleep(delay)
        GPIO.output(STEP, GPIO.LOW)
        sleep(delay)
        
    sleep(waitdelay)
    GPIO.output(DIR, CCW)
        
    for x in range(step_count+2000):
        print("STEP CCW")
        GPIO.output(STEP, GPIO.HIGH)
        sleep(delay)
        GPIO.output(STEP, GPIO.LOW)
        sleep(delay)
    counter=counter-1
    
print("PROC END")

GPIO.cleanup()