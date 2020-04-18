import StepperController as controller
import RPi.GPIO as GPIO
from time import sleep

def main():
    MotorOne = controller.StepperManager(24,23,800)
    MotorOne.MoveForward(0.0001, 90)
    sleep(1)
    MotorOne.MoveBackward(0.0001, 90)
    sleep(1)
    MotorOne.MoveForward(0.0001, 45)
    sleep(1)
    MotorOne.MoveBackward(0.0001, 180)
    GPIO.cleanup()


if __name__ == "__main__":
    main()
    

