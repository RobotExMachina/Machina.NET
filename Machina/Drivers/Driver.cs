using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace Machina.Drivers
{
    //  ██████╗ ██████╗ ██╗██╗   ██╗███████╗██████╗ 
    //  ██╔══██╗██╔══██╗██║██║   ██║██╔════╝██╔══██╗
    //  ██║  ██║██████╔╝██║██║   ██║█████╗  ██████╔╝
    //  ██║  ██║██╔══██╗██║╚██╗ ██╔╝██╔══╝  ██╔══██╗
    //  ██████╔╝██║  ██║██║ ╚████╔╝ ███████╗██║  ██║
    //  ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═══╝  ╚══════╝╚═╝  ╚═╝
    //                                              
    /// <summary>
    /// A class to handle communication with external controllers, real or virtual
    /// </summary>
    abstract class Driver
    {
        /// <summary>
        /// A reference to parent Machina Control object commanding this Driver.
        /// </summary>
        protected Control masterControl = null;

        //// ADDED A WRITE ROBOT CURSOR IN THE ABB OBJECT
        ///// <summary>
        ///// A reference to the shared streamQueue object
        ///// </summary>
        //protected StreamQueue streamQueue = null;
        //public abstract RobotCursor writeCursor = null;

        /// <summary>
        /// A reference to the shared Write RobotCursor object
        /// </summary>
        private RobotCursor _writeCursor;

        /// <summary>
        /// A reference to the shared streamQueue object
        /// </summary>
        public RobotCursor WriteCursor
        {
            get { return _writeCursor; }
            set { _writeCursor = value; }
        }
        
        

        private bool _connected = false;
        public bool Connected
        {
            get { return _connected; }
            internal set { _connected = value; }
        }

        private string _ip = "";
        public string IP
        {
            get { return _ip; }
            internal set { _ip = value; }
        }

        private int _port = 0;  // @TODO: figure this out as an input somewhere
        public int Port
        {
            get { return _port; }
            internal set { _port = value; }
        }


        


        //  ┌─┐┬┌─┐┌┐┌┌─┐┌┬┐┬ ┬┬─┐┌─┐┌─┐
        //  └─┐││ ┬│││├─┤ │ │ │├┬┘├┤ └─┐
        //  └─┘┴└─┘┘└┘┴ ┴ ┴ └─┘┴└─└─┘└─┘
        //
        /// <summary>
        /// Reverts the Driver object to a blank state before any connection attempt, objects retrieved, subscriptions, etc,
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Performs all necessary operations for a successful real-time connection to a real/virtual device.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public abstract bool ConnectToDevice(int deviceId);

        public abstract bool ConnectToDevice(string ip, int port);

        /// <summary>
        /// Performs all necessary operations and disposals for a full disconnection (and reset) from a real/virtual device.
        /// </summary>
        /// <returns></returns>
        public abstract bool DisconnectFromDevice();

        ///// <summary>
        ///// Sets the execution mode on the device to once or loop (useful for ControlMode.Execute)
        ///// </summary>
        ///// <param name="mode"></param>
        ///// <returns></returns>
        //public abstract bool SetRunMode(CycleType mode);

        ///// <summary>
        ///// Loads a program to the device from a file in the system.
        ///// </summary>
        ///// <param name="fullPath"></param>
        ///// <param name="wipeout"></param>
        ///// <returns></returns>
        //public abstract bool LoadFileToDevice(string fullPath, bool wipeout);

        ///// <summary>
        ///// Loads a program to the device from a list of lines of code as strings.
        ///// </summary>
        ///// <param name="program"></param>
        ///// <param name="programName"></param>
        ///// <returns></returns>
        //public abstract bool LoadProgramToController(List<string> program, string programName);

        ///// <summary>
        ///// Request the start of the program loaded on the device.
        ///// </summary>
        ///// <returns></returns>
        //public abstract bool StartProgramExecution();

        ///// <summary>
        ///// Request immediate or deferred stop of the program running on the device.
        ///// </summary>
        ///// <returns></returns>
        //public abstract bool StopProgramExecution(bool immediate);

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
        ///// Ticks the queue manager and potentially triggers streaming of targets to the controller.
        ///// </summary>
        ///// <param name="priority"></param>
        //public abstract void TickStreamQueue(bool priority);

        /// <summary>
        /// Dumps a bunch of info to the console.
        /// </summary>
        public abstract void DebugDump();


        /// <summary>
        /// Create a new instance of a Driver object given a Controller.
        /// </summary>
        /// <param name="ctrl"></param>
        public Driver(Control ctrl)
        {
            this.masterControl = ctrl;
            //Reset();
        }

        //public void LinkStreamQueue(StreamQueue q)
        //{
        //    streamQueue = q;
        //}

        public void LinkWriteCursor(ref RobotCursor wc)
        {
            WriteCursor = wc;
        }


    }


}
