﻿using System;
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
        ObjectTrack,
        PathFollow,
        TestMode
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

        public int GimbalJog()
        {
            return 0; 
        }

        public int PathFollow(string nCodeFile)
        {
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
