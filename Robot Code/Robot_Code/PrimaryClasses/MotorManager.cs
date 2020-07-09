using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.IO.Ports;
using System.Text;
using System.Threading;
using UtilityClasses;

namespace PrimaryClasses
{
    public enum PathNodeTypes { Stepper, Servo }


    public class PathNode
    {
        public PathNodeTypes NodeType { get; set; }
        public StepperMotorOptions TargetMotor { get; set; }
        public int StepsFromHome { get; set; }

        public int PanPosition { get; set; }
        public int RotatePosition { get; set; }
        public int TiltPosition { get; set; }

        public PathNode(PathNodeTypes type)
        {
            NodeType = type;
        }

        public void StepperPosition(StepperMotorOptions targetMotor, int stepsFromHome)
        {
            TargetMotor = targetMotor;
            StepsFromHome = stepsFromHome;
        }

        public void ServoPositions(int pan, int rot, int tilt)
        {
            if (NodeType == PathNodeTypes.Servo)
            {
                PanPosition = pan;
                RotatePosition = rot;
                TiltPosition = tilt;
            }
        }
    }

    public class MotorManager
    {
        private Dictionary<string, IMotor> m_RegisteredMotors;

        private bool m_UseJogMode = false;
        private readonly int m_StepsPerRevolution = 200;
        private readonly int m_StepMultiplier = 1;
        private readonly int m_JointBGearRatio = 7;
        private readonly int m_BaseGearRatio = 9;
        private readonly int m_MinimumAngle = 2;
        private readonly int m_MinimumServoAngle = 5;
        private readonly float m_SpeedSensitivity = 1;
        private readonly float m_CollisionDetectionFrequency = 0.01f;
        private int m_MovementSensitivity = 1;
        public readonly bool Cw = true;
        public readonly bool Ccw = false;

        private float m_OverallTime = 25;
        private float m_DefaultMaxSpeed = 0.002f;
        private float m_DefaultMinSpeed = 0.008f;

        // Poly motion variables 

        private float m_A0;
        private float m_A1;
        private float m_A2;
        private float m_A3;
        private float m_A4;
        private float m_A5;
        private float m_StartAngle = 0;
        private float m_FinalStep;
        private float m_StartVelocity = 0;
        private float m_FinalVelocity = 0;
        private float m_StartAcceleration = 0;
        private float m_FinalAcceleration = 0;
        private float m_ValueMapCoef;


        float m_Eti;
        private readonly float m_TimeDivisionSize = 0.0001f;

        // =====================


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
            StepperMotor motorA = new StepperMotor(MotorTypes.Stepper, "A", 90, -90, 20, 23);
            StepperMotor motorB = new StepperMotor(MotorTypes.Stepper, "B", 90, -90, 25, 24);
            m_RegisteredMotors = new Dictionary<string, IMotor>();
            m_RegisteredMotors.Add(motorA.MotorId, motorA);
            m_RegisteredMotors.Add(motorB.MotorId, motorB);
            //SetupMotors();
            Console.WriteLine($"\r\n[{DateTime.Now}] Motor Manager: Initializing...");
        }

        //ToDo add proper ratio selection based on used motor.
        public int ConvertAngleToSteps(int inputAngle, StepperMotorOptions targetMotor)
        {
            if (inputAngle == 0)
            {
                return 0;
            }

            int ratioToUse;
            switch (targetMotor)
            {
                case StepperMotorOptions.motorA:
                    ratioToUse = m_BaseGearRatio;
                    break;
                case StepperMotorOptions.motorB:
                    ratioToUse = m_JointBGearRatio;
                    break;
                default:
                    ratioToUse = m_JointBGearRatio;
                    break;
            }

            int stepsForCarrierRevolution = m_StepsPerRevolution * m_StepMultiplier * ratioToUse;
            int partOfCircle = 360 / inputAngle;

            if (partOfCircle == 0)
            {
                return 0;
            }

            int stepsForInputAngle = stepsForCarrierRevolution / partOfCircle;
            return stepsForInputAngle;
        }


        private void CalcCoefs(float motionTime)
        {
            m_A0 = m_StartAngle;
            m_A1 = m_StartVelocity;
            m_A2 = m_StartAcceleration / 2f;
            m_A3 = -1 * (20 * m_StartAngle - 20 * m_FinalStep + 12 * motionTime * m_StartVelocity + 8 * motionTime * m_FinalVelocity + 3 * m_StartAcceleration * motionTime * motionTime - m_FinalAcceleration * motionTime * motionTime) / (2 * motionTime * motionTime * motionTime);
            m_A4 = (30f * m_StartAngle - 30f * m_FinalStep + 16f * motionTime * m_StartVelocity + 14f * motionTime * m_FinalVelocity + 3f * m_StartAcceleration * motionTime * motionTime - 2f * m_FinalAcceleration * motionTime * motionTime) / (2f * motionTime * motionTime * motionTime * motionTime);
            m_A5 = -1f * (12f * m_StartAngle - 12f * m_FinalStep + 6f * motionTime * m_StartVelocity + 6f * motionTime * m_FinalVelocity + m_StartAcceleration * motionTime * motionTime - m_FinalAcceleration * motionTime * motionTime) / (2f * motionTime * motionTime * motionTime * motionTime * motionTime);
            if (float.IsNaN(m_A0))//make sure it is not undefined
            {
                m_A0 = 0;
            }
            if (float.IsNaN(m_A1))//make sure it is not undefined
            {
                m_A1 = 0;
            }
            if (float.IsNaN(m_A2))//make sure it is not undefined
            {
                m_A2 = 0;
            }
            if (float.IsNaN(m_A3))//make sure it is not undefined
            {
                m_A3 = 0;
            }
            if (float.IsNaN(m_A4))//make sure it is not undefined
            {
                m_A4 = 0;
            }
            if (float.IsNaN(m_A5))//make sure it is not undefined
            {
                m_A5 = 0;
            }
        }

        private float GetSpeed(float currentTime)
        {
            return m_A5 * 5 * (float)Math.Pow(currentTime, 4) + m_A4 * 4 * (float)Math.Pow(currentTime, 3) + m_A3 * 3 * (float)Math.Pow(currentTime, 2) + m_A2 * 2 * (float)Math.Pow(currentTime, 1) + m_A1;
        }

        private void InitMapCoef()
        {
            if (Math.Abs(m_FinalStep) > 0.01)
            {
                m_Eti = m_OverallTime / m_FinalStep;
            }
            else
            {
                Console.WriteLine("Error in Map Coef. initialization! (# of steps = 0)");
            }

            float numberOfTimeDivisions = m_OverallTime / m_TimeDivisionSize;
            float funcMaxSpeed = GetSpeed((numberOfTimeDivisions / 2) * m_TimeDivisionSize);
            if (funcMaxSpeed != 0)
            {
                m_ValueMapCoef = (m_DefaultMaxSpeed - m_DefaultMinSpeed) / funcMaxSpeed;
            }
            else
            {
                Console.WriteLine("Error in Map Coef. initialization! (funcMaxSpeed = 0)");

            }
            Console.WriteLine($"Number of time divisions {numberOfTimeDivisions} \n funcMaxSpeed {funcMaxSpeed} \n Eti {m_Eti} \n Final Angle {m_FinalStep}");
        }

        private float mapValue(float input)
        {
            return m_ValueMapCoef * input + m_DefaultMinSpeed;
        }

        public void MoveStepper(int angle, float speed, StepperMotorOptions stepperMotor, bool direction,
            bool useDebugMessages, FlagArgs flags, int steps = 0, bool usePoly = false)
        {
            int numberOfSteps = 0;
            if (steps == 0)
            {
                numberOfSteps = ConvertAngleToSteps(angle, stepperMotor);
            }
            else
            {
                numberOfSteps = steps;
            }
            if (usePoly)
            {
                m_FinalStep = numberOfSteps;
                CalcCoefs(m_OverallTime);
                InitMapCoef();
            }

            int stepPin;
            int dirPin;

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


            if (usePoly)
            {
                for (float i = 0; i <= m_OverallTime; i += (m_Eti))
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

                    float stepperSpeed = mapValue(GetSpeed(i));
                    PinManager.GetInstance().Controller.Write(stepPin, PinValue.High);
                    Thread.Sleep(TimeSpan.FromSeconds(stepperSpeed));
                    PinManager.GetInstance().Controller.Write(stepPin, PinValue.Low);
                    Thread.Sleep(TimeSpan.FromSeconds(stepperSpeed));
                    counter++;
                }

            }
            else
            {
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
        }

        //1= CW, 0 = CCW

        /// <summary>
        /// Determines if the robot can move depending on which limit switches are activated and what the desired direction is.
        /// </summary>
        /// <param name="directionFlag"></param>
        /// <param name="collisionInfo"></param>
        /// <returns></returns>
        private bool CanMove(bool directionFlag, FlagArgs collisionInfo)
        {
            if (collisionInfo.CollisionFlag)
            {
                if (directionFlag == Ccw && collisionInfo.topHit)
                {
                    return true;
                }

                if (directionFlag == Cw && collisionInfo.bottomHit)
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Tracks for collisions. If a limit switch is activated appropriate flags are raised.
        /// This function runs separately for each motor and on a different thread.
        /// This function can detect which limit switch has been hit.
        /// </summary>
        /// <param name="boolObject"></param>
        private void CollisionDetection(object boolObject)
        {
            Console.WriteLine($"[{DateTime.Now}] <Collision Info>: Collision detection enabled.");
            if (boolObject is FlagArgs flags)
            {
                int topSensor;
                int bottomSensor;
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

                    if (topSensorSate == PinValue.Low || bottomSensorState == PinValue.Low || emergencyButtonState == PinValue.Low)
                    {
                        if (emergencyButtonState == PinValue.Low)
                        {
                            flags.EmergencyStopActive = true;
                        }

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
        /// Returns target stepper motor to the home position.
        /// </summary>
        /// <param name="targetStepperMotor"></param>
        /// <param name="useDebugMessages"></param>
        public void GoToHome(StepperMotorOptions targetStepperMotor, bool useDebugMessages)
        {
            int stepPin;
            int dirPin;

            int numberOfStepsToHome = 0;
            bool direction = false;

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
                Thread.Sleep(TimeSpan.FromSeconds(m_DefaultMinSpeed));
                PinManager.GetInstance().Controller.Write(stepPin, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(m_DefaultMinSpeed));

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

            Console.WriteLine($"[{DateTime.Now}] There were {numberOfStepsToHome} taken to get to home position on {targetStepperMotor} ");

            PinManager.GetInstance().Controller.Write(dirPin, PinValue.Low);

            for (int i = 0; i < 20; i++)
            {
                PinManager.GetInstance().Controller.Write(stepPin, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(m_DefaultMinSpeed));
                PinManager.GetInstance().Controller.Write(stepPin, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(m_DefaultMinSpeed));
                Console.WriteLine("Move back");
            }
        }

        /// <summary>
        /// Returns gimbal to the home (center) position.
        /// </summary>
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

        /// <summary>
        /// Allows for movement 
        /// </summary>
        /// <param name="threadParams"></param>
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

                FlagArgs flags = new FlagArgs(false, false, tParams.TargetStepperMotor);
                Thread collisionDetectorThread = new Thread(CollisionDetection);
                collisionDetectorThread.Start(flags);

                GoToHome(tParams.TargetStepperMotor, false);

                while (!tParams.ShouldStop)
                {

                    if (PinManager.GetInstance().Controller.Read(emergencyButton) == PinValue.Low)
                    {
                        Console.WriteLine($"[{DateTime.Now}] <EMERGENCY>: Emergency button activated. Aborting Execution.");
                        NotificationManager.EmergencyStopLights();
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
                            direction = Cw;
                        }
                        else
                        {
                            Console.WriteLine("CCW ###################");
                            direction = Ccw;
                        }

                        int movementAngle = m_MinimumAngle * m_MovementSensitivity;
                        float speed = m_DefaultMinSpeed * m_SpeedSensitivity;
                        MoveStepper(movementAngle, speed, tParams.TargetStepperMotor, direction, false, flags);

                        currentEncoder.ReadSIA();
                        currentEncoder.ReadSIB();
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

            float panCounter = 75;
            float rotCounter = 80;
            float tiltCounter = 80;

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
                    panCounter = 75;
                    serialPort.WriteLine($"P{panCounter}");
                    Thread.Sleep(TimeSpan.FromMilliseconds(45));
                }

                if (PinManager.GetInstance().Controller.Read(resetRot) == PinValue.Low)
                {
                    rotCounter = 80;
                    serialPort.WriteLine($"R{rotCounter}");
                    Thread.Sleep(TimeSpan.FromMilliseconds(45));
                }

                if (PinManager.GetInstance().Controller.Read(resetTilt) == PinValue.Low)
                {
                    tiltCounter = 80;
                    serialPort.WriteLine($"T{tiltCounter}");
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
                        panCounter += 2.5f;
                    }
                    else
                    {
                        panCounter -= 2.5f;
                    }

                    if (Math.Abs(panCounter % 1) < 0.01)
                    {
                        Console.WriteLine($"Step: P{panCounter}");
                        serialPort.WriteLine($"P{panCounter}");
                        Thread.Sleep(TimeSpan.FromMilliseconds(35));
                    }

                    if (Math.Abs(panCounter - 360) < 0.0001 || Math.Abs(panCounter - (-360)) < 0.0001)
                    {
                        panCounter = 0;
                    }
                }


                if (RotState != RotLastState)
                {
                    if (PinManager.GetInstance().Controller.Read(rotSib) != RotState)
                    {
                        rotCounter += 2.5f;
                    }
                    else
                    {
                        rotCounter -= 2.5f;
                    }

                    if (Math.Abs(rotCounter % 1) < 0.01)
                    {
                        Console.WriteLine($"Step: R{rotCounter}");
                        serialPort.WriteLine($"R{rotCounter}");
                        Thread.Sleep(TimeSpan.FromMilliseconds(35));
                    }

                    if (Math.Abs(rotCounter - 360) < 0.0001 || Math.Abs(rotCounter - (-360)) < 0.0001)
                    {
                        rotCounter = 0;
                    }
                }


                if (TiltState != TiltLastState)
                {
                    if (PinManager.GetInstance().Controller.Read(tiltSib) != TiltState)
                    {
                        tiltCounter += 2.5f;
                    }
                    else
                    {
                        tiltCounter -= 2.5f;
                    }

                    if (Math.Abs(tiltCounter % 1) < 0.01)
                    {
                        Console.WriteLine($"Step: T{tiltCounter}");
                        serialPort.WriteLine($"T{tiltCounter}");
                        Thread.Sleep(TimeSpan.FromMilliseconds(35));
                    }

                    if (Math.Abs(tiltCounter - 360) < 0.0001 || Math.Abs(tiltCounter - (-360)) < 0.0001)
                    {
                        tiltCounter = 0;
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
            int panCounter = 75;
            int rotateCounter = 75;
            int tiltCounter = 75;

            int led1 = PinManager.GetInstance().NotificaitonLight;
            int led2 = PinManager.GetInstance().ErrorLight;
            int led3 = PinManager.GetInstance().StatusLight;

            bool hasLetGoOfButton = true;
            int numberOfStepperNodes = 0;
            int numberOfGimbalNodes = 0;
            int stopButton = PinManager.GetInstance().EmergencyStop;

            Dictionary<int, PathNode> movementSequence = new Dictionary<int, PathNode>();

            GoToHome(StepperMotorOptions.motorA, false);
            GoToHome(StepperMotorOptions.motorB, false);
            GoToHomeGimbal();

            RecordingModes mode = RecordingModes.StepperRecording;
            bool hasSelectedFile = false;
            int saveFile = 0;

            Console.WriteLine($"Please select a save file. Current selection is file number {saveFile}");
            while (!hasSelectedFile)
            {
                switch (saveFile)
                {
                    case 0:
                        PinManager.GetInstance().Controller.Write(led1, PinValue.High);
                        PinManager.GetInstance().Controller.Write(led2, PinValue.Low);
                        PinManager.GetInstance().Controller.Write(led3, PinValue.Low);
                        break;
                    case 1:
                        PinManager.GetInstance().Controller.Write(led1, PinValue.Low);
                        PinManager.GetInstance().Controller.Write(led2, PinValue.High);
                        PinManager.GetInstance().Controller.Write(led3, PinValue.Low);
                        break;
                    case 2:
                        PinManager.GetInstance().Controller.Write(led1, PinValue.Low);
                        PinManager.GetInstance().Controller.Write(led2, PinValue.Low);
                        PinManager.GetInstance().Controller.Write(led3, PinValue.High);
                        break;
                }

                if (controlEncoder.ReadSwitch() == PinValue.Low)
                {
                    hasSelectedFile = true;
                }
                controlEncoder.ReadSIA();
                controlEncoder.ReadSIB();
                if (controlEncoder.CompareStates())
                {
                    bool direction = controlEncoder.GetDirection();

                    if (direction)
                    {
                        if (saveFile > 0)
                        {
                            saveFile -= 1;
                        }

                    }
                    else
                    {
                        if (saveFile <= 1)
                        {
                            saveFile += 1;
                        }
                    }

                    controlEncoder.ReadSIA();
                    controlEncoder.ReadSIB();
                }
                controlEncoder.SetLastState();
                Thread.Sleep(TimeSpan.FromMilliseconds(1));

            }
            Console.WriteLine($"Save file {saveFile} has been selected.");


            Console.WriteLine($"[{DateTime.Now}] [INFO]: Entering recording mode. To complete a recording press the emergency stop button.");

            while (PinManager.GetInstance().Controller.Read(stopButton) != PinValue.Low)
            {
                if (controlEncoder.ReadSwitch() == PinValue.Low)
                {
                    if (mode == RecordingModes.StepperRecording)
                    {
                        mode = RecordingModes.GimbalRecording;
                    }
                    else
                    {
                        mode = RecordingModes.StepperRecording;
                    }

                    Console.WriteLine($"[Info]: Mode has been changed to {mode}");
                    PinManager.GetInstance().Controller.Write(led1, PinValue.High);
                    PinManager.GetInstance().Controller.Write(led3, PinValue.High);
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    PinManager.GetInstance().Controller.Write(led1, PinValue.Low);
                    PinManager.GetInstance().Controller.Write(led3, PinValue.Low);
                }

                if (mode == RecordingModes.StepperRecording)
                {
                    #region  Stepper A control logic

                    if (stepperAController.CompareStates())
                    {
                        bool direction = stepperAController.GetDirection();

                        int numberOfSteps = ConvertAngleToSteps(m_MinimumAngle * m_MovementSensitivity, StepperMotorOptions.motorA);

                        float speed = m_DefaultMinSpeed * m_SpeedSensitivity;

                        if (direction)
                        {
                            PinManager.GetInstance().Controller.Write(stepperADirPin, PinValue.High);
                            stepperACounter -= numberOfSteps;
                        }
                        else
                        {
                            PinManager.GetInstance().Controller.Write(stepperADirPin, PinValue.Low);
                            stepperACounter += numberOfSteps;
                        }

                        for (int i = 0; i < numberOfSteps; i++)
                        {
                            PinManager.GetInstance().Controller.Write(stepperAStepPin, PinValue.High);
                            Thread.Sleep(TimeSpan.FromSeconds(speed));
                            PinManager.GetInstance().Controller.Write(stepperAStepPin, PinValue.Low);
                            Thread.Sleep(TimeSpan.FromSeconds(speed));
                        }


                        stepperAController.ReadSIA();
                        stepperAController.ReadSIB();
                    }
                    stepperAController.SetLastState();
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                    #endregion

                    #region  Stepper B control logic
                    stepperBController.ReadSIA();
                    stepperBController.ReadSIB();

                    if (stepperBController.CompareStates())
                    {
                        bool direction = stepperBController.GetDirection();

                        int numberOfSteps = ConvertAngleToSteps(m_MinimumAngle * m_MovementSensitivity, StepperMotorOptions.motorB);

                        float speed = m_DefaultMinSpeed * m_SpeedSensitivity;

                        if (direction)
                        {
                            PinManager.GetInstance().Controller.Write(stepperBDirPin, PinValue.High);
                            stepperBCounter -= numberOfSteps;
                        }

                        else
                        {
                            PinManager.GetInstance().Controller.Write(stepperBDirPin, PinValue.Low);
                            stepperBCounter += numberOfSteps;

                        }

                        for (int i = 0; i < numberOfSteps; i++)
                        {
                            PinManager.GetInstance().Controller.Write(stepperBStepPin, PinValue.High);
                            Thread.Sleep(TimeSpan.FromSeconds(speed));
                            PinManager.GetInstance().Controller.Write(stepperBStepPin, PinValue.Low);
                            Thread.Sleep(TimeSpan.FromSeconds(speed));
                        }


                        stepperBController.ReadSIA();
                        stepperBController.ReadSIB();
                    }
                    stepperBController.SetLastState();
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                    #endregion

                    if (!hasLetGoOfButton)
                    {
                        if (stepperAController.ReadSwitch() != PinValue.Low && stepperBController.ReadSwitch() != PinValue.Low)
                        {
                            hasLetGoOfButton = true;
                        }
                        else
                        {

                            Console.WriteLine("Please remove finger from save button!");
                            for (int i = 0; i < 2; i++)
                            {
                                PinManager.GetInstance().Controller.Write(led1, PinValue.High);
                                PinManager.GetInstance().Controller.Write(led2, PinValue.High);
                                PinManager.GetInstance().Controller.Write(led3, PinValue.High);
                                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                                PinManager.GetInstance().Controller.Write(led1, PinValue.Low);
                                PinManager.GetInstance().Controller.Write(led2, PinValue.Low);
                                PinManager.GetInstance().Controller.Write(led3, PinValue.Low);
                                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                                PinManager.GetInstance().Controller.Write(led1, PinValue.High);
                                PinManager.GetInstance().Controller.Write(led2, PinValue.High);
                                PinManager.GetInstance().Controller.Write(led3, PinValue.High);
                                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                                PinManager.GetInstance().Controller.Write(led1, PinValue.Low);
                                PinManager.GetInstance().Controller.Write(led2, PinValue.Low);
                                PinManager.GetInstance().Controller.Write(led3, PinValue.Low);
                                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                            }

                            Thread.Sleep(TimeSpan.FromSeconds(1));
                        }
                    }


                    if (stepperAController.ReadSwitch() == PinValue.Low && hasLetGoOfButton)
                    {
                        Console.WriteLine($"Path node added for {StepperMotorOptions.motorA} that is {stepperACounter} steps from the home position.");
                        PathNode nodeToAdd = new PathNode(PathNodeTypes.Stepper);
                        nodeToAdd.StepperPosition(StepperMotorOptions.motorA, stepperACounter);
                        movementSequence.Add(movementSequence.Count + 1, nodeToAdd);
                        numberOfStepperNodes++;
                        hasLetGoOfButton = false;
                        PinManager.GetInstance().Controller.Write(led1, PinValue.High);
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                        PinManager.GetInstance().Controller.Write(led1, PinValue.Low);
                    }

                    if (stepperBController.ReadSwitch() == PinValue.Low && hasLetGoOfButton)
                    {
                        Console.WriteLine($"Path node added for {StepperMotorOptions.motorB} that is {stepperBCounter} steps from the home position.");
                        PathNode nodeToAdd = new PathNode(PathNodeTypes.Stepper);
                        nodeToAdd.StepperPosition(StepperMotorOptions.motorB, stepperBCounter);
                        movementSequence.Add(movementSequence.Count + 1, nodeToAdd);
                        numberOfStepperNodes++;
                        hasLetGoOfButton = false;
                        PinManager.GetInstance().Controller.Write(led2, PinValue.High);
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                        PinManager.GetInstance().Controller.Write(led2, PinValue.Low);
                    }
                }

                if (mode == RecordingModes.GimbalRecording)
                {

                    #region PanServo
                    if (stepperAController.CompareStates())
                    {
                        bool direction = stepperAController.GetDirection();

                        if (direction)
                        {
                            panCounter -= m_MinimumServoAngle;
                        }
                        else
                        {
                            panCounter += m_MinimumServoAngle;
                        }

                        Console.WriteLine($"Pan servo moved {panCounter}");
                        MoveServo(panCounter, ServoMotorOptions.pan);

                        stepperAController.ReadSIA();
                        stepperAController.ReadSIB();
                    }
                    stepperAController.SetLastState();
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                    #endregion

                    #region Rotate
                    if (stepperBController.CompareStates())
                    {
                        bool direction = stepperBController.GetDirection();

                        if (direction)
                        {
                            rotateCounter -= m_MinimumServoAngle;
                        }
                        else
                        {
                            rotateCounter += m_MinimumServoAngle;
                        }

                        Console.WriteLine($"Rot servo moved {panCounter}");
                        MoveServo(rotateCounter, ServoMotorOptions.rotate);

                        stepperBController.ReadSIA();
                        stepperBController.ReadSIB();
                    }
                    stepperBController.SetLastState();
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                    #endregion

                    #region Tilt
                    if (controlEncoder.CompareStates())
                    {
                        if (controlEncoder.GetDirection())
                        {
                            tiltCounter -= m_MinimumServoAngle;
                        }
                        else
                        {
                            tiltCounter += m_MinimumServoAngle;
                        }

                        Console.WriteLine($"Tilt servo moved {panCounter}");
                        MoveServo(tiltCounter, ServoMotorOptions.tilt);

                        controlEncoder.ReadSIA();
                        controlEncoder.ReadSIB();
                    }
                    controlEncoder.SetLastState();
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                    #endregion

                    //TODO This can be in a separate function.
                    if (!hasLetGoOfButton)
                    {
                        if (stepperAController.ReadSwitch() != PinValue.Low && stepperBController.ReadSwitch() != PinValue.Low)
                        {
                            hasLetGoOfButton = true;
                        }
                        else
                        {

                            Console.WriteLine("Please remove finger from save button!");
                            for (int i = 0; i < 2; i++)
                            {
                                PinManager.GetInstance().Controller.Write(led1, PinValue.High);
                                PinManager.GetInstance().Controller.Write(led2, PinValue.High);
                                PinManager.GetInstance().Controller.Write(led3, PinValue.High);
                                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                                PinManager.GetInstance().Controller.Write(led1, PinValue.Low);
                                PinManager.GetInstance().Controller.Write(led2, PinValue.Low);
                                PinManager.GetInstance().Controller.Write(led3, PinValue.Low);
                                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                                PinManager.GetInstance().Controller.Write(led1, PinValue.High);
                                PinManager.GetInstance().Controller.Write(led2, PinValue.High);
                                PinManager.GetInstance().Controller.Write(led3, PinValue.High);
                                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                                PinManager.GetInstance().Controller.Write(led1, PinValue.Low);
                                PinManager.GetInstance().Controller.Write(led2, PinValue.Low);
                                PinManager.GetInstance().Controller.Write(led3, PinValue.Low);
                                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                            }

                            Thread.Sleep(TimeSpan.FromSeconds(1));
                        }
                    }

                    if (stepperAController.ReadSwitch() == PinValue.Low && hasLetGoOfButton)
                    {
                        Console.WriteLine($"Path node added for gimbal. Values are: Pan {panCounter} | Rot. {rotateCounter} | Tilt {tiltCounter}.");
                        PathNode nodeToAdd = new PathNode(PathNodeTypes.Servo);
                        nodeToAdd.ServoPositions(panCounter, rotateCounter, tiltCounter);
                        movementSequence.Add(movementSequence.Count + 1, nodeToAdd);
                        numberOfGimbalNodes++;
                        hasLetGoOfButton = false;
                        PinManager.GetInstance().Controller.Write(led1, PinValue.High);
                        PinManager.GetInstance().Controller.Write(led3, PinValue.High);
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                        PinManager.GetInstance().Controller.Write(led1, PinValue.Low);
                        PinManager.GetInstance().Controller.Write(led3, PinValue.Low);
                    }

                }
            }
            Console.WriteLine($"Recording complete. There are {movementSequence.Count} path nodes. \n Stepper nodes: {numberOfStepperNodes} \n Gimbal nodes: {numberOfGimbalNodes}");

            SerializerManager.SaveMotionPath(movementSequence, saveFile);

            Console.WriteLine("The recorded path will now be replayed.");
            Thread.Sleep(TimeSpan.FromSeconds(3));
            FollowPath(saveFile);
        }

        public void Dance()
        {

        }


        public void FollowPath(int file)
        {
            GoToHome(StepperMotorOptions.motorA, false);
            GoToHome(StepperMotorOptions.motorB, false);
            GoToHomeGimbal();

            FlagArgs Aflags = new FlagArgs(false, false, StepperMotorOptions.motorA);
            Thread collisionDetectorThreadA = new Thread(CollisionDetection);
            collisionDetectorThreadA.Start(Aflags);

            FlagArgs Bflags = new FlagArgs(false, false, StepperMotorOptions.motorB);
            Thread collisionDetectorThreadB = new Thread(CollisionDetection);
            collisionDetectorThreadB.Start(Aflags);

            int currentAStepperPosition = 0;
            int currentBStepperPosition = 0;
            bool stepperDir;
            int stepADelta;
            int stepBDelta;

            float follwoSpeed = m_DefaultMinSpeed * m_SpeedSensitivity;

            int emergencyButton = PinManager.GetInstance().EmergencyStop;

            Dictionary<int, PathNode> movementSequence = SerializerManager.GetMotionPath(file);

            if (movementSequence.Count == 0)
            {
                NotificationManager.DoublePulse();
                Console.WriteLine($"Motion path {file} is empty.");
            }

            foreach (var pathNode in movementSequence)
            {
                if (PinManager.GetInstance().Controller.Read(emergencyButton) == PinValue.Low)
                {
                    Console.WriteLine($"[{DateTime.Now}] <EMERGENCY>: Emergency button activated. Aborting Execution.");
                    NotificationManager.EmergencyStopLights();
                    break;
                }


                if (pathNode.Value.NodeType == PathNodeTypes.Stepper)
                {
                    switch (pathNode.Value.TargetMotor)
                    {
                        case StepperMotorOptions.motorA:
                            stepADelta = Math.Abs(pathNode.Value.StepsFromHome - currentAStepperPosition);
                            if (pathNode.Value.StepsFromHome > currentAStepperPosition)
                            {
                                stepperDir = Ccw;
                                currentAStepperPosition += stepADelta;
                            }
                            else
                            {
                                stepperDir = Cw;
                                currentAStepperPosition -= stepADelta;
                            }
                            Console.WriteLine($"Moving motor {StepperMotorOptions.motorA} with delta {stepADelta} and direction {stepperDir}");
                            MoveStepper(0, follwoSpeed, StepperMotorOptions.motorA, stepperDir, false, Aflags, stepADelta, true);
                            break;
                        case StepperMotorOptions.motorB:
                            stepBDelta = Math.Abs(pathNode.Value.StepsFromHome - currentBStepperPosition);
                            if (pathNode.Value.StepsFromHome > currentBStepperPosition)
                            {
                                stepperDir = Ccw;
                                currentBStepperPosition += pathNode.Value.StepsFromHome;
                            }
                            else
                            {
                                stepperDir = Cw;
                                currentBStepperPosition -= pathNode.Value.StepsFromHome;
                            }

                            Console.WriteLine($"Moving motor {StepperMotorOptions.motorB} with delta {stepBDelta} and direction {stepperDir}");
                            MoveStepper(0, follwoSpeed, StepperMotorOptions.motorB, stepperDir, false, Bflags, stepBDelta, true);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (pathNode.Value.NodeType == PathNodeTypes.Servo)
                {
                    Console.WriteLine($"Moving servos: \n Pan: {pathNode.Value.PanPosition} \n Rot: {pathNode.Value.RotatePosition} \n Tilt: {pathNode.Value.TiltPosition}");
                    MoveServo(pathNode.Value.PanPosition, ServoMotorOptions.pan);
                    Thread.Sleep(TimeSpan.FromMilliseconds(420));

                    MoveServo(pathNode.Value.RotatePosition, ServoMotorOptions.rotate);
                    Thread.Sleep(TimeSpan.FromMilliseconds(420));

                    MoveServo(pathNode.Value.TiltPosition, ServoMotorOptions.tilt);
                    Thread.Sleep(TimeSpan.FromMilliseconds(420));
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