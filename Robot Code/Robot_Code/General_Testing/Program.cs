using System;
using System.Device.Gpio;
using System.Diagnostics.Tracing;
using System.Threading;

namespace General_Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            (bool collisionFlag, bool shouldStop) flags = (collisionFlag: false, shouldStop: false);

            Thread secondThread = new Thread(derp);
            secondThread.Start(flags);

            while (!flags.collisionFlag)
            {

                Console.WriteLine("Derp");
                Thread.Sleep(TimeSpan.FromSeconds(0.5));


            }
        }

        public static void derp(object tupleStuff)
        {
            if (tupleStuff is Tuple<bool, bool> flags)
            {
                int counter = 0;
                while (counter < 10)
                {
                    flags.Item1 = true;
                    counter++;
                }
            }
        }
    }
}
