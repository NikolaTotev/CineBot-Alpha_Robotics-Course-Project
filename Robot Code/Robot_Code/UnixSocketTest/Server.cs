using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Device.Gpio;
using System.IO.Ports;

namespace UnixSocketTest
{
    public class Server
    {
        private UnixDomainSocketEndPoint m_UnixSocketEndpoint;
        private Socket m_Socket;
        private Socket m_Handler;
        private readonly string m_SocketPath = "/home/pi/socketTest.sock";
        private readonly string m_TestMessage = "Hello from the other side! I will be your server today.";

        private readonly int m_StepMultiplier = 1;
        private readonly int m_GearRatio = 9;
        SerialPort serialPort = new SerialPort("/dev/ttyACM0", 9600); //Set the read/write timeouts    
        int currentServoPos = 75;

        public int ConvertAngleToSteps(int inputAngle)
        {


            int stepsForCarrierRevolution = 200 * m_StepMultiplier * m_GearRatio;
            int partOfCircle = 360 / inputAngle;
            int stepsForInputAngle = stepsForCarrierRevolution / partOfCircle;
            return stepsForInputAngle;
        }

        public Server()
        {
            Console.WriteLine("Starting stepper motor test.");
            serialPort.ReadTimeout = 1500;
            serialPort.WriteTimeout = 1500;
            serialPort.Open();
            GpioController controller = new GpioController();
            int jointAStep = 13;
            int jointADir = 6;
            int jointBStep = 26;
            int jointBDir = 19;


            controller.OpenPin(jointAStep, PinMode.Output);
            controller.OpenPin(jointADir, PinMode.Output);
            controller.OpenPin(jointBStep, PinMode.Output);
            controller.OpenPin(jointBDir, PinMode.Output);

            m_Socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            m_UnixSocketEndpoint = new UnixDomainSocketEndPoint(m_SocketPath);

            try
            {
                m_Socket.Bind(m_UnixSocketEndpoint);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured during socket binding.");
                throw;
            }
            serialPort.WriteLine("P75");

            int counter = 0;
            while (counter < 100)
            {
                try
                {
                    bool searchingForClient = true;
                    m_Socket.Listen(1);
                    Console.WriteLine($"{counter} Awaiting connection...");
                    counter++;
                    m_Socket.Blocking = false;
                    m_Handler = m_Socket.Accept();

                    if (m_Handler.Connected)
                    {
                        Console.WriteLine("Client connected!");
                        break;
                    }
                    if (m_Handler == null)
                    {
                        Console.WriteLine($"Handler not connected.");
                    }
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode == 11)
                    {
                        Console.WriteLine("Handler awaiting connection...\n");
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                    }
                    else
                    {
                        Console.WriteLine($"An exception with code {e.ErrorCode} occured during listening & accept phase.\n");
                    }
                }
            }

            try
            {
                m_Socket.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine($"An exception with code {e.HResult} occured during socket disposal.");

            }


            try
            {
                byte[] message = Encoding.ASCII.GetBytes(m_TestMessage);
                byte[] response = new byte[1024];
                bool waitingForMessage = true;
                while (waitingForMessage)
                {
                    if (m_Handler.Available > 0)
                    {
                        int numberOfBytes = m_Handler.Receive(response, 18, SocketFlags.None);
                        try
                        {
                            string[] splitVariables = Encoding.ASCII.GetString(response).Split('#');
                            string[] reading = splitVariables[1].Split('^');
                            float panMove = float.Parse(reading[0]);
                            float tiltMove = float.Parse(reading[1]);

                            if (panMove > 0)
                            {
                                controller.Write(jointADir, PinValue.Low);
                                currentServoPos += (int) panMove;
                            }

                            else
                            {
                                controller.Write(jointADir, PinValue.High);
                                currentServoPos -= (int)panMove;
                            }


                            serialPort.WriteLine($"P{Math.Abs((int)panMove)}");
                            Console.WriteLine($"Servo pos {(int)panMove},  {panMove}");
                            Thread.Sleep(TimeSpan.FromMilliseconds(100));


                            //for (int i = 0; i < ConvertAngleToSteps((int)Math.Abs(panMove)); i++)
                            //{

                            //    Console.WriteLine($"Write to A {i}");
                            //    controller.Write(jointAStep, PinValue.High);
                            //    Thread.Sleep(TimeSpan.FromSeconds(0.01));
                            //    controller.Write(jointAStep, PinValue.Low);
                            //    Thread.Sleep(TimeSpan.FromSeconds(0.01));
                            //}
                            //Console.WriteLine($"Pan value {panMove}, Tilt value {tiltMove}");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Parsing fail {Encoding.ASCII.GetString(response)}");
                            Console.WriteLine($"Split {Encoding.ASCII.GetString(response).Split('#')[1]}");
                        }


                        //m_Handler.Send(Encoding.ASCII.GetBytes($"SERVER GOT - {Encoding.ASCII.GetString(response)} SERVER"));
                        waitingForMessage = true;
                    }
                    // Thread.Sleep(TimeSpan.FromMilliseconds(420));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The socket may have closed.");
                m_Socket.Dispose();
                m_Handler.Dispose();
            }
        }
    }
}
