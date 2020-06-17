using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UnixSocketTest
{
    class Client
    {
        private UnixDomainSocketEndPoint m_UnixSocketEndpoint;
        private Socket m_ClientSocket;
        private readonly string m_SocketPath = "/var/run/socketTest.sock";
        private readonly string m_TestMessage = "Helloooo! I am the client.";
        public Client()
        {
            m_ClientSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            m_UnixSocketEndpoint = new UnixDomainSocketEndPoint(m_SocketPath);
            m_ClientSocket.Connect(m_UnixSocketEndpoint);

            byte[] message = Encoding.ASCII.GetBytes(m_TestMessage);
            m_ClientSocket.Send(message);
            byte[] response = new byte[256];
            bool waitingForMessage = true;

            while (waitingForMessage)
            {
                if (m_ClientSocket.Available > 0)
                {
                    int numberOfBytes = m_ClientSocket.Receive(response, 256, SocketFlags.None);
                    Console.WriteLine($"Number of bytes received = {numberOfBytes}");
                    Console.WriteLine($"Message from the other side is {Encoding.ASCII.GetString(response)}");
                    waitingForMessage = false;
                    Console.WriteLine("Message received. Stopping loop");
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(500));
            }
        }
    }
}
