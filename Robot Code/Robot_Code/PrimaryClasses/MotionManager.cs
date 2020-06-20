using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Iot.Device.Uln2003;
using Motor_Control;

namespace PrimaryClasses
{
    public enum MotionModes
    {
        StepperJog,
        GimbalJog,
        PathFollow
    };

    public enum MotorOptions
    {
        motorA,
        motorB
    }

    public class MotorThreadStartObj
    {
        public int CompleteStatus { get; set; }
        public MotorOptions TargetMotor { get; set; }
        public bool ShouldStop { get; set; }

        public MotorThreadStartObj(int initialStatus, MotorOptions targetMotor)
        {
            CompleteStatus = initialStatus;
            TargetMotor = targetMotor;
            ShouldStop = false;
        }
    }

    class MotionManager
    {
        private MotionModes m_CurrentMode;

        public MotionManager(MotionModes mode)
        {
            m_CurrentMode = mode;

            switch (m_CurrentMode)
            {
                case MotionModes.StepperJog:
                    break;
                case MotionModes.GimbalJog:
                    break;
                case MotionModes.PathFollow:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public int StepperJog()
        {
            
            MotorManager currentManager = new MotorManager();
            Thread motorAThread = new Thread(currentManager.JogMode);
            Thread motorBThread = new Thread(currentManager.JogMode);

            MotorThreadStartObj mtrAParams = new MotorThreadStartObj(42, MotorOptions.motorA);
            MotorThreadStartObj mtrBParams = new MotorThreadStartObj(42, MotorOptions.motorB);
         
            motorAThread.Start(mtrAParams);
            motorBThread.Start(mtrBParams);
            return 0;
        }

        public int GimbalJog()
        {
            return 0; 
        }
    }
}
