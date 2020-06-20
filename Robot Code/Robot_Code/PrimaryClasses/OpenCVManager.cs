using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PrimaryClasses
{
    class OpenCVManager
    {
        private UnixDomainSocketEndPoint m_UnixSocketEndpoint;
        private Socket m_Socket;
        private Socket m_Handler;
        private Thread m_ComsManagementThread;
        private bool m_ListenerStopFlag;
        private bool m_ComsThreadStopFlag;
        private bool m_StopClientHandling;
        //TODO Put this in a config file so it is available to the c++ program.
        private readonly string m_SocketPath = "/var/run/socketTest.sock";
        private readonly string m_ConnectionMessage = "Client V0.1";
        private readonly string m_ConfirmationMsg = "Server V0.1";

        public OpenCVManager()
        {
            
        }


        bool StartOpenCVManager()
        {
            if (m_ComsManagementThread != null)
            {
                Console.WriteLine($"[{DateTime.Now}] OpenCVManager: Coms. thread already started!");
                return false;
            }
            m_ListenerStopFlag = false;

            m_Socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            m_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            m_UnixSocketEndpoint = new UnixDomainSocketEndPoint(m_SocketPath);
            try
            {
                m_Socket.Bind(m_UnixSocketEndpoint);
                Console.WriteLine($"[{DateTime.Now}] OpenCVManager: Socket at address {m_SocketPath} created successfully.");

            }
            catch (SocketException e)
            {
                Console.WriteLine($"[{DateTime.Now}] OpenCVManager: An error with code {e.ErrorCode} occured during socket binding.");
            }

            Console.WriteLine($"[{DateTime.Now}] OpenCVManager: Starting Coms. thread.");
            m_ComsManagementThread = new Thread(BeginListen);
            m_ComsManagementThread.Start();
            return true;
        }

        bool StopOpenCVManager()
        {
            m_ListenerStopFlag = true;

            bool comsThreadStopped = m_ComsManagementThread.Join(5000);
            if (!comsThreadStopped)
            {
                Console.WriteLine($"[{DateTime.Now}] OpenCVManager: Coms. thread failed to join. Aborting.");
                m_ComsManagementThread.Abort();
            }
            try
            {
                m_Socket.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine($"An exception with code {e.HResult} occured during socket disposal.");

            }
            return true;
        }
        public void BeginListen()
        {
            int attemptCounter = 0;
            bool clientConnected = false;
            int listenerStartCount = 0;
            while (!m_ComsThreadStopFlag)
            {
                while (!m_ListenerStopFlag || attemptCounter < 10)
                {
                    Console.WriteLine($"[{DateTime.Now}] OpenCVManager: Starting listener. Listener has been started {listenerStartCount} times.");
                    try
                    {
                        m_Socket.Listen(1);
                        Console.WriteLine($"[{DateTime.Now}] OpenCVManager: Connection Attempt: {attemptCounter} Awaiting connection...");
                        attemptCounter++;
                        m_Socket.Blocking = false;
                        m_Handler = m_Socket.Accept();
                        if (m_Handler.Connected)
                        {
                            Console.WriteLine("Client connected!");
                            clientConnected = true;
                            m_ListenerStopFlag = true;
                            attemptCounter = 0;
                            break;
                        }
                    }
                    catch (SocketException e)
                    {
                        if (e.ErrorCode == 11)
                        {
                            Console.WriteLine($"[{DateTime.Now}] OpenCVManager: Handler awaiting connection...");
                            Thread.Sleep(TimeSpan.FromSeconds(2));
                        }
                        else
                        {
                            Console.WriteLine($"[{DateTime.Now}] OpenCVManager: An exception with code {e.ErrorCode} occured during listening & accept phase.\n");
                        }
                    }
                }

                if (attemptCounter == 10)
                {
                    Console.WriteLine($"[{DateTime.Now}] OpenCVManager: Maximum number of connection attempts exceeded. Terminating listening.");
                    m_ComsThreadStopFlag = true;
                }

                if (m_ListenerStopFlag)
                {
                    Console.WriteLine($"[{DateTime.Now}] OpenCVManager: Stop flag has been raised. Stopping listener.");
                }

                if (clientConnected)
                {
                   int handlingResult =  HandleClient();

                   if (handlingResult == 0)
                   {
                       Console.WriteLine($"[{DateTime.Now}] OpenCVManager: Client handling closed normally from external command.");
                       m_ComsThreadStopFlag = true;
                       m_ListenerStopFlag = true;
                   }

                   if (handlingResult == 1)
                   {
                       Console.WriteLine($"[{DateTime.Now}] OpenCVManager: Connection with client interrupted. Attempting to reconnect.");
                       m_ListenerStopFlag = false;
                       m_ComsThreadStopFlag = false;
                       listenerStartCount++;
                   }
                }
            }
        }

        public int HandleClient()
        {
            try
            {
                byte[] message = Encoding.ASCII.GetBytes(m_ConfirmationMsg);
                byte[] response = new byte[256];
                bool waitingForSetup = true;
                int loopSpeed = 1000;
                while (m_StopClientHandling)
                {
                    if (m_Handler.Available > 0)
                    {
                        int numberOfBytes = m_Handler.Receive(response, 256, SocketFlags.None);
                        if (waitingForSetup)
                        {
                            if (Encoding.ASCII.GetString(response) == m_ConnectionMessage)
                            {
                                waitingForSetup = false;
                                m_Handler.Send(message);
                                loopSpeed = 420;
                            }
                        }
                        else
                        {
                            //ToDo Call for stepperMotor move function.
                        }
                    }
                    Thread.Sleep(TimeSpan.FromMilliseconds(loopSpeed));
                }
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == 32)
                {
                    Console.WriteLine($"[{DateTime.Now}] OpenCVManager: An exception with code {e.ErrorCode} occured." +
                                      " \n This mean the pipe with client has been broken. " +
                                      "\n Restarting Listener");
                }

                Console.WriteLine($"[{DateTime.Now}] OpenCVManager: An exception with code {e.ErrorCode} occured. Terminating Client handling. Returning to listen mode.");
               //ToDo Determine which error codes mean what.
               return 1;
            }
            return 0; 
        }
    }
}
