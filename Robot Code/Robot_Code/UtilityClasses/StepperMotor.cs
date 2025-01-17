﻿using System.Device.Gpio;


namespace UtilityClasses
{
    public class StepperMotor : IMotor
    {
        public MotorTypes MotorType { get; set; }
        public string MotorId { get; set; }
        public float MaxAngle { get; set; }
        public float MinAngle { get; set; }
        public int StepPin { get; set; }
        public int DirPin { get; set; }


        public StepperMotor(MotorTypes type, string ID, float maxAng, float minAng, int stpPin, int dPin)
        {
            MotorType = type;
            MotorId = ID;
            MaxAngle = maxAng;
            MinAngle = minAng;
            StepPin = stpPin;
            DirPin = dPin;
        }

        public bool ValidPins()
        {
            return StepPin != DirPin;
        }

        public void MoveCw(float degrees, int speed)
        {
            throw new System.NotImplementedException();
        }

        public void MoveCcw(float degrees, int speed)
        {
            throw new System.NotImplementedException();
        }

        MotorTypes IMotor.MotorType()
        {
            return MotorType;
        }
    }
}