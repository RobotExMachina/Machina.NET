using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machina.Users;
using Machina.Types.Geometry;


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
        internal Control parentControl = null;


        /// <summary>
        /// A reference to the shared Write RobotCursor object
        /// </summary>
        public RobotCursor ReleaseCursor
        {
            get { return _releaseCursor; }
            set { _releaseCursor = value; }
        }
        private RobotCursor _releaseCursor;
                
        
        /// <summary>
        /// Is connected to device?
        /// </summary>
        public bool Connected
        {
            get { return _connected; }
            internal set { _connected = value; }
        }
        private bool _connected = false;

        /// <summary>
        /// Device's IP
        /// </summary>
        public string IP
        {
            get { return _ip; }
            internal set { _ip = value; }
        }
        private string _ip = "";

        /// <summary>
        /// Device's port
        /// </summary>
        public int Port
        {
            get { return _port; }
            internal set { _port = value; }
        }
        private int _port;


        /// <summary>
        /// The User profile used to log into the controller
        /// </summary>
        public User User
        {
            get { return _user; }
            set { _user = value; }
        }
        private User _user = new User();
        
        public abstract Dictionary<ConnectionType, bool> AvailableConnectionTypes { get; }



        /// <summary>
        /// Create a new instance of a Driver object given a Controller.
        /// </summary>
        /// <param name="ctrl"></param>
        public Driver(Control ctrl)
        {
            this.parentControl = ctrl;
            //Reset();
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
        /// Returns the driver modules necessary to run on this device for Machina to talk to it.
        /// Takes a dictionary of values to be replaced on the modules, such as {"IP","192.168.125.1"} or {"PORT","7000"}.
        /// Returns a dict with filename-file pairs. 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public abstract Dictionary<string, string> GetDeviceDriverModules(Dictionary<string, string> parameters);

        /// <summary>
        /// Performs all necessary operations for a successful real-time connection to a real/virtual device.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public abstract bool ConnectToDevice(int deviceId);

        /// <summary>
        /// Performs all necessary operations for a successful real-time connection to a real/virtual device.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public abstract bool ConnectToDevice(string ip, int port);

        /// <summary>
        /// Performs all necessary operations and disposals for a full disconnection (and reset) from a real/virtual device.
        /// </summary>
        /// <returns></returns>
        public abstract bool DisconnectFromDevice();

        public abstract bool Dispose();



        /// <summary>
        /// Sets the execution mode on the device to once or loop (useful for ControlMode.Execute)
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public abstract bool SetRunMode(CycleType mode);

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

        /// <summary>
        /// Returns a ExternalAxes object representing the rotations/translations of the 6 external axes of this robot.
        /// </summary>
        /// <returns></returns>
        public abstract ExternalAxes GetCurrentExternalAxes();

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
        /// Change the user profile usedfor logging operations.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        internal virtual bool SetUser(string name, string password)
        {
            this.User = new User(name, password);
            return true;
        }

        internal virtual bool ConfigureBuffer(int minActions, int maxActions)
        {
            return false;
        }




        //public void LinkStreamQueue(StreamQueue q)
        //{
        //    streamQueue = q;
        //}

        public void LinkWriteCursor(RobotCursor wc)
        {
            ReleaseCursor = wc;
        }


    }


}
