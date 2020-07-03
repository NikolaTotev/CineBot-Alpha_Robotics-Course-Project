using System;
using System.Device.Gpio;
using System.Drawing;

namespace PrimaryClasses
{
    public enum EncoderOptions { Pan, Rotate, Tilt }
    public class Encoder
    {
        private EncoderOptions m_TargetEncoder;
        private int m_SIAPin;
        private int m_SIBPin;
        private int m_SwitchPin;

        public  PinValue LastSIAState { get; set; }
        public PinValue SIAValue { get; set; }
        public PinValue SIBValue { get; set; }
        public PinValue SwitchValue { get; set; }

        public Encoder(EncoderOptions targetEncoder)
        {
            m_TargetEncoder = targetEncoder;

            switch (targetEncoder)
            {
                case EncoderOptions.Pan:
                    m_SIAPin = PinManager.GetInstance().PanSIA;
                    m_SIBPin = PinManager.GetInstance().PanSIB;
                    m_SwitchPin = PinManager.GetInstance().PanReset;
                    break;
                case EncoderOptions.Rotate:
                    m_SIAPin = PinManager.GetInstance().RotSIA;
                    m_SIBPin = PinManager.GetInstance().RotSIB;
                    m_SwitchPin = PinManager.GetInstance().RotReset;
                    break;
                case EncoderOptions.Tilt:
                    m_SIAPin = PinManager.GetInstance().TiltSIA;
                    m_SIBPin = PinManager.GetInstance().TiltSIB;
                    m_SwitchPin = PinManager.GetInstance().TiltReset;
                    break;
                default:
                    Console.WriteLine($"Encoder error setup error.");
                    break;
            }

            LastSIAState = ReadSIA();
        }

        public PinValue ReadSIA()
        {
            SIAValue = PinManager.GetInstance().Controller.Read(m_SIAPin);
            return SIAValue;
        }

        public PinValue ReadSIB()
        {
            SIBValue = PinManager.GetInstance().Controller.Read(m_SIBPin);
            return SIAValue;
        }

        public PinValue ReadSwitch()
        {
            SwitchValue = PinManager.GetInstance().Controller.Read(m_SwitchPin);
            return SwitchValue;
        }

        public void SetLastState()
        {
            LastSIAState = SIAValue;
        }
    }
}