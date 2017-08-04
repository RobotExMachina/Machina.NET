using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    /// <summary>
    /// Represents a tool object that can be attached to the end effector of the robot.
    /// This class is public and will be used directly by the user, so careful design of the API
    /// vs. internal methods will be relevant. 
    /// </summary>
    public class Tool
    {
        public string name { get; internal set; }
        public Point TCPPosition { get; internal set; }
        public Orientation TCPOrientation { get; internal set; }

        public double weight { get; internal set; }
        public Vector centerOfGravity { get; internal set; }

        // For the time being, tools will be defined through position (first) and orientation
        internal bool translationFirst = true;

        /// <summary>
        /// Create a new Tool object by defining the Position and Orientation of the 
        /// Tool Center Point (TCP) relative to the Tool's base coordinate system. 
        /// In other words, if the Tool gets attached to the robot flange in 
        /// XYZ [0, 0, 0], where is the tooltip and how is it oriented?
        /// </summary>
        /// <param name="TCPPosition"></param>
        /// <param name="TCPOrientation"></param>
        public Tool(string name, Point TCPPosition, Orientation TCPOrientation)
        {
            this.name = name;
            this.TCPPosition = TCPPosition;
            this.TCPOrientation = TCPOrientation;
            this.weight = 1;
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
        public Tool(string name, Point TCPPosition, Orientation TCPOrientation, double weightKg, Point centerOfGravity)
        {
            this.name = name;
            this.TCPPosition = TCPPosition;
            this.TCPOrientation = TCPOrientation;
            this.weight = weightKg;
            this.centerOfGravity = centerOfGravity;
        }


        public override string ToString()
        {
            return string.Format("Tool[\"{0}\", {1}, {2}, {3} kg, {4}]",
                this.name,
                this.TCPPosition,
                this.TCPOrientation,
                this.weight, 
                this.centerOfGravity);
        }

    }
}
