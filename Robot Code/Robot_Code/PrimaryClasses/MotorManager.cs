using System;
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

        public void MoveStepper(int angle, float speed, MotorOptions motor, bool direction, MotorThreadStartObj runtimeInfo, bool useDebugMessages)
        {
            int numberOfSteps = ConvertAngleToSteps(angle);
            int stepPin = 0;
            int dirPin = 0;

            switch (motor)
            {
                case MotorOptions.motorA:
                    stepPin = PinManager.GetInstance().JointAStep;
                    dirPin = PinManager.GetInstance().JointADir;
                    break;
                case MotorOptions.motorB:
                    stepPin = PinManager.GetInstance().JointBStep;
                    dirPin = PinManager.GetInstance().JointBDir;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(motor), motor, null);
            }

            if (stepPin == 0 || dirPin == 0)
            {
                Console.WriteLine($"[{DateTime.Now}] <ERROR>: Failed to get motor control pins. Aborting motor {motor} movement.");
                return;
            }


            FlagArgs flags = new FlagArgs(false, false, motor);
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
                    Console.WriteLine($"Moving {motor}, currently on step {counter}");
                }
                
                PinManager.GetInstance().Controller.Write(stepPin, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(speed));
                PinManager.GetInstance().Controller.Write(stepPin, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(speed));
                counter++;
            }

            flags.StopFlag = true;

            collisionDetectorThread.Join();
        }

        private void CollisionDetection(object boolObject)
        {
            Console.WriteLine($"[{DateTime.Now}] <Collision Info>: Collision detection enabled.");
            if (boolObject is FlagArgs flags)
            {
                int topSensor = 1;
                int bottomSensor = 1;
                int emergencyStop = PinManager.GetInstance().EmergencyStop;

                switch (flags.TargetMotor)
                {
                    case MotorOptions.motorA:
                        topSensor = PinManager.GetInstance().JointATop;
                        bottomSensor = PinManager.GetInstance().JointABottom;
                        break;
                    case MotorOptions.motorB:
                        topSensor = PinManager.GetInstance().JointBTop;
                        bottomSensor = PinManager.GetInstance().JointBBottom;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(flags.TargetMotor), flags.TargetMotor, null);
                }

                PinValue topSensorSate;
                PinValue bottomSensorState;

                while (!flags.StopFlag)
                {
                    topSensorSate = PinManager.GetInstance().Controller.Read(topSensor);
                    bottomSensorState = PinManager.GetInstance().Controller.Read(bottomSensor);

                    //ToDo: Invert button activation to provide redundancy in the event of disconnected cable.
                    if (topSensorSate == PinValue.Low || bottomSensorState == PinValue.Low || emergencyStop == PinValue.Low)
                    {
                        Console.WriteLine($"[{DateTime.Now}] <Collision Info>: Motor {flags.TargetMotor} has reached an end-stop.");
                        flags.CollisionFlag = true;
                    }
                    else
                    {
                        flags.CollisionFlag = false;
                    }

                }
                Thread.Sleep(TimeSpan.FromSeconds(m_CollisionDetectionFrequency));
            }
        }

        public void GoToHome()
        {
            Console.WriteLine($"\r\n[{DateTime.Now}] Going to home position.");
            int topSensorA = PinManager.GetInstance().JointATop;
            int topSensorB = PinManager.GetInstance().JointBTop;
            int emergencyStop = PinManager.GetInstance().EmergencyStop;

            bool jointAStop = false;
            bool jointBStop = false;

            int counter = 0;

            int stepPinA = 0;
            int dirPinA = 0;

            int stepPinB = 0;
            int dirPinB = 0;

            if (m_RegisteredMotors["A"] is StepperMotor stepperMotorA &&
                m_RegisteredMotors["B"] is StepperMotor stepperMotorB)
            {
                stepPinA = stepperMotorA.StepPin;
                dirPinA = stepperMotorA.DirPin;

                stepPinB = stepperMotorB.StepPin;
                dirPinB = stepperMotorB.DirPin;
            }

            PinManager.GetInstance().Controller.Write(dirPinA, PinValue.Low);
            PinManager.GetInstance().Controller.Write(dirPinB, PinValue.Low);

            Console.WriteLine($"\r\n[{DateTime.Now}] Moving motor A");
            while (!jointAStop)
            {

                if (topSensorA == PinValue.Low || emergencyStop == PinValue.Low)
                {
                    jointAStop = true;
                }
                else
                {
                    Console.WriteLine($"{counter}: Moove A, moove A {jointAStop}");
                    //  PinManager.GetInstance().Controller.Write(stepPinA, PinValue.High);
                    Thread.Sleep(TimeSpan.FromSeconds(0.01));
                    // PinManager.GetInstance().Controller.Write(stepPinA, PinValue.Low);
                    Thread.Sleep(TimeSpan.FromSeconds(0.01));
                    counter++;
                }
            }

            counter = 0;

            Console.WriteLine($"\r\n[{DateTime.Now}] Moving motor B");
            while (!jointBStop)
            {

                if (topSensorB == PinValue.Low || emergencyStop == PinValue.Low)
                {
                    jointBStop = true;
                }
                else
                {
                    Console.WriteLine($"{counter}: Moove B, moove B {jointBStop}");
                    PinManager.GetInstance().Controller.Write(stepPinB, PinValue.High);
                    Thread.Sleep(TimeSpan.FromSeconds(0.01));
                    PinManager.GetInstance().Controller.Write(stepPinB, PinValue.Low);
                    Thread.Sleep(TimeSpan.FromSeconds(0.01));
                    counter++;
                }
            }

            Console.WriteLine($"\r\n[{DateTime.Now}] Centering joints");
            //Fix code to correctly center the joints.
            MoveStepper(5, 0.1f, "A", true);
            MoveStepper(5, 0.1f, "B", true);
        }

        public void JogMode(object threadParams)
        {
            if (threadParams is MotorThreadStartObj tParams)
            {
                Console.WriteLine($"[{DateTime.Now}] <Motor Manager Jog Mode>: Motor {tParams.TargetMotor}: Control thread started.");
                int encoderSIA;
                int enncoderSIB;
                int resetPin;

                switch (tParams.TargetMotor)
                {
                    case MotorOptions.motorA:
                        encoderSIA = PinManager.GetInstance().PanSIA;
                        enncoderSIB = PinManager.GetInstance().PanSIB;
                        resetPin = PinManager.GetInstance().PanReset;
                        break;

                    case MotorOptions.motorB:
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
                            MoveStepper(movementAngle, speed, tParams.TargetMotor, direction);
                            Console.WriteLine($"[{DateTime.Now}] <Motor info>: Motor {tParams.TargetMotor}: \n Moving {movementAngle} with speed {speed} and direction {direction}");
                            Thread.Sleep(TimeSpan.FromMilliseconds(35));
                        }
                    }
                    encoderLastState = encoderState;
                }
            }

            m_UseJogMode = true;

            int exitButton = PinManager.GetInstance().EmergencyStop;

            FlagArgs flags = new FlagArgs(false, false, false, "A");
            Thread collisionDetectorThread = new Thread(CollisionDetection);
            collisionDetectorThread.Start(flags);

            int jogCW = PinManager.GetInstance().JogCW;
            int jogCCW = PinManager.GetInstance().JogCCW;

            int selectA = PinManager.GetInstance().SelectA;

            int stepPinA = 0;
            int dirPinA = 0;

            int stepPinB = 0;
            int dirPinB = 0;

            if (m_RegisteredMotors["A"] is StepperMotor stepperMotorA &&
                m_RegisteredMotors["B"] is StepperMotor stepperMotorB)
            {
                stepPinA = stepperMotorA.StepPin;
                dirPinA = stepperMotorA.DirPin;

                stepPinB = stepperMotorB.StepPin;
                dirPinB = stepperMotorB.DirPin;
            }

            Console.WriteLine(
                $"\r\n[{DateTime.Now}] Jog mode ready, standing by, awaiting input. Emergency stop is used to get out of jog mode.");

            bool aIsSelected = true;

            while (m_UseJogMode)
            {
                if (PinManager.GetInstance().Controller.Read(selectA) == PinValue.Low)
                {
                    if (!aIsSelected)
                    {
                        aIsSelected = true;
                        flags.motor = "A";
                        Console.WriteLine($"\r\n[{DateTime.Now}] A motor selected to be jogged.");
                    }
                    else
                    {
                        aIsSelected = false;
                        flags.motor = "A";
                        Console.WriteLine($"\r\n[{DateTime.Now}] B motor selected to be jogged.");
                    }

                }

                bool flagToWatch = (flags.motor == "A") ? flags.A_CollisionFlag : flags.B_CollisionFlag;

                while (PinManager.GetInstance().Controller.Read(jogCW) == PinValue.Low && m_UseJogMode &&
                       !((flags.motor == "A") ? flags.A_CollisionFlag : flags.B_CollisionFlag))
                {
                    if (aIsSelected)
                    {
                        PinManager.GetInstance().Controller.Write(dirPinA, PinValue.High);
                        SingleStep(stepPinA);
                        Console.WriteLine($"\r\n[{DateTime.Now}] Moving A {dirPinA}");
                    }
                    else
                    {
                        PinManager.GetInstance().Controller.Write(dirPinB, PinValue.High);
                        SingleStep(stepPinB);
                        Console.WriteLine($"\r\n[{DateTime.Now}] Moving B CW {dirPinB}");
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(0.01));
                }

                while (PinManager.GetInstance().Controller.Read(jogCCW) == PinValue.Low && m_UseJogMode &&
                       !((flags.motor == "A") ? flags.A_CollisionFlag : flags.B_CollisionFlag))
                {
                    if (aIsSelected)
                    {
                        PinManager.GetInstance().Controller.Write(dirPinA, PinValue.Low);
                        SingleStep(stepPinA);
                        Console.WriteLine($"\r\n[{DateTime.Now}] Moving A {dirPinA}");
                    }
                    else
                    {
                        PinManager.GetInstance().Controller.Write(dirPinB, PinValue.Low);
                        SingleStep(stepPinB);
                        Console.WriteLine($"\r\n[{DateTime.Now}] Moving B CCW {dirPinB}");
                    }


                    Thread.Sleep(TimeSpan.FromSeconds(0.01));
                }


                //Thread.Sleep(TimeSpan.FromSeconds(0.1));
                if (PinManager.GetInstance().Controller.Read(exitButton) == PinValue.Low)
                {
                    Console.WriteLine($"\r\n[{DateTime.Now}] Stop button has been pressed. {exitButton}");
                    m_UseJogMode = false;
                    flags.stopFlag = true;
                }
            }

            Console.WriteLine($"\r\n[{DateTime.Now}] Exiting Jog Mode. Buttons disabled.");
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
                        Console.WriteLine($"Stepper motor{stepperMotor.MotorId}: Status - Added on Step Pin: {stepperMotor.StepPin}, Dir Pin {stepperMotor.DirPin} , Mode: Output \n");
                    }
                    else
                    {
                        Console.WriteLine($"Stepper motor{stepperMotor.MotorId}: Status - NOT ADDED");
                    }

                }
                else if (registeredMotor.Value is ServoMotor servoMotor)
                {
                    bool addedControlPin = PinManager.GetInstance().SetupPin(servoMotor.ControlPin, pinMode: PinMode.Output);

                    if (addedControlPin)
                    {
                        Console.WriteLine(($"Stepper motor{servoMotor.MotorId}: Status - Added on Control Pin: {servoMotor.ControlPin}, Mode: Output \n"));
                    }
                    else
                    {
                        Console.WriteLine($"Stepper motor{servoMotor.MotorId}: Status - NOT ADDED");
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