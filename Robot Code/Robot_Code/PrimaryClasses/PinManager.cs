using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Threading;
using Iot.Device.ExplorerHat;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;

namespace PrimaryClasses
{
    public class PinManager
    {
        private Dictionary<int, PinMode> m_RegisteredPins;
        private GpioController m_Controller;
        private static PinManager m_Instance;

        private readonly int m_StatusLight = 23;
        private readonly int m_ErrorLight = 24;
        private readonly int m_NotificationLight = 5;

        private readonly int m_JointADir = 6;
        private readonly int m_JointAStep = 13;

        private readonly int m_JointBDir = 19;
        private readonly int m_JointBStep = 26;

        private readonly int m_PanSIA = 17;
        private readonly int m_PanSIB = 27;

        private readonly int m_RotSIA = 10;
        private readonly int m_RotSIB = 9;

        private readonly int m_TiltSIA = 16;
        private readonly int m_TiltSIB = 20;

        private readonly int m_PanSwitch = 22;
        private readonly int m_RotSwitch = 11;
        private readonly int m_TiltSwitch = 21;

        private readonly int m_JointBTopStop = 8;
        private readonly int m_JointBBottomStop = 7;

        private readonly int m_JointATopStop = 15;
        private readonly int m_JointABottomStop = 14;

        private readonly int m_EmergencyStop = 25;

        public int JointADir => m_JointADir;
        public int JointBDir => m_JointBDir;
        public int JointAStep => m_JointAStep;
        public int JointBStep => m_JointBStep;


        public int JointATop => m_JointATopStop;
        public int JointABottom => m_JointABottomStop;

        public int JointBTop => m_JointBTopStop;
        public int JointBBottom => m_JointBBottomStop;

        public int JogCW
        {
            get => m_PanSwitch;
        }

        public int JogCCW
        {
            get => m_RotSwitch;
        }

        public int SelectA
        {
            get => m_TiltSwitch;
        }
        //public int SelectB { get => m_SelectB; }


        public int PanSIA
        {
            get => m_PanSIA;
        }

        public int PanSIB
        {
            get => m_PanSIB;
        }

        public int RotSIA
        {
            get => m_RotSIA;
        }

        public int RotSIB
        {
            get => m_RotSIB;
        }


        public int TiltSIA
        {
            get => m_TiltSIA;
        }

        public int TiltSIB
        {
            get => m_TiltSIB;
        }

        public int PanReset
        {
            get => m_PanSwitch;
        }

        public int RotReset
        {
            get => m_RotSwitch;
        }

        public int TiltReset
        {
            get => m_TiltSwitch;
        }

        public int NotificaitonLight
        {
            get => m_NotificationLight;
        }

        public int ErrorLight
        {
            get => m_ErrorLight;
        }

        public int StatusLight
        {
            get => m_StatusLight;
        }

        //TODO Fix change button once solution for pull-up is found.
        public int EmergencyStop
        {
            get => m_EmergencyStop;
        }

        public GpioController Controller
        {
            get => m_Controller;
        }

        private PinManager()
        {
            Initialize();

        }

        public void Initialize()
        {
            Console.WriteLine($"\r\n[{DateTime.Now}] Pin Manager: Initializing...");
            m_RegisteredPins = new Dictionary<int, PinMode>();

            m_Controller = new GpioController(numberingScheme: PinNumberingScheme.Logical);
            SetupPin(m_StatusLight, PinMode.Output);
            SetupPin(m_ErrorLight, PinMode.Output);
            SetupPin(m_NotificationLight, PinMode.Output);

            SetupPin(m_JointATopStop, PinMode.InputPullUp);
            SetupPin(m_JointABottomStop, PinMode.InputPullUp);

            SetupPin(m_JointBTopStop, PinMode.InputPullUp);
            SetupPin(m_JointBBottomStop, PinMode.InputPullUp);

            SetupPin(m_EmergencyStop, PinMode.InputPullUp);


            SetupPin(m_PanSIA, PinMode.InputPullUp);
            SetupPin(m_PanSIB, PinMode.InputPullUp);

            SetupPin(m_RotSIA, PinMode.InputPullUp);
            SetupPin(m_RotSIB, PinMode.InputPullUp);

            SetupPin(m_TiltSIA, PinMode.InputPullUp);
            SetupPin(m_TiltSIB, PinMode.InputPullUp);

            SetupPin(m_PanSwitch, PinMode.InputPullUp);
            SetupPin(m_RotSwitch, PinMode.InputPullUp);
            SetupPin(m_TiltSwitch, PinMode.InputPullUp);

            SetupPin(SelectA, PinMode.InputPullUp);

            SetupPin(JogCW, PinMode.InputPullUp);
            SetupPin(JogCCW, PinMode.InputPullUp);

            SetupPin(m_JointADir, PinMode.Output);
            SetupPin(m_JointAStep, PinMode.Output);

            SetupPin(m_JointBDir, PinMode.Output);
            SetupPin(m_JointBStep, PinMode.Output);

            NotificationManager.SetupLights();

            if (m_Controller != null && m_RegisteredPins != null)
            {
                NotificationManager.SetupLights();
            }
            else
            {
                NotificationManager.StartupError();
            }
        }


        public static PinManager GetInstance()
        {
            return m_Instance ??= new PinManager();
        }

        public bool SetupPin(int pin, PinMode pinMode)
        {
            Console.WriteLine($"\r\n[{DateTime.Now}] Pin Manager: Setup called on pin {pin}");
            if (!m_RegisteredPins.ContainsKey(pin) && !m_Controller.IsPinOpen(pin))
            {
                m_Controller.OpenPin(pin, pinMode);
                m_RegisteredPins.Add(pin, pinMode);
                Console.WriteLine($"\r\n[{DateTime.Now}] Pin Manager: Pin {pin} is open.");
                return true;
            }

            return false;
        }
    }
}