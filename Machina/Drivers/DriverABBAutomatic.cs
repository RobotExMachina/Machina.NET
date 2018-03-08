using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;

using ABB.Robotics;
using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers.RapidDomain;  // This is for the Task Class
using ABB.Robotics.Controllers.EventLogDomain;
using ABB.Robotics.Controllers.FileSystemDomain;

namespace Machina
{

    //   █████╗ ██████╗ ██████╗ 
    //  ██╔══██╗██╔══██╗██╔══██╗
    //  ███████║██████╔╝██████╔╝
    //  ██╔══██║██╔══██╗██╔══██╗
    //  ██║  ██║██████╔╝██████╔╝
    //  ╚═╝  ╚═╝╚═════╝ ╚═════╝ 
    //                          
    class DriverABBAutomatic : Driver
    {
        // ABB stuff and flags
        private Controller controller;
        private ABB.Robotics.Controllers.RapidDomain.Task mainTask;
        //public object rapidDataLock = new object();
        private bool isLogged = false;

        private static string localBufferDirname = "C:";                 // Route names to be used for file handling
        private static string localBufferFilename = "buffer";
        private static string localBufferFileExtension = "mod";
        private static string remoteBufferDirectory = "Machina";

        //protected RobotCursorABB writeCursor;
        //protected RobotCursor writeCursor;

        private TcpClient clientSocket = new TcpClient();
        private NetworkStream clientStream;



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
        public DriverABBAutomatic(Control ctrl) : base(ctrl) { }

        /// <summary>
        /// Reverts the Comm object to a blank state before any connection attempt. 
        /// </summary>
        public override void Reset()
        {
            CloseTCPConnection();
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
            if (masterControl.GetControlMode() == ControlType.Stream)
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
            //UnhookStreamingVariables();
            return true;
        }

        /// <summary>
        /// Sets the Rapid ExecutionCycle to Once, Forever or None.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public override bool SetRunMode(CycleType mode)
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
                    controller.Rapid.Cycle = mode == CycleType.Once ? ExecutionCycle.Once : mode == CycleType.Loop ? ExecutionCycle.Forever : ExecutionCycle.None;
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
            if (!this.masterControl.SaveStringListToFile(module, $"{localBufferDirname}\\{localBufferFilename}.{localBufferFileExtension}"))
            {
                Console.WriteLine("Could not save module to file");
                return false;
            }

            if (!LoadFileToController(localBufferDirname, localBufferFilename, localBufferFileExtension))
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
        public override bool LoadFileToController(string dirname, string filename, string extension)
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
                Console.WriteLine("Cannot start program: not connected to controller");
                return false;
            }

            if (isRunning)
            {
                Console.WriteLine("Program is already running...");
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
                    bool isControllerRunning = controller.Rapid.ExecutionStatus == ExecutionStatus.Running;

                    if (isControllerRunning != isRunning)
                    {
                        throw new Exception("isRunning mismatch state");
                    }

                    StartResult res = controller.Rapid.Start(true);
                    if (res != StartResult.Ok)
                    {
                        Console.WriteLine($"Cannot start program: {res}");
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
            if (controller == null) return true;
            
            if (!isConnected)
            {
                Console.WriteLine("Cannot stop program: not connected to controller");
                return false;
            }

            if (!isRunning)
            {
                Console.WriteLine("Cannot stop program: execution is already stopped");
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
        /// Returns a Vector object representing the current robot's TCP position.
        /// </summary>
        /// <returns></returns>
        public override Vector GetCurrentPosition()
        {
            if (!isConnected)
            {
                Console.WriteLine("Cannot GetCurrentPosition: not connected to controller");
                return null;
            }

            RobTarget rt = controller.MotionSystem.ActiveMechanicalUnit.GetPosition(ABB.Robotics.Controllers.MotionDomain.CoordinateSystemType.World);

            return new Vector(rt.Trans.X, rt.Trans.Y, rt.Trans.Z);
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
            return Rotation.FromQuaternion(rt.Rot.Q1, rt.Rot.Q2, rt.Rot.Q3, rt.Rot.Q4);
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

            try
            {
                JointTarget jt = controller.MotionSystem.ActiveMechanicalUnit.GetPosition();
                return new Joints(jt.RobAx.Rax_1, jt.RobAx.Rax_2, jt.RobAx.Rax_3, jt.RobAx.Rax_4, jt.RobAx.Rax_5, jt.RobAx.Rax_6);
            }
            catch (ABB.Robotics.Controllers.ServiceNotSupportedException e)
            {
                Console.WriteLine("CANNOT RETRIEVE JOINTS FROM CONTROLLER");
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// This function will look at the state of the program pointer, the streamQueue, 
        /// and if necessary will add a new target to the stream. This is meant to be called
        /// to initiate the stream update chain, like when adding a new target, or pnum event handling.
        /// </summary>
        public override void TickStreamQueue(bool hasPriority)
        {
            Console.WriteLine("TICKING StreamQueue: {0} actions pending", WriteCursor.ActionsPending());
            if (WriteCursor.AreActionsPending())
            {
                Console.WriteLine("About to set targets");
                //SetNextVirtualTarget(hasPriority);
                SendActionAsMessage(hasPriority);
                TickStreamQueue(hasPriority);  // call this in case there are more in the queue...
            }
            else
            {
                Console.WriteLine($"Not setting targets, actions pending {WriteCursor.ActionsPending()}");
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
                try
                {
                    Console.WriteLine("    ExecutionType: " + mainTask.ExecutionType);
                }
                catch (ABB.Robotics.Controllers.ServiceNotSupportedException e)
                {
                    Console.WriteLine("    ExecutionType: UNSUPPORTED BY CONTROLLER");
                    Console.WriteLine(e);
                }
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
            Console.WriteLine("Scanning the network for controllers...");
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
                    Console.WriteLine($"Found controller {controller.SystemName} on {controller.Name}");
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
                Console.WriteLine($"Loaded IP {IP}");
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
                    Console.WriteLine("Logged on as DefaultUser");
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
                    Console.WriteLine("Retrieved task " + mainTask.Name);
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
                        // Gets the current execution cycle from the RAPID module and sets it back to the same value (just a stupid test)
                        ExecutionCycle mode = controller.Rapid.Cycle;
                        controller.Rapid.Cycle = mode;
                        Console.WriteLine("Mastership test OK");
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

        //private static int virtualRDCount = 4;
        //private int virtualStepCounter = 0;  // this keeps track of what is the next target index that needs to be assigned. Its %4 should asynchronously be ~4 units ahead of the 'pnum' rapid counter

        ////private RapidData
        ////    RD_aborted,
        ////    RD_pnum;
        ////private RapidData[]
        ////    RD_pset = new RapidData[virtualRDCount],
        ////    RD_act = new RapidData[virtualRDCount],
        ////    RD_vel = new RapidData[virtualRDCount],
        ////    RD_zone = new RapidData[virtualRDCount],
        ////    RD_rt = new RapidData[virtualRDCount],
        ////    RD_jt = new RapidData[virtualRDCount],
        ////    RD_wt = new RapidData[virtualRDCount],
        ////    RD_msg = new RapidData[virtualRDCount];


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

            if (!ResetProgramPointer())
            {
                Console.WriteLine("Could not reset the program pointer");
                return false;
            }

            if (!StartProgramExecution())
            {
                Console.WriteLine("Could not load start the streaming module");
                return false;
            }

            if (!EstablishTCPConnection())
            {
                Console.WriteLine("Could not connect to server in controller");
                return false;
            }
            
            // Hurray!
            return true;
        }

        private bool EstablishTCPConnection()
        {
            clientSocket = new TcpClient();
            clientSocket.Connect(IP, PORT);
            clientStream = clientSocket.GetStream();

            return clientSocket.Connected;
        }

        private bool CloseTCPConnection()
        {
            if (controller == null) return true;

            if (clientSocket != null)
            {
                clientSocket.Close();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Loads the default StreamModule designed for streaming.
        /// </summary>
        private bool LoadStreamingModule()
        {
            //return LoadProgramToController(StreamModuleV2.ToList());

            if (!LoadFileToController("C:", "Machina_Driver", "mod"))
            {
                Console.WriteLine("Could not load module to controller");
                return false;
            }
            return true;
        }


        private bool SendActionAsMessage(bool hasPriority)
        {

            if (!WriteCursor.ApplyNextAction())
            {
                Console.WriteLine("Could not apply next action");
                return false;
            }

            Action a = WriteCursor.GetLastAction();

            if (a == null)
                throw new Exception("Last action wasn't correctly stored...?");

            // dummy message to request version number
            string msg = $"@{a.id} 101 1;";
            byte[] msgBytes = Encoding.ASCII.GetBytes(msg);
            clientStream.Write(msgBytes, 0, msgBytes.Length);

            return true;
        }


        ///// <summary>
        ///// Figures out the appropriate virtual target in the streaming module and 
        ///// sets new values according to the streaming queue.
        ///// </summary>
        ///// <param name="hasPriority"></param>
        //private void SetNextVirtualTarget(bool hasPriority)
        //{
        //    Console.WriteLine("Setting frame #{0}", virtualStepCounter);

        //    lock (rapidDataLock)
        //    {
        //        bool applied = writeCursor.ApplyNextAction();
        //        if (applied)
        //        {
        //            int fid = virtualStepCounter % virtualRDCount;

        //            Action a = writeCursor.GetLastAction();

        //            if (a == null)
        //                throw new Exception("Last action wasn't correctly stored...?");

        //            bool moveOn = true;
        //            switch (a.type)
        //            {
        //                case ActionType.Translation:
        //                case ActionType.Rotation:
        //                case ActionType.Transformation:
        //                    //SetRapidDataVariable(RD_vel[fid], writeCursor.GetSpeedValue());
        //                    //SetRapidDataVariable(RD_zone[fid], writeCursor.GetZoneValue());
        //                    //SetRapidDataVariable(RD_rt[fid], writeCursor.GetUNSAFERobTargetValue());    
        //                    //SetRapidDataVariable(RD_act[fid], writeCursor.motionType == MotionType.Linear ? 1 : 2);
        //                    SetRapidDataVariable(RD_vel[fid], CompilerABB.GetSpeedValue(writeCursor));
        //                    SetRapidDataVariable(RD_zone[fid], CompilerABB.GetZoneValue(writeCursor));
        //                    SetRapidDataVariable(RD_rt[fid], CompilerABB.GetUNSAFERobTargetValue(writeCursor));
        //                    SetRapidDataVariable(RD_act[fid], writeCursor.motionType == MotionType.Linear ? 1 : 2);
        //                    break;

        //                case ActionType.Axes:
        //                    //SetRapidDataVariable(RD_vel[fid], writeCursor.GetSpeedValue());
        //                    //SetRapidDataVariable(RD_zone[fid], writeCursor.GetZoneValue());
        //                    //SetRapidDataVariable(RD_jt[fid], writeCursor.GetJointTargetValue());
        //                    //SetRapidDataVariable(RD_act[fid], 3);
        //                    SetRapidDataVariable(RD_vel[fid], CompilerABB.GetSpeedValue(writeCursor));
        //                    SetRapidDataVariable(RD_zone[fid], CompilerABB.GetZoneValue(writeCursor));
        //                    SetRapidDataVariable(RD_jt[fid], CompilerABB.GetJointTargetValue(writeCursor));
        //                    SetRapidDataVariable(RD_act[fid], 3);
        //                    break;

        //                case ActionType.Wait:
        //                    ActionWait aw = (ActionWait)a;
        //                    SetRapidDataVariable(RD_wt[fid], 0.001 * aw.millis);
        //                    SetRapidDataVariable(RD_act[fid], 4);
        //                    break;

        //                case ActionType.Message:
        //                    ActionMessage am = (ActionMessage)a;
        //                    // TPWrite can only handle 40 chars
        //                    string str = am.message;
        //                    if (am.message.Length > 40)
        //                        str = am.message.Substring(0, 40);

        //                    SetRapidDataVariable(RD_msg[fid], string.Format("\"{0}\"", str));  // when setting the value for a string rapidvar, the double quotes are needed as part of the value
        //                    SetRapidDataVariable(RD_act[fid], 5);
        //                    break;

        //                // speed, zone, motion, refcs...
        //                default:
        //                    // A speed change doesn't create a new target: do nothing, and prevent triggering a new target
        //                    moveOn = false;
        //                    break;
        //            }

        //            if (moveOn)
        //            {
        //                SetRapidDataVariable(RD_pset[fid], "TRUE");
        //                virtualStepCounter++;
        //            }

        //        }
        //    }
        //}





        public static string StreamModuleV3 =
            @"MODULE Machina_Driver

                !    ███╗   ███╗ █████╗  ██████╗██╗  ██╗██╗███╗   ██╗ █████╗
                !    ████╗ ████║██╔══██╗██╔════╝██║  ██║██║████╗  ██║██╔══██╗
                !    ██╔████╔██║███████║██║     ███████║██║██╔██╗ ██║███████║
                !    ██║╚██╔╝██║██╔══██║██║     ██╔══██║██║██║╚██╗██║██╔══██║
                !    ██║ ╚═╝ ██║██║  ██║╚██████╗██║  ██║██║██║ ╚████║██║  ██║
                !    ╚═╝     ╚═╝╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝
                !
                ! This file starts a server on a virtual/real ABB robot,
                ! waits for a TCP client, and listens to a stream of formatted
                ! string messages.
                !
                ! IMPORTANT: make sure to adjust SERVER_IP to your current setup
                !
                ! More info on https://github.com/garciadelcastillo/Machina


                !  ╔╦╗╔═╗╔═╗╦  ╔═╗╦═╗╔═╗╔╦╗╦╔═╗╔╗╔╔═╗
                !   ║║║╣ ║  ║  ╠═╣╠╦╝╠═╣ ║ ║║ ║║║║╚═╗
                !  ═╩╝╚═╝╚═╝╩═╝╩ ╩╩╚═╩ ╩ ╩ ╩╚═╝╝╚╝╚═╝

                ! An abstract representation of a robotic instruction
                RECORD action
                    num id;
                    num code;
                    string s1;
                    ! `Records` cannot contain arrays... :(
                    num p1; num p2; num p3; num p4; num p5;
                    num p6; num p7; num p8; num p9; num p10;
                    num p11;
                ENDRECORD

                ! Server data: change IP from localhost to ""192.168.125.1"" (typically) if working with a real robot
                CONST string SERVER_IP := ""127.0.0.1"";
                CONST num SERVER_PORT := 7000;

                ! Useful for handshakes and version compatibility checks...
                CONST string MACHINA_DRIVER_VERSION := ""0.6.0"";

                ! Should program exit on any kind of error?
                VAR bool USE_STRICT := TRUE;

                ! TCP stuff
                VAR string clientIp;
                VAR socketdev serverSocket;
                VAR socketdev clientSocket;

                ! A RAPID-code oriented API:
                !                                         INSTRUCTION P1 P2 P3 P4...
                CONST num INST_MOVEL := 1;              ! MoveL X Y Z QW QX QY QZ
                CONST num INST_MOVEJ := 2;              ! MoveJ X Y Z QW QX QY QZ
                CONST num INST_MOVEABSJ := 3;           ! MoveAbsJ J1 J2 J3 J4 J5 J6
                CONST num INST_SPEED := 4;              ! (setspeed V_TCP [V_ORI V_LEAX V_REAX])
                CONST num INST_ZONE := 5;               ! (setzone FINE TCP [ORI EAX ORI LEAX REAX])
                CONST num INST_WAITTIME := 6;           ! WaitTime T
                CONST num INST_TPWRITE := 7;            ! TPWrite ""MSG""
                CONST num INST_TOOL := 8;               ! (settool X Y Z QW QX QY QZ KG CX CY CZ)
                CONST num INST_NOTOOL := 9;             ! (settool tool0)
                CONST num INST_SETDO := 10;             ! SetDO ""NAME"" ON
                CONST num INST_SETAO := 11;             ! SetAO ""NAME"" V

                CONST num INST_STOP_EXECUTION := 100;       ! Stops execution of the server module
                CONST num INST_GET_INFO := 101;             ! A way to retreive state information from the server (not implemented)
                CONST num INST_SET_CONFIGURATION := 102;    ! A way to make some changes to the configuration of the server

                ! Characters used for buffer parsing
                CONST string STR_MESSAGE_END_CHAR := "";"";
                CONST string STR_MESSAGE_ID_CHAR := ""@"";
                CONST string STR_MESSAGE_RESPONSE_CHAR := "">"";

                ! RobotWare 5.x shim
                CONST num WAIT_MAX := 8388608;

                ! State variables representing a virtual cursor of data the robot is instructed to
                PERS tooldata cursorTool;
                PERS wobjdata cursorWObj;
                VAR robtarget cursorTarget;
                VAR jointtarget cursorJoints;
                VAR speeddata cursorSpeed;
                VAR zonedata cursorZone;
                VAR signaldo cursorDO;
                VAR signalao cursorAO;

                ! Buffer of incoming messages
                CONST num msgBufferSize := 1000;
                VAR string msgBuffer{msgBufferSize};
                VAR num msgBufferReadCurrPos;
                VAR num msgBufferReadPrevPos;
                VAR num msgBufferReadLine;
                VAR num msgBufferWriteLine;
                VAR bool isMsgBufferWriteLineWrapped;
                VAR bool streamBufferPending;

                CONST string STR_DOUBLE_QUOTES := """""""";  ! A escaped double quote is written twice

                ! Buffer of pending actions
                CONST num actionsBufferSize := 1000;
                VAR action actions{actionsBufferSize};
                VAR num actionPosWrite;
                VAR num actionPosExecute;
                VAR bool isActionPosWriteWrapped;

                ! Buffer of responses
                VAR string response;



                !  ╔╦╗╔═╗╦╔╗╔
                !  ║║║╠═╣║║║║
                !  ╩ ╩╩ ╩╩╝╚╝

                ! Main entry point
                PROC Main()
                    TPErase;

                    ! Avoid signularities
                    SingArea \Wrist;

                    CursorsInitialize;
                    ServerInitialize;

                    MainLoop;
                ENDPROC

                ! The main loop the program will execute endlessly to read incoming messages
                ! and execute pending Actions
                PROC MainLoop()
                    VAR action currentAction;
                    VAR bool stopExecution := FALSE;

                    WHILE stopExecution = FALSE DO
                        ! Read the incoming buffer stream until flagges complete
                        ! (must be done this way to avoid execution stack overflow through recursion)
                        ReadStream;
                        WHILE streamBufferPending = TRUE DO
                            ReadStream;
                        ENDWHILE
                        ParseStream;

                        ! Once the stream is flushed, execute all pending actions
                        WHILE stopExecution = FALSE AND (actionPosExecute < actionPosWrite OR isActionPosWriteWrapped = TRUE) DO
                            currentAction := actions{actionPosExecute};

                            TEST currentAction.code
                            CASE INST_MOVEL:
                                ! Send ack msg?
                                cursorTarget := GetRobTarget(currentAction);
                                MoveL cursorTarget, cursorSpeed, cursorZone, cursorTool, \WObj:=cursorWObj;
                                ! Senf ack msg?

                            CASE INST_MOVEJ:
                                cursorTarget := GetRobTarget(currentAction);
                                MoveJ cursorTarget, cursorSpeed, cursorZone, cursorTool, \WObj:=cursorWObj;

                            CASE INST_MOVEABSJ:
                                cursorJoints := GetJointTarget(currentAction);
                                MoveAbsJ cursorJoints, cursorSpeed, cursorZone, cursorTool, \WObj:=cursorWObj;

                            CASE INST_SPEED:
                                cursorSpeed := GetSpeedData(currentAction);

                            CASE INST_ZONE:
                                cursorZone := GetZoneData(currentAction);

                            CASE INST_WAITTIME:
                                WaitTime currentAction.p1;

                            CASE INST_TPWRITE:
                                TPWrite currentAction.s1;

                            CASE INST_TOOL:
                                cursorTool := GetToolData(currentAction);

                            CASE INST_NOTOOL:
                                cursorTool := tool0;

                            CASE INST_SETDO:
                                GetDataVal currentAction.s1, cursorDO;
                                SetDO cursorDO, currentAction.p1;

                            CASE INST_SETDO:
                                GetDataVal currentAction.s1, cursorAO;
                                SetAO cursorAO, currentAction.p1;


                            CASE INST_STOP_EXECUTION:
                                stopExecution := TRUE;

                            CASE INST_GET_INFO:
                                SendInformation(currentAction);

                            ENDTEST

                            ! Send acknowledgement message
                            SendAcknowledgement(currentAction);

                            actionPosExecute := actionPosExecute + 1;
                            IF actionPosExecute > actionsBufferSize THEN
                                actionPosExecute := 1;
                                isActionPosWriteWrapped := FALSE;
                            ENDIF

                        ENDWHILE
                    ENDWHILE

                    ServerFinalize;

                    ERROR
                        IF ERRNO = ERR_SYM_ACCESS THEN
                            TPWrite ""Could not find signal """""" + currentAction.s1 + """""""";
                            TPWrite ""Errors will follow"";
                            IF USE_STRICT THEN EXIT; ENDIF
                            STOP;
                        ENDIF

                ENDPROC







                !  ╔═╗╔═╗╔╦╗╔╦╗╦ ╦╔╗╔╦╔═╗╔═╗╔╦╗╦╔═╗╔╗╔
                !  ║  ║ ║║║║║║║║ ║║║║║║  ╠═╣ ║ ║║ ║║║║
                !  ╚═╝╚═╝╩ ╩╩ ╩╚═╝╝╚╝╩╚═╝╩ ╩ ╩ ╩╚═╝╝╚╝

                ! Start the TCP server
                PROC ServerInitialize()
                    TPWrite ""Initializing Machina Server..."";
                    ServerRecover;
                ENDPROC

                ! Recover from a disconnection
                PROC ServerRecover()
                    SocketClose serverSocket;
                    SocketClose clientSocket;
                    SocketCreate serverSocket;
                    SocketBind serverSocket, SERVER_IP, SERVER_PORT;
                    SocketListen serverSocket;

                    TPWrite ""Waiting for incoming connection..."";

                    SocketAccept serverSocket, clientSocket \ClientAddress:=clientIp \Time:=WAIT_MAX;

                    TPWrite ""Connected to client: "" + clientIp;
                    TPWrite ""Listening to TCP/IP commands..."";

                    ERROR
                        IF ERRNO = ERR_SOCK_TIMEOUT THEN
                            RETRY;
                        ELSEIF ERRNO = ERR_SOCK_CLOSED THEN
                            RETURN;
                        ELSE
                            ! No error recovery handling
                        ENDIF
                ENDPROC

                ! Close sockets
                PROC ServerFinalize()
                    SocketClose serverSocket;
                    SocketClose clientSocket;
                    WaitTime 2;
                ENDPROC

                ! Read string buffer from the client and try to parse it
                PROC ReadStream()
                    VAR string strBuffer;
                    VAR num strBufferLength;
                    SocketReceive clientSocket \Str:=strBuffer \NoRecBytes:=strBufferLength \Time:=WAIT_MAX;
                    ParseBuffer strBuffer, strBufferLength;

                    ERROR
                    IF ERRNO = ERR_SOCK_CLOSED THEN
                        ServerRecover;
                        RETRY;
                    ENDIF
                ENDPROC

                ! Sends a short acknowledgement response to the client with the recently
                ! executed instruction and an optional id
                PROC SendAcknowledgement(action a)
                    response := """";

                    IF a.id <> 0 THEN
                        response := STR_MESSAGE_ID_CHAR + NumToStr(a.id, 0) + STR_WHITE;
                    ENDIF

                    response := response + NumToStr(a.code, 0);

                    SocketSend clientSocket \Str:=response;
                ENDPROC

                ! Responds to an information request by sending a formatted message
                PROC SendInformation(action a)
                    response := STR_MESSAGE_RESPONSE_CHAR;

                    IF a.id <> 0 THEN
                        response := response + STR_MESSAGE_ID_CHAR + NumToStr(a.id, 0) + STR_WHITE;
                    ENDIF

                    response := response + NumToStr(a.code, 0) + STR_WHITE + NumToStr(a.p1, 0) + STR_WHITE;

                    TEST a.p1

                    CASE 1:  ! Driver version
                        response := response + STR_DOUBLE_QUOTES + MACHINA_DRIVER_VERSION + STR_DOUBLE_QUOTES;

                    CASE 2:  ! IP and PORT
                        response := response + STR_DOUBLE_QUOTES + SERVER_IP + STR_DOUBLE_QUOTES + STR_WHITE + NumToStr(SERVER_PORT, 0);

                    ENDTEST

                    SocketSend clientSocket \Str:=response;
                ENDPROC




                !  ╔═╗╔═╗╦═╗╔═╗╦╔╗╔╔═╗
                !  ╠═╝╠═╣╠╦╝╚═╗║║║║║ ╦
                !  ╩  ╩ ╩╩╚═╚═╝╩╝╚╝╚═╝

                ! Parse an incoming string buffer, and decide what to do with it
                ! based on its quality
                PROC ParseBuffer(string sb, num sbl)
                    VAR num statementsLength;
                    VAR num endCurrentPos := 1;
                    VAR num endLastPos := 1;
                    VAR num endings := 0;

                    endCurrentPos := StrFind(sb, 1, STR_MESSAGE_END_CHAR);
                    WHILE endCurrentPos <= sbl DO
                        endings := endings + 1;
                        endLastPos := endCurrentPos;
                        endCurrentPos := StrFind(sb, endCurrentPos + 1, STR_MESSAGE_END_CHAR);
                    ENDWHILE

                    ! Corrupt buffer
                    IF endings = 0 THEN
                        TPWrite ""Received corrupt buffer"";
                        TPWrite sb;
                        IF USE_STRICT THEN EXIT; ENDIF
                    ENDIF

                    ! Store the buffer
                    StoreBuffer sb;

                    ! Keep going if the chunk was trimmed
                    streamBufferPending := endLastPos < sbl;

                ENDPROC

                ! Add a string buffer to the buffer of received messages
                PROC StoreBuffer(string buffer)
                    IF isMsgBufferWriteLineWrapped = TRUE AND msgBufferWriteLine = msgBufferReadLine THEN
                        TPWrite ""MACHINA WARNING: memory overload. Maximum string buffer size is "" + NumToStr(actionsBufferSize, 0);
                        TPWrite ""Reduce the amount of stream messages while they execute."";
                        EXIT;
                    ENDIF

                    msgBuffer{msgBufferWriteLine} := buffer;
                    msgBufferWriteLine := msgBufferWriteLine + 1;

                    IF msgBufferWriteLine > msgBufferSize THEN
                        msgBufferWriteLine := 1;
                        isMsgBufferWriteLineWrapped := TRUE;
                    ENDIF
                ENDPROC

                ! Parse the buffer of received messages into the buffer of pending actions
                PROC ParseStream()
                    VAR string statement;
                    VAR string part;
                    VAR num partLength;
                    VAR num lineLength;

                    ! TPWrite ""Parsing buffered stream, actionPosWrite: "" + NumToStr(actionPosWrite, 0);

                    WHILE msgBufferReadLine < msgBufferWriteLine OR isMsgBufferWriteLineWrapped = TRUE DO
                        lineLength := StrLen(msgBuffer{msgBufferReadLine});

                        WHILE msgBufferReadCurrPos <= lineLength DO
                            msgBufferReadCurrPos := StrFind(msgBuffer{msgBufferReadLine}, msgBufferReadPrevPos, STR_MESSAGE_END_CHAR);

                            partLength := msgBufferReadCurrPos - msgBufferReadPrevPos;
                            part := part + StrPart(msgBuffer{msgBufferReadLine}, msgBufferReadPrevPos, partLength);  ! take the statement without the STR_MESSAGE_END_CHAR

                            IF msgBufferReadCurrPos <= lineLength THEN
                                ParseStatement(part + STR_MESSAGE_END_CHAR);  ! quick and dirty add of the end_char... XD
                                part := """";
                            ENDIF

                            msgBufferReadCurrPos := msgBufferReadCurrPos + 1;
                            msgBufferReadPrevPos := msgBufferReadCurrPos;
                        ENDWHILE

                        msgBufferReadCurrPos := 1;
                        msgBufferReadPrevPos := 1;

                        msgBufferReadLine := msgBufferReadLine + 1;
                        IF msgBufferReadLine > msgBufferSize THEN
                            msgBufferReadLine := 1;
                            isMsgBufferWriteLineWrapped := FALSE;
                        ENDIF
                    ENDWHILE
                ENDPROC

                ! Parse a string representation of a statement into an Action
                ! and store it in the buffer.
                PROC ParseStatement(string st)
                    ! This assumes a string formatted in the following form:
                    ! [@IDNUM ]INSCODE[ ""stringParam""][ p0 p1 p2 ... p11]STR_MESSAGE_END_CHAR

                    VAR bool ok;
                    VAR bool end;
                    VAR num pos := 1;
                    VAR num nPos;
                    VAR string s;
                    VAR num len;
                    VAR num params{11};
                    VAR num paramsPos := 1;
                    VAR action a;

                    ! Sanity
                    len := StrLen(st);
                    IF len < 2 THEN
                        TPWrite ""MACHINA ERROR: received too short of a message:"";
                        TPWrite st;
                        IF USE_STRICT THEN EXIT; ENDIF
                    ENDIF

                    ! Does the message come with a leading ID?
                    IF StrPart(st, 1, 1) = STR_MESSAGE_ID_CHAR THEN  ! can't strings be treated as char arrays? st{1} = ... ?
                        nPos := StrFind(st, pos, STR_WHITE);
                        IF nPos > len THEN
                            TPWrite ""MACHINA ERROR: incorrectly formatted message:"";
                            TPWrite st;
                            IF USE_STRICT THEN EXIT; ENDIF
                        ENDIF

                        s := StrPart(st, 2, nPos - 2);
                        ok := StrToVal(s, a.id);
                        IF NOT ok THEN
                            TPWrite ""MACHINA ERROR: incorrectly formatted message:"";
                            TPWrite st;
                            IF USE_STRICT THEN EXIT; ENDIF
                            RETURN;
                        ENDIF

                        pos := nPos + 1;
                    ENDIF

                    ! Read instruction code
                    nPos := StrFind(st, pos, STR_WHITE + STR_MESSAGE_END_CHAR);
                    s := StrPart(st, pos, nPos - pos);
                    ok := StrToVal(s, a.code);

                    ! Couldn't read instruction code, discard this message
                    IF NOT ok THEN
                        TPWrite ""MACHINA ERROR: received corrupt message:"";
                        TPWrite st;
                        IF USE_STRICT THEN EXIT; ENDIF
                        RETURN;
                    ENDIF

                    ! Is there any string param?
                    pos := nPos + 1;
                    nPos := StrFind(st, pos, STR_DOUBLE_QUOTES);
                    IF nPos < len THEN
                        pos := nPos + 1;
                        nPos := StrFind(st, pos, STR_DOUBLE_QUOTES);  ! Find the matching double quote
                        IF nPos < len THEN
                            ! Succesful find of a double quote
                            a.s1 := StrPart(st, pos, nPos - pos);
                            pos := nPos + 2;  ! skip quotes and following char
                            ! Reached end of string?
                            IF pos > len THEN
                                end := TRUE;
                            ENDIF
                        ELSE
                            TPWrite ""MACHINA ERROR: corrupt message, missing closing double quotes"";
                            TPWrite st;
                            IF USE_STRICT THEN EXIT; ENDIF
                            RETURN;
                        ENDIF
                    ENDIF

                    ! Parse rest of numerical characters
                    WHILE end = FALSE DO
                        nPos := StrFind(st, pos, STR_WHITE + STR_MESSAGE_END_CHAR);
                        IF nPos > len THEN
                            end := TRUE;
                        ELSE
                            ! Parameters should be parsed differently depending on code
                            ! for example, a TPWrite action will have a string rather than nums...
                            s := StrPart(st, pos, nPos - pos);
                            ok := StrToVal(s, params{paramsPos});
                            IF ok = FALSE THEN
                                end := TRUE;
                                TPWrite ""MACHINA ERROR: received corrupt parameter:"";
                                TPWrite s;
                                IF USE_STRICT THEN EXIT; ENDIF
                            ENDIF
                            paramsPos := paramsPos + 1;
                            pos := nPos + 1;
                        ENDIF
                    ENDWHILE

                    ! Quick and dity to avoid a huge IF ELSE statement... unassigned vars use zeros
                    a.p1 := params{1};
                    a.p2 := params{2};
                    a.p3 := params{3};
                    a.p4 := params{4};
                    a.p5 := params{5};
                    a.p6 := params{6};
                    a.p7 := params{7};
                    a.p8 := params{8};
                    a.p9 := params{9};
                    a.p10 := params{10};
                    a.p11 := params{11};

                    ! Save it to the buffer
                    StoreAction a;

                ENDPROC

                ! Stores this action in the buffer
                PROC StoreAction(action a)
                    IF isActionPosWriteWrapped = TRUE AND actionPosWrite = actionPosExecute THEN
                        TPWrite ""MACHINA WARNING: memory overload. Maximum Action buffer size is "" + NumToStr(actionsBufferSize, 0);
                        TPWrite ""Reduce the amount of stream messages while they execute."";
                        EXIT;
                    ENDIF

                    actions{actionPosWrite} := a;
                    actionPosWrite := actionPosWrite + 1;

                    IF actionPosWrite > actionsBufferSize THEN
                        actionPosWrite := 1;
                        isActionPosWriteWrapped := TRUE;
                    ENDIF

                ENDPROC





                !  ╦ ╦╔╦╗╦╦  ╦╔╦╗╦ ╦  ╔═╗╦ ╦╔╗╔╔═╗╔╦╗╦╔═╗╔╗╔╔═╗
                !  ║ ║ ║ ║║  ║ ║ ╚╦╝  ╠╣ ║ ║║║║║   ║ ║║ ║║║║╚═╗
                !  ╚═╝ ╩ ╩╩═╝╩ ╩  ╩   ╚  ╚═╝╝╚╝╚═╝ ╩ ╩╚═╝╝╚╝╚═╝

                ! Initialize robot cursor values to current state and some defaults
                PROC CursorsInitialize()
                    msgBufferReadCurrPos := 1;
                    msgBufferReadPrevPos := 1;
                    msgBufferReadLine := 1;
                    msgBufferWriteLine := 1;
                    isMsgBufferWriteLineWrapped := FALSE;
                    streamBufferPending := FALSE;

                    actionPosWrite := 1;
                    actionPosExecute := 1;
                    isActionPosWriteWrapped := FALSE;

                    response := """";

                    cursorTool := tool0;
                    cursorWObj := wobj0;
                    cursorJoints := CJointT();
                    cursorTarget := CRobT();
                    cursorSpeed := v20;
                    cursorZone := z5;
                ENDPROC

                ! Return the jointtarget represented by an Action
                FUNC jointtarget GetJointTarget(action a)
                    RETURN [[a.p1, a.p2, a.p3, a.p4, a.p5, a.p6], [9E9,9E9,9E9,9E9,9E9,9E9]];
                ENDFUNC

                ! Return the robottarget represented by an Action
                FUNC robtarget GetRobTarget(action a)
                    RETURN [[a.p1, a.p2, a.p3], [a.p4, a.p5, a.p6, a.p7], [0,0,0,0], [9E9,9E9,9E9,9E9,9E9,9E9]];
                ENDFUNC

                FUNC speeddata GetSpeedData(action a)
                    ! Fill in the gaps
                    IF a.p2 = 0 THEN
                        a.p2 := a.p1;
                    ENDIF
                    IF a.p3 = 0 THEN
                        a.p3 := 5000;
                    ENDIF
                    IF a.p4 = 0 THEN
                        a.p4 := 1000;
                    ENDIF

                    RETURN [a.p1, a.p2, a.p3, a.p4];
                ENDFUNC

                ! Return the zonedata represented by an Action
                FUNC zonedata GetZoneData(action a)
                    IF a.p1 = 0 THEN
                        RETURN fine;
                    ENDIF

                    ! Fill in some gaps
                    IF a.p2 = 0 THEN
                        a.p2 := 1.5 * a.p1;
                    ENDIF
                    IF a.p3 = 0 THEN
                        a.p3 := 1.5 * a.p1;
                    ENDIF
                    IF a.p4 = 0 THEN
                        a.p4 := 0.1 * a.p1;
                    ENDIF
                    IF a.p5 = 0 THEN
                        a.p5 := 1.5 * a.p1;
                    ENDIF
                    IF a.p6 = 0 THEN
                        a.p6 := 0.1 * a.p1;
                    ENDIF

                    RETURN [FALSE, a.p1, a.p2, a.p3, a.p4, a.p5, a.p6];
                ENDFUNC

                ! Return the tooldata represented by an Action
                FUNC tooldata GetToolData(action a)
                    ! If missing weight info
                    IF a.p8 = 0 THEN
                        a.p8 := 1;
                    ENDIF

                    ! If missing center of gravity info
                    IF a.p9 = 0 THEN
                        a.p9 := 0.5 * a.p1;
                    ENDIF
                    IF a.p10 = 0 THEN
                        a.p10 := 0.5 * a.p2;
                    ENDIF
                    IF a.p11 = 0 THEN
                        a.p11 := 0.5 * a.p3;
                    ENDIF

                    RETURN [TRUE, [[a.p1, a.p2, a.p3], [a.p4, a.p5, a.p6, a.p7]],
                        [a.p8, [a.p9, a.p10, a.p11], [1, 0, 0, 0], 0, 0, 0]];
                ENDFUNC

                ! TPWrite a string representation of an Action
                PROC log(action a)
                    TPWrite ""ACTION: "" + NumToStr(a.code, 0) + "" ""
                        + a.s1 + "" ""
                        + NumToStr(a.p1, 0) + "" "" + NumToStr(a.p2, 0) + "" ""
                        + NumToStr(a.p3, 0) + "" "" + NumToStr(a.p4, 0) + "" ""
                        + NumToStr(a.p5, 0) + "" "" + NumToStr(a.p6, 0) + "" ""
                        + NumToStr(a.p7, 0) + "" "" + NumToStr(a.p8, 0) + "" ""
                        + NumToStr(a.p9, 0) + "" "" + NumToStr(a.p10, 0) + "" ""
                        + NumToStr(a.p11, 0) + STR_MESSAGE_END_CHAR;
                ENDPROC

            ENDMODULE";


        

































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
                if (masterControl.GetControlMode() == ControlType.Execute)
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

        ///// <summary>
        ///// What to do when the 'pnum' rapid var changes value
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void OnRD_pnum_ValueChanged(object sender, DataValueChangedEventArgs e)
        //{
        //    RapidData rd = (RapidData)sender;
        //    Console.WriteLine("   variable '{0}' changed: {1}", rd.Name, rd.StringValue);

        //    // Do not add target if pnum is being reset to initial value (like on program load)
        //    if (!rd.StringValue.Equals("-1"))
        //    {

        //        // If no actions pending, raise BufferEmpty event
        //        if (writeCursor.AreActionsPending())
        //        {
        //            Console.WriteLine("Ticking from pnum event handler");
        //            TickStreamQueue(true);
        //        }
        //        else
        //        {
        //            Console.WriteLine("Raising OnBufferEmpty event");
        //            masterControl.parent.OnBufferEmpty(EventArgs.Empty);
        //        }
        //    }

        //    if (rd != null)
        //    {
        //        //Console.WriteLine("Disposing rd");
        //        //rd.Dispose();
        //        //rd = null;
        //    }

        //}

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
