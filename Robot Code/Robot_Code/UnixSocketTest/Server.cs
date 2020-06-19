using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UnixSocketTest
{
    class Server
    {
        private UnixDomainSocketEndPoint m_UnixSocketEndpoint;
        private Socket m_Socket;
        private Socket m_Handler;
        private readonly string m_SocketPath = "/var/run/socketTest.sock";
        private readonly string m_TestMessage = "Hello from the other side! I will be your server today.";
        public Server()
        {
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
                byte[] response = new byte[256];
                bool waitingForMessage = true;
                while (waitingForMessage)
                {
                    if (m_Handler.Available > 0)
                    {
                        int numberOfBytes = m_Handler.Receive(response, 256, SocketFlags.None);
                     Console.WriteLine($"Number of bytes received = {numberOfBytes}");
                        Console.WriteLine($"Message from the other side is {Encoding.ASCII.GetString(response)}");
                        m_Handler.Send(message);
                        waitingForMessage = false;
                        Console.WriteLine("Closing Server Loop");
                    }
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
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
