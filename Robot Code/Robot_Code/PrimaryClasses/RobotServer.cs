using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Device.Gpio;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UtilityClasses;

namespace PrimaryClasses
{
    public class RobotServer
    {

        private bool m_StopFlag = false;
        private readonly int m_Port = 4200;
        private readonly IPAddress m_Ip = IPAddress.Parse("192.168.12.133");
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

        /// <summary>
        /// Starts the server. Flag & listener thread setup and start.
        /// </summary>
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

            try
            {
                m_ListenerThread.Start(m_localEndPoint);
                ServerNotification?.Invoke($"[{DateTime.Now}] Status Update:  Server version {m_ServerVersion} started!");
                NotificationManager.ServerStarted();
                SerialComsManager.GetInstance();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured during thread start. Aborting operation\n");
                NotificationManager.Exception();
            }

        }


        /// <summary>
        /// Runs on listener thread. Accepts state which is the TCP endpoint the listener listens on.
        /// If no connection detected sleeps for 1 second.
        /// </summary>
        /// <param name="state"></param>
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
                catch (Exception e)
                {
                    ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Listener Thread: Failed to start listener! Exception {e.HResult} occured. Aborting operation\n");
                    NotificationManager.Exception();
                    return;
                }
            }
            else
            {
                ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Listener Thread: Invalid listener state!");
                return;
            }

            ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Listener Thread: Starting listening loop.");
            int counter = 0;

            while (!m_StopFlag)
            {
               try
                {
                    if (listener.Pending())
                    {
                        ServerNotification?.Invoke(
                            $"\r\n[{DateTime.Now}] Listener Thread: Pending connection detected.");
                        m_ControlClient = listener.AcceptSocket();
                        ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Listener Thread: Connected to Client!");
                        ServerNotification?.Invoke(
                            $"\r\n[{DateTime.Now}] Listener Thread: Switching listener thread name to control thread");
                        listener.Stop();
                        NotificationManager.ClientConnected();

                        int handlerResult = ClientHandler();
                        if (handlerResult == 0)
                        {
                            m_StopFlag = true;
                        }

                        if (handlerResult == 1)
                        {
                            m_StopFlag = false;
                        }
                    }
                    else
                    {
                        ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Listener Thread: Awaiting connection...");
                        NotificationManager.DoublePulse();
                        Thread.Sleep(1000);
                    }

                    counter = 0;
                }
                catch (SocketException e)
                {
                    if (counter < 3)
                    {
                        ServerNotification?.Invoke(
                            $"\r\n[{DateTime.Now}] Listener Thread: The server encountered a socket with error code {e.ErrorCode}. Attempting to restart. \n {e}");
                        counter++;
                        NotificationManager.StartupError();
                    }
                    else
                    {
                        ServerNotification?.Invoke(
                            $"\r\n[{DateTime.Now}] Server attempted to recover {counter} times from socket error. Terminating Operation.\n");
                        NotificationManager.Exception();
                        m_StopFlag = true;
                    }

                }
                catch (Exception e)
                {
                    if (counter < 3)
                    {
                        ServerNotification?.Invoke(
                            $"\r\n[{DateTime.Now}] Listener Thread: The server encountered a fatal error. Attempting to restart. Attempt {counter} \n {e}");
                        counter++;
                        NotificationManager.StartupError();
                    }
                    else
                    {
                        ServerNotification?.Invoke(
                            $"\r\n[{DateTime.Now}] Server attempted to restart {counter} times. Aborting restart. \n");
                        NotificationManager.Exception();
                        m_StopFlag = true;
                    }
                }
            }

            if (!m_StopFlag)
            {
                ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Listener Thread: Stop command received. Listener loop exited.");
            }
        }

        /// <summary>
        /// Handles client once connected. Does not run on own thread, that way if client disconnects server can go back to listening.
        /// </summary>
        /// <returns></returns>
        public int ClientHandler()
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
                    try
                    {
                        m_ControlClient.Disconnect(true);
                        ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Control Thread: Client has disconnected. Terminating handling.");
                        ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Control Thread: Returning to listening mode.");
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine($"A socket exception with code {e.ErrorCode} occured.");
                        throw;
                    }
                    return 1;
                }
            }
            ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Control Thread: Received stop instruction. Stopping client handling.");
            return 0;
        }


        /// <summary>
        /// Checks if socket that is pass is still connected.
        /// </summary>
        /// <param name="socketToCheck"></param>
        /// <returns></returns>
        public bool IsSocketConnected(Socket socketToCheck)
        {
            bool part1 = socketToCheck.Poll(1000, SelectMode.SelectRead);
            bool part2 = (socketToCheck.Available == 0);
            return !part1 || !part2;
        }

        /// <summary>
        /// Parses input from client. Starts motion manager for each mode.
        /// </summary>
        /// <param name="executionCode"></param>
        /// <returns></returns>
        public bool ExecutionHandler(string executionCode)
        {
            MotionManager currentManager;
            switch (executionCode)
            {
                case "NOOP":
                    break;

                case "GJ":
                    currentManager = new MotionManager(MotionModes.GimbalJog);
                    currentManager.ExecuteCommand();
                    break;

                case "SJ":
                    currentManager = new MotionManager(MotionModes.StepperJog);
                    currentManager.ExecuteCommand();
                    break;

                case "PF":
                    currentManager = new MotionManager(MotionModes.PathFollow);
                    currentManager.ExecuteCommand();
                    break;

                case "HM":
                    currentManager = new MotionManager(MotionModes.StepperHome);
                    currentManager.ExecuteCommand();
                    break;
                case "GHM":
                    currentManager = new MotionManager(MotionModes.GimbalHome);
                    currentManager.ExecuteCommand();
                    break;
                case "REC":
                    currentManager = new MotionManager(MotionModes.RecordPath);
                    currentManager.ExecuteCommand();
                    break;
                case "RPL":
                    currentManager = new MotionManager(MotionModes.Replay);
                    currentManager.ExecuteCommand();
                    break;
                default:
                    ServerNotification?.Invoke($"\r\n[{DateTime.Now}] <ERROR> Wrong command passed to server. Unable to execute.");
                    break;
            }
            ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Execution of command {executionCode} completed.");
            return true;
        }

        /// <summary>
        /// Handles server stop sequence.
        /// </summary>
        public void StopServer()
        {
            ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Calling stop command. ");
            m_StopFlag = true;
            ServerNotification?.Invoke($"\r\n[{DateTime.Now}] Moving to home position.");
            MotionManager steppersHome = new MotionManager(MotionModes.StepperHome);
            MotionManager gimbalHome = new MotionManager(MotionModes.GimbalHome);
            steppersHome.ExecuteCommand();
            gimbalHome.ExecuteCommand();
            NotificationManager.DoublePulse();
        }

        public void RestartServer()
        {

        }
    }
}
