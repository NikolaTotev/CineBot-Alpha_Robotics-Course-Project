using System;
using System.Collections.Generic;
using System.Text;

namespace PrimaryClasses
{
    public class FlagArgs
    {
        public bool CollisionFlag { get; set; }
        public bool StopFlag { get; set; }
        public bool topHit { get; set; }
        public bool bottomHit { get; set; }
        public StepperMotorOptions TargetStepperMotor { get; set; }
        public bool EmergencyStopActive { get; set; }

        public FlagArgs(bool clFlag, bool stpFlag, StepperMotorOptions targetStepperMotor)
        {
           CollisionFlag = clFlag;
           StopFlag = stpFlag;
           TargetStepperMotor = targetStepperMotor;
           EmergencyStopActive = false;
           topHit = false;
           bottomHit = false;
        }
    }
}
