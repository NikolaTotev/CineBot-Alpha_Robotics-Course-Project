using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Device.Gpio;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Motor_Control;
using UtilityClasses;

namespace PrimaryClasses
{
    public class RobotServer
    {

        private bool m_StopFlag = false;
        private readonly int m_Port = 4200;
        private readonly IPAddress m_Ip = IPAddress.Parse("192.168.12.119");
        private readonly string m_ServerVersion = "0.01";
        private IPEndPoint m_localEndPoint;
        private Socket m_ControlClient;
        private Thread m_ListenerThread;
        private ConcurrentQueue<Guid> m_ClientHandlersQueue = new ConcurrentQueue<Guid>();
        private GpioController controller;
        public delegate void Messenger(string message);
        public event Messenger ServerNotification;
        
        public RobotServer()
        {
            ServerNotification?.Invoke($"[{DateTime.Now}]: Server version {m_ServerVersion} created. Preparing to start.");
        }

        public void StartServer()
        {
            if (m_ListenerThread != null)
            {
                ServerNotification?.Invoke($"[{DateTime.Now}]: Listener already started!");
                return;
            }
            ServerNotification?.Invoke($"[{DateTime.Now}]: Setting stop flag to {m_StopFlag}");
            m_StopFlag = false;
            m_localEndPoint = new IPEndPoint(m_Ip, m_Port);

            m_ListenerThread = new Thread(AwaitConnection);
            ServerNotification?.Invoke($"[{DateTime.Now}]: Creating listener thread.");


            m_ListenerThread.Start(m_localEndPoint);
            ServerNotification?.Invoke($"[{DateTime.Now}] Status Update:  Server version {m_ServerVersion} started!");
            PinManager.GetInstance().ServerStarted();
        }


        public void AwaitConnection(object state)
        {
            ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Listener Thread: Awaiting connection. Starting listener");
            TcpListener listener;

            if (state is IPEndPoint localEndPoint)
            {
                try
                {
                    listener = new TcpListener(localEndPoint);
                    listener.Start();
                    ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Listener Thread: Awaiting connection on port: {m_Port}");
                }
                catch (Exception)
                {
                    ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Listener Thread: Failed to start listener!");
                    throw;
                }
            }
            else
            {
                ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Listener Thread: Invalid listener state!");
                return;
            }


            ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Listener Thread: Starting listening loop.");
            while (!m_StopFlag)
            {
                try
                {
                    if (listener.Pending())
                    {
                        ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Listener Thread: Pending connection detected.");
                        m_ControlClient = listener.AcceptSocket();
                        ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Listener Thread: Connected to Client!");
                        ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Listener Thread: Switching listener thread name to control thread");
                        listener.Stop();
                        PinManager.GetInstance().ClientConnected();
                        ClientHandler();
                    }
                    else
                    {
                        ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Listener Thread: Awaiting connection...");
                        PinManager.GetInstance().TriplePulse();
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception e)
                {
                    ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Listener Thread: The server encountered a fatal error. Terminating listening. \n {e}");
                    throw;
                }
            }

            if (!m_StopFlag)
            {
                ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Listener Thread: Stop command received. Listener loop exited.");
            }
        }

        public void ClientHandler()
        {
            ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Control Thread: Starting client handler. ");
            NetworkStream stream = new NetworkStream(m_ControlClient);
            StreamWriter writer = new StreamWriter(stream, Encoding.ASCII);
            StreamReader reader = new StreamReader(stream, Encoding.ASCII);
            writer.AutoFlush = true;
            bool initialResponseReceived = false; 

            string initialMessage = $"INITCONF Hello client! I'm server version {m_ServerVersion}.";
            writer.WriteLine(initialMessage);
            writer.Flush();


            ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Control Thread: Starting coms. loop.");
            while (!m_StopFlag)
            {
                try
                {
                    while (IsSocketConnected(m_ControlClient) && !m_StopFlag)
                    {
                        if (m_ControlClient.Available > 0)
                        {
                            ServerNotification?.Invoke(
                                $"\r\n[{DateTime.Now}] Control Thread: Client is available.");
                            string clientInstructions = reader.ReadLine();
                            if (clientInstructions != null && (clientInstructions.StartsWith("INITCONF") || !initialResponseReceived))
                            {
                                ServerNotification?.Invoke($"\n[{DateTime.Now}] Server: Initial command confirmation   {clientInstructions}");
                                initialResponseReceived = true;
                            }
                            else
                            {
                               // ServerNotification?.Invoke(
                                 //   $"\r\n[{DateTime.Now}] Control Thread: Control statement is: \n {clientInstructions} ");
                               // ServerNotification?.Invoke(
                                //    $"\r\n[{DateTime.Now}] Control Thread: Sending instructions to ExecutionHandler.");
                                bool result = ExecutionHandler(clientInstructions);
                                string textResult = result ? "success" : "failure";
                                string responseMessage = $"Execution of {clientInstructions} was a {textResult}.";
                                writer.WriteLine(responseMessage);
                                writer.Flush();
                            }
                            
                        }
                        else
                        {
                            m_ControlClient.Poll(30, SelectMode.SelectRead);
                          //  ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Control Thread: Awaiting instructions... ");
                            Thread.Sleep(TimeSpan.FromSeconds(0.01));
                        }

                    }
                }
                catch (Exception e)
                {
                    ServerNotification?.Invoke($"\r\n[{DateTime.Now}]  Control Thread: The server encountered a fatal error during client handling. Terminating. \n {e}");
                    throw;
                }

                if (!IsSocketConnected(m_ControlClient))
                {
                    ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Control Thread: Client has disconnected. Terminating handling.");
                    ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Control Thread: Returning to listening mode.");
                    m_ControlClient.Disconnect(true);
                    AwaitConnection(m_localEndPoint);
                }
            }
            ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Control Thread: Received stop instruction. Stopping client handling.");
        }

        public bool IsSocketConnected(Socket socketToCheck)
        {
            bool part1 = socketToCheck.Poll(1000, SelectMode.SelectRead);
            bool part2 = (socketToCheck.Available == 0);
            return !part1 || !part2;
        }

        public bool ExecutionHandler(string executionCode)
        {
            if (executionCode == "B")
            {
                MotorManager.GetInstance().MoveStepper(100, 0.1f, "A", true);
            }

            if (executionCode == "OFF")
            {
                controller.Write(24, PinValue.Low);
            }

            return true;
        }

        public void StopServer()
        {
            ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Calling stop command. ");
            m_StopFlag = true;
        }

        public void RestartServer()
        {

        }
    }
}
