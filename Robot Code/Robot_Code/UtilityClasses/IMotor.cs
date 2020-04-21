using System;
using System.Collections.Generic;
using System.Text;

namespace UtilityClasses
{
    public interface IMotor
    {
        void MoveCw(float degrees, int speed);
        void MoveCcw(float degrees, int speed);

        MotorTypes MotorType();
    }
}
