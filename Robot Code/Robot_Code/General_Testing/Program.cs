﻿using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Device.Pwm;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Dynamic;
using System.IO;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;
using Iot.Device.ServoMotor;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Native;
using Unosquare.RaspberryIO.Camera;


namespace General_Testing
{
    class Program
    {
        //static SerialPort serialPort;
        //static int counter = 0;

        static void Main(string[] args)
        {

            // Define a regular expression for repeated words.
            Regex rx = new Regex(@"([\+-]?\d*\.?\d{4}?){1}",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Define a test string.
            string text = "25.2312 -23.2324";

            // Find matches.
            MatchCollection matches = rx.Matches(text);

            // Report the number of matches found.
            Console.WriteLine("{0} matches found in:\n   {1}",
                matches.Count,
                text);

            // Report on each match.
            Console.WriteLine(matches[1]);
            //foreach (Match match in matches)
            //{
            //    GroupCollection groups = match.Groups;
            //    Console.WriteLine("'{0}' repeated at positions {1} and {2}",
            //        groups["word"].Value,
            //        groups[0].Index,
            //        groups[1].Index);
            //}


            return;
            //string[] args;
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "Buttons":
                        TestButtons(int.Parse(args[1]), int.Parse(args[2]));
                        break;

                    case "Camera":
                        ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "python", Arguments = $"/home/pi/Desktop/CameraTest.py /home/pi/Desktop/Photos/image-{0}-{DateTime.Now.TimeOfDay}-capture.jpg"};
                        Process proc = new Process() { StartInfo = startInfo, };
                        proc.Start();
                        break;

                    case "Rotary":
                        TestEncoders();
                        break;

                    case "MStp":
                        Thread stepperA = new Thread(MultiThreadStepperTest);
                        Thread stepperB = new Thread(MultiThreadStepperTest);

                        stepperA.Start("A");
                        stepperB.Start("B");
                        break;

                    case "Stp":
                        TestStepper(false);
                        break;

                    case "Serv":
                        TestServos();
                        break;
                    case "Poly":
                        int counter = 0;
                        while (counter < 1)
                        {
                            if (args.Length == 6)
                            {
                                bool useB;
                                useB = args[5] != "0";
                                PolyMotorControl(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]), float.Parse(args[4]));
                            }
                            else
                            {
                               // PolyMotorControl();
                            }

                            counter++;
                        }
                       
                        break;
                }
            }
        }

        static void TestButtons(int buttonNumber, int mode)
        {

            GpioController controller = new GpioController();
            Console.WriteLine("Starting button test");

            controller.OpenPin(buttonNumber, PinMode.InputPullUp);

            while (true)
            {
                if (controller.Read(buttonNumber) == PinValue.Low)
                {
                    Console.WriteLine("Derp");
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }
        }

        static void TestEncoders()
        {
            Console.WriteLine("Starting encoder test");

            int panSIA = 17;
            int panSIB = 27;

            int rotSIA = 10;
            int rotSIB = 9;

            int tiltSIA = 16;
            int tiltSIB = 20;



            GpioController controller = new GpioController();

            controller.OpenPin(panSIA, PinMode.InputPullUp);
            controller.OpenPin(panSIB, PinMode.InputPullUp);

            controller.OpenPin(rotSIA, PinMode.InputPullUp);
            controller.OpenPin(rotSIB, PinMode.InputPullUp);

            controller.OpenPin(tiltSIA, PinMode.InputPullUp);
            controller.OpenPin(tiltSIB, PinMode.InputPullUp);


            controller.OpenPin(23, PinMode.Output);
            controller.OpenPin(24, PinMode.Output);
            controller.OpenPin(5, PinMode.Output);

            for (int i = 0; i < 2; i++)
            {
                controller.Write(23, PinValue.High);
                controller.Write(24, PinValue.High);
                controller.Write(5, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(1));
                controller.Write(23, PinValue.Low);
                controller.Write(24, PinValue.Low);
                controller.Write(5, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            float counterA = 0;
            float counterB = 0;
            float counterC = 0;

            PinValue stateA;
            PinValue lastStateA;

            PinValue stateB;
            PinValue lastStateB;

            PinValue stateC;
            PinValue lastStateC;

            lastStateA = controller.Read(panSIA);
            //  lastStateB = controller.Read(rotSIA);
            // lastStateC = controller.Read(tiltSIA);

            while (true)
            {
                stateA = controller.Read(panSIA);
                stateB = controller.Read(panSIB);

                if (stateA != lastStateA)
                {

                    if (stateB == stateA)
                    {
                        Console.WriteLine("Moving CW");
                    }
                    else
                    {
                        Console.WriteLine("Moving CCW");
                    }


                    bool stateABool;
                    bool stateBBool;
                    if (stateA == PinValue.Low)
                    {
                        Console.WriteLine("A is low");
                        stateABool = false;
                    }
                    else
                    {
                        Console.WriteLine("A is high");
                        stateABool = true;
                    }

                    if (stateB == PinValue.Low)
                    {
                        Console.WriteLine("B is low");
                        stateBBool = false;
                    }
                    else
                    {
                        Console.WriteLine("B is high");
                        stateBBool = true;
                    }
                    //bool sequence = (stateABool ^ stateBBool) | stateBBool;
                    //Console.WriteLine($"The direction is {sequence}");
                }
                lastStateA = stateA;
                Thread.Sleep(TimeSpan.FromMilliseconds(1));
            }

            //while (true)
            //{

            //    Thread.Sleep(TimeSpan.FromSeconds(0.001));

            //    stateA = controller.Read(panSIB);
            //    stateB = controller.Read(rotSIB);
            //    stateC = controller.Read(tiltSIB);

            //    if (stateA != lastStateA)
            //    {
            //        if (controller.Read(panSIA) != stateA)
            //        {
            //            counterA += 2.5f;
            //        }
            //        else
            //        {
            //            counterA -= 2.5f;
            //        }

            //        if (Math.Abs(counterA % 1) < 0.01)
            //        {
            //            Console.WriteLine($"Counter A: {counterA}");
            //        }
            //    }

            //    if (stateB != lastStateB)
            //    {
            //        if (controller.Read(rotSIA) != stateB)
            //        {
            //            counterB += 2.5f;
            //        }
            //        else
            //        {
            //            counterB -= 2.5f;
            //        }

            //        if (Math.Abs(counterB % 1) < 0.01)
            //        {
            //            Console.WriteLine($"Counter B: {counterB}");
            //        }
            //    }

            //    if (stateC != lastStateC)
            //    {
            //        if (controller.Read(tiltSIA) != stateC)
            //        {
            //            counterC += 2.5f;
            //        }
            //        else
            //        {
            //            counterC -= 2.5f;
            //        }

            //        if (Math.Abs(counterC % 1) < 0.01)
            //        {
            //            Console.WriteLine($"Counter C: {counterC}");
            //        }
            //    }

            //    lastStateA = stateA;
            //    lastStateB = stateB;
            //    lastStateC = stateC;
            //}
        }

        private static float A0;
        private static float A1;
        private static float A2;
        private static float A3;
        private static float A4;
        private static float A5;
        private static float startAngle = 0;
        private static float m_FinalAngle = 45;
        private static float startVelocity = 0;
        private static float finalVelocity = 0;
        private static float startAcceleration = 0;
        private static float finalAcceleration = 0;

        private static readonly int m_StepsPerRevolution = 200;
        private static readonly int m_StepMultiplier = 1;
        private static readonly int m_GearRatio = 7;
        private static readonly int m_MinimumAngle = 2;
        private static readonly float m_MinimumSpeed = 0.01f;
        private static readonly float m_SpeedSensitivity = 1;
        private static readonly float m_CollisionDetectionFrequency = 0.01f;
        private int m_MovementSensitivity = 1;
        private static float m_OverallTime = 5;
        private static float m_MaxSpeed=0.003f;
        private static float m_MinSpeed = 0.008f;
        static void CalcCoefs(float t)
        {
            A0 = startAngle;
            A1 = startVelocity;
            A2 = startAcceleration / 2f;
            A3 = -1 * (20 * startAngle - 20 * m_FinalAngle + 12 * t * startVelocity + 8 * t * finalVelocity + 3 * startAcceleration * t * t - finalAcceleration * t * t) / (2 * t * t * t);
            A4 = (30f * startAngle - 30f * m_FinalAngle + 16f * t * startVelocity + 14f * t * finalVelocity + 3f * startAcceleration * t * t - 2f * finalAcceleration * t * t) / (2f * t * t * t * t);
            A5 = -1f * (12f * startAngle - 12f * m_FinalAngle + 6f * t * startVelocity + 6f * t * finalVelocity + startAcceleration * t * t - finalAcceleration * t * t) / (2f * t * t * t * t * t);
            if (float.IsNaN(A0))//make sure it is not undefined
            {
                A0 = 0;
            }
            if (float.IsNaN(A1))//make sure it is not undefined
            {
                A1 = 0;
            }
            if (float.IsNaN(A2))//make sure it is not undefined
            {
                A2 = 0;
            }
            if (float.IsNaN(A3))//make sure it is not undefined
            {
                A3 = 0;
            }
            if (float.IsNaN(A4))//make sure it is not undefined
            {
                A4 = 0;
            }
            if (float.IsNaN(A5))//make sure it is not undefined
            {
                A5 = 0;
            }
        }

        static float GetSpeed(float t)
        {
            return A5 * 5 * (float)Math.Pow(t, 4) + A4 * 4 * (float)Math.Pow(t, 3) + A3 * 3 * (float)Math.Pow(t, 2) + A2 * 2 * (float)Math.Pow(t, 1) + A1;

        }
        public static int ConvertAngleToSteps(int inputAngle )
        {
            int stepsForCarrierRevolution = m_StepsPerRevolution * m_StepMultiplier * m_GearRatio;
            int partOfCircle = 360 / inputAngle;
            int stepsForInputAngle = stepsForCarrierRevolution / partOfCircle;
            return stepsForInputAngle;
        }

        private static float mapCoef;
        static float mapValue(float input)
        {
            return mapCoef * input + m_MinSpeed;
        }
//        static void PolyMotorControl(float maxSpeed = 0.003f, float minSpeed = 0.008f, float finalAngle=45f, float overAllTime = 5f, bool useB = false)
        static void PolyMotorControl(float maxSpeed, float minSpeed, float finalAngle, float overAllTime, bool useB = false)

        {
            m_MaxSpeed = maxSpeed;
            m_MinSpeed = minSpeed;
            m_FinalAngle = finalAngle;
            m_OverallTime = overAllTime;
            GpioController controller = new GpioController();
            int jointAStep = 13;
            int jointADir = 6;
            int jointBStep = 26;
            int jointBDir = 19;


            controller.OpenPin(jointAStep, PinMode.Output);
            controller.OpenPin(jointADir, PinMode.Output);
            controller.OpenPin(jointBStep, PinMode.Output);
            controller.OpenPin(jointBDir, PinMode.Output);

            CalcCoefs(m_OverallTime);
            float numberOfSteps = ConvertAngleToSteps((int)finalAngle);
            float eti = m_OverallTime / numberOfSteps;
            float timeDivisionSize = 0.0001f;
            float numberOfTimeDivisions = m_OverallTime / timeDivisionSize;
            float funcMaxSpeed = GetSpeed((numberOfTimeDivisions / 2) * timeDivisionSize);
            mapCoef = (maxSpeed-minSpeed) / funcMaxSpeed;

            int counter = 0;
            controller.Write(jointADir, PinValue.Low);

            for (float i = 0; i <= m_OverallTime; i += (eti))
            {
                float speed = mapValue(GetSpeed(i));
                controller.Write(jointAStep, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(speed));
                controller.Write(jointAStep, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(speed));
                // Console.WriteLine(mapValue(GetSpeed(i)));
                counter++;
            }
            Console.WriteLine($"Counter value: {counter}, Needed Step Count {numberOfSteps}, etiValue {eti}");

            controller.Write(jointADir, PinValue.High);

            for (float i = 0; i <= m_OverallTime; i += (eti))
            {
                controller.Write(jointAStep, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(mapValue(GetSpeed(i))));
                controller.Write(jointAStep, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(mapValue(GetSpeed(i))));
                // Console.WriteLine(mapValue(GetSpeed(i)));
                counter++;
            }

            Console.WriteLine($"Counter value: {counter}, Needed Step Count {numberOfSteps}, etiValue {eti}");

            if (useB)
            {
                m_MinSpeed = 0.0098f;
                mapCoef = (maxSpeed - minSpeed) / funcMaxSpeed;

                controller.Write(jointBDir, PinValue.Low);


                for (float i = 0; i <= m_OverallTime; i += (eti))
                {
                    float speed = mapValue(GetSpeed(i));
                    controller.Write(jointBStep, PinValue.High);
                    Thread.Sleep(TimeSpan.FromSeconds(speed));
                    controller.Write(jointBStep, PinValue.Low);
                    Thread.Sleep(TimeSpan.FromSeconds(speed));
                    // Console.WriteLine(mapValue(GetSpeed(i)));
                    counter++;
                }
                Console.WriteLine($"Counter value: {counter}, Needed Step Count {numberOfSteps}, etiValue {eti}");

                controller.Write(jointBDir, PinValue.High);

                for (float i = 0; i <= m_OverallTime; i += (eti))
                {
                    controller.Write(jointBStep, PinValue.High);
                    Thread.Sleep(TimeSpan.FromSeconds(mapValue(GetSpeed(i))));
                    controller.Write(jointBStep, PinValue.Low);
                    Thread.Sleep(TimeSpan.FromSeconds(mapValue(GetSpeed(i))));
                    // Console.WriteLine(mapValue(GetSpeed(i)));
                    counter++;
                }

                Console.WriteLine($"Counter value: {counter}, Needed Step Count {numberOfSteps}, etiValue {eti}");
            }
        
        }



        static void TestStepper(bool pinTest)
        {
            Console.WriteLine("Starting stepper motor test.");

            GpioController controller = new GpioController();
            int jointAStep = 13;
            int jointADir = 6;
            int jointBStep = 26;
            int jointBDir = 19;


            controller.OpenPin(jointAStep, PinMode.Output);
            controller.OpenPin(jointADir, PinMode.Output);
            controller.OpenPin(jointBStep, PinMode.Output);
            controller.OpenPin(jointBDir, PinMode.Output);


            Console.WriteLine("ENTERING PIN TEST MODE");
            Console.WriteLine("Testing motor A");

            controller.Write(jointADir, PinValue.Low);
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine($"Write to A {i}");
                controller.Write(jointAStep, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(0.01));
                controller.Write(jointAStep, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(0.01));
            }

            controller.Write(jointADir, PinValue.High);
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine($"Write to A {i}");
                controller.Write(jointAStep, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(0.01));
                controller.Write(jointAStep, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(0.01));
            }

            Thread.Sleep(TimeSpan.FromSeconds(2));


            Console.WriteLine("Testing motor B");

            controller.Write(jointBDir, PinValue.Low);
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine($"Write to B {i}");
                controller.Write(jointBStep, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(0.01));
                controller.Write(jointBStep, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(0.01));
            }

            controller.Write(jointBDir, PinValue.High);
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine($"Write to B {i}");
                controller.Write(jointBStep, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(0.01));
                controller.Write(jointBStep, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(0.01));
            }
        }



        static void MultiThreadStepperTest(object threadInput)
        {

            if (threadInput is string motor)
            {
                Console.WriteLine("Starting stepper motor test.");

                GpioController controller = new GpioController();

                if (motor == "A")
                {
                    int jointAStep = 13;
                    int jointADir = 6;
                    controller.OpenPin(jointAStep, PinMode.Output);
                    controller.OpenPin(jointADir, PinMode.Output);

                    Console.WriteLine("ENTERING PIN TEST MODE");
                    Console.WriteLine("Testing motor A");

                    controller.Write(jointADir, PinValue.Low);
                    for (int i = 0; i < 100; i++)
                    {
                        Console.WriteLine($"Write to A {i}");
                        controller.Write(jointAStep, PinValue.High);
                        Thread.Sleep(TimeSpan.FromSeconds(0.01));
                        controller.Write(jointAStep, PinValue.Low);
                        Thread.Sleep(TimeSpan.FromSeconds(0.01));
                    }

                    controller.Write(jointADir, PinValue.High);
                    for (int i = 0; i < 100; i++)
                    {
                        Console.WriteLine($"Write to A {i}");
                        controller.Write(jointAStep, PinValue.High);
                        Thread.Sleep(TimeSpan.FromSeconds(0.01));
                        controller.Write(jointAStep, PinValue.Low);
                        Thread.Sleep(TimeSpan.FromSeconds(0.01));
                    }

                    Console.WriteLine("Thread for stepper A is complete.");
                }

                if (motor == "B")
                {
                    int jointBStep = 26;
                    int jointBDir = 19;

                    controller.OpenPin(jointBStep, PinMode.Output);
                    controller.OpenPin(jointBDir, PinMode.Output);

                    Console.WriteLine("Testing motor B");

                    controller.Write(jointBDir, PinValue.Low);
                    for (int i = 0; i < 100; i++)
                    {
                        Console.WriteLine($"Write to B {i}");
                        controller.Write(jointBStep, PinValue.High);
                        Thread.Sleep(TimeSpan.FromSeconds(0.01));
                        controller.Write(jointBStep, PinValue.Low);
                        Thread.Sleep(TimeSpan.FromSeconds(0.01));
                    }

                    controller.Write(jointBDir, PinValue.High);
                    for (int i = 0; i < 100; i++)
                    {
                        Console.WriteLine($"Write to B {i}");
                        controller.Write(jointBStep, PinValue.High);
                        Thread.Sleep(TimeSpan.FromSeconds(0.01));
                        controller.Write(jointBStep, PinValue.Low);
                        Thread.Sleep(TimeSpan.FromSeconds(0.01));
                    }

                    Console.WriteLine("Thread for stepper B is complete.");

                }
            }
        }


        static void TestServos()
        {
            Console.WriteLine($"Starting servo test.");
            GpioController controller = new GpioController();

            controller.OpenPin(23, PinMode.Output);
            controller.OpenPin(24, PinMode.Output);
            controller.OpenPin(5, PinMode.Output);

            for (int i = 0; i < 2; i++)
            {
                controller.Write(23, PinValue.High);
                controller.Write(24, PinValue.High);
                controller.Write(5, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(1));
                controller.Write(23, PinValue.Low);
                controller.Write(24, PinValue.Low);
                controller.Write(5, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            SerialPort serialPort = new SerialPort("/dev/ttyACM0", 9600); //Set the read/write timeouts    
            serialPort.ReadTimeout = 1500;
            serialPort.WriteTimeout = 1500;
            serialPort.Open();

            Console.WriteLine(serialPort.ReadLine());
            serialPort.WriteLine("CLEAR");

            for (int i = 0; i < 2; i++)
            {
                serialPort.WriteLine("P25");
                Thread.Sleep(TimeSpan.FromSeconds(2));
                serialPort.WriteLine("P170");
                Thread.Sleep(TimeSpan.FromSeconds(2));
                serialPort.WriteLine("P75");

                Thread.Sleep(TimeSpan.FromSeconds(4));

                serialPort.WriteLine("R25");
                Thread.Sleep(TimeSpan.FromSeconds(2));
                serialPort.WriteLine("R180");
                Thread.Sleep(TimeSpan.FromSeconds(2));
                serialPort.WriteLine("R75");

                Thread.Sleep(TimeSpan.FromSeconds(4));

                serialPort.WriteLine("T25");
                Thread.Sleep(TimeSpan.FromSeconds(2));
                serialPort.WriteLine("T170");
                Thread.Sleep(TimeSpan.FromSeconds(2));
                serialPort.WriteLine("T75");

                Thread.Sleep(TimeSpan.FromSeconds(4));
            }
        }
    }
}




//float counter = 0;
//PinValue state;
//PinValue lastState;

//lastState = controller.Read(10);
//while (true)
//{
//    state = controller.Read(10);

//    if (state != lastState)
//    {
//        if (controller.Read(11) != state)
//        {
//            counter+=0.5f;
//        }
//        else
//        {
//            counter-=0.5f;
//        }

//        Console.WriteLine($"Counter: {counter}, State: {state}, Last State: {lastState}");

//    }

//    Thread.Sleep(TimeSpan.FromSeconds(0.001));
//    lastState = state;

//    if (controller.Read(9) == PinValue.Low)
//    {
//        Console.WriteLine("I'VE BEEN CLICKED");
//    }


//}



//    PinValue state;
//    PinValue lastState;
//    lastState = controller.Read(11);

//    PinValue emergencyStop = PinValue.High;

//    Console.Write("Port no: ");
//    Console.Write("baudrate: ");

//    serialPort = new SerialPort("/dev/ttyACM0", 9600); //Set the read/write timeouts    
//    serialPort.ReadTimeout = 1500;
//    serialPort.WriteTimeout = 1500;
//    serialPort.Open();
//    serialPort.WriteLine("80");
//    while (emergencyStop != PinValue.Low)
//    {
//    emergencyStop = controller.Read(26);
//    if (controller.Read(10) == PinValue.Low)
//    {
//        stepCounter = 80;
//        serialPort.WriteLine("80");
//        Thread.Sleep(TimeSpan.FromMilliseconds(15));
//    }
//    Read();
//    Thread.Sleep(TimeSpan.FromSeconds(0.001));

//    state = controller.Read(11);

//    if (state != lastState)
//    {
//        if (controller.Read(9) != state)
//        {
//            stepCounter += 2.5f;
//        }
//        else
//        {
//            stepCounter -= 2.5f;
//        }

//        if (Math.Abs(stepCounter % 1) < 0.01)
//        {
//            Console.WriteLine($"Step: {stepCounter}");
//            serialPort.WriteLine($"{stepCounter}");
//            Thread.Sleep(TimeSpan.FromMilliseconds(15));
//        }

//        if (Math.Abs(stepCounter - 360) < 0.0001 || Math.Abs(stepCounter - (-360)) < 0.0001)
//        {
//            stepCounter = 0;
//        }
//    }

//    Thread.Sleep(TimeSpan.FromSeconds(0.001));
//    lastState = state;
//    }
//    serialPort.Close();
//}
//public static void Read()
//{
//try
//{
//    if (serialPort.BytesToRead > 0)
//    {
//        string message = serialPort.ReadLine();
//        Console.WriteLine(message);
//    }

//}
//catch (TimeoutException)
//{
//}
//}
