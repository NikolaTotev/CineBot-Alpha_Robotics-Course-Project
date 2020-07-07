using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Iot.Device.Uln2003;

namespace PrimaryClasses
{
    public enum MotionModes
    {
        StepperJog,
        GimbalJog,
        ObjectTrack,
        PathFollow,
        TestMode,
        StepperHome,
        GimbalHome,
        RecordPath,
        Replay
    };

    public enum RecordingModes
    {
        StepperRecording,
        GimbalRecording
    }

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

    public class TestResults
    {

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
        }

        public void ExecuteCommand()
        {
            switch (m_CurrentMode)
            {
                case MotionModes.StepperJog:
                    StepperJog();
                    break;
                case MotionModes.GimbalJog:
                    GimbalJog();
                    break;
                case MotionModes.PathFollow:
                    break;
                case MotionModes.ObjectTrack:
                    ObjectTrack();
                    break;
                case MotionModes.TestMode:
                    TestMode();
                    break;
                case MotionModes.StepperHome:
                    StepperGoHome();
                    break;
                case MotionModes.GimbalHome:
                    GimbalGoHome();
                    break;
                case MotionModes.RecordPath:
                    RecordPath();
                    break;
                case MotionModes.Replay:
                    PathFollow(1);
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

                MotorThreadStartObj mtrAParams = new MotorThreadStartObj(42, StepperMotorOptions.motorA);
                MotorThreadStartObj mtrBParams = new MotorThreadStartObj(42, StepperMotorOptions.motorB);

                motorAThread.Start(mtrAParams);
                motorBThread.Start(mtrBParams);

                return 0;
        }

        public int StepperGoHome()
        {
            MotorManager currentManager = new MotorManager();

            currentManager.GoToHome(StepperMotorOptions.motorA, false);
            
            currentManager.GoToHome(StepperMotorOptions.motorB, false);
            return 0;

        }

        public int RecordPath()
        {
            MotorManager currentManager = new MotorManager();
            currentManager.RecordMotion();
            return 0;
        }
        public int GimbalGoHome()
        {
            MotorManager currentManager = new MotorManager();
            currentManager.GoToHomeGimbal();
            return 0;
        }
        public int GimbalJog()
        {
            return 0; 
        }

        public int PathFollow(int nCodeFile)
        {
            MotorManager currentManager = new MotorManager();
            currentManager.FollowPath(nCodeFile);
            return 0;
        }

        public int ObjectTrack()
        {
            return 0;
        }

        public TestResults TestMode()
        {
            TestResults results = new TestResults();
            return results;
        }
    }

}
