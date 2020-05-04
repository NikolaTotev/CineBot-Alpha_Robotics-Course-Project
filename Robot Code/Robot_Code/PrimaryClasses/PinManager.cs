using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Threading;
using Unosquare.RaspberryIO.Abstractions;

namespace PrimaryClasses
{
    public class PinManager
    {
        private Dictionary<int, PinMode> m_RegisteredPins;
        private GpioController m_Controller;
        private static PinManager m_Instance;
        private readonly int m_StatusPin = 25;
        private readonly int m_ServerStatus = 12;
        private readonly int m_ErrorLight = 16;
        private readonly int m_JointATopStop = 5;
        private readonly int m_JointABottomStop = 6;

        private readonly int m_JointBTopStop = 20;
        private readonly int m_JointBBottomStop = 21;

        public int JointATop { get => m_JointATopStop; }
        public int JointABottom { get => m_JointABottomStop; }

        public int JointBTop { get => m_JointBTopStop; }
        public int JointBBottom { get => m_JointBBottomStop; }

        public GpioController Controller { get => m_Controller; }

        private PinManager()
        {
            Initialize();

        }

        public void Initialize()
        {
            Console.WriteLine($"\r\n[{DateTime.Now}] Pin Manager: Initializing...");
            m_RegisteredPins = new Dictionary<int, PinMode>();
            //Pi.Init<BootstrapWiringPi>();
            m_Controller = new GpioController(numberingScheme: PinNumberingScheme.Logical);
            SetupPin(m_StatusPin, PinMode.Output);
            SetupPin(m_ServerStatus, PinMode.Output);
            SetupPin(m_ErrorLight, PinMode.Output);
            SetupPin(m_JointATopStop, PinMode.InputPullUp);
            SetupPin(m_JointABottomStop, PinMode.InputPullUp);
            if (m_Controller != null && m_RegisteredPins != null)
            {
                SetupLights();
            }
            else
            {
                StartupError();
            }
        }


        public static PinManager GetInstance()
        {
            return m_Instance ??= new PinManager();
        }

        public bool SetupPin(int pin, PinMode pinMode)
        {
            if (!m_RegisteredPins.ContainsKey(pin) && !m_Controller.IsPinOpen(pin))
            {
                m_Controller.OpenPin(pin, pinMode);
                m_RegisteredPins.Add(pin, pinMode);
                return true;
            }
            return false;
        }

        public void TriplePulse()
        {
            m_Controller.Write(m_ServerStatus, PinValue.High);
            Thread.Sleep(TimeSpan.FromSeconds(0.1));
            m_Controller.Write(m_ServerStatus, PinValue.Low);

            Thread.Sleep(TimeSpan.FromSeconds(0.1));

            m_Controller.Write(m_ErrorLight, PinValue.High);
            Thread.Sleep(TimeSpan.FromSeconds(0.1));
            m_Controller.Write(m_ErrorLight, PinValue.Low);

            Thread.Sleep(TimeSpan.FromSeconds(0.1));

            m_Controller.Write(m_StatusPin, PinValue.High);
            Thread.Sleep(TimeSpan.FromSeconds(0.1));
            m_Controller.Write(m_StatusPin, PinValue.Low);

            Thread.Sleep(TimeSpan.FromSeconds(0.1));

        }

        public void SetupLights()
        {
            m_Controller.Write(m_StatusPin, PinValue.High);
            Thread.Sleep(TimeSpan.FromSeconds(0.5));

            for (int i = 0; i < 4; i++)
            {
                m_Controller.Write(m_StatusPin, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                m_Controller.Write(m_StatusPin, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(0.2));
            }
            m_Controller.Write(m_StatusPin, PinValue.Low);
        }

        public void StartupError()
        {
            for (int i = 0; i < 5; i++)
            {
                m_Controller.Write(m_ErrorLight, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                m_Controller.Write(m_ErrorLight, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            m_Controller.Write(m_ErrorLight, PinValue.Low);
        }

        public void ServerStarted()
        {
            m_Controller.Write(m_ServerStatus, PinValue.High);
            Thread.Sleep(TimeSpan.FromSeconds(0.5));
            m_Controller.Write(m_ServerStatus, PinValue.Low);
            Thread.Sleep(TimeSpan.FromSeconds(0.5));

            for (int i = 0; i < 2; i++)
            {
                m_Controller.Write(m_ServerStatus, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                m_Controller.Write(m_ServerStatus, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(0.3));
            }
            m_Controller.Write(m_ServerStatus, PinValue.Low);
        }

        public void ClientConnected()
        {
            for (int i = 0; i < 4; i++)
            {
                m_Controller.Write(m_ServerStatus, PinValue.High);
                m_Controller.Write(m_ErrorLight, PinValue.High);
                m_Controller.Write(m_StatusPin, PinValue.High);

                Thread.Sleep(TimeSpan.FromSeconds(0.2));
                m_Controller.Write(m_ServerStatus, PinValue.Low);
                m_Controller.Write(m_ErrorLight, PinValue.Low);
                m_Controller.Write(m_StatusPin, PinValue.Low);
                Thread.Sleep(TimeSpan.FromSeconds(0.3));
            }
            m_Controller.Write(m_ServerStatus, PinValue.Low);
        }
    }
}