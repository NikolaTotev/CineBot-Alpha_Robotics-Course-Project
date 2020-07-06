using System;
using Iot.Device.Lsm9Ds1;
using UtilityClasses;

namespace PrimaryClasses
{
    public class RobotManager
    {

        private static RobotManager m_Instance;
        private RobotServer m_Server;
        private PinManager m_PinManager;

        public delegate void GetInput(ServerControlObject message);
        public event GetInput CheckConsole;
        private bool m_StopFlag;
        private RobotManager()
        {
            //MotorManager.GetInstance().Initialize();
            m_PinManager= PinManager.GetInstance();
            Console.WriteLine("Starting server.");
            m_Server = new RobotServer();
            m_Server.ServerNotification += DisplayMessage;
            m_Server.StartServer();



            while (!m_StopFlag)
            {
                ServerControlObject obj = new ServerControlObject();
                CheckConsole?.Invoke(obj);
                string debugInput = Console.ReadLine();
                Console.WriteLine(debugInput);
                if (debugInput == "STOP")
                {
                    m_Server.StopServer();
                    m_StopFlag = true;
                }
            }
        }

        public static void DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }
        public static RobotManager GetInstance()
        {
            return m_Instance ??= new RobotManager();
        }






    }
}