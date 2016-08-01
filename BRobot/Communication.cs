using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ABB.Robotics;
using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers.RapidDomain;  // This is for the Task Class
using ABB.Robotics.Controllers.EventLogDomain;
using ABB.Robotics.Controllers.FileSystemDomain;


namespace BRobot
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
    abstract class Communication
    {
        /// <summary>
        /// A reference to parent BRobot Control object commanding this Comm.
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
        public abstract bool SetRunMode(RunMode mode);

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
        /// Returns a Point object representing the current robot's TCP position.
        /// </summary>
        /// <returns></returns>
        public abstract Point GetCurrentPosition();

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
        public Communication(Control ctrl)
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





    //   █████╗ ██████╗ ██████╗ 
    //  ██╔══██╗██╔══██╗██╔══██╗
    //  ███████║██████╔╝██████╔╝
    //  ██╔══██║██╔══██╗██╔══██╗
    //  ██║  ██║██████╔╝██████╔╝
    //  ╚═╝  ╚═╝╚═════╝ ╚═════╝ 
    //                          
    class CommunicationABB : Communication
    {
        // ABB stuff and flags
        private Controller controller;
        private ABB.Robotics.Controllers.RapidDomain.Task mainTask;
        public object rapidDataLock = new object();
        private bool isLogged = false;

        private static string localBufferDirname = "C:";                 // Route names to be used for file handling
        private static string localBufferFilename = "buffer";
        private static string localBufferFileExtension = "mod";
        private static string remoteBufferDirectory = "BRobot";
        
        protected RobotCursorABB writeCursor;



        //  ██████╗ ██╗   ██╗██████╗ ██╗     ██╗ ██████╗
        //  ██╔══██╗██║   ██║██╔══██╗██║     ██║██╔════╝
        //  ██████╔╝██║   ██║██████╔╝██║     ██║██║     
        //  ██╔═══╝ ██║   ██║██╔══██╗██║     ██║██║     
        //  ██║     ╚██████╔╝██████╔╝███████╗██║╚██████╗
        //  ╚═╝      ╚═════╝ ╚═════╝ ╚══════╝╚═╝ ╚═════╝
        //                                                                             
        /// <summary>
        /// Main constructor
        /// </summary>
        public CommunicationABB(Control ctrl) : base(ctrl) { }

        public override RobotCursor WriteCursor
        {
            get
            {
                return writeCursor;
            }

            set
            {
                writeCursor = (RobotCursorABB)value;
            }
        }

        /// <summary>
        /// Reverts the Comm object to a blank state before any connection attempt. 
        /// </summary>
        public override void Reset()
        {
            StopProgramExecution(true);
            ReleaseIP();
            LogOff();
            ReleaseMainTask();
            ReleaseController();
            isConnected = false;
        }

        /// <summary>
        /// Performs all necessary actions to establish a connection to a real/virtual device, 
        /// including connecting to the controller, loggin in, checking required states, etc.
        /// </summary>
        /// <param name="deviceId"></param>
        public override bool ConnectToDevice(int deviceId)
        {
            isConnected = false;

            // Connect to the ABB real/virtual controller
            if (!LoadController(deviceId))
            {
                Console.WriteLine("Could not connect to controller");
                Reset();
                return false;
            }

            // Load the controller's IP
            if (!LoadIP())
            {
                Console.WriteLine("Could not find the controller's IP");
                Reset();
                return false;
            }

            // Log on to the controller
            if (!LogOn())
            {
                Console.WriteLine("Could not log on to the controller");
                Reset();
                return false;
            }

            // Is controller in Automatic mode with motors on?
            if (!IsControllerInAutoMode())
            {
                Console.WriteLine("Please set up controller to AUTOMATIC MODE and try again.");
                Reset();
                return false;
            }
            if (!IsControllerMotorsOn())
            {
                Console.WriteLine("Please set up Motors On mode in controller");
                Reset();
                return false;
            }

            // Test if Rapid Mastership is available
            if (!TestMastershipRapid())
            {
                Console.WriteLine("Mastership not available");
                Reset();
                return false;
            }

            // Load main task from the controller
            if (!LoadMainTask())
            {
                Console.WriteLine("Could not load main task");
                Reset();
                return false;
            }

            // Subscribe to relevant events to keep track of robot execution
            if (!SubscribeToEvents())
            {
                Console.WriteLine("Could not subscribe to robot controller events");
                Reset();
                return false;
            }

            // If here, everything went well and successfully connected 
            isConnected = true;

            // If on 'stream' mode, set up stream connection flow
            if (masterControl.GetControlMode() == ControlMode.Stream)
            {
                if (!SetupStreamingMode())
                {
                    Console.WriteLine("Could not initialize 'stream' mode in controller");
                    Reset();
                    return false;
                }
            }

            return isConnected;
        }


        /// <summary>
        /// Forces disconnection from current controller and manages associated logoffs, disposals, etc.
        /// </summary>
        /// <returns></returns>
        public override bool DisconnectFromDevice()
        {           
            Reset();
            return true;
        }

        /// <summary>
        /// Sets the Rapid ExecutionCycle to Once, Forever or None.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public override bool SetRunMode(RunMode mode)
        {
            if (!isConnected)
            {
                Console.WriteLine("Cannot set RunMode, not connected to any controller");
                return false;
            }

            try
            {
                using (Mastership.Request(controller.Rapid))
                {
                    controller.Rapid.Cycle = mode == RunMode.Once ? ExecutionCycle.Once : mode == RunMode.Loop ? ExecutionCycle.Forever : ExecutionCycle.None;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error setting RunMode in controller...");
                Console.WriteLine(ex);
            }
  
            return false;
        }

        /// <summary>
        /// Loads a module to the ABB controller given as a string list of Rapid code lines.
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public override bool LoadProgramToController(List<string> module)
        {
            if (!isConnected)
            {
                Console.WriteLine("Cannot load program, not connected to controller");
                return false;
            }

            // Modules can only be uploaded to ABB controllers using a file
            if (!SaveProgramToFilename(module, string.Format(@"{0}\{1}.{2}", localBufferDirname, localBufferFilename, localBufferFileExtension)))
            {
                Console.WriteLine("Could not save module to file");
                return false;
            }

            if (!LoadProgramToController(localBufferDirname, localBufferFilename, localBufferFileExtension))
            {
                Console.WriteLine("Could not load module to controller");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads a module into de controller from a local file. 
        /// @TODO: This is an expensive operation, should probably become threaded. 
        /// @TODO: By default, wipes out all previous modules --> parameterize.
        /// </summary>
        /// <param name="dirname"></param>
        /// <returns></returns>
        public override bool LoadProgramToController(string dirname, string filename, string extension)
        {
            // When connecting to a real controller, the reference filesystem 
            // for Task.LoadModuleFromFile() becomes the controller's, so it is necessary
            // to copy the file to the system first, and then load it. 
            //string fullPath = dirname + "\\" + filename + "." ;
            string fullPath = string.Format(@"{0}\{1}.{2}", dirname, filename, extension);

            if (!isConnected)
            {
                Console.WriteLine("Could not load module '{0}', not connected to controller", fullPath);
                return false;
            }

            // check for correct ABB file extension
            if (!extension.ToLower().Equals("mod"))
            {
                Console.WriteLine("Wrong file type, must use .mod files for ABB robots");
                return false;
            }

            // For the time being, we will always wipe out previous modules on load
            if (ClearAllModules() < 0)
            {
                Console.WriteLine("Error clearing modules");
                return false;
            }

            // Load the module
            bool success = false;
            try
            {
                using (Mastership.Request(controller.Rapid))
                {
                    // Create the remoteBufferDirectory if applicable
                    FileSystem fs = controller.FileSystem;
                    string remotePath = fs.RemoteDirectory + "/" + remoteBufferDirectory;
                    bool dirExists = fs.DirectoryExists(remoteBufferDirectory);
                    //Console.WriteLine("Exists? " + dirExists);
                    if (!dirExists)
                    {
                        Console.WriteLine("Creating {0} on remote controller", remotePath);
                        fs.CreateDirectory(remoteBufferDirectory);
                    }
                    
                    //@TODO: Should implement some kind of file cleanup at somepoint...

                    // Copy the file to the remote controller
                    controller.FileSystem.PutFile(fullPath, remoteBufferDirectory + "/" + filename + "." + extension, true);
                    Console.WriteLine("Copied {0} to {1}", filename + "." + extension, remoteBufferDirectory);

                    // Loads a Rapid module to the task in the robot controller
                    success = mainTask.LoadModuleFromFile(remotePath + "/" + filename + "." + extension, RapidLoadMode.Replace);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Could not load module: {0}", ex);
            }

            // True if loading succeeds without any errors, otherwise false.  
            if (!success)
            {
                //// Gets the available categories of the EventLog. 
                //foreach (EventLogCategory category in controller.EventLog.GetCategories())
                //{
                //    if (category.Name == "Common")
                //    {
                //        if (category.Messages.Count > 0)
                //        {
                //            foreach (EventLogMessage message in category.Messages)
                //            {
                //                Console.WriteLine("Program [{1}:{2}({0})] {3} {4}",
                //                    message.Name, message.SequenceNumber,
                //                    message.Timestamp, message.Title, message.Body);
                //            }
                //        }
                //    }
                //}
            }
            else
            {
                Console.WriteLine("Sucessfully loaded {0}", fullPath);
            }

            return success;
        }

        /// <summary>
        /// Requests start executing the program in the main task. Remember to call ResetProgramPointer() before. 
        /// </summary>
        public override bool StartProgramExecution()
        {
            if (!isConnected)
            {
                Console.WriteLine("Cannot start program, not connected to controller");
                return false;
            }

            if (!ResetProgramPointer())
            {
                Console.WriteLine("Cannot start program: cannot reset program pointer");
                return false;
            }

            try
            {
                using (Mastership.Request(controller.Rapid))
                {
                    StartResult res = controller.Rapid.Start(true);
                    if (res != 0)
                    {
                        Console.WriteLine("Cannot start program: {0}", res);
                    }
                    else
                    {
                        isRunning = true;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("PROGRAM START ERROR: " + ex);
            }

            return false;
        }

        /// <summary>
        /// Requests stop executing the program in the main task.
        /// </summary>
        /// <param name="immediate">Stop right now or wait for current cycle to complete?</param>
        /// <returns></returns>
        public override bool StopProgramExecution(bool immediate)
        {
            if (!isConnected)
            {
                Console.WriteLine("Cannot stop program, not connected to controller");
                return false;
            }

            try
            {
                using (Mastership.Request(controller.Rapid))
                {
                    controller.Rapid.Stop(immediate ? StopMode.Immediate : StopMode.Cycle);
                    isRunning = false;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not stop program...");
                Console.WriteLine(ex);
            }

            return false;
        }

        /// <summary>
        /// Returns a Point object representing the current robot's TCP position.
        /// </summary>
        /// <returns></returns>
        public override Point GetCurrentPosition()
        {
            if (!isConnected)
            {
                Console.WriteLine("Cannot GetCurrentPosition, not connected to controller");
                return null;
            }

            RobTarget rt = controller.MotionSystem.ActiveMechanicalUnit.GetPosition(ABB.Robotics.Controllers.MotionDomain.CoordinateSystemType.World);

            return new Point(rt.Trans.X, rt.Trans.Y, rt.Trans.Z);
        }

        /// <summary>
        /// Returns a Rotation object representing the current robot's TCP orientation.
        /// </summary>
        /// <returns></returns>
        public override Rotation GetCurrentOrientation()
        {
            if (!isConnected)
            {
                Console.WriteLine("Cannot GetCurrentRotation, not connected to controller");
                return null;
            }

            RobTarget rt = controller.MotionSystem.ActiveMechanicalUnit.GetPosition(ABB.Robotics.Controllers.MotionDomain.CoordinateSystemType.World);

            // ABB's convention is Q1..Q4 as W..Z
            return new Rotation(rt.Rot.Q1, rt.Rot.Q2, rt.Rot.Q3, rt.Rot.Q4);
        }

        ///// <summary>
        ///// Returns a Frame object representing the current robot's TCP position and orientation. 
        ///// NOTE: the Frame object's velocity and zone still do not represent the acutal state of the robot.
        ///// </summary>
        ///// <returns></returns>
        //public override Frame GetCurrentFrame()
        //{
        //    if (!isConnected)
        //    {
        //        Console.WriteLine("Cannot GetCurrentFrame, not connected to controller");
        //        return null;
        //    }

        //    RobTarget rt = controller.MotionSystem.ActiveMechanicalUnit.GetPosition(ABB.Robotics.Controllers.MotionDomain.CoordinateSystemType.World);

        //    return new Frame(rt.Trans.X, rt.Trans.Y, rt.Trans.Z, rt.Rot.Q1, rt.Rot.Q2, rt.Rot.Q3, rt.Rot.Q4);
        //}

        /// <summary>
        /// Returns a Joints object representing the rotations of the 6 axes of this robot.
        /// </summary>
        /// <returns></returns>
        public override Joints GetCurrentJoints()
        {
            if (!isConnected)
            {
                Console.WriteLine("Cannot GetCurrentJoints, not connected to controller");
                return null;
            }

            JointTarget jt = controller.MotionSystem.ActiveMechanicalUnit.GetPosition();

            return new Joints(jt.RobAx.Rax_1, jt.RobAx.Rax_2, jt.RobAx.Rax_3, jt.RobAx.Rax_4, jt.RobAx.Rax_5, jt.RobAx.Rax_6);
        }

        /// <summary>
        /// This function will look at the state of the program pointer, the streamQueue, 
        /// and if necessary will add a new target to the stream. This is meant to be called
        /// to initiate the stream update chain, like when adding a new target, or pnum event handling.
        /// </summary>
        public override void TickStreamQueue(bool hasPriority)
        {
            //Console.WriteLine("TICKING StreamQueue: {0} targets pending", streamQueue.FramesPending());
            //if (streamQueue.AreFramesPending() && RD_pset[virtualStepCounter % virtualRDCount].StringValue.Equals("FALSE"))
            //{
            //    Console.WriteLine("About to set targets");
            //    SetNextVirtualTarget(hasPriority);
            //    virtualStepCounter++;
            //    TickStreamQueue(hasPriority);  // call this in case there are more in the queue...
            //}
            //else
            //{
            //    Console.WriteLine("Not setting targets, streamQueue.AreFramesPending() {0} RD_pset[virtualStepCounter % virtualRDCount].StringValue.Equals(\"FALSE\") {1}",
            //         streamQueue.AreFramesPending(),
            //        RD_pset[virtualStepCounter % virtualRDCount].StringValue.Equals("FALSE"));
            //}

            Console.WriteLine("TICKING StreamQueue: {0} actions pending", writeCursor.ActionsPending());
            if (writeCursor.AreActionsPending() && RD_pset[virtualStepCounter % virtualRDCount].StringValue.Equals("FALSE"))
            {
                Console.WriteLine("About to set targets");
                SetNextVirtualTarget(hasPriority);
                virtualStepCounter++;
                TickStreamQueue(hasPriority);  // call this in case there are more in the queue...
            }
            else
            {
                Console.WriteLine("Not setting targets, streamQueue.AreFramesPending() {0} RD_pset[{2}].StringValue.Equals(\"FALSE\") {1}",
                     writeCursor.AreActionsPending(),
                    RD_pset[virtualStepCounter % virtualRDCount].StringValue.Equals("FALSE"),
                    virtualStepCounter % virtualRDCount);
            }
        }

        /// <summary>
        /// Dumps a bunch of controller info to the console.
        /// </summary>
        public override void DebugDump()
        {
            if (isConnected)
            {
                Console.WriteLine("");
                Console.WriteLine("DEBUG CONTROLLER DUMP:");
                Console.WriteLine("     AuthenticationSystem: " + controller.AuthenticationSystem.Name);
                Console.WriteLine("     BackupInProgress: " + controller.BackupInProgress);
                Console.WriteLine("     Configuration: " + controller.Configuration);
                Console.WriteLine("     Connected: " + controller.Connected);
                Console.WriteLine("     CurrentUser: " + controller.CurrentUser);
                Console.WriteLine("     DateTime: " + controller.DateTime);
                Console.WriteLine("     EventLog: " + controller.EventLog);
                Console.WriteLine("     FileSystem: " + controller.FileSystem);
                Console.WriteLine("     IOSystem: " + controller.IOSystem);
                Console.WriteLine("     IPAddress: " + controller.IPAddress);
                Console.WriteLine("     Ipc: " + controller.Ipc);
                Console.WriteLine("     IsMaster: " + controller.IsMaster);
                Console.WriteLine("     IsVirtual: " + controller.IsVirtual);
                Console.WriteLine("     MacAddress: " + controller.MacAddress);
                //Console.WriteLine("     MainComputerServiceInfo: ");
                //Console.WriteLine("         BoardType: " + controller.MainComputerServiceInfo.BoardType);
                //Console.WriteLine("         CpuInfo: " + controller.MainComputerServiceInfo.CpuInfo);
                //Console.WriteLine("         RamSize: " + controller.MainComputerServiceInfo.RamSize);
                //Console.WriteLine("         Temperature: " + controller.MainComputerServiceInfo.Temperature);
                Console.WriteLine("     MastershipPolicy: " + controller.MastershipPolicy);
                Console.WriteLine("     MotionSystem: " + controller.MotionSystem);
                Console.WriteLine("     Name: " + controller.Name);
                //Console.WriteLine("     NetworkSettings: " + controller.NetworkSettings);
                Console.WriteLine("     OperatingMode: " + controller.OperatingMode);
                Console.WriteLine("     Rapid: " + controller.Rapid);
                Console.WriteLine("     RobotWare: " + controller.RobotWare);
                Console.WriteLine("     RobotWareVersion: " + controller.RobotWareVersion);
                Console.WriteLine("     RunLevel: " + controller.RunLevel);
                Console.WriteLine("     State: " + controller.State);
                Console.WriteLine("     SystemId: " + controller.SystemId);
                Console.WriteLine("     SystemName: " + controller.SystemName);
                //Console.WriteLine("     TimeServer: " + controller.TimeServer);
                //Console.WriteLine("     TimeZone: " + controller.TimeZone);
                //Console.WriteLine("     UICulture: " + controller.UICulture);
                        
                Console.WriteLine("");
                Console.WriteLine("DEBUG TASK DUMP:");
                Console.WriteLine("    Cycle: " + mainTask.Cycle);
                Console.WriteLine("    Enabled: " + mainTask.Enabled);
                Console.WriteLine("    ExecutionStatus: " + mainTask.ExecutionStatus);
                Console.WriteLine("    ExecutionType: " + mainTask.ExecutionType);
                Console.WriteLine("    Motion: " + mainTask.Motion);
                Console.WriteLine("    MotionPointer: " + mainTask.MotionPointer.Module);
                Console.WriteLine("    Name: " + mainTask.Name);
                Console.WriteLine("    ProgramPointer: " + mainTask.ProgramPointer.Module);
                Console.WriteLine("    RemainingCycles: " + mainTask.RemainingCycles);
                Console.WriteLine("    TaskType: " + mainTask.TaskType);
                Console.WriteLine("    Type: " + mainTask.Type);
                Console.WriteLine("");
            }
        }

        















        //██████╗ ██████╗ ██╗██╗   ██╗ █████╗ ████████╗███████╗
        //██╔══██╗██╔══██╗██║██║   ██║██╔══██╗╚══██╔══╝██╔════╝
        //██████╔╝██████╔╝██║██║   ██║███████║   ██║   █████╗  
        //██╔═══╝ ██╔══██╗██║╚██╗ ██╔╝██╔══██║   ██║   ██╔══╝  
        //██║     ██║  ██║██║ ╚████╔╝ ██║  ██║   ██║   ███████╗
        //╚═╝     ╚═╝  ╚═╝╚═╝  ╚═══╝  ╚═╝  ╚═╝   ╚═╝   ╚══════╝

        /// <summary>
        /// Searches the network for a robot controller and establishes a connection with the specified one by position.
        /// Performs no LogOn actions or similar. 
        /// </summary>
        /// <returns></returns>
        private bool LoadController(int controllerID)
        {

            // Scan the network and hookup to the specified controller
            bool success = false;

            // This is specific to ABB, should become abstracted at some point...
            NetworkScanner scanner = new NetworkScanner();
            ControllerInfo[] controllers = scanner.GetControllers();
            if (controllers.Length > 0)
            {
                int cId = controllerID > controllers.Length ? controllers.Length - 1 :
                    controllerID < 0 ? 0 : controllerID;
                controller = ControllerFactory.CreateFrom(controllers[cId]);
                if (controller != null)
                {
                    //isConnected = true;
                    success = true;
                }
                else
                {
                    Console.WriteLine("Could not connect to controller...");
                }
            }
            else
            {
                Console.WriteLine("No controllers found on the network");
            }

            return success;
        }

        /// <summary>
        /// Disposes the controller object. This has to be done manually, since COM objects are not
        /// automatically garbage collected. 
        /// </summary>
        /// <returns></returns>
        private bool ReleaseController()
        {
            if (controller != null)
            {
                controller.Dispose();
                controller = null;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Load the controller's IP address into the object.
        /// </summary>
        /// <returns></returns>
        private bool LoadIP()
        {
            if (controller != null && controller.IPAddress != null)
            {
                IP = controller.IPAddress.ToString();
                return true;
            }
            return false;            
        }

        /// <summary>
        /// Resets IP address. 
        /// </summary>
        /// <returns></returns>
        private bool ReleaseIP()
        {
            IP = "";
            return true;
        }


        /// <summary>
        /// Logs on to the controller with a default user.
        /// </summary>
        /// <returns></returns>
        private bool LogOn()
        {
            // Sanity
            if (isLogged) LogOff();

            if (controller != null)
            {
                try
                {
                    controller.Logon(UserInfo.DefaultUser);
                    isLogged = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not log on to the controller");
                    Console.WriteLine(ex);
                    isLogged = false;
                }
            }

            return isLogged;
        }

        /// <summary>
        /// Logs off from the controller.
        /// </summary>
        /// <returns></returns>
        private bool LogOff()
        {
            if (controller != null)
            {
                controller.Logoff();
            }
            isLogged = false;
            return true;
        }

        /// <summary>
        /// Returns true if controller is in automatic mode.
        /// </summary>
        /// <returns></returns>
        private bool IsControllerInAutoMode()
        {
            if (controller != null)
            {
                if (controller.OperatingMode == ControllerOperatingMode.Auto)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if controller has Motors On
        /// </summary>
        /// <returns></returns>
        private bool IsControllerMotorsOn()
        {
            if (controller != null)
            {
                if (controller.State == ControllerState.MotorsOn)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves the main task from the ABB controller, typically 't_rob1'.
        /// </summary>
        /// <returns></returns>
        private bool LoadMainTask()
        {
            if (controller == null)
            {
                Console.WriteLine("Cannot retreive main task: no controller available");
                return false;
            }

            try
            {
                ABB.Robotics.Controllers.RapidDomain.Task[] tasks = controller.Rapid.GetTasks();
                if (tasks.Length > 0)
                {
                    mainTask = tasks[0];
                    return true;
                }
                else
                {
                    mainTask = null;
                    Console.WriteLine("Could not retrieve any task from the controller");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not retrieve main task from controller");
                Console.WriteLine(ex);
            }
            
            return false;
        }

        /// <summary>
        /// Disposes the task object. This has to be done manually, since COM objects are not
        /// automatically garbage collected. 
        /// </summary>
        /// <returns></returns>
        private bool ReleaseMainTask()
        {
            if (mainTask != null)
            {
                mainTask.Dispose();
                mainTask = null;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Pings the controller's Rapid resource with a bogus request to check if it is available for
        /// Mastership, or it is held by someone else.
        /// </summary>
        /// <returns></returns>
        private bool TestMastershipRapid()
        {
            if (controller != null)
            {
                try
                {
                    using (Mastership.Request(controller.Rapid))
                    {
                        // Gets the current execution cycle from the RAPID module and sets it back to the same value
                        ExecutionCycle mode = controller.Rapid.Cycle;
                        controller.Rapid.Cycle = mode;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Rapid Mastership not available");
                    Console.WriteLine(ex);
                }
            }
            else
            {
                Console.WriteLine("Cannot test Rapid Mastership, no controller available");
            }
            return false;
        }

        /// <summary>
        /// Subscribe to relevant events in the controller and assign them handlers.
        /// </summary>
        private bool SubscribeToEvents()
        {
            if (controller == null)
            {
                Console.WriteLine("Cannot subscribe to controller events: not connected to controller.");
            }
            else
            {
                // Suscribe to changes in the controller
                controller.OperatingModeChanged += OnOperatingModeChanged;
                controller.ConnectionChanged += OnConnectionChanged;
                //controller.MastershipChanged += OnMastershipChanged;
                controller.StateChanged += OnStateChanged;

                // Suscribe to Rapid program execution (Start, Stop...)
                controller.Rapid.ExecutionStatusChanged += OnRapidExecutionStatusChanged;

                // Suscribe to Mastership changes 
                controller.Rapid.MastershipChanged += OnRapidMastershipChanged;

                // Suscribe to Task Enabled changes
                controller.Rapid.TaskEnabledChanged += OnRapidTaskEnabledChanged;

                return true;
            }

            return false;
        }   

        /// <summary>
        /// Deletes all existing modules from main task in the controller. 
        /// </summary>
        /// <returns></returns>
        private int ClearAllModules()
        {
            if (controller == null)
            {
                Console.WriteLine("Cannot clear modules: not connected to controller");
                return -1;
            }

            if (mainTask == null)
            {
                Console.WriteLine("Cannot clear modules: main task not retrieved");
                return -1;
            }

            int count = -1;

            Module[] modules = mainTask.GetModules();
            count = 0;

            try
            {
                using (Mastership.Request(controller.Rapid))
                {
                    foreach (Module m in modules)
                    {
                        Console.WriteLine("Deleting module: {0}", m.Name);
                        m.Delete();
                        count++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("CLEAR MODULES ERROR: {0}", ex);
            }

            Console.WriteLine("Cleared {0} modules from main task", count);
            return count;
        }


        /// <summary>
        /// Resets the program pointer in the controller to the main entry point. Needs to be called
        /// before starting execution of a program, otherwise the controller will throw an error. 
        /// </summary>
        private bool ResetProgramPointer()
        {
            if (controller == null)
            {
                Console.WriteLine("Cannot reset pointer: not connected to controller");
                return false;
            }

            if (mainTask == null)
            {
                Console.WriteLine("Cannot reset pointer: mainTask not present");
                return false;
            }

            try
            {
                using (Mastership.Request(controller.Rapid))
                {
                    mainTask.ResetProgramPointer();
                    return true;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot reset pointer...");
                Console.WriteLine(ex);
            }

            return false;
        }








        //███████╗████████╗██████╗ ███████╗ █████╗ ███╗   ███╗██╗███╗   ██╗ ██████╗ 
        //██╔════╝╚══██╔══╝██╔══██╗██╔════╝██╔══██╗████╗ ████║██║████╗  ██║██╔════╝ 
        //███████╗   ██║   ██████╔╝█████╗  ███████║██╔████╔██║██║██╔██╗ ██║██║  ███╗
        //╚════██║   ██║   ██╔══██╗██╔══╝  ██╔══██║██║╚██╔╝██║██║██║╚██╗██║██║   ██║
        //███████║   ██║   ██║  ██║███████╗██║  ██║██║ ╚═╝ ██║██║██║ ╚████║╚██████╔╝
        //╚══════╝   ╚═╝   ╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝╚═╝     ╚═╝╚═╝╚═╝  ╚═══╝ ╚═════╝ 

        private static int virtualRDCount = 4;
        private int virtualStepCounter = 0;  // this keeps track of what is the next target index that needs to be assigned. Its %4 should asynchronously be ~4 units ahead of the 'pnum' rapid counter

        private RapidData
            RD_aborted,
            RD_pnum;
        private RapidData[]
            RD_vel = new RapidData[virtualRDCount],
            RD_zone = new RapidData[virtualRDCount],
            RD_p = new RapidData[virtualRDCount],
            RD_pset = new RapidData[virtualRDCount];
        
        /// <summary>
        /// Performs necessary operations to set up 'stream' control mode in the controller
        /// </summary>
        /// <returns></returns>
        private bool SetupStreamingMode()
        {
            if (!LoadStreamingModule())
            {
                Console.WriteLine("Could not load streaming module");
                return false;
            }

            if (!HookUpStreamingVariables())
            {
                Console.WriteLine("Could not load streaming variables");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Loads the default StreamModule designed for streaming.
        /// </summary>
        private bool LoadStreamingModule()
        {
            return LoadProgramToController(StaticData.StreamModule.ToList());
        }

        /// <summary>
        /// Loads all relevant Rapid variables
        /// </summary>
        private bool HookUpStreamingVariables()
        {
            // Load RapidData control variables
            RD_aborted = LoadRapidDataVariable("aborted");
            if (RD_aborted == null)
                return false;

            RD_pnum = LoadRapidDataVariable("pnum");
            if (RD_pnum == null)
                return false;

            RD_pnum.ValueChanged += OnRD_pnum_ValueChanged;  // add an eventhandler to 'pnum' to track when it changes

            Console.WriteLine(RD_aborted.StringValue);
            Console.WriteLine(RD_pnum.StringValue);

            // Load and set the first four targets
            for (int i = 0; i < virtualRDCount; i++)
            {
                RD_vel[i] = LoadRapidDataVariable("vel" + i);
                RD_zone[i] = LoadRapidDataVariable("zone" + i);
                RD_p[i] = LoadRapidDataVariable("p" + i);
                RD_pset[i] = LoadRapidDataVariable("pset" + i);
                //AddVirtualTarget();

                if (RD_vel[i] == null || RD_zone[i] == null || RD_p[i] == null || RD_pset[i] == null)
                    return false;

                Console.WriteLine("{0}, {1}, {2}, {3}",
                    RD_vel[i].StringValue,
                    RD_zone[i].StringValue,
                    RD_p[i].StringValue,
                    RD_pset[i].StringValue);
            }

            return true;
        }

        /// <summary>
        /// Retrieves a Rapid variable in current module and returns it
        /// </summary>
        /// <param name="varName"></param>
        /// <returns></returns>
        private RapidData LoadRapidDataVariable(string varName)
        {
            RapidData rd = null;
            try
            {
                rd = mainTask.GetModule("StreamModule").GetRapidData(varName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return rd;
        }

        /// <summary>
        /// Sets the value of a Rapid variable from a string representation
        /// </summary>
        /// <param name="rd"></param>
        /// <param name="declaration"></param>
        private void SetRapidDataVariable(RapidData rd, string declaration)
        {
            try
            {
                using (Mastership.Request(controller.Rapid))
                {
                    Console.WriteLine("    current value for '{0}': {1}", rd.Name, rd.StringValue);
                    rd.StringValue = declaration;
                    Console.WriteLine("        NEW value for '{0}': {1}", rd.Name, rd.StringValue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("    ERROR SetRapidDataVarString: {0}", ex);
            }
        }


        /// <summary>
        /// Figures out the appropriate virtual target in the streaming module and 
        /// sets new values according to the streaming queue.
        /// </summary>
        /// <param name="hasPriority"></param>
        private void SetNextVirtualTarget(bool hasPriority)
        {
            Console.WriteLine("Setting frame #{0}", virtualStepCounter);

            lock (rapidDataLock)
            {
                //Frame target = streamQueue.GetNext();
                //if (target != null)
                //{
                //    int fid = virtualStepCounter % virtualRDCount;

                //    SetRapidDataVariable(RD_p[fid], target.GetUNSAFERobTargetDeclaration());
                //    SetRapidDataVariable(RD_vel[fid], target.GetSpeedDeclaration());
                //    SetRapidDataVariable(RD_zone[fid], target.GetZoneDeclaration());
                //    SetRapidDataVariable(RD_pset[fid], "TRUE");
                //}

                bool applied = writeCursor.ApplyNextAction();
                if (applied)
                {
                    int fid = virtualStepCounter % virtualRDCount;

                    SetRapidDataVariable(RD_p[fid], writeCursor.GetUNSAFERobTargetDeclaration());
                    SetRapidDataVariable(RD_vel[fid], writeCursor.GetSpeedDeclaration());
                    SetRapidDataVariable(RD_zone[fid], writeCursor.GetZoneDeclaration());
                    SetRapidDataVariable(RD_pset[fid], "TRUE");
                }


            }
        }



































        //███████╗██╗   ██╗███████╗███╗   ██╗████████╗    ██╗  ██╗ █████╗ ███╗   ██╗██████╗ ██╗     ██╗███╗   ██╗ ██████╗ 
        //██╔════╝██║   ██║██╔════╝████╗  ██║╚══██╔══╝    ██║  ██║██╔══██╗████╗  ██║██╔══██╗██║     ██║████╗  ██║██╔════╝ 
        //█████╗  ██║   ██║█████╗  ██╔██╗ ██║   ██║       ███████║███████║██╔██╗ ██║██║  ██║██║     ██║██╔██╗ ██║██║  ███╗
        //██╔══╝  ╚██╗ ██╔╝██╔══╝  ██║╚██╗██║   ██║       ██╔══██║██╔══██║██║╚██╗██║██║  ██║██║     ██║██║╚██╗██║██║   ██║
        //███████╗ ╚████╔╝ ███████╗██║ ╚████║   ██║       ██║  ██║██║  ██║██║ ╚████║██████╔╝███████╗██║██║ ╚████║╚██████╔╝
        //╚══════╝  ╚═══╝  ╚══════╝╚═╝  ╚═══╝   ╚═╝       ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═════╝ ╚══════╝╚═╝╚═╝  ╚═══╝ ╚═════╝ 

        /// <summary>
        /// What to do when the robot starts running or stops.
        /// @TODO: add new behavior here when execution changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRapidExecutionStatusChanged(object sender, ExecutionStatusChangedEventArgs e)
        {
            Console.WriteLine("EXECUTION STATUS CHANGED: " + e.Status);

            if (e.Status == ExecutionStatus.Running)
            {
                isRunning = true;
            }
            else
            {
                isRunning = false;

                // Only trigger Instruct queue
                if (masterControl.GetControlMode() == ControlMode.Execute)
                {
                    // Tick queue to move forward
                    //masterControl.TriggerQueue();
                    masterControl.TickWriteCursor();
                }
            }
        }

        /// <summary>
        /// What to do when Mastership changes.
        /// @TODO: add behaviors...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRapidMastershipChanged(object sender, MastershipChangedEventArgs e)
        {
            Console.WriteLine("RAPID MASTERSHIP STATUS CHANGED: {0}", e.Status);

            // @TODO: what to do when mastership changes
        }

        /// <summary>
        /// What to do when the Task Enabled property changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRapidTaskEnabledChanged(object sender, TaskEnabledChangedEventArgs e)
        {
            Console.WriteLine("TASK ENABLED CHANGED: {0}", e.Enabled);

            // @TODO: add behaviors
        }

        /// <summary>
        /// What to do when the controller changes Operating Mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOperatingModeChanged(object sender, OperatingModeChangeEventArgs e)
        {
            Console.WriteLine("OPERATING MODE CHANGED: {0}", e.NewMode);
        }

        /// <summary>
        /// What to do when the 'pnum' rapid var changes value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRD_pnum_ValueChanged(object sender, DataValueChangedEventArgs e)
        {
            RapidData rd = (RapidData)sender;
            Console.WriteLine("   variable '{0}' changed: {1}", rd.Name, rd.StringValue);

            // Do not add target if pnum is being reset to initial value (like on program load)
            if (!rd.StringValue.Equals("-1"))
            {
                Console.WriteLine("Ticking from pnum event handler");
                //mHeld = true;
                TickStreamQueue(true);
                //mHeld = false;
            }

            if (rd != null)
            {
                //Console.WriteLine("Disposing rd");
                //rd.Dispose();
                //rd = null;
            }

        }

        private void OnStateChanged(object sender, StateChangedEventArgs e)
        {
            Console.WriteLine("CONTROLLER STATECHANGED: {0}", e.NewState);
        }

        private void OnMastershipChanged(object sender, MastershipChangedEventArgs e)
        {
            Console.WriteLine("CONTROLLER MASTERSHIP CHANGED: {0}", e.Status);
        }

        private void OnConnectionChanged(object sender, ConnectionChangedEventArgs e)
        {
            Console.WriteLine("CONTROLLER CONNECTION CHANGED: {0}", e.Connected);
        }









    }

}
