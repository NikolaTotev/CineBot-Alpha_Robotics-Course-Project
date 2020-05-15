using System;
using System.Collections.Generic;
using System.Text;

namespace PrimaryClasses
{
    public class FlagArgs
    {
        public bool A_CollisionFlag { get; set; }
        public bool B_CollisionFlag { get; set; }
        public bool stopFlag { get; set; }
        public string motor { get; set; }

        

        public FlagArgs(bool a_clFlag, bool b_clFlag, bool stpFlag, string mtr)
        {
            A_CollisionFlag = a_clFlag;
            B_CollisionFlag = b_clFlag;
            stopFlag = stpFlag;
            motor = mtr;
        }
    }
}
