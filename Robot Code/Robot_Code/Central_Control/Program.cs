using System;
using System.Threading;
using PrimaryClasses;
using UtilityClasses;

namespace Central_Control
{
    class ProgramSTOP
    {
        public static RobotManager RobotManager;

        static void Main(string[] args)
        {
            RobotManager = RobotManager.GetInstance();
            Console.WriteLine("Main program stopping.");
        }
    }
}
