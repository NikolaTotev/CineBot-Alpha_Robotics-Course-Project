using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;
using PrimaryClasses;
using UtilityClasses;

namespace Motor_Control
{
    class MotorManager
    {
        private Dictionary<string, IMotor> m_RegisteredMotors;

        public void SetupMotors()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var registeredMotor in m_RegisteredMotors)
            {
                if (registeredMotor.Value is StepperMotor stepperMotor)
                {
                    bool addedStepPin = PinManager.GetInstance().SetupPin(stepperMotor.StepPin, pinMode: PinMode.Output);
                    bool addedDirPin = PinManager.GetInstance().SetupPin(stepperMotor.DirPin, pinMode: PinMode.Output);

                    if ((addedDirPin && addedStepPin))
                    {
                        sb.Append($"Stepper motor{stepperMotor.MotorId}: Status - Added on Step Pin: {stepperMotor.StepPin}, Dir Pin {stepperMotor.DirPin} , Mode: Output \n");
                    }
                    else
                    {
                        sb.Append($"Stepper motor{stepperMotor.MotorId}: Status - NOT ADDED");
                    }
                    
                }
                else if (registeredMotor.Value is ServoMotor servoMotor)
                {
                    bool addedControlPin = PinManager.GetInstance().SetupPin(servoMotor.ControlPin, pinMode: PinMode.Output);

                    if (addedControlPin)
                    {
                        sb.Append($"Stepper motor{servoMotor.MotorId}: Status - Added on Control Pin: {servoMotor.ControlPin}, Mode: Output \n");
                    }
                    else
                    {
                        sb.Append($"Stepper motor{servoMotor.MotorId}: Status - NOT ADDED");
                    }
                }
                else
                {
                    sb.Append("Element in configuration not recognized motor!");
                }
            }
        }
    }

}