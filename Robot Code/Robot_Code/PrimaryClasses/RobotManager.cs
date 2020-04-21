using Motor_Control;
using ServerClientUtils;

namespace PrimaryClasses
{
    public class RobotManager
    {

        private static RobotManager m_Instance;
        private RobotServer m_Server;

        private RobotManager()
        {
            MotorManager.GetInstance().Initialize();
            PinManager.GetInstance().Initialize();
            m_Server = new RobotServer();
            m_Server.StartServer();
        }

        public static RobotManager GetInstance()
        {
            return m_Instance ?? (m_Instance = new RobotManager());
        }


    }
}