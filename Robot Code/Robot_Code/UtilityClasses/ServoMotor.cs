using System;
using System.Collections.Generic;
using System.Text;

namespace UtilityClasses
{
    public class ServoMotor:IMotor
    {

        public MotorTypes MotorType { get; set; }
        public string MotorId { get; set; }
        public float MaxAngle { get; set; }
        public float MinAngle { get; set; }
        public int ControlPin { get; set; }

        public void MoveCw(float degrees, int speed)
        {
            throw new NotImplementedException();
        }

        public void MoveCcw(float degrees, int speed)
        {
            throw new NotImplementedException();
        }

        MotorTypes IMotor.MotorType()
        {
            return MotorType;
        }
    }
}
