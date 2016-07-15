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


namespace RobotControl
{
    /// <summary>
    /// A class to handle communication with external controllers, real or virtual
    /// </summary>
    abstract class Communication
    {
        // Public properties
        protected bool isConnected = false;
        protected string IP = "";

        // Abstract methods required in subclasses
        public abstract void Reset();
        public abstract bool ConnectToDevice(int deviceId);
        public abstract bool DisconnectFromDevice();
        public abstract bool SetRunMode(RunMode mode);
        public abstract bool LoadProgramFromFilename(string dirname, string filename, string extension);
        public abstract bool LoadProgramFromStringList(List<string> program);

        // Base constructor
        public Communication() { }

        public bool IsConnected()
        {
            return isConnected;
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

    class CommunicationABB : Communication
    {
        // ABB stuff and flags
        private Controller controller;
        private ABB.Robotics.Controllers.RapidDomain.Task mainTask;
        private bool isLogged = false;
        //private bool isMainTaskRetrieved = false;                         // just do null check on the mainTask object
        private static string localBufferDirname = "C:";                 // Route names to be used for file handling
        private static string localBufferFilename = "buffer";
        private static string localBufferFileExtension = "mod";
        private static string remoteBufferDirectory = "RobotControl";



        //██████╗ ██╗   ██╗██████╗ ██╗     ██╗ ██████╗
        //██╔══██╗██║   ██║██╔══██╗██║     ██║██╔════╝
        //██████╔╝██║   ██║██████╔╝██║     ██║██║     
        //██╔═══╝ ██║   ██║██╔══██╗██║     ██║██║     
        //██║     ╚██████╔╝██████╔╝███████╗██║╚██████╗
        //╚═╝      ╚═════╝ ╚═════╝ ╚══════╝╚═╝ ╚═════╝
                                            
        /// <summary>
        /// Main constructor
        /// </summary>
        public CommunicationABB() : base() 
        {
            Reset();
        }

        /// <summary>
        /// Reverts the Comm object to a blank state before any connection attempt. 
        /// </summary>
        public override void Reset()
        {
            //if (SafetyStopImmediateOnDisconnect) StopProgram(true);

            // revert to a pristine state before any connection attempt
            // logoff, disconnect, dispose mainTask, turn flags off...
            ReleaseIP();
            LogOff();
            ReleaseMainTask();
            ReleaseController();
        }

        /// <summary>
        /// Performs all necessary actions to establish a connection to a real/virtual device, 
        /// including connecting to the controller, loggin in, etc.
        /// </summary>
        /// <param name="deviceId"></param>
        public override bool ConnectToDevice(int deviceId)
        {
            isConnected = false;
            bool good = true;

            // Connect to the ABB real/virtual controller
            good = good && LoadController(deviceId);
            if (!good)
            {
                Console.WriteLine("Could not connect to controller");
                Reset();
                return false;
            }

            // Load the controller's IP
            good = good && LoadIP();
            if (!good)
            {
                Console.WriteLine("Could not find the controller's IP");
                Reset();
                return false;
            }

            // Log on to the controller
            good = good && LogOn();
            if (!good)
            {
                Console.WriteLine("Could not log on to the controller");
                Reset();
                return false;
            }

            // @TODO" IS IN AUTOMODE with motors on?

            // Test if Rapid Mastership is available
            good = good && TestMastershipRapid();
            if (!good)
            {
                Console.WriteLine("Mastership not available");
                Reset();
                return false;
            }

            // Load main task from the controller
            good = good && LoadMainTask();
            if (!good)
            {
                Console.WriteLine("Could not load main task");
                Reset();
                return false;
            }

            // @TODO: SubscribeToEvents(), like execution changed, etc.

            // @TODO: deal with isConnected at the end, when everything was successful
            isConnected = good;

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
            bool success = false;
            if (isConnected)
            {
                try
                {
                    using (Mastership.Request(controller.Rapid))
                    {
                        controller.Rapid.Cycle = mode == RunMode.Once ? ExecutionCycle.Once : mode == RunMode.Loop ? ExecutionCycle.Forever : ExecutionCycle.None;
                        success = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error setting RunMode in controller...");
                    Console.WriteLine(ex);
                }
            }
            else
            {
                Console.WriteLine("Cannot set RunMode, not connected to any controller");
            }
            return success;
        }

        /// <summary>
        /// Loads a module to the ABB controller given as a string list of Rapid code lines.
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public override bool LoadProgramFromStringList(List<string> module)
        {
            // Modules can only be uploaded to ABB controllers using a file
            if (!SaveProgramToFilename(module, string.Format(@"{0}\{1}.{2}", localBufferDirname, localBufferFilename, localBufferFileExtension)))
            {
                Console.WriteLine("Could not save module to file");
                return false;
            }

            if (!LoadProgramFromFilename(localBufferDirname, localBufferFilename, localBufferFileExtension))
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
        public override bool LoadProgramFromFilename(string dirname, string filename, string extension)
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

                    // @TODO: create dedicated methods for all this stuff, and invoke them form a generic Connect() method
                    //IP = controller.IPAddress.ToString();
                    //if (DEBUG) Console.WriteLine("Found controller on " + IP);

                    //LogOn();
                    //RetrieveMainTask();
                    //if (TestMastership()) SetRunMode(RunMode.Once);  // why was this here? 
                    //SubscribeToEvents();
                }
                else
                {
                    Console.WriteLine("Could not connect to controller...");
                    //isConnected = false;
                }

            }
            else
            {
                Console.WriteLine("No controllers found on the network");
                //isConnected = false;
            }

            // @TODO: abstract all this somewhere else
            //// Pick up the state of the robot if doing Stream mode
            //if (controlMode == ControlMode.Stream)
            //{
            //    LoadStreamingModule();
            //    HookUpStreamingVariables();
            //    //TCPPosition = new Point(GetTCPRobTarget().Trans);
            //    TCPPosition = GetTCPPosition();
            //    //TCPRotation = new Rotation(GetTCPRobTarget().Rot);
            //    TCPRotation = GetTCPRotation();
            //    if (DEBUG) Console.WriteLine("Current TCP Position: {0}", TCPPosition);
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
        /// Loads the main task in the ABB controller, typically 't_rob1'.
        /// </summary>
        /// <returns></returns>
        private bool LoadMainTask()
        {
            bool success = false;
            ABB.Robotics.Controllers.RapidDomain.Task[] tasks = controller.Rapid.GetTasks();
            if (tasks.Length > 0)
            {
                success = true;
                mainTask = tasks[0];
            }
            else
            {
                Console.WriteLine("Could not retrieve any task from the controller");
                mainTask = null;
            }
            return success;
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
            bool available = false;
            if (controller != null)
            {
                try
                {
                    using (Mastership.Request(controller.Rapid))
                    {
                        // Gets the current execution cycle from the RAPID module and sets it back to the same value
                        ExecutionCycle mode = controller.Rapid.Cycle;
                        controller.Rapid.Cycle = mode;
                        available = true;
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
            return available;
        }

        /// <summary>
        /// Upon connection, subscribe to relevant events in the controller and handle them.
        /// </summary>
        private bool SubscribeToEvents()
        {
            if (controller == null)
            {
                Console.WriteLine("Can't subscribe to controller events, not connected to controller...");
            }
            else
            {
                // Suscribe to Rapid program execution (Start, Stop...)
                controller.Rapid.ExecutionStatusChanged += OnExecutionStatusChanged;

                // Suscribe to Mastership changes 
                controller.Rapid.MastershipChanged += OnMastershipChanged;

                // Suscribe to Task Enabled changes
                controller.Rapid.TaskEnabledChanged += OnTaskEnabledChanged;

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
                Console.WriteLine("Cannot clear modules, not connected to controller");
                return -1;
            }

            if (mainTask == null)
            {
                Console.WriteLine("Cannot clear modules, main task not retrieved");
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
            if (mainTask == null)
            {
                Console.WriteLine("Cannot reset pointer, mainTask not present");
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


        /// <summary>
        /// Requests start executing the program in the main task. Remember to call ResetProgramPointer() before. 
        /// </summary>
        private bool StartProgram()
        {
            if (mainTask == null)
            {
                Console.WriteLine("Cannot start program, main task not present");
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
        private void OnExecutionStatusChanged(object sender, ExecutionStatusChangedEventArgs e)
        {
            Console.WriteLine("EXECUTION STATUS CHANGED: " + e.Status);

            if (e.Status == ExecutionStatus.Stopped)
            {
                //// Only trigger Instruct queue
                //if (controlMode == ControlMode.Execute)
                //{
                //    // Tick queue to move forward
                //    TriggerQueue(true);
                //}

                // @TODO: add new behavior here when execution changes
            }
        }

        /// <summary>
        /// What to do when Mastership changes.
        /// @TODO: add behaviors...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMastershipChanged(object sender, MastershipChangedEventArgs e)
        {
            Console.WriteLine("MASTERSHIP STATUS CHANGED: {0}", e.Status);

            // @TODO: what to do when mastership changes
        }

        /// <summary>
        /// What to do when the Task Enabled property changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTaskEnabledChanged(object sender, TaskEnabledChangedEventArgs e)
        {
            Console.WriteLine("TASK ENABLED CHANGED: {0}", e.Enabled);

            // @TODO: add behaviors
        }
        








    }

}
