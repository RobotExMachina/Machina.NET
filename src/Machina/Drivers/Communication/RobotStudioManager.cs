using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

using Machina.Users;

using ABB.Robotics;
using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers.RapidDomain;
using ABB.Robotics.Controllers.EventLogDomain;
using ABB.Robotics.Controllers.FileSystemDomain;

namespace Machina.Drivers.Communication
{
    /// <summary>
    /// This class acts as a bridge between Machina and the ABB controller, 
    /// using RobotStudio's SDK. 
    /// Ideally, this will be abstracted somewhere else in the future, so that
    /// Machina doesn't have this dependencies...
    /// </summary>
    class RobotStudioManager
    {
        private Driver _parentDriver;  // the parent object
        private RobotLogger logger;

        // ABB stuff and flags
        private Controller controller;
        private ABB.Robotics.Controllers.RapidDomain.Task tRob1Task;
        private RobotWare robotWare;
        private RobotWareOptionCollection robotWareOptions;
        private int _deviceId;
        private bool _hasMultiTasking = false;
        private bool _hasEGM = false;
        private bool _isLogged = false;
        private bool _isRunning = false;
        private bool _isConnected = false;
        public bool Connected => _isConnected;

        private string _ip = "";
        public string IP => _ip;
        private int _port = 7000;
        public int Port => _port;

        //private int _writePort;
        //public int WritePort => _writePort;
        //private int _readPort;
        //public int ReadPort => _readPort;
        
        private string _streamingModule;
            
        private const string REMOTE_BUFFER_DIR = "Machina";

        

        public RobotStudioManager(Driver parent)
        {
            _parentDriver = parent;
            logger = _parentDriver.parentControl.logger;
        }


        /// <summary>
        /// Reverts the Comm object to a blank state before any connection attempt. 
        /// </summary>
        public bool Disconnect()
        {
            StopProgramExecution(true);
            ReleaseIP();
            LogOff();
            ReleaseMainTask();
            ReleaseController();
            _isConnected = false;
            return !_isConnected;
        }

        /// <summary>
        /// Performs all necessary steps to successfuly connect to the device using the RobotStudio API.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public bool Connect(int deviceId)
        {
            _deviceId = deviceId;

            // In general, the Disconnect() + return false pattern instead of throwing errors
            // avoids the controller getting hung with an undisposed mastership or log from the client,
            // making failures a little more robust (and less annoying...)

            // Connect to the ABB real/virtual controller
            if (!LoadController(deviceId))
            {
                logger.Warning("Could not connect to controller, found no device on the network");
                Disconnect();
                return false;
            }

            // Load the controller's IP
            if (!LoadIP())
            {
                logger.Warning("Could not connect to controller, failed to find the controller's IP");
                Disconnect();
                return false;
            }

            // Log on to the controller
            if (!LogOn())
            {
                logger.Warning("Could not connect to controller, failed to log on to the controller");
                Disconnect();
                return false;
            }

            if (!LoadRobotWareOptions())
            {
                logger.Warning("Could not connect to controller, failed to retrieve RobotWare options from the controller");
                Disconnect();
                return false;
            }

            // Is controller in Automatic mode with motors on?
            if (!IsControllerInAutoMode())
            {
                logger.Warning("Could not connect to controller, please set up controller to AUTOMATIC MODE and try again.");
                Disconnect();
                return false;
            }

            if (!IsControllerMotorsOn())
            {
                logger.Warning("Could not connect to controller, please set up Motors On mode in controller");
                Disconnect();
                return false;
            }

            // Test if Rapid Mastership is available
            if (!TestMastershipRapid())
            {
                logger.Warning("Could not connect to controller, mastership not available");
                Disconnect();
                return false;
            }

            // Load main task from the controller
            if (!LoadMainTask())
            {
                logger.Warning("Could not connect to controller, failed to load main task");
                Disconnect();
                return false;
            }

            if (!SetRunMode(CycleType.Once))
            {
                logger.Warning("Could not connect to controller, failed to set runmode to once");
                Disconnect();
                return false;
            }

            // Subscribe to relevant events to keep track of robot execution
            if (!SubscribeToEvents())
            {
                logger.Warning("Could not connect to controller, failed to subscribe to robot controller events");
                Disconnect();
                return false;
            }

            _isConnected = true;

            return _isConnected;
        }

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
            logger.Verbose("Scanning the network for controllers...");
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
                    logger.Verbose($"Found controller {controller.SystemName} on {controller.Name}");
                    success = true;

                    logger.Debug(controller);
                    //Console.WriteLine(controller.RobotWare);
                    //Console.WriteLine(controller.RobotWareVersion);

                    //try
                    //{
                    //    this.robotWare = this.controller.RobotWare;
                    //    this.robotWareOptions = this.robotWare.Options;

                    //    this._hasMultiTasking = HasMultiTaskOption(this.robotWareOptions);
                    //    this._hasEGM = HasEGMOption(this.robotWareOptions);
                    //}
                    //catch
                    //{
                    //    Console.WriteLine("Could not access ROBOTWARE options");
                    //}
                }
                else
                {
                    logger.Debug("Could not connect to controller...");
                }
            }
            else
            {
                logger.Debug("No controllers found on the network");
            }

            //if (!success)
            //{
            //    Disconnect();
            //    //throw new Exception("ERROR: could not LoadController()");
            //}

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
                this._ip = controller.IPAddress.ToString();
                logger.Debug($"Loaded IP {this._ip}");
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
            this._ip = "";
            return true;
        }

        /// <summary>
        /// Logs on to the controller with a default user.
        /// </summary>
        /// <returns></returns>
        private bool LogOn()
        {
            // Sanity
            if (_isLogged) LogOff();

            if (controller != null)
            {
                try
                {
                    User user = this._parentDriver.User;
                    UserInfo robotStudioUser = user.Name == "" ?
                        UserInfo.DefaultUser :
                        new UserInfo(user.Name, user.Password);

                    controller.Logon(robotStudioUser);
                    logger.Debug($"Logged on as {robotStudioUser} user");
                    _isLogged = true;
                }
                catch (Exception ex)
                {
                    logger.Debug("Could not log on to the controller");
                    logger.Debug(ex);
                    _isLogged = false;
                }
            }

            return _isLogged;
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
            _isLogged = false;
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
                logger.Debug("Cannot retreive main task: no controller available");
                return false;
            }

            try
            {
                ABB.Robotics.Controllers.RapidDomain.Task[] tasks = controller.Rapid.GetTasks();
                if (tasks.Length > 0)
                {
                    tRob1Task = tasks[0];
                    logger.Debug("Retrieved task " + tRob1Task.Name);
                    return true;
                }
                else
                {
                    tRob1Task = null;
                    logger.Debug("Could not retrieve any task from the controller");
                }
            }
            catch (Exception ex)
            {
                logger.Debug("Could not retrieve main task from controller");
                logger.Debug(ex);
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
            if (tRob1Task != null)
            {
                tRob1Task.Dispose();
                tRob1Task = null;
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
                        logger.Debug("Mastership test OK");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("Rapid Mastership not available");
                    logger.Debug(ex);
                }
            }
            else
            {
                logger.Debug("Cannot test Rapid Mastership, no controller available");
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
                logger.Debug("Cannot subscribe to controller events: not connected to controller.");
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
                logger.Debug("Cannot clear modules: not connected to controller");
                return -1;
            }

            if (tRob1Task == null)
            {
                logger.Debug("Cannot clear modules: main task not retrieved");
                return -1;
            }

            int count = 0;
            try
            {
                using (Mastership.Request(controller.Rapid))
                {
                    ABB.Robotics.Controllers.RapidDomain.Module[] modules = tRob1Task.GetModules();
                    foreach (ABB.Robotics.Controllers.RapidDomain.Module m in modules)
                    {
                        logger.Verbose($"Deleting module: {m.Name}");
                        m.Delete();
                        count++;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Debug($"CLEAR MODULES ERROR: {ex}");
            }

            logger.Debug($"Cleared {count} modules from main task");
            return count;
        }


        /// <summary>
        /// Resets the program pointer in the controller to the main entry point. Needs to be called
        /// before starting execution of a program, otherwise the controller will throw an error. 
        /// </summary>
        internal bool ResetProgramPointer()
        {
            if (controller == null)
            {
                logger.Debug("Cannot reset pointer: not connected to controller");
                return false;
            }

            if (tRob1Task == null)
            {
                logger.Debug("Cannot reset pointer: mainTask not present");
                return false;
            }

            try
            {
                using (Mastership.Request(controller.Rapid))
                {
                    tRob1Task.ResetProgramPointer();
                    return true;
                }

            }
            catch (Exception ex)
            {
                logger.Debug("Cannot reset pointer...");
                logger.Debug(ex);
            }

            return false;
        }

        /// <summary>
        /// Try to fetch RW options from this robot.
        /// </summary>
        /// <returns></returns>
        private bool LoadRobotWareOptions()
        {
            try
            {
                this.robotWare = this.controller.RobotWare;
                this.robotWareOptions = this.robotWare.Options;
                this._hasMultiTasking = HasMultiTaskOption(this.robotWareOptions);
                this._hasEGM = HasEGMOption(this.robotWareOptions);

                logger.Debug("RobotWare " + controller.RobotWare);
                logger.Debug("RobotWareVersion " + controller.RobotWareVersion);
                logger.Debug("hasMultiTasking? " + this._hasMultiTasking);
                logger.Debug("hasEGM? " + this._hasEGM);
                return true;

            }
            catch
            {
                this.robotWare = null;
                this.robotWareOptions = null;
                this._hasMultiTasking = false;
                this._hasEGM = false;
                logger.Debug("Could not access ROBOTWARE options");
            }
            return false;
        }

        /// <summary>
        /// Does this robot have the MultiTask option?
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private bool HasMultiTaskOption(RobotWareOptionCollection options)
        {
            var available = false;
            foreach (RobotWareOption option in options)
            {
                if (option.Description.Contains("623-1"))
                {
                    available = true;
                    break;
                }
            }
            return available;
        }

        /// <summary>
        /// Does this robot have the Externally Guided Motion option?
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private bool HasEGMOption(RobotWareOptionCollection options)
        {
            var available = false;
            foreach (RobotWareOption option in options)
            {
                if (option.Description.Contains("689-1"))
                {
                    available = true;
                    break;
                }
            }
            return available;
        }


        /// <summary>
        /// Requests start executing the program in the main task. Remember to call ResetProgramPointer() before. 
        /// </summary>
        public bool StartProgramExecution()
        {
            if (!_isConnected)
            {
                logger.Debug("Cannot start program: not connected to controller");
                return false;
            }

            if (_isRunning)
            {
                logger.Debug("Program is already running...");
                return false;
            }

            if (!ResetProgramPointer())
            {
                logger.Debug("Cannot start program: cannot reset program pointer");
                return false;
            }

            try
            {
                using (Mastership.Request(controller.Rapid))
                {
                    bool isControllerRunning = controller.Rapid.ExecutionStatus == ExecutionStatus.Running;

                    if (isControllerRunning != _isRunning)
                    {
                        throw new Exception("isRunning mismatch state");
                    }

                    StartResult res = controller.Rapid.Start(true);
                    if (res != StartResult.Ok)
                    {
                        logger.Debug($"Cannot start program: {res}");
                    }
                    else
                    {
                        _isRunning = true;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Debug("PROGRAM START ERROR: " + ex);
            }

            return false;
        }


        /// <summary>
        /// Requests stop executing the program in the main task.
        /// </summary>
        /// <param name="immediate">Stop right now or wait for current cycle to complete?</param>
        /// <returns></returns>
        public bool StopProgramExecution(bool immediate)
        {
            if (controller == null) return true;

            if (!_isConnected)
            {
                logger.Debug("Cannot stop program: not connected to controller");
                return false;
            }

            if (!_isRunning)
            {
                logger.Debug("Cannot stop program: execution is already stopped");
                return false;
            }

            try
            {
                using (Mastership.Request(controller.Rapid))
                {
                    controller.Rapid.Stop(immediate ? StopMode.Immediate : StopMode.Cycle);
                    _isRunning = false;
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Debug("Could not stop program...");
                logger.Debug(ex);
            }

            return false;
        }

        /// <summary>
        /// Sets the Rapid ExecutionCycle to Once, Forever or None.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool SetRunMode(CycleType mode)
        {
            if (controller == null)
            {
                logger.Debug("Cannot set RunMode, not connected to any controller");
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
                logger.Debug("Error setting RunMode in controller...");
                logger.Debug(ex);
            }

            return false;
        }



        public bool SetupStreamingMode()
        {
            if (!LoadDriverScript())
            {
                logger.Debug("Could not setup streaming modules");
                return false;
            }

            if (!UploadStreamingModules())
            {
                logger.Debug("Could not upload streaming modules");
                return false;
            }

            if (!ResetProgramPointer())
            {
                logger.Debug("Could not reset the program pointer");
                return false;
            }

            if (!StartProgramExecution())
            {
                logger.Debug("Could not load start the streaming module");
                return false;
            }

            return true;
        }


        ///// <summary>
        ///// Read
        ///// </summary>
        ///// <returns></returns>
        //private bool SetupStreamingModules()
        //{
        //    //if (hasMultiTasking)
        //    //{
        //    //    // TODO: depending on availability of multitasking, upload different modules... 
        //    //}


        //    // Read the resource as a string
        //    _streamingModule = Machina.IO.ReadTextResource("Machina.Resources.DriverModules.ABB.SingleTask.Machina_ABB_Server_SingleTask.mod");

        //    // Get the port number/s from the file's "CONST num SERVER_PORT := 7000;"
        //    // @TODO: this is super flimsy, use regex here...
        //    int portPos = _streamingModule.IndexOf("SERVER_PORT") + 15;
        //    string portStr = _streamingModule.Substring(portPos, 4);
        //    _writePort = Convert.ToInt32(portStr);

        //    _driverScript = _driverScript.Replace("{{HOSTNAME}}", _serverIP);
        //    _driverScript = _driverScript.Replace("{{PORT}}", _serverPort.ToString());

        //    logger.Debug($"_writePort set to {_writePort}");

        //    // Replace the IP in the module with the one found by this manager: "CONST string SERVER_IP := "127.0.0.1";"
        //    _streamingModule = _streamingModule.Replace("127.0.0.1", IP);

        //    return true;
        //}

        private bool LoadDriverScript()
        {
            //if (hasMultiTasking)
            //{
            //    // TODO: depending on availability of multitasking, upload different modules... 
            //}

            // Read the resource as a string
            _streamingModule = Machina.IO.ReadTextResource("Machina.Resources.DriverModules.ABB.SingleTask.Machina_ABB_Server_SingleTask.mod");

            // @TODO: remove comments, trailing spaces and empty lines from script
            _streamingModule = _streamingModule.Replace("{{HOSTNAME}}", IP);
            _streamingModule = _streamingModule.Replace("{{PORT}}", Port.ToString());

            logger.Debug($"Loaded ABB Driver module and cofigured to {IP}:{Port}");

            return true;
        }


        /// <summary>
        /// Loads the default StreamModule designed for streaming.
        /// </summary>
        internal bool UploadStreamingModules()
        {
            //if (hasMultiTasking)
            //{
            //    // TODO: depending on availability of multitasking, upload different modules... 
            //}

            return LoadModuleToController(_streamingModule, "Machina_ABB_Server_SingleTask.mod");
        }



        /// <summary>
        /// Loads a module to the device from a text resource in the assembly, with a target name on the controller.
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="targetName"></param>
        /// <returns></returns>
        public bool LoadModuleToController(string module, string targetName)
        {
            if (!_isConnected)
            {
                throw new Exception("Cannot load program, not connected to controller");
            }

            string path = Path.Combine(Path.GetTempPath(), targetName);
            if (!Machina.IO.SaveStringToFile(path, module, Encoding.ASCII))
            {
                throw new Exception("Could not save module to temp file");
            }
            else
            {
                logger.Debug("Saved module to " + path);
            }

            if (!LoadFileToDevice(path))
            {
                throw new Exception("Could not load module to controller");
            }

            return true;
        }

        ///// <summary>
        ///// Loads a module to the ABB controller given as a string list of Rapid code lines.
        ///// </summary>
        ///// <param name="module"></param>
        ///// <param name="programName"></param>
        ///// <returns></returns>
        //public bool LoadProgramToController(List<string> module, string programName)
        //{
        //    if (!isConnected)
        //    {
        //        Console.WriteLine("Cannot load program, not connected to controller");
        //        return false;
        //    }

        //    string path = Path.Combine(Path.GetTempPath(), $"Machina_{programName}.mod");

        //    // Modules can only be uploaded to ABB controllers using a file
        //    if (!Machina.IO.SaveStringListToFile(path, module, Encoding.ASCII))  // 
        //    {
        //        Console.WriteLine("Could not save module to temp file");
        //        return false;
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Saved {programName} to {path}");
        //    }

        //    if (!LoadFileToDevice(path))
        //    {
        //        Console.WriteLine("Could not load module to controller");
        //        return false;
        //    }

        //    return true;
        //}

        /// <summary>
        /// Loads a module into de controller from a local file. 
        /// @TODO: This is an expensive operation, should probably become threaded. 
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="wipeout"></param>
        /// <returns></returns>
        public bool LoadFileToDevice(string fullPath, bool wipeout = true)
        {
            string extension = Path.GetExtension(fullPath),     // ".mod"
                filename = Path.GetFileName(fullPath);          // "Machina_Server.mod"

            if (!_isConnected)
            {
                throw new Exception($"Could not load module '{fullPath}', not connected to controller");
            }

            // check for correct ABB file extension
            if (!extension.ToLower().Equals(".mod"))
            {
                throw new Exception("Wrong file type, must use .mod files for ABB robots");
            }

            // For the time being, we will always wipe out previous modules on load
            if (ClearAllModules() < 0)
            {
                throw new Exception("Error clearing modules");
            }

            // Load the module
            bool success = false;
            try
            {
                using (Mastership.Request(controller.Rapid))
                {
                    // When connecting to a real controller, the reference filesystem 
                    // for Task.LoadModuleFromFile() becomes the controller's, so it is necessary
                    // to copy the file to the system first, and then load it. 

                    // Create the remoteBufferDirectory if applicable
                    FileSystem fs = controller.FileSystem;
                    string remotePath = fs.RemoteDirectory + "/" + REMOTE_BUFFER_DIR;
                    bool dirExists = fs.DirectoryExists(REMOTE_BUFFER_DIR);
                    if (!dirExists)
                    {
                        logger.Debug($"Creating {remotePath} on remote controller");
                        fs.CreateDirectory(REMOTE_BUFFER_DIR);
                    }

                    //@TODO: Should implement some kind of file cleanup at somepoint...

                    // Copy the file to the remote controller
                    controller.FileSystem.PutFile(fullPath, $"{REMOTE_BUFFER_DIR}/{filename}", wipeout);
                    logger.Debug($"Copied {filename} to {REMOTE_BUFFER_DIR}");

                    // Loads a Rapid module to the task in the robot controller
                    success = tRob1Task.LoadModuleFromFile($"{remotePath}/{filename}", RapidLoadMode.Replace);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("ERROR: Could not load module");
                logger.Debug(ex);
                throw new Exception("ERROR: Could not load module");
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
                logger.Debug($"Sucessfully loaded {fullPath}");
            }

            return success;
        }


        /// <summary>
        /// Returns a Vector object representing the current robot's TCP position.
        /// </summary>
        /// <returns></returns>
        public Vector GetCurrentPosition()
        {
            if (!_isConnected)
            {
                logger.Debug("Cannot GetCurrentPosition: not connected to controller");
                return null;
            }

            RobTarget rt = controller.MotionSystem.ActiveMechanicalUnit.GetPosition(ABB.Robotics.Controllers.MotionDomain.CoordinateSystemType.World);

            return new Vector(rt.Trans.X, rt.Trans.Y, rt.Trans.Z);
        }

        /// <summary>
        /// Returns a Rotation object representing the current robot's TCP orientation.
        /// </summary>
        /// <returns></returns>
        public Rotation GetCurrentOrientation()
        {
            if (!_isConnected)
            {
                logger.Debug("Cannot GetCurrentRotation, not connected to controller");
                return null;
            }

            RobTarget rt = controller.MotionSystem.ActiveMechanicalUnit.GetPosition(ABB.Robotics.Controllers.MotionDomain.CoordinateSystemType.World);

            // ABB's convention is Q1..Q4 as W..Z
            return Rotation.FromQuaternion(rt.Rot.Q1, rt.Rot.Q2, rt.Rot.Q3, rt.Rot.Q4);
        }


        /// <summary>
        /// Returns a Joints object representing the rotations of the 6 axes of this robot.
        /// </summary>
        /// <returns></returns>
        public Joints GetCurrentJoints()
        {
            if (!_isConnected)
            {
                logger.Debug("Cannot GetCurrentJoints, not connected to controller");
                return null;
            }

            try
            {
                JointTarget jt = controller.MotionSystem.ActiveMechanicalUnit.GetPosition();
                return new Joints(jt.RobAx.Rax_1, jt.RobAx.Rax_2, jt.RobAx.Rax_3, jt.RobAx.Rax_4, jt.RobAx.Rax_5, jt.RobAx.Rax_6);
            }
            catch (ABB.Robotics.Controllers.ServiceNotSupportedException e)
            {
                logger.Debug("CANNOT RETRIEVE JOINTS FROM CONTROLLER");
                logger.Debug(e);
                return null;
            }
        }

        /// <summary>
        /// Dumps a bunch of controller info to the console.
        /// </summary>
        public void DebugDump()
        {
            if (_isConnected)
            {
                logger.Debug("");
                logger.Debug("DEBUG CONTROLLER DUMP:");
                logger.Debug("     AuthenticationSystem: " + controller.AuthenticationSystem.Name);
                logger.Debug("     BackupInProgress: " + controller.BackupInProgress);
                logger.Debug("     Configuration: " + controller.Configuration);
                logger.Debug("     Connected: " + controller.Connected);
                logger.Debug("     CurrentUser: " + controller.CurrentUser);
                logger.Debug("     DateTime: " + controller.DateTime);
                logger.Debug("     EventLog: " + controller.EventLog);
                logger.Debug("     FileSystem: " + controller.FileSystem);
                logger.Debug("     IOSystem: " + controller.IOSystem);
                logger.Debug("     IPAddress: " + controller.IPAddress);
                logger.Debug("     Ipc: " + controller.Ipc);
                logger.Debug("     IsMaster: " + controller.IsMaster);
                logger.Debug("     IsVirtual: " + controller.IsVirtual);
                logger.Debug("     MacAddress: " + controller.MacAddress);
                //Console.WriteLine("     MainComputerServiceInfo: ");
                //Console.WriteLine("         BoardType: " + controller.MainComputerServiceInfo.BoardType);
                //Console.WriteLine("         CpuInfo: " + controller.MainComputerServiceInfo.CpuInfo);
                //Console.WriteLine("         RamSize: " + controller.MainComputerServiceInfo.RamSize);
                //Console.WriteLine("         Temperature: " + controller.MainComputerServiceInfo.Temperature);
                logger.Debug("     MastershipPolicy: " + controller.MastershipPolicy);
                logger.Debug("     MotionSystem: " + controller.MotionSystem);
                logger.Debug("     Name: " + controller.Name);
                //Console.WriteLine("     NetworkSettings: " + controller.NetworkSettings);
                logger.Debug("     OperatingMode: " + controller.OperatingMode);
                logger.Debug("     Rapid: " + controller.Rapid);
                logger.Debug("     RobotWare: " + controller.RobotWare);
                logger.Debug("     RobotWareVersion: " + controller.RobotWareVersion);
                logger.Debug("     RunLevel: " + controller.RunLevel);
                logger.Debug("     State: " + controller.State);
                logger.Debug("     SystemId: " + controller.SystemId);
                logger.Debug("     SystemName: " + controller.SystemName);
                //Console.WriteLine("     TimeServer: " + controller.TimeServer);
                //Console.WriteLine("     TimeZone: " + controller.TimeZone);
                //Console.WriteLine("     UICulture: " + controller.UICulture);

                logger.Debug("");
                logger.Debug("DEBUG TASK DUMP:");
                logger.Debug("    Cycle: " + tRob1Task.Cycle);
                logger.Debug("    Enabled: " + tRob1Task.Enabled);
                logger.Debug("    ExecutionStatus: " + tRob1Task.ExecutionStatus);
                try
                {
                    logger.Debug("    ExecutionType: " + tRob1Task.ExecutionType);
                }
                catch (ABB.Robotics.Controllers.ServiceNotSupportedException e)
                {
                    logger.Debug("    ExecutionType: UNSUPPORTED BY CONTROLLER");
                    logger.Debug(e);
                }
                logger.Debug("    Motion: " + tRob1Task.Motion);
                logger.Debug("    MotionPointer: " + tRob1Task.MotionPointer.Module);
                logger.Debug("    Name: " + tRob1Task.Name);
                logger.Debug("    ProgramPointer: " + tRob1Task.ProgramPointer.Module);
                logger.Debug("    RemainingCycles: " + tRob1Task.RemainingCycles);
                logger.Debug("    TaskType: " + tRob1Task.TaskType);
                logger.Debug("    Type: " + tRob1Task.Type);
                logger.Debug("");

                logger.Debug("HAS MULTITASKING: " + this._hasMultiTasking);
                logger.Debug("HAS EGM: " + this._hasEGM);

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
            logger.Debug("EXECUTION STATUS CHANGED: " + e.Status);

            if (e.Status == ExecutionStatus.Running)
            {
                _isRunning = true;
            }
            else
            {
                _isRunning = false;

                //// Only trigger Instruct queue
                //if (masterControl.GetControlMode() == ControlType.Execute)
                //{
                //    // Tick queue to move forward
                //    //masterControl.TriggerQueue();
                //    masterControl.TickWriteCursor();
                //}
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
            logger.Debug($"RAPID MASTERSHIP STATUS CHANGED: {e.Status}");

            // @TODO: what to do when mastership changes
        }

        /// <summary>
        /// What to do when the Task Enabled property changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRapidTaskEnabledChanged(object sender, TaskEnabledChangedEventArgs e)
        {
            logger.Debug($"TASK ENABLED CHANGED: {e.Enabled}");

            // @TODO: add behaviors
        }

        /// <summary>
        /// What to do when the controller changes Operating Mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOperatingModeChanged(object sender, OperatingModeChangeEventArgs e)
        {
            logger.Debug($"OPERATING MODE CHANGED: {e.NewMode}");
        }


        private void OnStateChanged(object sender, StateChangedEventArgs e)
        {
            logger.Debug($"CONTROLLER STATECHANGED: {e.NewState}");
        }

        private void OnMastershipChanged(object sender, MastershipChangedEventArgs e)
        {
            logger.Debug($"CONTROLLER MASTERSHIP CHANGED: {e.Status}");
        }

        private void OnConnectionChanged(object sender, ConnectionChangedEventArgs e)
        {
            logger.Debug($"CONTROLLER CONNECTION CHANGED: {e.Connected}");
        }



    }
}
