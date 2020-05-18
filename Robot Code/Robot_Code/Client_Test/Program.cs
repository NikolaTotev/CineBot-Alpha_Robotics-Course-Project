using System;
using PrimaryClasses;
using UtilityClasses;

namespace Client_Test
{
    class Program
    {
        //Todo: Add Reconnect command
        static ControlClient client = new ControlClient("192.168.12.119", 4200);
        static void Main(string[] args)
        {
            client.RequestInput += Input;
            client.ClientNotification += DisplayMessage;
            Start();
        }

        static void Input(Configuration config)
        {
            string debugInput = Console.ReadLine();
            if (debugInput == "STOP")
            {
                client.StopClient();
            }
            else if (debugInput == "R")
            {
               client.StopClient();
               Start();
            }
            else
            {
                config.Debug = debugInput;
            }
        }

        public static void Start()
        {
            client.Start();

        }


        public static void DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
