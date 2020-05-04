using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Drawing.Text;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using PrimaryClasses;
using UtilityClasses;

namespace Motor_Control
{
    public class MotorManager
    {
        private Dictionary<string, IMotor> m_RegisteredMotors;

        private static MotorManager m_Instance;

        public static MotorManager GetInstance()
        {
            return m_Instance ?? (m_Instance = new MotorManager());
        }

        private MotorManager()
        {
            Initialize();
        }

        public void Initialize()
        {

            StepperMotor MotorA = new StepperMotor(MotorTypes.Stepper, "A", 90,-90, 23, 24);
            StepperMotor MotorB = new StepperMotor(MotorTypes.Stepper, "B", 90, -90, 17, 27);
            m_RegisteredMotors=new Dictionary<string, IMotor>();
            m_RegisteredMotors.Add(MotorA.MotorId, MotorA);
            m_RegisteredMotors.Add(MotorB.MotorId, MotorB);
            SetupMotors();
            Console.WriteLine($"\r\n[{DateTime.Now}] Motor Manager: Initializing...");
        }


        public void StepperMoveForward(int angle, float speed)
        {
            //Add angle conversion
            int counter = 0;

            while (counter < angle)
            {

            }
        }

        public void MoveStepper(int angle, float speed, string motor, bool direction)
        {
            //Add angle conversion

            int stepPin = 0;
            int dirPin = 0;
            if (m_RegisteredMotors[motor] is StepperMotor stepperMotor)
            {
                switch (motor)
                {
                    case "A":

                        stepPin =stepperMotor.StepPin;
                        dirPin =stepperMotor.DirPin;
                        break;
                        
                    case "B":

                        stepPin = stepperMotor.StepPin;
                        dirPin = stepperMotor.DirPin;
                        break;
                }
            }

            FlagArgs  flags =new FlagArgs(false,false,"A");
            Thread collisionDetectorThread = new Thread(CollisionDetection);
            collisionDetectorThread.Start(flags);
            int counter = 0;

            if (!direction)
            {
                PinManager.GetInstance().Controller.Write(dirPin, PinValue.Low);
            }
            PinManager.GetInstance().Controller.Write(dirPin, PinValue.High);
            // PinManager.GetInstance().Controller.Write(dirPin, PinValue.Low);
            while (counter < angle && !flags.collisionFlag)
            {
                Console.WriteLine($"{counter}: Moove moove {flags.collisionFlag}");
              //  PinManager.GetInstance().Controller.Write(stepPin, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(speed));
               // PinManager.GetInstance().Controller.Write(stepPin, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(speed));
                counter++;
            }

            collisionDetectorThread.Join();
            flags.stopFlag = true;
        }

        private void CollisionDetection(object boolObject)
        {
            Console.WriteLine("ENTERING COLLISION DETECTION");
            if (boolObject is FlagArgs flags)
            {
                int topSensor = 1;
                int bottomSensor = 1;
                switch (flags.motor)
                {
                    case "A":
                        topSensor = PinManager.GetInstance().JointATop;
                        bottomSensor = PinManager.GetInstance().JointABottom;
                        break;

                    case "B":
                        topSensor = PinManager.GetInstance().JointBTop;
                        bottomSensor = PinManager.GetInstance().JointBBottom;
                        break;
                }

                PinValue topSensorSate;
                PinValue bottomSensorState;
                while (!flags.collisionFlag && !flags.stopFlag)
                {
                    topSensorSate = PinManager.GetInstance().Controller.Read(topSensor);
                    bottomSensorState = PinManager.GetInstance().Controller.Read(bottomSensor);

                    if (topSensorSate == PinValue.Low)
                    {
                        Console.WriteLine("Boop Top");
                        flags.collisionFlag = true;
                    }

                    if (bottomSensorState == PinValue.Low)
                    {
                        Console.WriteLine("Boop Bottom");
                        flags.collisionFlag = true;
                    }
                    
                    Thread.Sleep(TimeSpan.FromSeconds(0.01));
                }
            }
        }

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