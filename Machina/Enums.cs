using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    /// <summary>
    /// Represents the type of control that will be performed over the real/virtual robot.
    /// </summary>
    public enum ControlType : int
    {
        /// <summary>
        /// Not connected to any controller. Useful for robot code generation and export.
        /// </summary>
        Offline = 0,

        /// <summary>
        /// Online connection to a controller, the library will upload complete programs 
        /// and run them. Provides robust and fluid movement, useful on real-time 
        /// interactivity where response time is not a priority. 
        /// </summary>
        Execute = 1,

        /// <summary>
        /// Online connection to a controller, the library will stream individual targets
        /// at run time as they get priority. Provides the closest approximation to real-time
        /// interaction, useful on situations where low latency is required.
        /// </summary>
        Stream = 2
    }

    /// <summary>
    /// Defines the different cycle type modes a program can be ran.
    /// </summary>
    public enum CycleType : int
    {
        None = 0,
        Once = 1,
        Loop = 2
    }

    /// <summary>
    /// Defines which reference coordinate system to use for transform actions.
    /// </summary>
    public enum ReferenceCS : int
    {
        World = 0,
        Local = 1
    }

    /// <summary>
    /// Defines which type of motion to use for translation actions.
    /// </summary>
    public enum MotionType
    {
        Linear,
        Joint
    }

    /// <summary>
    /// Defines the type of robotic device.
    /// </summary>
    public enum RobotType
    {
        Undefined,
        HUMAN,
        ABB,
        UR,
        KUKA,
        ZMORPH,
    }

    ///// <summary>
    ///// Defines if the parameters for new Actions will be considered in absolute values 
    ///// or relative increments.
    ///// </summary>
    //public enum ActionModes
    //{
    //    Absolute, 
    //    Relative
    //}

    /// <summary>
    /// An enum with different robotic parts, to be used as targets for execution operations, 
    /// e.g. 3D printing, I/O, etc.
    /// @TODO: temp, this should probably go somewhere else... 
    /// </summary>
    public enum RobotPartType
    {
        Extruder,
        Bed,
        Chamber
    }
}
