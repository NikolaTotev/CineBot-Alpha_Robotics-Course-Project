from time import sleep
import RPi.GPIO as GPIO
class StepperManager:
    def __init__(self, StepPin, DirPin, StepsPerRev):
        self.StepPin = StepPin
        self.DirPin = DirPin
        self.StepsPerRev = StepsPerRev
        self.DirectionDictionary = {
            "CW": 1,
            "CCW": 0
        }
        GPIO.setmode(GPIO.BCM)
        GPIO.setup(self.DirPin, GPIO.OUT)
        GPIO.setup(self.StepPin, GPIO.OUT)

    def MoveForward(self, speed, angle):
        GPIO.output(self.DirPin, self.DirectionDictionary["CW"])
        steps = angle/0.45

        for step in range(round(steps)):
            GPIO.output(self.StepPin, GPIO.HIGH)
            sleep(speed)
            GPIO.output(self.StepPin, GPIO.LOW)
            sleep(speed)

    def MoveBackward(self, speed,angle):
        GPIO.output(self.DirPin, self.DirectionDictionary["CCW"])
        steps = steps = angle/0.45

        for step in range(round(steps)):
            GPIO.output(self.StepPin, GPIO.HIGH)
            sleep(speed)
            GPIO.output(self.StepPin, GPIO.LOW)
            sleep(speed)





