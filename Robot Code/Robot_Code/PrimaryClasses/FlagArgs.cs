using System;
using System.Collections.Generic;
using System.Text;

namespace PrimaryClasses
{
    public class FlagArgs
    {
        public bool collisionFlag { get; set; }
        public bool stopFlag { get; set; }
        public string motor { get; set; }

        public FlagArgs(bool clFlag, bool stpFlag, string mtr)
        {
            collisionFlag = clFlag;
            stopFlag = stpFlag;
            motor = mtr;
        }
    }
}
