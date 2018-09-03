using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    //  ██████╗ ███████╗███████╗██╗███╗   ██╗███████╗████████╗ ██████╗  ██████╗ ██╗     
    //  ██╔══██╗██╔════╝██╔════╝██║████╗  ██║██╔════╝╚══██╔══╝██╔═══██╗██╔═══██╗██║     
    //  ██║  ██║█████╗  █████╗  ██║██╔██╗ ██║█████╗     ██║   ██║   ██║██║   ██║██║     
    //  ██║  ██║██╔══╝  ██╔══╝  ██║██║╚██╗██║██╔══╝     ██║   ██║   ██║██║   ██║██║     
    //  ██████╔╝███████╗██║     ██║██║ ╚████║███████╗   ██║   ╚██████╔╝╚██████╔╝███████╗
    //  ╚═════╝ ╚══════╝╚═╝     ╚═╝╚═╝  ╚═══╝╚══════╝   ╚═╝    ╚═════╝  ╚═════╝ ╚══════╝
    //                                                                                  
    /// <summary>
    /// Defines a new Tool on the Robot that will be available for Attach/Detach
    /// </summary>
    public class ActionDefineTool : Action
    {
        public Tool tool;

        public override ActionType Type => ActionType.DefineTool;

        public ActionDefineTool(Tool tool)
        {
            this.tool = tool;
        }

        public ActionDefineTool(string name, 
            double tcpX, double tcpY, double tcpZ,
            double tcp_vX0, double tcp_vX1, double tcp_vX2, 
            double tcp_vY0, double tcp_vY1, double tcp_vY2,
            double weight, 
            double cogX, double cogY, double cogZ) : base()
        {
            this.tool = Tool.Create(name,
                tcpX, tcpY, tcpZ,
                tcp_vX0, tcp_vX1, tcp_vX2,
                tcp_vY0, tcp_vY1, tcp_vY2,
                weight,
                cogX, cogY, cogZ);
        }

        public override string ToString()
        {
            return string.Format("Define tool \"{0}\" on the Robot.", this.tool.name);
        }

        public override string ToInstruction()
        {
            return $"DefineTool(\"{tool.name}\",{tool.TCPPosition.X},{tool.TCPPosition.Y},{tool.TCPPosition.Z},{tool.TCPOrientation.XAxis.X},{tool.TCPOrientation.XAxis.Y},{tool.TCPOrientation.XAxis.Z},{tool.TCPOrientation.YAxis.X},{tool.TCPOrientation.YAxis.Y},{tool.TCPOrientation.YAxis.Z},{tool.Weight},{tool.CenterOfGravity.X},{tool.CenterOfGravity.Y},{tool.CenterOfGravity.Z});";
        }
    }
}
