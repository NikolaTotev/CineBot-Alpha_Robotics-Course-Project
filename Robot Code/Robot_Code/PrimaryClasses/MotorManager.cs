using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Drawing.Text;
using System.IO.Ports;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using PrimaryClasses;
using Swan;
using Swan.DependencyInjection;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
using UtilityClasses;
using Encoder = PrimaryClasses.Encoder;

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
        private readonly int m_MinimumAngle = 2;
        private readonly float m_MinimumSpeed = 0.01f;
        private readonly float m_SpeedSensitivity = 1;
        private readonly float m_CollisionDetectionFrequency = 0.01f;
        private int m_MovementSensitivity = 1;
        private int m_StepsInRange = 0;
        public readonly bool CW = true;
        public readonly bool CCW = false;
        public bool JogModeFlag
        {
            get => m_UseJogMode;
            set => m_UseJogMode = value;
        }
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
            int partOfCircle = 360 / inputAngle;
            int stepsForInputAngle = stepsForCarrierRevolution / partOfCircle;
            return stepsForInputAngle;
        }

        public void MoveStepper(int angle, float speed, StepperMotorOptions stepperMotor, bool direction, bool useDebugMessages, FlagArgs flags)
        {
            int numberOfSteps = ConvertAngleToSteps(angle);
            Console.WriteLine($"Number of steps = {numberOfSteps}");
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

            int counter = 0;

            if (direction)
            {
                PinManager.GetInstance().Controller.Write(dirPin, PinValue.High);
            }
            else
            {
                PinManager.GetInstance().Controller.Write(dirPin, PinValue.Low);
            }

            while (counter < numberOfSteps)
            {
                if (useDebugMessages)
                {
                    Console.WriteLine($"Moving {stepperMotor}, currently on step {counter}");
                }

                if (flags.EmergencyStopActive)
                {
                    Console.WriteLine($"[{DateTime.Now}] <EMERGENCY>: Emergency button activated. Aborting Execution.");
                    flags.StopFlag = true;
                    break;
                }

                Console.WriteLine($"CAN MOVE RESULT = {CanMove(direction, flags)} {direction}");
                if (CanMove(direction, flags))
                {
                    PinManager.GetInstance().Controller.Write(stepPin, PinValue.High);
                    Thread.Sleep(TimeSpan.FromSeconds(speed));
                    PinManager.GetInstance().Controller.Write(stepPin, PinValue.Low);
                    Thread.Sleep(TimeSpan.FromSeconds(speed));
                    counter++;
                }
                else
                {
                    break;
                }
            }
        }

        //1= CW, 0 = CCW
        private bool CanMove(bool directionFlag, FlagArgs collisionInfo)
        {
            if (collisionInfo.CollisionFlag)
            {
                if (directionFlag == CCW && collisionInfo.topHit)
                {
                    return true;
                }

                if (directionFlag == CW && collisionInfo.bottomHit)
                {
                    return true;
                }

                return false;
            }

            return true;
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
                        // Console.WriteLine($"[{DateTime.Now}] <Collision Info>: Motor {flags.TargetStepperMotor} has reached an end-stop or the emergency switch as been activated..");
                        flags.CollisionFlag = true;
                        if (topSensorSate == PinValue.Low)
                        {
                            flags.topHit = true;
                        }

                        if (bottomSensorState == PinValue.Low)
                        {
                            flags.bottomHit = true;
                        }
                    }
                    else
                    {
                        flags.CollisionFlag = false;
                        flags.topHit = false;
                        flags.bottomHit = false;
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(m_CollisionDetectionFrequency));
                }
                Console.WriteLine($"[{DateTime.Now}] <Collision Info>: Stop flag has been raised. Ending collision detection.");
            }
        }


        /// <summary>
        /// Returns target section to the home positions. (DONE)
        /// </summary>
        /// <param name="targetStepperMotor"></param>
        /// <param name="useDebugMessages"></param>
        public void GoToHome(StepperMotorOptions targetStepperMotor, bool useDebugMessages)
        {
            int stepPin = 0;
            int dirPin = 0;

            int numberOfStepsToHome = 0;
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


            PinManager.GetInstance().Controller.Write(dirPin, PinValue.High);


            while (!flags.CollisionFlag || !flags.topHit)
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

                numberOfStepsToHome++;
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

            hasReachedEndStop = true;
            Console.WriteLine($"[{DateTime.Now}] There were {numberOfStepsToHome} taken to get to home position on {targetStepperMotor} ");

            PinManager.GetInstance().Controller.Write(dirPin, PinValue.Low);

            for (int i = 0; i < 20; i++)
            {
                PinManager.GetInstance().Controller.Write(stepPin, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(m_MinimumSpeed));
                PinManager.GetInstance().Controller.Write(stepPin, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(m_MinimumSpeed));
                Console.WriteLine("Move back");
            }
        }

        public void GoToHomeGimbal()
        {
            SerialComsManager.GetInstance().Write("P30");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            SerialComsManager.GetInstance().Write("P150");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            SerialComsManager.GetInstance().Write("P75");
            Thread.Sleep(TimeSpan.FromSeconds(1));


            SerialComsManager.GetInstance().Write("R30");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            SerialComsManager.GetInstance().Write("R150");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            SerialComsManager.GetInstance().Write("R75");
            Thread.Sleep(TimeSpan.FromSeconds(1));

            SerialComsManager.GetInstance().Write("T30");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            SerialComsManager.GetInstance().Write("T125");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            SerialComsManager.GetInstance().Write("T75");
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }

        public void MoveServo(int angle, ServoMotorOptions targetServo)
        {
            string servoLetter;

            switch (targetServo)
            {
                case ServoMotorOptions.pan:
                    servoLetter = "P";
                    break;
                case ServoMotorOptions.rotate:
                    servoLetter = "R";
                    break;
                case ServoMotorOptions.tilt:
                    servoLetter = "T";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetServo), targetServo, null);
            }
            SerialComsManager.GetInstance().Write($"{servoLetter}{angle}");
            Thread.Sleep(TimeSpan.FromSeconds(0.5));
        }

        public void JogMode(object threadParams)
        {
            if (threadParams is MotorThreadStartObj tParams)
            {
                Console.WriteLine($"[{DateTime.Now}] <Motor Manager Jog Mode>: Motor {tParams.TargetStepperMotor}: Control thread started.");
                Encoder currentEncoder;

                int emergencyButton = PinManager.GetInstance().EmergencyStop;
                switch (tParams.TargetStepperMotor)
                {
                    case StepperMotorOptions.motorA:
                        currentEncoder = new Encoder(EncoderOptions.Pan);
                        break;

                    case StepperMotorOptions.motorB:
                        currentEncoder = new Encoder(EncoderOptions.Rotate);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                float stepsFromHome = 0;

                FlagArgs flags = new FlagArgs(false, false, tParams.TargetStepperMotor);
                Thread collisionDetectorThread = new Thread(CollisionDetection);
                collisionDetectorThread.Start(flags);

                GoToHome(tParams.TargetStepperMotor, false);

                while (!tParams.ShouldStop)
                {

                    if (PinManager.GetInstance().Controller.Read(emergencyButton) == PinValue.Low)
                    {
                        Console.WriteLine($"[{DateTime.Now}] <EMERGENCY>: Emergency button activated. Aborting Execution.");
                        PinManager.GetInstance().EmergencyStopLights();
                        break;
                    }


                    if (currentEncoder.ReadSwitch() == PinValue.Low)
                    {
                        Console.WriteLine($"Go to home on motor {tParams.TargetStepperMotor} pressed.");
                        GoToHome(tParams.TargetStepperMotor, false);
                        Thread.Sleep(TimeSpan.FromMilliseconds(45));
                    }

                    currentEncoder.ReadSIA();
                    currentEncoder.ReadSIB();

                    if (currentEncoder.SIAValue != currentEncoder.LastSIAState)
                    {
                        bool direction;

                        //Console.WriteLine($"Encoder state for {tParams.TargetStepperMotor} has changed.");

                        //if (SIAValue == PinValue.High)
                        //{
                        //    Console.WriteLine("SIA HIGH");
                        //}
                        //else
                        //{
                        //    Console.WriteLine("SIA LOW");
                        //}


                        //if (SIBValue == PinValue.High)
                        //{
                        //    Console.WriteLine("SIB HIGH");
                        //}
                        //else
                        //{
                        //    Console.WriteLine("SIB LOW");
                        //}
                        if (currentEncoder.SIAValue == currentEncoder.SIBValue)
                        {
                            Console.WriteLine("CW ====================");
                            direction = CW;
                        }
                        else
                        {
                            Console.WriteLine("CCW ###################");
                            direction = CCW;
                        }

                        int movementAngle = m_MinimumAngle * m_MovementSensitivity;
                        float speed = m_MinimumSpeed * m_SpeedSensitivity;
                        MoveStepper(movementAngle, speed, tParams.TargetStepperMotor, direction, false, flags);

                        currentEncoder.ReadSIA();
                        currentEncoder.ReadSIB();
                        stepsFromHome++;
                    }
                    currentEncoder.SetLastState();
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
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

        public void RecordMotion()
        {
            Encoder stepperAController = new Encoder(EncoderOptions.Pan);
            Encoder stepperBController = new Encoder(EncoderOptions.Rotate);
            Encoder controlEncoder = new Encoder(EncoderOptions.Tilt);

            int stepperAStepPin = PinManager.GetInstance().JointAStep;
            int stepperADirPin = PinManager.GetInstance().JointADir;

            int stepperBStepPin = PinManager.GetInstance().JointBStep;
            int stepperBDirPin = PinManager.GetInstance().JointBDir;

            int stepperACounter = 0;
            int stepperBCounter = 0;
            int panCounter = 0;
            int rotateCounter = 0;
            int tiltCounter = 0;

            int stopButton = PinManager.GetInstance().EmergencyStop;

            Dictionary<StepperMotorOptions, int> stepperMovementSequence = new Dictionary<StepperMotorOptions, int>();
            Dictionary<ServoMotorOptions, int> servoMovementSequence = new Dictionary<ServoMotorOptions, int>();

            GoToHome(StepperMotorOptions.motorA, false);
            GoToHome(StepperMotorOptions.motorB, false);
            GoToHomeGimbal();

            RecordingModes mode = RecordingModes.StepperRecording;

            Console.WriteLine($"[{DateTime.Now}] [INFO]: Entering recording mode. To complete a recording press the emergency stop button.");

            while (PinManager.GetInstance().Controller.Read(stopButton) != PinValue.Low)
            {
                if (controlEncoder.ReadSwitch() == PinValue.Low)
                {
                    if (mode == RecordingModes.StepperRecording)
                    {
                        mode = RecordingModes.GimbalRecording;
                    }
                    if (mode == RecordingModes.GimbalRecording)
                    {
                        mode = RecordingModes.StepperRecording;
                    }
                }

                if (mode == RecordingModes.StepperRecording)
                {
                    stepperAController.ReadSIA();
                    stepperAController.ReadSIB();

                    stepperBController.ReadSIA();
                    stepperBController.ReadSIB();

                    #region  Stepper A control logic
                    if (stepperAController.SIAValue != stepperAController.LastSIAState)
                    {
                        bool direction;

                        if (stepperAController.SIAValue == stepperAController.SIBValue)
                        {
                            Console.WriteLine("A CW  ====================");
                            direction = CW;
                        }
                        else
                        {
                            Console.WriteLine("A CCW ###################");
                            direction = CCW;
                        }

                        int movementAngle = m_MinimumAngle * m_MovementSensitivity;
                        float speed = m_MinimumSpeed * m_SpeedSensitivity;

                        if (direction)
                        {
                            PinManager.GetInstance().Controller.Write(stepperADirPin, PinValue.High);
                        }
                        else
                        {
                            PinManager.GetInstance().Controller.Write(stepperADirPin, PinValue.Low);
                        }

                        for (int i = 0; i < movementAngle; i++)
                        {
                            PinManager.GetInstance().Controller.Write(stepperAStepPin, PinValue.High);
                            Thread.Sleep(TimeSpan.FromSeconds(speed));
                            PinManager.GetInstance().Controller.Write(stepperAStepPin, PinValue.Low);
                            Thread.Sleep(TimeSpan.FromSeconds(speed));
                        }


                        stepperAController.ReadSIA();
                        stepperAController.ReadSIB();
                        stepperACounter++;
                    }
                    stepperAController.SetLastState();
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                    #endregion

                    #region  Stepper B control logic
                    if (stepperBController.SIAValue != stepperBController.LastSIAState)
                    {
                        bool direction;

                        if (stepperBController.SIAValue == stepperBController.SIBValue)
                        {
                            Console.WriteLine("B CW  ====================");
                            direction = CW;
                        }
                        else
                        {
                            Console.WriteLine("B CCW ###################");
                            direction = CCW;
                        }

                        int movementAngle = m_MinimumAngle * m_MovementSensitivity;
                        float speed = m_MinimumSpeed * m_SpeedSensitivity;

                        if (direction)
                        {
                            PinManager.GetInstance().Controller.Write(stepperBDirPin, PinValue.High);
                        }
                        else
                        {
                            PinManager.GetInstance().Controller.Write(stepperBDirPin, PinValue.Low);
                        }

                        for (int i = 0; i < movementAngle; i++)
                        {
                            PinManager.GetInstance().Controller.Write(stepperBStepPin, PinValue.High);
                            Thread.Sleep(TimeSpan.FromSeconds(speed));
                            PinManager.GetInstance().Controller.Write(stepperBStepPin, PinValue.Low);
                            Thread.Sleep(TimeSpan.FromSeconds(speed));
                        }


                        stepperBController.ReadSIA();
                        stepperBController.ReadSIB();
                        stepperBCounter++;
                    }
                    stepperBController.SetLastState();
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                    #endregion
                }

                if (mode == RecordingModes.GimbalRecording)
                {

                }

            }
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