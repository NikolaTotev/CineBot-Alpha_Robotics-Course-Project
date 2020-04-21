using System;
using System.Threading;
using ServerClientUtils;

namespace Central_Control
{
    class Program
    {
        static void Main(string[] args)
        {
           RobotServer server= new RobotServer();
           server.ServerNotification += DisplayMessage;
           server.StartServer();
           Console.WriteLine("This is from the main thread!");
        }

        public static void DisplayMessage(string message)
        {
           Console.WriteLine(message);
        }
    }
}
