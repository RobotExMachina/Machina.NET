using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;





namespace RobotControl
{
    /// <summary>
    /// Represents the type of control that will be performed over the real/virtual robot.
    /// </summary>
    public enum ControlMode : int {
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
    };

    /// <summary>
    /// Defines the different modes a program can be ran.
    /// </summary>
    public enum RunMode : int { None = 0, Once = 1, Loop = 2 };
    


    //██████╗  ██████╗ ██████╗  ██████╗ ████████╗
    //██╔══██╗██╔═══██╗██╔══██╗██╔═══██╗╚══██╔══╝
    //██████╔╝██║   ██║██████╔╝██║   ██║   ██║   
    //██╔══██╗██║   ██║██╔══██╗██║   ██║   ██║   
    //██║  ██║╚██████╔╝██████╔╝╚██████╔╝   ██║   
    //╚═╝  ╚═╝ ╚═════╝ ╚═════╝  ╚═════╝    ╚═╝   
    /// <summary>
    /// The core Class in RobotControl. Represents a state & action-based virtual robot, 
    /// and exposes the public API for robot manipulation and control.
    /// </summary>
    public class Robot
    {
        /// <summary>
        /// Build number.
        /// </summary>
        public static readonly int Build = 1107;

        /// <summary>
        /// The main Control object, acts as an interface to all classes that
        /// manage robot control.
        /// </summary>
        private Control c;  // the main control object

 
        
        //██████╗ ██╗   ██╗██████╗ ██╗     ██╗ ██████╗     █████╗ ██████╗ ██╗
        //██╔══██╗██║   ██║██╔══██╗██║     ██║██╔════╝    ██╔══██╗██╔══██╗██║
        //██████╔╝██║   ██║██████╔╝██║     ██║██║         ███████║██████╔╝██║
        //██╔═══╝ ██║   ██║██╔══██╗██║     ██║██║         ██╔══██║██╔═══╝ ██║
        //██║     ╚██████╔╝██████╔╝███████╗██║╚██████╗    ██║  ██║██║     ██║
        //╚═╝      ╚═════╝ ╚═════╝ ╚══════╝╚═╝ ╚═════╝    ╚═╝  ╚═╝╚═╝     ╚═╝
        /// <summary>
        /// Base constructor.
        /// </summary>                                                       
        public Robot()
        {
            c = new Control();
        }

        /// <summary>
        /// Sets the control mode the robot will operate under.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool ControlMode(ControlMode mode)
        {
            return c.SetControlMode(mode);
        }

        /// <summary>
        /// Sets the control mode the robot will operate under.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool ControlMode(string mode)
        {
            mode = mode.ToLower();
            bool success = true;
            if (mode.Equals("offline"))
            {
                return ControlMode(RobotControl.ControlMode.Offline);
            }
            else if (mode.Equals("execute"))
            {
                return ControlMode(RobotControl.ControlMode.Execute);
            }
            else if (mode.Equals("stream"))
            {
                return ControlMode(RobotControl.ControlMode.Stream);
            }
            else
            {
                Console.WriteLine("ConnectionMode '" + mode + "' is not available.");
                success = false;
            }
            return success;
        }
        
        /// <summary>
        /// Sets the cycle the robot will run program in (Once or Loop).
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool RunMode(RunMode mode)
        {
            return c.SetRunMode(mode);
        }

        /// <summary>
        /// Sets the cycle the robot will run program in (Once or Loop).
        /// </summary>
        /// <param name="mode"></param>
        public bool RunMode(string mode)
        {
            mode = mode.ToLower();

            if (mode.Equals("once"))
            {
                return RunMode(RobotControl.RunMode.Once);
            }
            else if (mode.Equals("loop"))
            {
                return RunMode(RobotControl.RunMode.Once);
            }
            else
            {
                Console.WriteLine("RunMode '" + mode + "' is not available.");
            }

            return false;
        }

        /// <summary>
        /// Performs all necessary operations to connect to a robot device, real or virtual.
        /// This is necessary for 'online' modes.
        /// </summary>
        /// <param name="mode">If multiple devices are connected, choose this id from the list.</param>
        /// <returns></returns>
        public bool Connect(int robotId)
        {
            return c.ConnectToDevice(robotId);
        }

        /// <summary>
        /// Performs all necessary operations to connect to the first robot device found on the network, real or virtual.
        /// This is necessary for 'online' modes.
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            return c.ConnectToDevice(0);
        }

        /// <summary>
        /// Performs all necessary instructions to disconnect from and dispose a robot device, real or virtual. 
        /// This is necessary before leaving current execution thread.
        /// </summary>
        public bool Disconnect()
        {
            return c.DisconnectFromDevice();
        }

        /// <summary>
        /// Returns a string representation of the IP of the currently connected robot device.
        /// </summary>
        /// <returns></returns>
        public string GetIP()
        {
            return c.GetControllerIP();
        }

        /// <summary>
        /// Loads a program to the robot from a local file.
        /// </summary>
        /// <param name="filepath">Full absolute filepath including root, directory structure, filename and extension.</param>
        /// <returns></returns>
        public bool LoadProgram(string filepath)
        {
            return c.LoadProgramToDevice(filepath);
        }

        /// <summary>
        /// Loads a program to the robot from a string list of code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool LoadProgram(List<string> code)
        {
            return c.LoadProgramToDevice(code);
        }

        /// <summary>
        /// Starts execution of the current module/s in the controller.
        /// @TODO: The behavior of this method will change depending based on Off/Online mode
        /// </summary>
        public bool Start()
        {
            return c.StartProgramOnDevice();
        }

        /// <summary>
        /// Immediately stops execution of the current program/s in the connected robot. 
        /// </summary>
        public bool Stop()
        {
            return c.StopProgramOnDevice(true);
        }

        /// <summary>
        /// Returns a Point representation of the Robot's TCP position in mm and World coordinates.
        /// </summary>
        /// <returns></returns>
        public Point GetCurrentPosition()
        {
            return c.GetCurrentPosition();
        }

        /// <summary>
        /// Returns a Rottaion representation of the Robot's TCP orientation in quaternions.
        /// </summary>
        /// <returns></returns>
        public Rotation GetCurrentOrientation()
        {
            return c.GetCurrentOrientation();
        }

        /// <summary>
        /// Returns a Frame object representing the Robot's current TCP position and orientation.
        /// NOTE: the Frame's velocity and zone are still not representative of the current state.
        /// </summary>
        /// <returns></returns>
        public Frame GetCurrentFrame()
        {
            return c.GetCurrentFrame();
        }

        /// <summary>
        /// Returns a Joint object representing the current angular rotations of the robot's 6 axes.
        /// </summary>
        /// <returns></returns>
        public Joints GetCurrentJoints()
        {
            return c.GetCurrentJoints();
        }

        /// <summary>
        /// Loads the path to the queue manager and triggers execution of the program if applicable.
        /// </summary>
        /// <param name="path"></param>
        public void LoadPath(Path path)
        {
            c.AddPathToQueue(path);
        }

        /// <summary>
        /// Stops the robot after execution of current program. This will also clear the queue.
        /// </summary>
        public void StopAfterProgram()
        {
            c.ClearQueue();
            c.StopProgramOnDevice(false);
        }

        public bool Export(string filepath)
        {
            return c.Export(filepath);
        }





        //  ███████╗███████╗████████╗████████╗██╗███╗   ██╗ ██████╗ ███████╗
        //  ██╔════╝██╔════╝╚══██╔══╝╚══██╔══╝██║████╗  ██║██╔════╝ ██╔════╝
        //  ███████╗█████╗     ██║      ██║   ██║██╔██╗ ██║██║  ███╗███████╗
        //  ╚════██║██╔══╝     ██║      ██║   ██║██║╚██╗██║██║   ██║╚════██║
        //  ███████║███████╗   ██║      ██║   ██║██║ ╚████║╚██████╔╝███████║
        //  ╚══════╝╚══════╝   ╚═╝      ╚═╝   ╚═╝╚═╝  ╚═══╝ ╚═════╝ ╚══════╝
        //                                                                  
        /// <summary>
        /// Sets the default velocity new actions will be run at.
        /// </summary>
        /// <param name="vel"></param>
        public void SetVelocity(int vel)
        {
            c.SetCurrentVelocity(vel);
        }

        /// <summary>
        /// Sets the default zone value new actions will be given.
        /// </summary>
        /// <param name="zone"></param>
        public void SetZone(int zone)
        {
            c.SetCurrentZone(zone);
        }

        /// <summary>
        /// Sets the motion type (linear, joint...) for future issued actions.
        /// </summary>
        /// <param name="type"></param>
        public void SetMotionType(MotionType type)
        {
            c.SetCurrentMotionType(type);
        }

        /// <summary>
        /// Sets the motion type (linear, joint...) for future issued actions.
        /// </summary>
        /// <param name="type">"linear", "joint" or "joints"</param>
        public void SetMotionType(string type)
        {
            MotionType t = MotionType.Undefined;
            type = type.ToLower();
            if (type.Equals("linear")) {
                t = MotionType.Linear;
            }
            else if (type.Equals("joint"))
            {
                t = MotionType.Joint;
            }
            else if (type.Equals("joints"))
            {
                t = MotionType.Joints;
            }

            if (t == MotionType.Undefined)
            {
                Console.WriteLine("Invalid motion type");
                return;
            }

            SetMotionType(t);
        }

        public void PushSettings()
        {
            c.PushCurrentSettings();
        }

        public void PopSettings()
        {
            c.PopCurrentSettings();
        }



        //   █████╗  ██████╗████████╗██╗ ██████╗ ███╗   ██╗███████╗
        //  ██╔══██╗██╔════╝╚══██╔══╝██║██╔═══██╗████╗  ██║██╔════╝
        //  ███████║██║        ██║   ██║██║   ██║██╔██╗ ██║███████╗
        //  ██╔══██║██║        ██║   ██║██║   ██║██║╚██╗██║╚════██║
        //  ██║  ██║╚██████╗   ██║   ██║╚██████╔╝██║ ╚████║███████║
        //  ╚═╝  ╚═╝ ╚═════╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝
        //                                                         
        
        /// <summary>
        /// Issue a relative movement action request.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool Move(Point direction)
        {
            return c.IssueTranslationRequest(direction, true);
        }

        /// <summary>
        /// Issue a relative movement action request.
        /// </summary>
        /// <param name="incX"></param>
        /// <param name="incY"></param>
        /// <param name="incZ"></param>
        /// <returns></returns>
        public bool Move(double incX, double incY, double incZ)
        {
            return Move(new Point(incX, incY, incZ));
        }

        /// <summary>
        /// Issue an absolute movement action request.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool MoveTo(Point position)
        {
            return c.IssueTranslationRequest(position, false);
        }

        /// <summary>
        /// Issue an absolute movement action request.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public bool MoveTo(double x, double y, double z)
        {
            return MoveTo(new Point(x, y, z));
        }

        /// <summary>
        /// Issue an absolute movement action request to a tagged position. 
        /// </summary>
        /// <param name="bookmarkedPosition"></param>
        /// <returns></returns>
        public bool MoveTo(string bookmarkedPosition)
        {
            throw new NotImplementedException();
        }

        // @TODO: add overloads with custom velocity and speed?





        public bool Rotate(Rotation rotation)
        {
            throw new NotImplementedException();
        }

        public bool Rotate(double q1, double q2, double q3, double q4)
        {
            throw new NotImplementedException();
        }

        
        public bool Rotate(
            double x0, double x1, double x2, 
            double y0, double y1, double y2, 
            double z0, double z1, double z2)
        {
            throw new NotImplementedException();
        }

        public bool Rotate(Point vecX, Point vecY, Point vecZ)
        {
            throw new NotImplementedException();
        }





        public bool RotateTo(Rotation rotation)
        {
            return c.IssueRotationRequest(rotation, false);
        }

        public bool RotateTo(double w, double x, double y, double z)
        {
            return RotateTo( new Rotation(w, x, y, z));
        }

        public bool RotateTo(
            double x0, double x1, double x2,
            double y0, double y1, double y2)
        {
            return RotateTo( new Rotation(x0, x1, x2, y0, y1, y2) );
        }

        //public bool RotateTo(Point vecX, Point vecY)
        //{
        //    throw new NotImplementedException();
        //}

        //public bool RotateTo(string bookmarkedRotation)
        //{
        //    throw new NotImplementedException();
        //}

        // @TODO: add overloads with custom velocity and speed?




        ///// <summary>
        ///// Issue an absolute rotation of the TCP action request.
        ///// </summary>
        ///// <param name="q1"></param>
        ///// <param name="q2"></param>
        ///// <param name="q3"></param>
        ///// <param name="q4"></param>
        ///// <returns></returns>
        //public bool RotateTo(double q1, double q2, double q3, double q4)
        //{
        //    return c.IssueAbsoluteRotationRequest(q1, q2, q3, q4);
        //}

        ///// <summary>
        ///// Issue an absolute rotation of the TCP action request.
        ///// </summary>
        ///// <param name="x1"></param>
        ///// <param name="x2"></param>
        ///// <param name="x3"></param>
        ///// <param name="y1"></param>
        ///// <param name="y2"></param>
        ///// <param name="y3"></param>
        ///// <param name="z1"></param>
        ///// <param name="z2"></param>
        ///// <param name="z3"></param>
        ///// <returns></returns>
        //public bool RotateTo(double x1, double x2, double x3, double y1, double y2, double y3, double z1, double z2, double z3)
        //{
        //    List<double> q = Rotation.PlaneToQuaternions(x1, x2, x3, y1, y2, y3, z1, z2, z3);
        //    return RotateTo(q[0], q[1], q[2], q[3]);
        //}

        ///// <summary>
        ///// Issue an absolute rotation of the TCP action request.
        ///// </summary>
        ///// <param name="rot"></param>
        ///// <returns></returns>
        //public bool RotateTo(Rotation rot)
        //{
        //    return RotateTo(rot.Q1, rot.Q2, rot.Q3, rot.Q4);
        //}

        ///// <summary>
        ///// Issue an absolute rotation of the TCP action request.
        ///// </summary>
        ///// <param name="bookmarkRotation"></param>
        ///// <returns></returns>
        //public bool RotateTo(string bookmarkRotation)
        //{
        //    string str = bookmarkRotation.ToLower();

        //    if (str.Equals("globalxy"))
        //    {
        //        return RotateTo(Rotation.GlobalXY);
        //    }
        //    else
        //    {
        //        Console.WriteLine("Named rotation '{0}' not found", bookmarkRotation);
        //    }

        //    return false;
        //}

        


        // @TODO: not quite sure yet about how compound relative transformations should work, since sequence order matters a lot here
        
        public bool TransformTo(Point position, Rotation rotation)
        {
            throw new NotImplementedException();
        }       

        public bool TransformTo(double x, double y, double z, double q1, double q2, double q3, double q4)
        {
            throw new NotImplementedException();
        }






        public bool Joints(Joints incJoints)
        {
            throw new NotImplementedException();
        }

        public bool Joints(double incJ1, double incJ2, double incJ3, double incJ4, double incJ5, double incJ6)
        {
            throw new NotImplementedException();
        }

        public bool JointsTo(Joints joints)
        {
            throw new NotImplementedException();
        }

        public bool JointsTo(double j1, double j2, double j3, double j4, double j5, double j6)
        {
            throw new NotImplementedException();
        }





        // @TODO: feels like Path is too much of a spatial geometry description. 
        // Should it become something more action-related? Like Instructions, Program, Commands?
        public bool Follow(Path path)
        {
            throw new NotImplementedException();
        }

        





        public bool Wait(long millis)
        {
            throw new NotImplementedException();
        }














        //  ██████╗ ███████╗██████╗ ██╗   ██╗ ██████╗ 
        //  ██╔══██╗██╔════╝██╔══██╗██║   ██║██╔════╝ 
        //  ██║  ██║█████╗  ██████╔╝██║   ██║██║  ███╗
        //  ██║  ██║██╔══╝  ██╔══██╗██║   ██║██║   ██║
        //  ██████╔╝███████╗██████╔╝╚██████╔╝╚██████╔╝
        //  ╚═════╝ ╚══════╝╚═════╝  ╚═════╝  ╚═════╝ 
        //                                            
        /// <summary>
        /// Dumps a bunch of information to the console about the controller, the main task, etc.
        /// </summary>
        public void DebugDump()
        {
            c.DebugDump();
        }

        /// <summary>
        /// Dumps a list of the remaining buffered actions.
        /// </summary>
        public void DebugBuffer()
        {
            c.DebugBuffer();
        }

        /// <summary>
        /// Dumps the state of the internal RobotPointers
        /// </summary>
        public void DebugRobotCursors()
        {
            c.DebugRobotCursors();
        }

        public void DebugSettingsBuffer()
        {
            c.DebugSettingsBuffer();
        }


    }
}
