using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace Machina
{
    //  ██████╗  █████╗ ███████╗███████╗
    //  ██╔══██╗██╔══██╗██╔════╝██╔════╝
    //  ██████╔╝███████║███████╗█████╗  
    //  ██╔══██╗██╔══██║╚════██║██╔══╝  
    //  ██████╔╝██║  ██║███████║███████╗
    //  ╚═════╝ ╚═╝  ╚═╝╚══════╝╚══════╝
    //                                  
    /// <summary>
    /// A class to handle communication with external controllers, real or virtual
    /// </summary>
    abstract class Driver
    {
        /// <summary>
        /// A reference to parent Machina Control object commanding this Comm.
        /// </summary>
        protected Control masterControl = null;

        //// ADDED A WRITE ROBOT CURSOR IN THE ABB OBJECT
        ///// <summary>
        ///// A reference to the shared streamQueue object
        ///// </summary>
        //protected StreamQueue streamQueue = null;
        //public abstract RobotCursor writeCursor = null;

        public abstract RobotCursor WriteCursor { get; set; }


        /// <summary>
        /// Is the connection to the controller fully operative?
        /// </summary>
        protected bool isConnected = false;
        
        /// <summary>
        /// Is the device currently running a program?
        /// </summary>
        protected bool isRunning = true;
        protected string IP = "";



        //  ███████╗██╗ ██████╗ ███╗   ██╗ █████╗ ████████╗██╗   ██╗██████╗ ███████╗███████╗
        //  ██╔════╝██║██╔════╝ ████╗  ██║██╔══██╗╚══██╔══╝██║   ██║██╔══██╗██╔════╝██╔════╝
        //  ███████╗██║██║  ███╗██╔██╗ ██║███████║   ██║   ██║   ██║██████╔╝█████╗  ███████╗
        //  ╚════██║██║██║   ██║██║╚██╗██║██╔══██║   ██║   ██║   ██║██╔══██╗██╔══╝  ╚════██║
        //  ███████║██║╚██████╔╝██║ ╚████║██║  ██║   ██║   ╚██████╔╝██║  ██║███████╗███████║
        //  ╚══════╝╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚═╝  ╚═╝   ╚═╝    ╚═════╝ ╚═╝  ╚═╝╚══════╝╚══════╝
        //                                                                                  
        /// <summary>
        /// Reverts the Comm object to a blank state before any connection attempt, objects retrieved, subscriptions, etc,
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Performs all necessary operations for a successful real-time connection to a real/virtual device.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public abstract bool ConnectToDevice(int deviceId);

        /// <summary>
        /// Performs all necessary operations and disposals for a full disconnection (and reset) from a real/virtual device.
        /// </summary>
        /// <returns></returns>
        public abstract bool DisconnectFromDevice();

        /// <summary>
        /// Sets the execution mode on the device to once or loop (useful for ControlMode.Execute)
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public abstract bool SetRunMode(CycleType mode);

        /// <summary>
        /// Loads a program to the device.
        /// </summary>
        /// <param name="dirname"></param>
        /// <param name="filename"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public abstract bool LoadProgramToController(string dirname, string filename, string extension);

        /// <summary>
        /// Loads a program to the device.
        /// </summary>
        /// <param name="program"></param>
        /// <returns></returns>
        public abstract bool LoadProgramToController(List<string> program);

        /// <summary>
        /// Request the start of the program loaded on the device.
        /// </summary>
        /// <returns></returns>
        public abstract bool StartProgramExecution();

        /// <summary>
        /// Request immediate or deferred stop of the program running on the device.
        /// </summary>
        /// <returns></returns>
        public abstract bool StopProgramExecution(bool immediate);

        /// <summary>
        /// Returns a Vector object representing the current robot's TCP position.
        /// </summary>
        /// <returns></returns>
        public abstract Vector GetCurrentPosition();

        /// <summary>
        /// Returns a Rotation object representing the current robot's TCP orientation.
        /// </summary>
        /// <returns></returns>
        public abstract Rotation GetCurrentOrientation();

        /// <summary>
        /// Returns a Joints object representing the rotations of the 6 axes of this robot.
        /// </summary>
        /// <returns></returns>
        public abstract Joints GetCurrentJoints();

        ///// <summary>
        ///// Returns a Frame object representing the current robot's TCP position and orientation. 
        ///// NOTE: the Frame object's velocity and zone still do not represent the acutal state of the robot.
        ///// </summary>
        ///// <returns></returns>
        //public abstract Frame GetCurrentFrame();

        /// <summary>
        /// Ticks the queue manager and potentially triggers streaming of targets to the controller.
        /// </summary>
        /// <param name="priority"></param>
        public abstract void TickStreamQueue(bool priority);

        /// <summary>
        /// Dumps a bunch of info to the console.
        /// </summary>
        public abstract void DebugDump();


        // Base constructor
        public Driver(Control ctrl)
        {
            masterControl = ctrl;
            Reset();
        }

        //public void LinkStreamQueue(StreamQueue q)
        //{
        //    streamQueue = q;
        //}
        public void LinkWriteCursor(ref RobotCursor wc)
        {
            WriteCursor = wc;
        }

        public bool IsConnected()
        {
            return isConnected;
        }

        public bool IsRunning()
        {
            return isRunning;
        }

        public string GetIP()
        {
            return IP;
        }

        /// <summary>
        /// Saves a string representation of a program to a local file. 
        /// </summary>
        /// <param name="module"></param>
        /// <param name="filepath"></param>
        protected bool SaveProgramToFilename(List<string> module, string filepath)
        {
            try
            {
                System.IO.File.WriteAllLines(filepath, module, System.Text.Encoding.ASCII);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not save module to file...");
                Console.WriteLine(ex);
            }
            return false;
        }

    }

    
}
