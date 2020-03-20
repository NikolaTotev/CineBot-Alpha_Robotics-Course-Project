from time import sleep
import RPi.GPIO as GPIO
class StepppeManager:
    def __init__(self, StepPin, DirPin, StepsPerRev):
        self.StepPin = StepPin
        self.DirPin = DirPin
        self.StepsPerRev = StepsPerRev
        self.DirectionDictionary = {
            "CW": 1,
            "CCW": 0
        }
        GPIO.setupMode(GPIO.BCM)
        GPIO.setup(self.DirPin, GPIO.OUT)
        GPIO.setup(self.StepPin, GPIO.OUT)

    def MoveForward(self, speed, angle):
        GPIO.output(self.DirPin, self.DirectionDictionary["CW"])
        steps = 0.45*angle

        for step in range(steps):
            GPIO.output(self.StepPin, GPIO.HIGH)
            sleep(speed)
            GPIO.output(self.StepPin, GPIO.LOW)
            sleep(speed)

    def MoveBackward(self, speed,angle):
        GPIO.output(self.DirPin, self.DirectionDictionary["CW"])
        steps = 0.45 * angle

        for step in range(steps):
            GPIO.output(self.StepPin, GPIO.HIGH)
            sleep(speed)
            GPIO.output(self.StepPin, GPIO.LOW)
            sleep(speed)





