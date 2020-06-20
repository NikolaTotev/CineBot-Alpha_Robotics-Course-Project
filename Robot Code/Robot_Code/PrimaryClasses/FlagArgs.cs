using System;
using System.Collections.Generic;
using System.Text;

namespace PrimaryClasses
{
    public class FlagArgs
    {
        public bool CollisionFlag { get; set; }
        public bool StopFlag { get; set; }
        public MotorOptions TargetMotor { get; set; }

        

        public FlagArgs(bool clFlag, bool stpFlag, MotorOptions targetMotor)
        {
           CollisionFlag = clFlag;
           StopFlag = stpFlag;
           TargetMotor = targetMotor
        }
    }
}
