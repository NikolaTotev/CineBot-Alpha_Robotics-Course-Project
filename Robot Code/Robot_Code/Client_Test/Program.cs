using System;
using PrimaryClasses;
using UtilityClasses;

namespace Client_Test
{
    class Program
    {
        private static readonly int m_StepsPerRevolution = 200;
        private static readonly int m_StepMultiplier = 1;
        private static readonly int m_GearRatio = 7;
        private static readonly int m_MinimumAngle = 5;
        private static readonly float m_MinimumSpeed = 0.01f;
        private static readonly float m_SpeedSensitivity = 1;
        private static readonly float m_CollisionDetectionFrequency = 0.01f;
        private int m_MovementSensitivity = 1;
        private int m_StepsInRange = 0;
        //Todo: Add Reconnect command
        static ControlClient client = new ControlClient("192.168.12.133", 4200);
        static void Main(string[] args)
        {
        //    client.RequestInput += Input;
        //    client.ClientNotification += DisplayMessage;
        //    Start();
            Console.WriteLine($"{ConvertAngleToSteps(5)}");
        }


        public static int ConvertAngleToSteps(int inputAngle)
        {

            int stepsForCarrierRevolution = m_StepsPerRevolution * m_StepMultiplier * m_GearRatio;
            int partOfCircle = 360 / inputAngle;
            int stepsForInputAngle = stepsForCarrierRevolution / partOfCircle;

            Console.WriteLine(partOfCircle);
            return stepsForInputAngle;
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
