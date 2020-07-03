using System;
using System.IO;
using System.IO.Ports;

namespace PrimaryClasses
{
    public class SerialComsManager
    {
        private static SerialComsManager m_Instance;
        private SerialPort serialPort;

        private SerialComsManager()
        {
            //if (Directory.Exists("/dev/ttyACM0"))
            //{
                
            //}
            serialPort = new SerialPort("/dev/ttyACM0", 9600); //Set the read/write timeouts    
            serialPort.ReadTimeout = 1500;
            serialPort.WriteTimeout = 1500;
            serialPort.Open();
            InitSerialComs();
        }

        public static SerialComsManager GetInstance()
        {
            return m_Instance ??= new SerialComsManager();
        }

        private void InitSerialComs()
        {
            string arduinoResponse =  serialPort.ReadLine();
            Console.WriteLine($"\r\n[{DateTime.Now}] [Serial Coms] : Arduino said {arduinoResponse}");
            serialPort.WriteLine("CLEAR");
            arduinoResponse = serialPort.ReadLine();
            if (arduinoResponse == "CLEAR")
            {
                Console.WriteLine($"\r\n[{DateTime.Now}] [Serial Coms] : Arduino is ready. Init response {arduinoResponse}");
            }
            else
            {
                Console.WriteLine($"\r\n[{DateTime.Now}] [Serial Coms] : Arduino coms error. ");
            }


        }

        public void Write(string input)
        {
            serialPort.WriteLine(input);
        }

        public string Read()
        {
            return serialPort.ReadLine();
        }
    }
}