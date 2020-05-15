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
using UtilityClasses;

namespace Motor_Control
{
    public class MotorManager
    {
        private Dictionary<string, IMotor> m_RegisteredMotors;

        private static MotorManager m_Instance;
        private bool m_UseJogMode = false;

        public bool JogModeFlag
        {
            get => m_UseJogMode;
            set => m_UseJogMode = value;
        }

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
            StepperMotor MotorA = new StepperMotor(MotorTypes.Stepper, "A", 90, -90, 18, 23);
            StepperMotor MotorB = new StepperMotor(MotorTypes.Stepper, "B", 90, -90, 25, 24);
            m_RegisteredMotors = new Dictionary<string, IMotor>();
            m_RegisteredMotors.Add(MotorA.MotorId, MotorA);
            m_RegisteredMotors.Add(MotorB.MotorId, MotorB);
            //SetupMotors();
            Console.WriteLine($"\r\n[{DateTime.Now}] Motor Manager: Initializing...");
        }


        public int ConvertAngle(float inputAngle)
        {
            return 1;
        }
        public void MoveStepper(int angle, float speed, string motor, bool direction)
        {

            int numberOfSteps = ConvertAngle(angle);

            int stepPin = 0;
            int dirPin = 0;
            if (m_RegisteredMotors[motor] is StepperMotor stepperMotor)
            {
                switch (motor)
                {
                    case "A":

                        stepPin = stepperMotor.StepPin;
                        dirPin = stepperMotor.DirPin;
                        break;

                    case "B":

                        stepPin = stepperMotor.StepPin;
                        dirPin = stepperMotor.DirPin;
                        break;
                }
            }

            FlagArgs flags = new FlagArgs(false, false, false, "A");
            Thread collisionDetectorThread = new Thread(CollisionDetection);
            collisionDetectorThread.Start(flags);
            int counter = 0;

            if (!direction)
            {
                PinManager.GetInstance().Controller.Write(dirPin, PinValue.Low);
            }
            PinManager.GetInstance().Controller.Write(dirPin, PinValue.High);
            // PinManager.GetInstance().Controller.Write(dirPin, PinValue.Low);

            bool flagToWatch = (flags.motor == "A") ? flags.A_CollisionFlag : flags.B_CollisionFlag;

            while (counter < numberOfSteps && !flagToWatch)
            {
                Console.WriteLine($"{counter}: Moove moove {flagToWatch}");
                //  PinManager.GetInstance().Controller.Write(stepPin, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(speed));
                // PinManager.GetInstance().Controller.Write(stepPin, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(speed));
                counter++;
            }
            flags.stopFlag = true;

            collisionDetectorThread.Join();
        }

        private void CollisionDetection(object boolObject)
        {
            Console.WriteLine("ENTERING COLLISION DETECTION");
            if (boolObject is FlagArgs flags)
            {
                int topSensorA = 1;
                int bottomSensorA = 1;

                int topSensorB = 1;
                int bottomSensorB = 1;
                int emergencyStop = PinManager.GetInstance().EmergencyStop;


                topSensorA = PinManager.GetInstance().JointATop;
                bottomSensorA = PinManager.GetInstance().JointABottom;



                topSensorB = PinManager.GetInstance().JointBTop;
                bottomSensorB = PinManager.GetInstance().JointBBottom;


                PinValue topSensorSateA;
                PinValue bottomSensorStateA;

                PinValue topSensorSateB;
                PinValue bottomSensorStateB;
                while (!flags.stopFlag)
                {
                    topSensorSateA = PinManager.GetInstance().Controller.Read(topSensorA);
                    bottomSensorStateA = PinManager.GetInstance().Controller.Read(bottomSensorA);

                    topSensorSateB = PinManager.GetInstance().Controller.Read(topSensorB);
                    bottomSensorStateB = PinManager.GetInstance().Controller.Read(bottomSensorB);

                    if (flags.motor == "A")
                    {
                        if (topSensorSateA == PinValue.Low || bottomSensorStateA == PinValue.Low || emergencyStop == PinValue.Low)
                        {
                            Console.WriteLine("Boop Top of A");
                            flags.A_CollisionFlag = true;
                        }
                        else
                        {
                            flags.A_CollisionFlag = false;
                        }

                    }

                    if (flags.motor == "B")
                    {
                        if (topSensorSateB == PinValue.Low || bottomSensorStateB == PinValue.Low || emergencyStop == PinValue.Low)
                        {
                            Console.WriteLine("Boop Top of B");
                            flags.B_CollisionFlag = true;
                        }
                        else
                        {
                            flags.B_CollisionFlag = false;
                        }
                    }


                    Thread.Sleep(TimeSpan.FromSeconds(0.01));
                }
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

            if (m_RegisteredMotors["A"] is StepperMotor stepperMotorA && m_RegisteredMotors["B"] is StepperMotor stepperMotorB)
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

        public void JogMode()
        {
            Console.WriteLine($"\r\n[{DateTime.Now}] Jog mode enabled. Control the arm via the buttons.");

            m_UseJogMode = true;

            int exitButton = PinManager.GetInstance().EmergencyStop;

            FlagArgs flags = new FlagArgs(false, false, false, "A");
            Thread collisionDetectorThread = new Thread(CollisionDetection);
            collisionDetectorThread.Start(flags);

            int jogCW = PinManager.GetInstance().JogCW;
            int jogCCW = PinManager.GetInstance().JogCCW;

            int selectA = PinManager.GetInstance().SelectA;
            int selectB = PinManager.GetInstance().SelectB;

            int stepPinA = 0;
            int dirPinA = 0;

            int stepPinB = 0;
            int dirPinB = 0;

            if (m_RegisteredMotors["A"] is StepperMotor stepperMotorA && m_RegisteredMotors["B"] is StepperMotor stepperMotorB)
            {
                stepPinA = stepperMotorA.StepPin;
                dirPinA = stepperMotorA.DirPin;

                stepPinB = stepperMotorB.StepPin;
                dirPinB = stepperMotorB.DirPin;
            }

            Console.WriteLine($"\r\n[{DateTime.Now}] Jog mode ready, standing by, awaiting input. Emergency stop is used to get out of jog mode.");

            bool aIsSelected = true;

            while (m_UseJogMode)
            {
                if (PinManager.GetInstance().Controller.Read(selectA) == PinValue.Low)
                {
                    aIsSelected = true;
                    flags.motor = "A";
                    Console.WriteLine($"\r\n[{DateTime.Now}] A motor selected to be jogged.");
                }

                if (PinManager.GetInstance().Controller.Read(selectB) == PinValue.Low)
                {
                    aIsSelected = false;
                    flags.motor = "B";
                    Console.WriteLine($"\r\n[{DateTime.Now}] B motor selected to be jogged.");
                }
                bool flagToWatch = (flags.motor == "A") ? flags.A_CollisionFlag : flags.B_CollisionFlag;

                while (PinManager.GetInstance().Controller.Read(jogCW) == PinValue.Low && m_UseJogMode && !((flags.motor == "A") ? flags.A_CollisionFlag : flags.B_CollisionFlag))
                {
                    if (aIsSelected)
                    {
                        PinManager.GetInstance().Controller.Write(dirPinA, PinValue.High);
                        SingleStep(stepPinA);
                        Console.WriteLine($"\r\n[{DateTime.Now}] Moving A {flagToWatch}");
                    }
                    else
                    {
                        PinManager.GetInstance().Controller.Write(dirPinB, PinValue.High);
                        SingleStep(stepPinB);
                    }


                    Thread.Sleep(TimeSpan.FromSeconds(0.01));
                }

                while (PinManager.GetInstance().Controller.Read(jogCCW) == PinValue.Low && m_UseJogMode && !((flags.motor == "A") ? flags.A_CollisionFlag : flags.B_CollisionFlag))
                {
                    if (aIsSelected)
                    {
                        PinManager.GetInstance().Controller.Write(dirPinA, PinValue.Low);
                        SingleStep(stepPinA);
                        Console.WriteLine($"\r\n[{DateTime.Now}] Moving A {flagToWatch}");
                    }
                    else
                    {
                        PinManager.GetInstance().Controller.Write(dirPinB, PinValue.Low);
                        SingleStep(stepPinB);
                    }


                    Thread.Sleep(TimeSpan.FromSeconds(0.01));
                }


                //Thread.Sleep(TimeSpan.FromSeconds(0.1));
                if (PinManager.GetInstance().Controller.Read(exitButton) == PinValue.Low)
                {
                    m_UseJogMode = false;
                    flags.stopFlag = true;
                }
            }

            Console.WriteLine($"\r\n[{DateTime.Now}] Exiting Jog Mode. Buttons disabled.");
        }

        private void SingleStep(int pin)
        {
            //Console.WriteLine($"Moove A Cw{pin}");
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

            int PanCounter = 75;
            int RotCounter = 80;
            int TiltCounter = 80;

            PinValue PanState;
            PinValue PanLastState;

            PinValue RotState;
            PinValue RotLastState;

            PinValue TiltState;
            PinValue TiltLastState;

            while (PinManager.GetInstance().Controller.Read(emergencyStop) != PinValue.Low)
            {
                if (PinManager.GetInstance().Controller.Read(resetPan)== PinValue.Low)
                {
                    PanCounter = 75;
                    serialPort.WriteLine($"P{PanCounter}");
                    Thread.Sleep(TimeSpan.FromMilliseconds(45));
                }

                if (PinManager.GetInstance().Controller.Read(resetRot) == PinValue.Low)
                {
                    RotCounter = 80;
                    serialPort.WriteLine($"P{RotCounter}");
                    Thread.Sleep(TimeSpan.FromMilliseconds(45));
                }

                if (PinManager.GetInstance().Controller.Read(resetTilt) == PinValue.Low)
                {
                    TiltCounter = 80;
                    serialPort.WriteLine($"P{TiltCounter}");
                    Thread.Sleep(TimeSpan.FromMilliseconds(45));
                }
                //Read();
                Thread.Sleep(TimeSpan.FromSeconds(0.001));

                PanState = 

                if (state != lastState)
                {
                    if (controller.Read(9) != state)
                    {
                        stepCounter += 2.5f;
                    }
                    else
                    {
                        stepCounter -= 2.5f;
                    }

                    if (Math.Abs(stepCounter % 1) < 0.01)
                    {
                        Console.WriteLine($"Step: {stepCounter}");
                        serialPort.WriteLine($"{stepCounter}");
                        Thread.Sleep(TimeSpan.FromMilliseconds(15));
                    }

                    if (Math.Abs(stepCounter - 360) < 0.0001 || Math.Abs(stepCounter - (-360)) < 0.0001)
                    {
                        stepCounter = 0;
                    }
                }

                Thread.Sleep(TimeSpan.FromSeconds(0.001));
                lastState = state;
            }
            serialPort.Close();
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