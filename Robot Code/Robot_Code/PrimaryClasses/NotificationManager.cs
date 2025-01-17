﻿using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Drawing.Text;
using System.Text;
using System.Threading;
using Unosquare.RaspberryIO;

namespace PrimaryClasses
{
    public class NotificationManager
    {
        static GpioController m_Controller;
        static int m_StatusLight;
        static int m_ErrorLight;
        static int m_NotificationLight;

        public NotificationManager(GpioController controller, int status, int error, int notification)
        {
            m_Controller = controller;
            m_StatusLight = status;
            m_ErrorLight = error;
            m_NotificationLight = notification;
        }

        public static void EmergencyStopLights()
        {
            for (int i = 0; i < 5; i++)
            {
                m_Controller.Write(m_StatusLight, PinValue.High);
                m_Controller.Write(m_ErrorLight, PinValue.High);
                m_Controller.Write(m_NotificationLight, PinValue.High);

                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                m_Controller.Write(m_StatusLight, PinValue.Low);
                m_Controller.Write(m_ErrorLight, PinValue.Low);
                m_Controller.Write(m_NotificationLight, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(0.1));
            }


            m_Controller.Write(m_StatusLight, PinValue.High);
            m_Controller.Write(m_ErrorLight, PinValue.High);
            m_Controller.Write(m_NotificationLight, PinValue.High);

            Thread.Sleep(TimeSpan.FromSeconds(1));
            m_Controller.Write(m_StatusLight, PinValue.Low);
            m_Controller.Write(m_ErrorLight, PinValue.Low);
            m_Controller.Write(m_NotificationLight, PinValue.Low);
        }


        public static void DoublePulse()
        {
            m_Controller.Write(m_NotificationLight, PinValue.High);
            Thread.Sleep(TimeSpan.FromSeconds(0.1));
            m_Controller.Write(m_NotificationLight, PinValue.Low);

            Thread.Sleep(TimeSpan.FromSeconds(0.1));

            m_Controller.Write(m_ErrorLight, PinValue.High);
            Thread.Sleep(TimeSpan.FromSeconds(0.1));
            m_Controller.Write(m_ErrorLight, PinValue.Low);

            Thread.Sleep(TimeSpan.FromSeconds(0.1));

            m_Controller.Write(m_StatusLight, PinValue.High);
            Thread.Sleep(TimeSpan.FromSeconds(0.1));
            m_Controller.Write(m_StatusLight, PinValue.Low);

            Thread.Sleep(TimeSpan.FromSeconds(0.1));
        }

        public static void SetupLights()
        {
            m_Controller.Write(m_StatusLight, PinValue.High);
            Thread.Sleep(TimeSpan.FromSeconds(0.5));

            for (int i = 0; i < 4; i++)
            {
                m_Controller.Write(m_StatusLight, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                m_Controller.Write(m_StatusLight, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(0.2));
            }
            m_Controller.Write(m_StatusLight, PinValue.Low);
        }

        public static void StartupError()
        {
            for (int i = 0; i < 10; i++)
            {
                m_Controller.Write(m_ErrorLight, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                m_Controller.Write(m_ErrorLight, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            m_Controller.Write(m_ErrorLight, PinValue.Low);
        }

        public static void ServerStarted()
        {
            m_Controller.Write(m_StatusLight, PinValue.High);
            Thread.Sleep(TimeSpan.FromSeconds(0.5));
            m_Controller.Write(m_StatusLight, PinValue.Low);
            Thread.Sleep(TimeSpan.FromSeconds(0.5));

            for (int i = 0; i < 2; i++)
            {
                m_Controller.Write(m_StatusLight, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                m_Controller.Write(m_StatusLight, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(0.3));
            }
            m_Controller.Write(m_StatusLight, PinValue.Low);
        }

        public static void ClientConnected()
        {
            for (int i = 0; i < 4; i++)
            {
                m_Controller.Write(m_StatusLight, PinValue.High);
                m_Controller.Write(m_ErrorLight, PinValue.High);
                m_Controller.Write(m_NotificationLight, PinValue.High);

                Thread.Sleep(TimeSpan.FromSeconds(0.2));
                m_Controller.Write(m_StatusLight, PinValue.Low);
                m_Controller.Write(m_ErrorLight, PinValue.Low);
                m_Controller.Write(m_NotificationLight, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(0.3));
            }
            m_Controller.Write(m_StatusLight, PinValue.Low);
            m_Controller.Write(m_ErrorLight, PinValue.Low);
            m_Controller.Write(m_NotificationLight, PinValue.Low);
        }

        public static void Exception()
        {
            for (int i = 0; i < 4; i++)
            {
                m_Controller.Write(m_StatusLight, PinValue.High);
                m_Controller.Write(m_ErrorLight, PinValue.High);
                m_Controller.Write(m_NotificationLight, PinValue.High);

                Thread.Sleep(TimeSpan.FromSeconds(0.2));

                m_Controller.Write(m_StatusLight, PinValue.Low);
                m_Controller.Write(m_ErrorLight, PinValue.Low);
                m_Controller.Write(m_NotificationLight, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(0.2));

                m_Controller.Write(m_StatusLight, PinValue.High);
                m_Controller.Write(m_ErrorLight, PinValue.High);
                m_Controller.Write(m_NotificationLight, PinValue.High);

                Thread.Sleep(TimeSpan.FromSeconds(0.2));

                m_Controller.Write(m_StatusLight, PinValue.Low);
                m_Controller.Write(m_ErrorLight, PinValue.Low);
                m_Controller.Write(m_NotificationLight, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        public static void ModeEntryLights(MotionModes mode)
        {
            switch (mode)
            {
                case MotionModes.StepperJog:
                    for (int i = 0; i < 2; i++)
                    {
                        m_Controller.Write(m_NotificationLight, PinValue.High);
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                        m_Controller.Write(m_NotificationLight, PinValue.Low);

                        Thread.Sleep(TimeSpan.FromSeconds(0.5));

                        m_Controller.Write(m_ErrorLight, PinValue.High);
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                        m_Controller.Write(m_ErrorLight, PinValue.Low);

                        Thread.Sleep(TimeSpan.FromSeconds(0.5));

                        m_Controller.Write(m_StatusLight, PinValue.High);
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                        m_Controller.Write(m_StatusLight, PinValue.Low);

                        Thread.Sleep(TimeSpan.FromSeconds(0.5));
                    }

                    break;
                case MotionModes.GimbalJog:
                    m_Controller.Write(m_NotificationLight, PinValue.High);
                    m_Controller.Write(m_ErrorLight, PinValue.Low);
                    m_Controller.Write(m_StatusLight, PinValue.High);
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                    m_Controller.Write(m_NotificationLight, PinValue.Low);
                    m_Controller.Write(m_ErrorLight, PinValue.High);
                    m_Controller.Write(m_StatusLight, PinValue.Low);
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                    m_Controller.Write(m_NotificationLight, PinValue.Low);
                    m_Controller.Write(m_ErrorLight, PinValue.Low);
                    m_Controller.Write(m_StatusLight, PinValue.Low);
                    break;
                case MotionModes.ObjectTrack:
                    break;
                case MotionModes.PathFollow:
                    break;
                case MotionModes.TestMode:
                    break;
                case MotionModes.StepperHome:
                    break;
                case MotionModes.GimbalHome:
                    break;
                case MotionModes.RecordPath:
                    break;
                case MotionModes.Replay:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}


