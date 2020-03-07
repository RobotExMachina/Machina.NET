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

    /// <summary>
    /// Defines the type of RobotJoint
    /// </summary>
    public enum RobotJointType {
        /// <summary>
        /// Joint moves linearly along Z axis.
        /// </summary>
        Linear,

        /// <summary>
        /// Joint rotates around Z axis.
        /// </summary>
        Revolute
    };
    
    internal class RobotJoint
    {
        /// <summary>
        /// The base plane for this joint's transformation (linear or rotary).
        /// </summary>
        Matrix4x4 BasePlane { get; set; }
        
        /// <summary>
        /// The end plane this joint transforms. 
        /// </summary>
        Matrix4x4 TransformedPlane { get; set; }

        /// <summary>
        /// Is this a linear a revolution joint?
        /// </summary>
        RobotJointType RobotJointType { get; set; }
        
        // mesh if applicable?
    }
}
