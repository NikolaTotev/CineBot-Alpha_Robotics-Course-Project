﻿using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Drawing.Text;
using System.IO.Ports;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using PrimaryClasses;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
using UtilityClasses;

namespace Motor_Control
{
    public class MotorManager
    {
        private Dictionary<string, IMotor> m_RegisteredMotors;

        private static MotorManager m_Instance;
        private bool m_UseJogMode = false;
        private readonly int m_StepsPerRevolution = 200;
        private readonly int m_StepMultiplier = 1;
        private readonly int m_GearRatio = 7;
        private readonly int m_MinimumAngle = 5;
        private readonly float m_MinimumSpeed = 0.01f;
        private readonly float m_SpeedSensitivity = 1;
        private readonly float m_CollisionDetectionFrequency = 0.01f;
        private int m_MovementSensitivity = 1;
        private int m_StepsInRange = 0;

        public bool JogModeFlag
        {
            get => m_UseJogMode;
            set => m_UseJogMode = value;
        }

        //public static MotorManager GetInstance()
        //{
        //    return m_Instance ?? (m_Instance = new MotorManager());
        //}

        public MotorManager()
        {
            Initialize();
        }

        public void Initialize()
        {
            StepperMotor MotorA = new StepperMotor(MotorTypes.Stepper, "A", 90, -90, 20, 23);
            StepperMotor MotorB = new StepperMotor(MotorTypes.Stepper, "B", 90, -90, 25, 24);
            m_RegisteredMotors = new Dictionary<string, IMotor>();
            m_RegisteredMotors.Add(MotorA.MotorId, MotorA);
            m_RegisteredMotors.Add(MotorB.MotorId, MotorB);
            //SetupMotors();
            Console.WriteLine($"\r\n[{DateTime.Now}] Motor Manager: Initializing...");
        }


        public int ConvertAngleToSteps(int inputAngle)
        {

            int stepsForCarrierRevolution = m_StepsPerRevolution * m_StepMultiplier * m_GearRatio;
            int stepsForInputAngle = stepsForCarrierRevolution / inputAngle;

            return stepsForInputAngle;
        }

        public void MoveStepper(int angle, float speed, StepperMotorOptions stepperMotor, bool direction, bool useDebugMessages)
        {
            int numberOfSteps = ConvertAngleToSteps(angle);
            int stepPin = 0;
            int dirPin = 0;

            switch (stepperMotor)
            {
                case StepperMotorOptions.motorA:
                    stepPin = PinManager.GetInstance().JointAStep;
                    dirPin = PinManager.GetInstance().JointADir;
                    break;
                case StepperMotorOptions.motorB:
                    stepPin = PinManager.GetInstance().JointBStep;
                    dirPin = PinManager.GetInstance().JointBDir;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stepperMotor), stepperMotor, null);
            }

            if (stepPin == 0 || dirPin == 0)
            {
                Console.WriteLine($"[{DateTime.Now}] <ERROR>: Failed to get stepperMotor control pins. Aborting stepperMotor {stepperMotor} movement.");
                return;
            }


            FlagArgs flags = new FlagArgs(false, false, stepperMotor);
            Thread collisionDetectorThread = new Thread(CollisionDetection);
            collisionDetectorThread.Start(flags);

            int counter = 0;

            if (!direction)
            {
                PinManager.GetInstance().Controller.Write(dirPin, PinValue.Low);
            }
            else
            {
                PinManager.GetInstance().Controller.Write(dirPin, PinValue.High);
            }

            while (counter < numberOfSteps && !flags.CollisionFlag)
            {
                if (useDebugMessages)
                {
                    Console.WriteLine($"Moving {stepperMotor}, currently on step {counter}");
                    counter++;
                }

                if (flags.EmergencyStopActive)
                {
                    Console.WriteLine($"[{DateTime.Now}] <EMERGENCY>: Emergency button activated. Aborting Execution.");
                    flags.StopFlag = true;
                    break;
                }
                PinManager.GetInstance().Controller.Write(stepPin, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(speed));
                PinManager.GetInstance().Controller.Write(stepPin, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(speed));
            }

            flags.StopFlag = true;

            try
            {
                if (collisionDetectorThread.IsAlive)
                {
                    Console.WriteLine($"[{DateTime.Now}] <Movement Warning>: Collision detector thread is still running. Attempting to join it.");
                    bool collisionThreadStopped = collisionDetectorThread.Join(1500);
                    if (!collisionThreadStopped)
                    {
                        Console.WriteLine($"[{DateTime.Now}] <COLLISION ERROR>: Failed to join collision detector thread gracefully. Aborting thread.");
                        collisionDetectorThread.Abort();
                    }
                    else
                    {
                        Console.WriteLine($"[{DateTime.Now}] <Movement Info>: Collision detector thread has been successfully joined.");
                    }
                }
            }
            catch (ThreadAbortException e)
            {
                Console.WriteLine($"[{DateTime.Now}] <EXCEPTION>: An exception with code {e.HResult} occured during thread abortion.");
            }
        }

        private void CollisionDetection(object boolObject)
        {
            Console.WriteLine($"[{DateTime.Now}] <Collision Info>: Collision detection enabled.");
            if (boolObject is FlagArgs flags)
            {
                int topSensor = 1;
                int bottomSensor = 1;
                int emergencyStop = PinManager.GetInstance().EmergencyStop;

                switch (flags.TargetStepperMotor)
                {
                    case StepperMotorOptions.motorA:
                        topSensor = PinManager.GetInstance().JointATop;
                        bottomSensor = PinManager.GetInstance().JointABottom;
                        break;
                    case StepperMotorOptions.motorB:
                        topSensor = PinManager.GetInstance().JointBTop;
                        bottomSensor = PinManager.GetInstance().JointBBottom;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(flags.TargetStepperMotor), flags.TargetStepperMotor, null);
                }

                PinValue topSensorSate;
                PinValue bottomSensorState;
                PinValue emergencyButtonState;

                while (!flags.StopFlag)
                {
                    topSensorSate = PinManager.GetInstance().Controller.Read(topSensor);
                    bottomSensorState = PinManager.GetInstance().Controller.Read(bottomSensor);
                    emergencyButtonState = PinManager.GetInstance().Controller.Read(emergencyStop);

                    //ToDo: Invert button activation to provide redundancy in the event of disconnected cable.
                    if (topSensorSate == PinValue.Low || bottomSensorState == PinValue.Low || emergencyButtonState == PinValue.Low)
                    {
                        if (emergencyButtonState == PinValue.Low)
                        {
                            flags.EmergencyStopActive = true;
                        }
                        Console.WriteLine($"[{DateTime.Now}] <Collision Info>: Motor {flags.TargetStepperMotor} has reached an end-stop or the emergency switch as been activated..");
                        flags.CollisionFlag = true;
                    }
                    else
                    {
                        flags.CollisionFlag = false;
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(m_CollisionDetectionFrequency));
                }
                Console.WriteLine($"[{DateTime.Now}] <Collision Info>: Stop flag has been raised. Ending collision detection.");
            }
        }

        public void GoToHome(StepperMotorOptions targetStepperMotor, bool useDebugMessages)
        {
            int stepPin = 0;
            int dirPin = 0;

            int numberOfSteps = 0;
            bool direction = false;
            bool hasReachedEndStop = false;
            bool hasCalculatedSteps = false;

            switch (targetStepperMotor)
            {
                case StepperMotorOptions.motorA:
                    stepPin = PinManager.GetInstance().JointAStep;
                    dirPin = PinManager.GetInstance().JointADir;
                    break;
                case StepperMotorOptions.motorB:
                    stepPin = PinManager.GetInstance().JointBStep;
                    dirPin = PinManager.GetInstance().JointBDir;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetStepperMotor), targetStepperMotor, null);
            }

            if (stepPin == 0 || dirPin == 0)
            {
                Console.WriteLine($"[{DateTime.Now}] <MOVEMENT ERROR>: Failed to get stepperMotor control pins. Aborting stepperMotor {targetStepperMotor} movement.");
                return;
            }


            FlagArgs flags = new FlagArgs(false, false, targetStepperMotor);
            Thread collisionDetectorThread = new Thread(CollisionDetection);
            collisionDetectorThread.Start(flags);

            int counter = 0;
            while (!hasCalculatedSteps)
            {
                if (flags.EmergencyStopActive)
                {
                    Console.WriteLine($"[{DateTime.Now}] <EMERGENCY>: Emergency button activated. Aborting Execution.");
                    break;
                }

                if (direction)
                {
                    PinManager.GetInstance().Controller.Write(dirPin, PinValue.Low);
                }
                else
                {
                    PinManager.GetInstance().Controller.Write(dirPin, PinValue.High);
                }

                while (!flags.CollisionFlag)
                {
                    if (useDebugMessages)
                    {
                        Console.WriteLine($"Moving {targetStepperMotor}, currently on step {counter}");
                        counter++;
                    }

                    if (flags.EmergencyStopActive)
                    {
                        Console.WriteLine($"[{DateTime.Now}] <EMERGENCY>: Emergency button activated. Aborting Execution.");
                        break;
                    }
                    PinManager.GetInstance().Controller.Write(stepPin, PinValue.High);
                    Thread.Sleep(TimeSpan.FromSeconds(m_MinimumSpeed));
                    PinManager.GetInstance().Controller.Write(stepPin, PinValue.Low);
                    Thread.Sleep(TimeSpan.FromSeconds(m_MinimumSpeed));
                    if (hasReachedEndStop)
                    {
                        numberOfSteps++;
                    }
                }

                direction = true;
                hasReachedEndStop = true;

                if (numberOfSteps != 0)
                {
                    hasCalculatedSteps = true;
                }
            }


            for (int i = 0; i < numberOfSteps / 2; i++)
            {
                if (flags.EmergencyStopActive)
                {
                    Console.WriteLine($"[{DateTime.Now}] <EMERGENCY>: Emergency button activated. Aborting Execution.");
                    break;
                }
                PinManager.GetInstance().Controller.Write(stepPin, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(m_MinimumSpeed * 0.5));
                PinManager.GetInstance().Controller.Write(stepPin, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(m_MinimumSpeed * 0.5));
            }

            flags.StopFlag = true;

            try
            {
                if (collisionDetectorThread.IsAlive)
                {
                    Console.WriteLine($"[{DateTime.Now}] <Movement Warning>: Collision detector thread is still running. Attempting to join it.");
                    bool collisionThreadStopped = collisionDetectorThread.Join(1500);
                    if (!collisionThreadStopped)
                    {
                        Console.WriteLine($"[{DateTime.Now}] <COLLISION ERROR>: Failed to join collision detector thread gracefully. Aborting thread.");
                        collisionDetectorThread.Abort();
                    }
                    else
                    {
                        Console.WriteLine($"[{DateTime.Now}] <Movement Info>: Collision detector thread has been successfully joined.");
                    }
                }
            }
            catch (ThreadAbortException e)
            {
                Console.WriteLine($"[{DateTime.Now}] <EXCEPTION>: An exception with code {e.HResult} occured during thread abortion.");
            }
        }

        public void JogMode(object threadParams)
        {
            if (threadParams is MotorThreadStartObj tParams)
            {
                Console.WriteLine($"[{DateTime.Now}] <Motor Manager Jog Mode>: Motor {tParams.TargetStepperMotor}: Control thread started.");
                int encoderSIA;
                int enncoderSIB;
                int resetPin;
                int emergencyButton = PinManager.GetInstance().EmergencyStop;
                switch (tParams.TargetStepperMotor)
                {
                    case StepperMotorOptions.motorA:
                        encoderSIA = PinManager.GetInstance().PanSIA;
                        enncoderSIB = PinManager.GetInstance().PanSIB;
                        resetPin = PinManager.GetInstance().PanReset;
                        break;

                    case StepperMotorOptions.motorB:
                        encoderSIA = PinManager.GetInstance().RotSIA;
                        enncoderSIB = PinManager.GetInstance().RotSIB;
                        resetPin = PinManager.GetInstance().RotReset;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                float encoderCounter = 0;
                float previousCounterState = encoderCounter;
                PinValue encoderState;
                PinValue encoderLastState;

                while (!tParams.ShouldStop)
                {

                    if (PinManager.GetInstance().Controller.Read(emergencyButton) == PinValue.Low)
                    {
                        Console.WriteLine($"[{DateTime.Now}] <EMERGENCY>: Emergency button activated. Aborting Execution.");
                        break;
                    }
                    encoderState = PinManager.GetInstance().Controller.Read(encoderSIA);
                    encoderLastState = PinManager.GetInstance().Controller.Read(encoderSIA);

                    if (PinManager.GetInstance().Controller.Read(resetPin) == PinValue.Low)
                    {
                        //ToDo return to home. 
                        Thread.Sleep(TimeSpan.FromMilliseconds(45));
                    }

                    if (encoderState != encoderLastState)
                    {
                        if (PinManager.GetInstance().Controller.Read(enncoderSIB) != encoderState)
                        {
                            previousCounterState = encoderCounter;
                            encoderCounter += 0.5f;
                        }
                        else
                        {
                            previousCounterState = encoderCounter;
                            encoderCounter -= 0.5f;
                        }

                        if (Math.Abs(encoderCounter % 1) < 0.01)
                        {
                            int movementAngle = m_MinimumAngle * m_MovementSensitivity;
                            float speed = m_MinimumSpeed * m_SpeedSensitivity;
                            bool direction = previousCounterState < encoderCounter;
                            MoveStepper(movementAngle, speed, tParams.TargetStepperMotor, direction, false);
                            Console.WriteLine($"[{DateTime.Now}] <Motor info>: Motor {tParams.TargetStepperMotor}: \n Moving {movementAngle} with speed {speed} and direction {direction}");
                            Thread.Sleep(TimeSpan.FromMilliseconds(35));
                        }
                    }
                    encoderLastState = encoderState;
                }
            }
        }

        private void SingleStep(int pin)
        {
            PinManager.GetInstance().Controller.Write(pin, PinValue.High);
            Thread.Sleep(TimeSpan.FromSeconds(0.0001));
            PinManager.GetInstance().Controller.Write(pin, PinValue.Low);
            Thread.Sleep(TimeSpan.FromSeconds(0.0001));
        }


        public void GimbalJog()
        {
            int emergencyStop = PinManager.GetInstance().EmergencyStop;

            SerialPort serialPort;
            serialPort = new SerialPort("/dev/ttyACM0", 9600); //Set the read/write timeouts    
            serialPort.ReadTimeout = 1500;
            serialPort.WriteTimeout = 1500;
            serialPort.Open();
            serialPort.WriteLine("P75");
            Thread.Sleep(TimeSpan.FromMilliseconds(45));
            serialPort.WriteLine("R80");
            Thread.Sleep(TimeSpan.FromMilliseconds(45));
            serialPort.WriteLine("T80");
            Thread.Sleep(TimeSpan.FromMilliseconds(45));


            int panSia = PinManager.GetInstance().PanSIA;
            int panSib = PinManager.GetInstance().PanSIB;

            int rotSia = PinManager.GetInstance().RotSIA;
            int rotSib = PinManager.GetInstance().RotSIB;

            int tiltSia = PinManager.GetInstance().TiltSIA;
            int tiltSib = PinManager.GetInstance().TiltSIB;

            int resetPan = PinManager.GetInstance().PanReset;
            int resetRot = PinManager.GetInstance().RotReset;
            int resetTilt = PinManager.GetInstance().TiltReset;

            float PanCounter = 75;
            float RotCounter = 80;
            float TiltCounter = 80;

            PinValue PanState;
            PinValue PanLastState;

            PinValue RotState;
            PinValue RotLastState;

            PinValue TiltState;
            PinValue TiltLastState;

            PanLastState = PinManager.GetInstance().Controller.Read(panSia);
            RotLastState = PinManager.GetInstance().Controller.Read(rotSia);
            TiltLastState = PinManager.GetInstance().Controller.Read(tiltSia);

            while (PinManager.GetInstance().Controller.Read(emergencyStop) != PinValue.Low)
            {
                if (PinManager.GetInstance().Controller.Read(resetPan) == PinValue.Low)
                {
                    PanCounter = 75;
                    serialPort.WriteLine($"P{PanCounter}");
                    Thread.Sleep(TimeSpan.FromMilliseconds(45));
                }

                if (PinManager.GetInstance().Controller.Read(resetRot) == PinValue.Low)
                {
                    RotCounter = 80;
                    serialPort.WriteLine($"R{RotCounter}");
                    Thread.Sleep(TimeSpan.FromMilliseconds(45));
                }

                if (PinManager.GetInstance().Controller.Read(resetTilt) == PinValue.Low)
                {
                    TiltCounter = 80;
                    serialPort.WriteLine($"T{TiltCounter}");
                    Thread.Sleep(TimeSpan.FromMilliseconds(45));
                }

                //Read();
                Thread.Sleep(TimeSpan.FromSeconds(0.001));

                PanState = PinManager.GetInstance().Controller.Read(panSia);
                RotState = PinManager.GetInstance().Controller.Read(rotSia);
                TiltState = PinManager.GetInstance().Controller.Read(tiltSia);

                if (PanState != PanLastState)
                {
                    if (PinManager.GetInstance().Controller.Read(panSib) != PanState)
                    {
                        PanCounter += 2.5f;
                    }
                    else
                    {
                        PanCounter -= 2.5f;
                    }

                    if (Math.Abs(PanCounter % 1) < 0.01)
                    {
                        Console.WriteLine($"Step: P{PanCounter}");
                        serialPort.WriteLine($"P{PanCounter}");
                        Thread.Sleep(TimeSpan.FromMilliseconds(35));
                    }

                    if (Math.Abs(PanCounter - 360) < 0.0001 || Math.Abs(PanCounter - (-360)) < 0.0001)
                    {
                        PanCounter = 0;
                    }
                }


                if (RotState != RotLastState)
                {
                    if (PinManager.GetInstance().Controller.Read(rotSib) != RotState)
                    {
                        RotCounter += 2.5f;
                    }
                    else
                    {
                        RotCounter -= 2.5f;
                    }

                    if (Math.Abs(RotCounter % 1) < 0.01)
                    {
                        Console.WriteLine($"Step: R{RotCounter}");
                        serialPort.WriteLine($"R{RotCounter}");
                        Thread.Sleep(TimeSpan.FromMilliseconds(35));
                    }

                    if (Math.Abs(RotCounter - 360) < 0.0001 || Math.Abs(RotCounter - (-360)) < 0.0001)
                    {
                        RotCounter = 0;
                    }
                }


                if (TiltState != TiltLastState)
                {
                    if (PinManager.GetInstance().Controller.Read(tiltSib) != TiltState)
                    {
                        TiltCounter += 2.5f;
                    }
                    else
                    {
                        TiltCounter -= 2.5f;
                    }

                    if (Math.Abs(TiltCounter % 1) < 0.01)
                    {
                        Console.WriteLine($"Step: T{TiltCounter}");
                        serialPort.WriteLine($"T{TiltCounter}");
                        Thread.Sleep(TimeSpan.FromMilliseconds(35));
                    }

                    if (Math.Abs(TiltCounter - 360) < 0.0001 || Math.Abs(TiltCounter - (-360)) < 0.0001)
                    {
                        TiltCounter = 0;
                    }
                }

                Thread.Sleep(TimeSpan.FromSeconds(0.001));
                PanLastState = PanState;
                RotLastState = RotState;
                TiltLastState = TiltState;


                try
                {
                    if (serialPort.BytesToRead > 0)
                    {
                        string message = serialPort.ReadLine();
                        Console.WriteLine(message);
                    }

                }
                catch (TimeoutException)
                {
                }
            }

            serialPort.Close();
        }



        public void Dance()
        {

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
                        Console.WriteLine($"Stepper stepperMotor{stepperMotor.MotorId}: Status - Added on Step Pin: {stepperMotor.StepPin}, Dir Pin {stepperMotor.DirPin} , Mode: Output \n");
                    }
                    else
                    {
                        Console.WriteLine($"Stepper stepperMotor{stepperMotor.MotorId}: Status - NOT ADDED");
                    }

                }
                else if (registeredMotor.Value is ServoMotor servoMotor)
                {
                    bool addedControlPin = PinManager.GetInstance().SetupPin(servoMotor.ControlPin, pinMode: PinMode.Output);

                    if (addedControlPin)
                    {
                        Console.WriteLine(($"Stepper stepperMotor{servoMotor.MotorId}: Status - Added on Control Pin: {servoMotor.ControlPin}, Mode: Output \n"));
                    }
                    else
                    {
                        Console.WriteLine($"Stepper stepperMotor{servoMotor.MotorId}: Status - NOT ADDED");
                    }
                }
                else
                {
                    sb.Append("Element in configuration not recognized stepperMotor!");
                }
            }

        }
    }

}