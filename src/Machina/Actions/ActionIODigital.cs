using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    //  ██╗ ██████╗ ██████╗ ██╗ ██████╗ ██╗████████╗ █████╗ ██╗     
    //  ██║██╔═══██╗██╔══██╗██║██╔════╝ ██║╚══██╔══╝██╔══██╗██║     
    //  ██║██║   ██║██║  ██║██║██║  ███╗██║   ██║   ███████║██║     
    //  ██║██║   ██║██║  ██║██║██║   ██║██║   ██║   ██╔══██║██║     
    //  ██║╚██████╔╝██████╔╝██║╚██████╔╝██║   ██║   ██║  ██║███████╗
    //  ╚═╝ ╚═════╝ ╚═════╝ ╚═╝ ╚═════╝ ╚═╝   ╚═╝   ╚═╝  ╚═╝╚══════╝
    //                                                              
    /// <summary>
    /// Turns digital pin # on or off.
    /// </summary>
    public class ActionIODigital : Action
    {
        // See RobotCursor for string/int32 naming
        public string pinName;
        public bool on;
        public bool isDigit = false;
        public int pinNum = 0;
        public bool isToolPin = false;

        public override ActionType Type => ActionType.IODigital;

        public ActionIODigital(string pin, bool isOn, bool toolPin) : base()
        {
            this.pinName = pin;
            this.on = isOn;
            this.isDigit = Int32.TryParse(this.pinName, out this.pinNum);
            this.isToolPin = toolPin;
        }

        public override string ToString()
        {
            return $"Turn {(this.isToolPin ? "tool " : "")}digital IO {(this.isDigit ? this.pinNum.ToString() : "\"" + this.pinName + "\"")} {(this.on ? "ON" : "OFF")}";
        }

        public override string ToInstruction()
        {
            return $"WriteDigital({(this.isDigit ? this.pinNum.ToString() : "\"" + this.pinName + "\"")},{this.on},{this.isToolPin});";
        }
    }
}
