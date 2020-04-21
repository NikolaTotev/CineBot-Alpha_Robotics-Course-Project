using System;
using ServerClientUtils;
using UtilityClasses;


namespace Client_Test
{
    class Program
    {

        static ControlClient client = new ControlClient("192.168.12.119", 4200);
        static void Main(string[] args)
        {
            client.RequestInput += Input;
            client.ClientNotification += DisplayMessage;
            client.Start();
        }

        static void Input(Configuration config)
        {
            string debugInput = Console.ReadLine();
            if (debugInput == "STOP")
            {
                client.StopClient();
            }
            else
            {
                config.Debug = debugInput;
            }
        }


        public static void DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
