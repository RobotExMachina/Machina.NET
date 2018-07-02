using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{

    //  ████████╗ ██████╗  ██████╗ ██╗     
    //  ╚══██╔══╝██╔═══██╗██╔═══██╗██║     
    //     ██║   ██║   ██║██║   ██║██║     
    //     ██║   ██║   ██║██║   ██║██║     
    //     ██║   ╚██████╔╝╚██████╔╝███████╗
    //     ╚═╝    ╚═════╝  ╚═════╝ ╚══════╝
    //                                     

    /// <summary>
    /// Represents a tool object that can be attached to the end effector of the robot.
    /// This class is public and will be used directly by the user, so careful design of the API
    /// vs. internal methods will be relevant. 
    /// </summary>
    public class Tool
    {
        /// <summary>
        /// Gets a Tool object representing no tool attached. 
        /// </summary>
        public static Tool Unset => new Tool("noTool", Point.Origin, Orientation.WorldXY, 0, Point.Origin);

        public string name { get; internal set; }

        /// <summary>
        /// Position of the Tool Center Point (TCP) relative to the Tool's base coordinate system. 
        /// In other words, if the Tool gets attached to the robot flange in XYZ [0, 0, 0], where is the tooltip relative to this?
        /// </summary>
        public Point TCPPosition { get; internal set; }

        /// <summary>
        /// Orientation of the Tool Center Point (TCP) relative to the Tool's base coordinate system. 
        /// In other words, if the Tool gets attached to the robot flange in XYZ [0, 0, 0], what is the relative rotation?
        /// </summary>
        public Orientation TCPOrientation { get; internal set; }

        /// <summary>
        /// Weight of the tool in Kg.
        /// </summary>
        public double Weight { get; internal set; }

        /// <summary>
        /// Position of the Tool's CoG relative to the flange.
        /// </summary>
        public Vector centerOfGravity { get; internal set; }   

        // For the time being, tools will be defined through position (first) and orientation
        internal bool translationFirst = true;

        private Tool(string name, double tcpX, double tcpY, double tcpZ,
            double tcp_vX0, double tcp_vX1, double tcp_vX2, double tcp_vY0, double tcpvY1, double tcp_vY2,
            double weight, double cogX, double cogY, double cogZ)
        {
            this.name = name;
            this.TCPPosition = new Point(tcpX, tcpY, tcpZ);
            this.TCPOrientation = new Orientation(tcp_vX0, tcp_vX1, tcp_vX2, tcp_vY0, tcpvY1, tcp_vY2);
            this.Weight = weight;
            this.centerOfGravity = new Point(cogX, cogY, cogZ);
        }

        /// <summary>
        /// Create a new Tool object by defining the Position and Orientation of the 
        /// Tool Center Point (TCP) relative to the Tool's base coordinate system. 
        /// In other words, if the Tool gets attached to the robot flange in 
        /// XYZ [0, 0, 0], where is the tooltip and how is it oriented?
        /// </summary>
        /// <param name="TCPPosition"></param>
        /// <param name="TCPOrientation"></param>
        [System.Obsolete("Deprecated constructor, use Tool.Create() instead")]
        public Tool(string name, Point TCPPosition, Orientation TCPOrientation)
        {
            this.name = name;
            this.TCPPosition = TCPPosition;
            this.TCPOrientation = TCPOrientation;
            this.Weight = 1;
            this.centerOfGravity = new Vector(TCPPosition);
            this.centerOfGravity.Scale(0.5);  // quick estimation
        }

        /// <summary>
        /// Create a new Tool object by defining the Position and Orientation of the 
        /// Tool Center Point (TCP), its weight in Kg and its center of gravity.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="TCPPosition"></param>
        /// <param name="TCPOrientation"></param>
        /// <param name="weightKg"></param>
        /// <param name="centerOfGRavity"></param>
        [System.Obsolete("Deprecated constructor, use Tool.Create() instead")]
        public Tool(string name, Point TCPPosition, Orientation TCPOrientation, double weightKg, Point centerOfGravity)
        {
            this.name = name;
            this.TCPPosition = TCPPosition;
            this.TCPOrientation = TCPOrientation;
            this.Weight = weightKg;
            this.centerOfGravity = centerOfGravity;
        }

        /// <summary>
        /// Create a new Tool object by defining the Position and Orientation of the 
        /// Tool Center Point (TCP) relative to the Tool's base coordinate system. 
        /// In other words, if the Tool gets attached to the robot flange in 
        /// XYZ [0, 0, 0], where is the tooltip and how is it oriented?
        /// </summary>
        /// <param name="name">Tool name</param>
        /// <param name="TCPPosition">Tool Center Point</param>
        /// <param name="TCPOrientation">Orientation of Tool Center Point</param>
        /// <returns></returns>
        static public Tool Create(string name, Point TCPPosition, Orientation TCPOrientation)
        {
            Vector centerOfGravity = new Vector(TCPPosition);
            centerOfGravity.Scale(0.5);  // quick estimation
            return new Tool(name, 
                TCPPosition.X, TCPPosition.Y, TCPPosition.Z, 
                TCPOrientation.XAxis.X, TCPOrientation.XAxis.Y, TCPOrientation.XAxis.Z,
                TCPOrientation.YAxis.X, TCPOrientation.YAxis.Y, TCPOrientation.YAxis.Z, 
                1, 
                centerOfGravity.X, centerOfGravity.Y, centerOfGravity.Z);
        }

        /// <summary>
        /// Create a new Tool object by defining the Position and Orientation of the 
        /// Tool Center Point (TCP), its weight in Kg and its center of gravity. 
        /// </summary>
        /// <param name="name">Tool name</param>
        /// <param name="TCPPosition">Tool Center Point</param>
        /// <param name="TCPOrientation">Orientation of Tool Center Point</param>
        /// <param name="weightKg">Tool weight in Kg</param>
        /// <param name="centerOfGravity">Center Of Gravity</param>
        /// <returns></returns>
        static public Tool Create(string name, Point TCPPosition, Orientation TCPOrientation, double weightKg, Point centerOfGravity)
        {
            return new Tool(name,
                TCPPosition.X, TCPPosition.Y, TCPPosition.Z,
                TCPOrientation.XAxis.X, TCPOrientation.XAxis.Y, TCPOrientation.XAxis.Z,
                TCPOrientation.YAxis.X, TCPOrientation.YAxis.Y, TCPOrientation.YAxis.Z,
                weightKg,
                centerOfGravity.X, centerOfGravity.Y, centerOfGravity.Z);
        }

        /// <summary>
        /// Create a new Tool object by defining the Position and Orientation of the 
        /// Tool Center Point (TCP) relative to the Tool's base coordinate system, 
        /// its weight in Kg and its center of gravity. 
        /// In other words, if the Tool gets attached to the robot flange in 
        /// XYZ [0, 0, 0], where is the tooltip and how is it oriented?
        /// </summary>
        /// <param name="name">Tool name</param>
        /// <param name="tcpX">X coordinate of Tool Center Point</param>
        /// <param name="tcpY">Y coordinate of Tool Center Point</param>
        /// <param name="tcpZ">Z coordinate of Tool Center Point</param>
        /// <param name="tcp_vX0">X coordinate of X Axis of Tool Center Point</param>
        /// <param name="tcp_vX1">Y coordinate of X Axis of Tool Center Point</param>
        /// <param name="tcp_vX2">Z coordinate of X Axis of Tool Center Point</param>
        /// <param name="tcp_vY0">X coordinate of Y Axis of Tool Center Point</param>
        /// <param name="tcp_vY1">Y coordinate of Y Axis of Tool Center Point</param>
        /// <param name="tcp_vY2">Z coordinate of Y Axis of Tool Center Point</param>
        /// <param name="weight">Tool weight in Kg</param>
        /// <param name="cogX">X coordinate of Center Of Gravity</param>
        /// <param name="cogY">Y coordinate of Center Of Gravity</param>
        /// <param name="cogZ">Z coordinate of Center Of Gravity</param>
        /// <returns></returns>
        static public Tool Create(string name, double tcpX, double tcpY, double tcpZ,
            double tcp_vX0, double tcp_vX1, double tcp_vX2, double tcp_vY0, double tcp_vY1, double tcp_vY2, 
            double weight, double cogX, double cogY, double cogZ)
        {
            return new Tool(name,
                tcpX, tcpY, tcpZ,
                tcp_vX0, tcp_vX1, tcp_vX2, tcp_vY0, tcp_vY1, tcp_vY2,
                weight,
                cogX, cogY, cogZ);
        }


        public override string ToString()
        {
            return string.Format("Tool[\"{0}\", Tip{1}, Orientation{2}, {3} kg]",
                this.name,
                this.TCPPosition,
                this.TCPOrientation,
                this.Weight); 
                //this.centerOfGravity);
        }

        /// <summary>
        /// Converts this Tool object to message-compatible instruction.
        /// </summary>
        /// <returns></returns>
        public string ToInstruction()
        {
            return $@"Tool.Create({this.name}, 
                {this.TCPPosition.X}, {this.TCPPosition.Y}, {this.TCPPosition.Z}, 
                {this.TCPOrientation.XAxis.X}, {this.TCPOrientation.XAxis.Y}, {this.TCPOrientation.XAxis.Z}, 
                {this.TCPOrientation.YAxis.X}, {this.TCPOrientation.YAxis.Y}, {this.TCPOrientation.YAxis.Z}, 
                {this.Weight}, 
                {this.centerOfGravity.X}, {this.centerOfGravity.Y}, {this.centerOfGravity.Z});";
        }

    }
}
