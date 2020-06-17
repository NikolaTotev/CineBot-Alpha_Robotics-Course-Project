using System;
using System.Net.Sockets;

namespace UnixSocketTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args[1] == "Client")
            {
                Console.WriteLine("Starting as Client");
                Client UnixSocketClient =  new Client();
            }

            if (args[1] == "Server")
            {
                Console.WriteLine("Starting as Client");
                Server UnixSocketServer = new Server();
            }
            
        }
    }
}
