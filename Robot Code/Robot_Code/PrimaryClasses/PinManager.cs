using System;
using System.Collections.Generic;
using System.Device.Gpio;

namespace PrimaryClasses
{
    public class PinManager
    {
        private Dictionary<int, PinMode> m_RegisteredPins;
        private GpioController m_Controller = new GpioController(numberingScheme: PinNumberingScheme.Board);
        private static PinManager m_Instance;

        private PinManager()
        {

        }

        public void Initialize()
        {
            Console.WriteLine($"\r\n[{DateTime.Now}] Motor Manager: Initializing...");
        }


        public static PinManager GetInstance()
        {
            return m_Instance ?? (m_Instance = new PinManager());
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
    }
}