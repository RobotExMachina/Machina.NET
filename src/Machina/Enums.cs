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
        Offline,

        /// <summary>
        /// Online connection to a controller, the library will upload complete programs 
        /// and run them. Provides robust and fluid movement, useful on real-time 
        /// interactivity where response time is not a priority. 
        /// </summary>
        Execute,

        /// <summary>
        /// Online connection to a controller, the library will stream individual targets
        /// at run time as they get priority. Provides the closest approximation to real-time
        /// interaction, useful on situations where low latency is required.
        /// </summary>
        Stream
    }

    /// <summary>
    /// Defines the different cycle type modes a program can be ran.
    /// </summary>
    public enum CycleType : int
    {
        /// <summary>
        /// It will not run.
        /// </summary>
        None,

        /// <summary>
        /// Program will be executed once.
        /// </summary>
        Once,

        /// <summary>
        /// Program will be executed in a loop.
        /// </summary>
        Loop
    }

    /// <summary>
    /// Defines which reference coordinate system to use for transform actions.
    /// </summary>
    public enum ReferenceCS : int
    {
        World,
        Local
    }

    /// <summary>
    /// Defines which type of motion to use for translation actions.
    /// </summary>
    public enum MotionType
    {
        /// <summary>
        /// Motion between targets will happen linearly in Euclidean space, 
        /// this is, a straight line in 3D space. 
        /// </summary>
        Linear,

        /// <summary>
        /// USE WITH CAUTION. Motion between targets will hapen linearly in Configuration space, 
        /// this is, a linear interpolation between the joint angular values for 
        /// each target. This is much easier for the robot, and generally avoids some
        /// singularity problems. However, it may produce unpredictable trajectories 
        /// and reorientations, specially between targets far apart. 
        /// </summary>
        Joint
    }

    /// <summary>
    /// Defines the type of robotic device.
    /// </summary>
    public enum RobotType
    {
        HUMAN,
        MACHINA,
        ABB,
        UR,
        KUKA,
        ZMORPH
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

    /// <summary>
    /// Defines who will be in charge of setting up a device for correct connection, 
    /// i.e. having Machina try to load a server/firmata modules to the controller or 
    /// leave that task to the User (default). 
    /// </summary>
    public enum ConnectionType
    {
        User, 
        Machina
    }

    internal enum TCPConnectionStatus
    {
        Connected,
        Disconnected,
        Validated,
        InSession
    }

    /// <summary>
    /// Defines what state is being represented by a cursor.
    /// </summary>
    public enum CursorType
    {
        Virtual, 
        Write, 
        Motion
    }

    /// <summary>
    /// Defines the types of External Axes
    /// </summary>
    public enum ExternalAxisType
    {
        Undefined, 
        Linear, 
        Angular
    }
}
