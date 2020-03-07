using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machina.Types.Geometry;

namespace Machina.Descriptors.Components
{
    //  ██████╗  ██████╗ ██████╗  ██████╗ ████████╗  ██╗ ██████╗ ██╗███╗   ██╗████████╗
    //  ██╔══██╗██╔═══██╗██╔══██╗██╔═══██╗╚══██╔══╝  ██║██╔═══██╗██║████╗  ██║╚══██╔══╝
    //  ██████╔╝██║   ██║██████╔╝██║   ██║   ██║     ██║██║   ██║██║██╔██╗ ██║   ██║   
    //  ██╔══██╗██║   ██║██╔══██╗██║   ██║   ██║██   ██║██║   ██║██║██║╚██╗██║   ██║   
    //  ██║  ██║╚██████╔╝██████╔╝╚██████╔╝   ██║╚█████╔╝╚██████╔╝██║██║ ╚████║   ██║   
    //  ╚═╝  ╚═╝ ╚═════╝ ╚═════╝  ╚═════╝    ╚═╝ ╚════╝  ╚═════╝ ╚═╝╚═╝  ╚═══╝   ╚═╝   
    //                                                                                 
    /// <summary>
    /// Represents a physical Joint between two axes.
    /// </summary>
    internal class RobotJoint
    {
        /// <summary>
        /// The base plane for this joint's transformation (linear or rotary).
        /// </summary>
        internal Matrix4x4 BasePlane { get; set; }

        /// <summary>
        /// The end plane this joint transforms. 
        /// </summary>
        internal Matrix4x4 TransformedPlane { get; set; }

        /// <summary>
        /// Is this a linear a revolution joint?
        /// </summary>
        internal RobotJointType RobotJointType { get; set; }

        /// <summary>
        /// The linear/angular limitations for this joint.
        /// </summary>
        internal Interval JointRange { get; set; }

        /// <summary>
        /// Max joint speed in degrees/sec or mm/sec.
        /// </summary>
        internal double MaxSpeed { get; set; }
        
        // mesh if applicable?
    }
}
