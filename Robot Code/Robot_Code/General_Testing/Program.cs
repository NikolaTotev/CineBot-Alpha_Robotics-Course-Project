using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Diagnostics.Tracing;
using System.IO.Ports;
using System.Threading;
using Iot.Device.ServoMotor;

namespace General_Testing
{
	class Program
	{
		static SerialPort serialPort;
		static int counter = 0;
		static void Main(string[] args)
		{
			float stepCounter = 0;
			GpioController controller = new GpioController();
			controller.OpenPin(26, PinMode.InputPullUp);
			controller.OpenPin(11 , PinMode.InputPullUp);
			controller.OpenPin(10, PinMode.InputPullUp);
			controller.OpenPin(9, PinMode.InputPullUp);
			PinValue state;
			PinValue lastState;
			lastState = controller.Read(11);

			PinValue emergencyStop = PinValue.High;

			Console.Write("Port no: ");
			Console.Write("baudrate: ");

			serialPort = new SerialPort("/dev/ttyACM0", 9600); //Set the read/write timeouts    
			serialPort.ReadTimeout = 1500;
			serialPort.WriteTimeout = 1500;
			serialPort.Open();
			serialPort.WriteLine("80");
			while (emergencyStop != PinValue.Low)
			{
				emergencyStop = controller.Read(26);
				if (controller.Read(10) == PinValue.Low)
				{
					stepCounter = 80;
					serialPort.WriteLine("80");
					Thread.Sleep(TimeSpan.FromMilliseconds(15));
				}
				Read();
				Thread.Sleep(TimeSpan.FromSeconds(0.001));

				state = controller.Read(11);

				if (state != lastState)
				{
					if (controller.Read(9) != state)
					{
						stepCounter += 2.5f;
					}
					else
					{
						stepCounter -= 2.5f;
					}

					if (Math.Abs(stepCounter % 1) < 0.01)
					{
						Console.WriteLine($"Step: {stepCounter}");
						serialPort.WriteLine($"{stepCounter}");
						Thread.Sleep(TimeSpan.FromMilliseconds(15));
					}

					if (Math.Abs(stepCounter - 360) < 0.0001 || Math.Abs(stepCounter - (-360)) < 0.0001)
					{
						stepCounter = 0;
					}
				}

				Thread.Sleep(TimeSpan.FromSeconds(0.001));
				lastState = state;
			}
			serialPort.Close();
		}
		public static void Read()
		{
			try
			{
				if (serialPort.BytesToRead > 0)
				{
					string message = serialPort.ReadLine();
					Console.WriteLine(message);
				}

			}
			catch (TimeoutException)
			{
			}
		}


	}
}




//float counter = 0;
//PinValue state;
//PinValue lastState;

//lastState = controller.Read(10);
//while (true)
//{
//    state = controller.Read(10);

//    if (state != lastState)
//    {
//        if (controller.Read(11) != state)
//        {
//            counter+=0.5f;
//        }
//        else
//        {
//            counter-=0.5f;
//        }

//        Console.WriteLine($"Counter: {counter}, State: {state}, Last State: {lastState}");

//    }

//    Thread.Sleep(TimeSpan.FromSeconds(0.001));
//    lastState = state;

//    if (controller.Read(9) == PinValue.Low)
//    {
//        Console.WriteLine("I'VE BEEN CLICKED");
//    }


//}