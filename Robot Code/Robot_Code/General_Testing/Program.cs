using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Device.Pwm;
using System.Diagnostics.Tracing;
using System.IO.Ports;
using System.Threading;
using Iot.Device.ServoMotor;

namespace General_Testing
{
    class Program
    {
        //static SerialPort serialPort;
        //static int counter = 0;

        static void Main(string[] args)
        {
            
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "Buttons":
                        TestButtons(int.Parse(args[1]), int.Parse(args[2]));
                        break;
                    case "Rotary":
                        TestEncoders();
                        break;

                    case "StpPT":
                       // TestStepper(true);
                        break;

                    case "Stp":
                        TestStepper(false);
                        break;

                    case "Serv":
                      //  TestServos();
                        break;
                }
            }

            //float stepCounter = 0;
            //GpioController controller = new GpioController();
            //Console.WriteLine("STARTING GENERAL TESTING");

            //controller.OpenPin(18, PinMode.Output);

            //while (true)
            //{
            //    controller.Write(18, PinValue.High);
            //    Thread.Sleep(TimeSpan.FromSeconds(1));
            //    controller.Write(18, PinValue.Low);
            //    Thread.Sleep(TimeSpan.FromSeconds(1));
            //}


            //Thread.Sleep(TimeSpan.FromSeconds(20));

            //for (int i = 0; i < 400; i++)
            //{
            //    Console.WriteLine($"Write {i}");
            //    controller.Write(18, PinValue.High);
            //    Thread.Sleep(TimeSpan.FromSeconds(0.01));
            //    controller.Write(18, PinValue.Low);
            //    Thread.Sleep(TimeSpan.FromSeconds(0.01));
            //}

            //controller.Write(24, PinValue.Low);
            //for (int i = 0; i < 400; i++)
            //{
            //    Console.WriteLine($"Write {i}");
            //    controller.Write(25, PinValue.High);
            //    Thread.Sleep(TimeSpan.FromSeconds(0.01));
            //    controller.Write(25, PinValue.Low);
            //    Thread.Sleep(TimeSpan.FromSeconds(0.01));
            //}

        }

        static void TestButtons(int buttonNumber, int mode)
        {
            
            GpioController controller = new GpioController(PinNumberingScheme.Logical);
            Console.WriteLine("Starting button test");

            if (mode == 1)
            {
                controller.OpenPin(buttonNumber, PinMode.InputPullUp);
            }
            
            if (mode == 2)
            {
                controller.OpenPin(buttonNumber, PinMode.InputPullDown);
            }

            while (true)
            {
                if (controller.Read(buttonNumber) == PinValue.Low)
                {
                    Console.WriteLine("Derp");
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }

            Console.WriteLine("Button test complete");
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

            for (int i = 0; i < 4; i++)
            {
                controller.Write(23, PinValue.High);
                controller.Write(24, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(2));
                controller.Write(23, PinValue.Low);
                controller.Write(24, PinValue.Low);
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
            lastStateB = controller.Read(rotSIA);
            lastStateC = controller.Read(tiltSIA);



            while (true)
            {

                Thread.Sleep(TimeSpan.FromSeconds(0.001));

                stateA = controller.Read(panSIB);
                stateB = controller.Read(rotSIB);
                stateC = controller.Read(tiltSIB);

                if (stateA != lastStateA)
                {
                    if (controller.Read(panSIA) != stateA)
                    {
                        counterA += 2.5f;
                    }
                    else
                    {
                        counterA -= 2.5f;
                    }

                    if (Math.Abs(counterA % 1) < 0.01)
                    {
                        Console.WriteLine($"Counter A: {counterA}");
                    }
                }

                if (stateB != lastStateB)
                {
                    if (controller.Read(rotSIA) != stateB)
                    {
                        counterB += 2.5f;
                    }
                    else
                    {
                        counterB -= 2.5f;
                    }

                    if (Math.Abs(counterB % 1) < 0.01)
                    {
                        Console.WriteLine($"Counter B: {counterB}");
                    }
                }

                if (stateC != lastStateC)
                {
                    if (controller.Read(tiltSIA) != stateC)
                    {
                        counterC += 2.5f;
                    }
                    else
                    {
                        counterC -= 2.5f;
                    }

                    if (Math.Abs(counterC % 1) < 0.01)
                    {
                        Console.WriteLine($"Counter C: {counterC}");
                    }
                }

                lastStateA = stateA;
                lastStateB = stateB;
                lastStateC = stateC;
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

        static void TestServos()
        {
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
