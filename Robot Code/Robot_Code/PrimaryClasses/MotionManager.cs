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

    public enum StepperMotorOptions
    {
        motorA,
        motorB
    }

    public enum ServoMotorOptions
    {
        pan,
        rotate,
        tilt
    }

    public class MotorThreadStartObj
    {
        public int CompleteStatus { get; set; }
        public StepperMotorOptions TargetStepperMotor { get; set; }
        public bool ShouldStop { get; set; }

        public MotorThreadStartObj(int initialStatus, StepperMotorOptions targetStepperMotor)
        {
            CompleteStatus = initialStatus;
            TargetStepperMotor = targetStepperMotor;
            ShouldStop = false;
        }
    }

    public class ErrorCodeObj
    {

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
                    Thread jogThread = new Thread(StepperJog);

                    break;
                case MotionModes.GimbalJog:
                    break;
                case MotionModes.PathFollow:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void StepperJog(object returnReference)
        {
            if (returnReference is int retValue)
            {
                MotorManager currentManager = new MotorManager();
                Thread motorAThread = new Thread(currentManager.JogMode);
                Thread motorBThread = new Thread(currentManager.JogMode);

                MotorThreadStartObj mtrAParams = new MotorThreadStartObj(42, StepperMotorOptions.motorA);
                MotorThreadStartObj mtrBParams = new MotorThreadStartObj(42, StepperMotorOptions.motorB);

                motorAThread.Start(mtrAParams);
                motorBThread.Start(mtrBParams);
            }
            else
            {
                Console.WriteLine($"\r\n[{DateTime.Now}] <Motion Manager> Given parameter is not integer. \n" + 
                                  $" Aborting Jog Mode as there is no way to return execution status.");
            }
        }

        public int GimbalJog()
        {
            return 0; 
        }
    }
}
