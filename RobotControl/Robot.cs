using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ABB.Robotics;
using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers.RapidDomain;  // This is for the Task Class
using ABB.Robotics.Controllers.EventLogDomain;


namespace RobotControl
{
    /// <summary>
    /// Different connection modes. 
    /// </summary>
    public enum ConnectionMode : int { Instruct = 1, Stream = 2 };
    


    //██████╗  ██████╗ ██████╗  ██████╗ ████████╗
    //██╔══██╗██╔═══██╗██╔══██╗██╔═══██╗╚══██╔══╝
    //██████╔╝██║   ██║██████╔╝██║   ██║   ██║   
    //██╔══██╗██║   ██║██╔══██╗██║   ██║   ██║   
    //██║  ██║╚██████╔╝██████╔╝╚██████╔╝   ██║   
    //╚═╝  ╚═╝ ╚═════╝ ╚═════╝  ╚═════╝    ╚═╝   

    public class Robot
    {
        private const bool DEBUG = true;
        private static string tempBufferFilepath = @"C:\buffer.mod";
        private Thread pathExecuter;

        // Private properties
        private Controller controller;
        private ABB.Robotics.Controllers.RapidDomain.Task mainTask;

        private ConnectionMode connectionMode = ConnectionMode.Instruct;  // Instruct mode by default

        private Queue queue;

        // Public properties
        public bool isConnected { get; protected set; }
        public bool isLogged { get; protected set; }
        public bool isMainTaskRetrieved { get; private set; }

        public string IP { get; protected set; }





        //██████╗ ██╗   ██╗██████╗ ██╗     ██╗ ██████╗     █████╗ ██████╗ ██╗
        //██╔══██╗██║   ██║██╔══██╗██║     ██║██╔════╝    ██╔══██╗██╔══██╗██║
        //██████╔╝██║   ██║██████╔╝██║     ██║██║         ███████║██████╔╝██║
        //██╔═══╝ ██║   ██║██╔══██╗██║     ██║██║         ██╔══██║██╔═══╝ ██║
        //██║     ╚██████╔╝██████╔╝███████╗██║╚██████╗    ██║  ██║██║     ██║
        //╚═╝      ╚═════╝ ╚═════╝ ╚══════╝╚═╝ ╚═════╝    ╚═╝  ╚═╝╚═╝     ╚═╝
        /// <summary>
        /// Base constructor
        /// </summary>                                                       
        public Robot()
        {
            Reset();
        }

        /// <summary>
        /// In 'online' modes, performs all necessary instructions to connect to the robot controller. 
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            return Connect(ConnectionMode.Instruct);
        }

        //public bool Connect(string mode)
        //{
        //    if (mode.ToLower().Equals("stream"))
        //    {
        //        return Connect(ConnectionMode.Stream);
        //    }
        //    else if (mode.ToLower().Equals("instruct"))
        //    {
        //        return Connect(ConnectionMode.Instruct);
        //    }
        //    else
        //    {
        //        Console.WriteLine("Unknown connection mode, please specify 'instruct' or 'stream'");
        //    }
        //    return false;

        //}

        /// <summary>
        /// This will at some point differentiate between 'instruct' and 'stream', not at the moment
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool Connect(ConnectionMode mode)
        {
            connectionMode = mode;
            return ConnectToController();
        }

        /// <summary>
        /// In 'online' modes, performs all necessary instructions to disconnect from the controller.
        /// </summary>
        public void Disconnect()
        {
            if (DEBUG) Console.WriteLine("Disconnecting from controller on " + IP);

            DisconnectFromController();
        }

        /// <summary>
        /// Loads a module to the controller from a local file. 
        /// TODO: By default, it will wipe out any other modules in the task, add the possibility to choose.
        /// </summary>
        /// <param name="filepath"></param>
        public void LoadModule(string filepath)
        {
            LoadModuleFromFilename(filepath);
        }

        /// <summary>
        /// Allows to toggle between "once" and "loop" modes.
        /// </summary>
        /// <param name="mode"></param>
        public void RunMode(string mode)
        {
            if (isConnected)
            {
                using (Mastership.Request(controller.Rapid))
                {
                    controller.Rapid.Cycle = mode.ToLower().Equals("loop") ? ExecutionCycle.Forever : ExecutionCycle.Once;
                    if (DEBUG) Console.WriteLine("RunMode set to " + controller.Rapid.Cycle);
                }
            }
        }

        /// <summary>
        /// Starts execution of the current module/s in the controller.
        /// </summary>
        public void Start()
        {
            ResetProgramPointer();
            StartProgram();
        }

        /// <summary>
        /// Stops execution of the current module/s in the controller immediately. 
        /// Use StopAfterProgram() to schedule robot atop after completion of current cycle.
        /// </summary>
        public void Stop()
        {
            StopProgram(true);
        }

        /// <summary>
        /// Returns a string representation of the end frame's TCP position in mm.
        /// </summary>
        /// <returns></returns>
        public string GetPosition()
        {
            RobTarget rt = GetTCPRobTarget();
            return rt.Trans.ToString();
        }

        /// <summary>
        /// Returns a string representation of the end frame's TCP orientation in quaterions.
        /// </summary>
        /// <returns></returns>
        public string GetOrientation()
        {
            RobTarget rt = GetTCPRobTarget();
            return rt.Rot.ToString();
        }

        /// <summary>
        /// Returns a string representation of the robot's joint rotations in degrees.
        /// </summary>
        /// <returns></returns>
        public string GetJoints()
        {
            RobJoint rj = GetRobotJoints();
            return rj.ToString();
        }

        /// <summary>
        /// Loads the path to the queue manager and triggers execution of the program if applicable
        /// </summary>
        /// <param name="path"></param>
        public void LoadPath(Path path)
        {
            AddPathToQueue(path);
            TriggerQueue();
        }

        /// <summary>
        /// Stops the robot after execution of current program. This will also clear the queue.
        /// </summary>
        public void StopAfterProgram()
        {
            ClearQueue();
            StopProgram(false);
        }

        /// <summary>
        /// Dumps a ton of information to the console about the controller, the main task, etc.
        /// </summary>
        public void DebugDump()
        {
            DebugBanner();
            DebugControllerDump();
            DebugTaskDump();
            TestMastership();
        }











        //██████╗ ██████╗ ██╗██╗   ██╗ █████╗ ████████╗███████╗    ███╗   ███╗███████╗████████╗██╗  ██╗ ██████╗ ██████╗ ███████╗
        //██╔══██╗██╔══██╗██║██║   ██║██╔══██╗╚══██╔══╝██╔════╝    ████╗ ████║██╔════╝╚══██╔══╝██║  ██║██╔═══██╗██╔══██╗██╔════╝
        //██████╔╝██████╔╝██║██║   ██║███████║   ██║   █████╗      ██╔████╔██║█████╗     ██║   ███████║██║   ██║██║  ██║███████╗
        //██╔═══╝ ██╔══██╗██║╚██╗ ██╔╝██╔══██║   ██║   ██╔══╝      ██║╚██╔╝██║██╔══╝     ██║   ██╔══██║██║   ██║██║  ██║╚════██║
        //██║     ██║  ██║██║ ╚████╔╝ ██║  ██║   ██║   ███████╗    ██║ ╚═╝ ██║███████╗   ██║   ██║  ██║╚██████╔╝██████╔╝███████║
        //╚═╝     ╚═╝  ╚═╝╚═╝  ╚═══╝  ╚═╝  ╚═╝   ╚═╝   ╚══════╝    ╚═╝     ╚═╝╚══════╝   ╚═╝   ╚═╝  ╚═╝ ╚═════╝ ╚═════╝ ╚══════╝

        /// <summary>
        /// Resets all internal state properties to default values. To be invoked upon
        /// an internal robot reset.
        /// </summary>
        private void Reset()
        {
            isConnected = false;
            isLogged = false;
            isMainTaskRetrieved = false;
            IP = "";

            queue = new Queue();
        }

        /// <summary>
        /// Searches the network for a robot controller and establishes a connection with the first one available. 
        /// Necessary for "online" modes.
        /// </summary>
        /// <returns></returns>
        private bool ConnectToController()
        {
            NetworkScanner scanner = new NetworkScanner();

            ControllerInfo[] controllers = scanner.GetControllers(NetworkScannerSearchCriterias.Virtual);
            if (controllers.Length > 0)
            {

                controller = ControllerFactory.CreateFrom(controllers[0]);
                isConnected = true;
                IP = controller.IPAddress.ToString();
                if (DEBUG) Console.WriteLine("Found controller on " + IP);

                LogOn();
                RetrieveMainTask();
                if (TestMastership()) RunMode("once");
                SubscribeToEvents();
            }
            else
            {
                if (DEBUG) Console.WriteLine("No controllers found on the network");
                isConnected = false;
            }
            return true;
        }

        /// <summary>
        /// Checks if the controller resource is available for Mastership, or held by someone else. 
        /// </summary>
        /// <returns></returns>
        private bool TestMastership()
        {
            bool available = false;
            if (isConnected)
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
                catch (GenericControllerException ex)
                {
                    Console.WriteLine("MASTERSHIP ERROR: The controller is held by someone else");
                    if (DEBUG) Console.WriteLine(ex);
                }
            }
            else
            {
                Console.WriteLine("TestMastership(): not connected to controller");
            }

            if (available && DEBUG) Console.WriteLine("Controller Mastership available");

            return available;
        }

        /// <summary>
        /// Forces disconnection from current controller and manages associated logoffs, disposals, etc.
        /// </summary>
        private void DisconnectFromController()
        {
            DisposeMainTask();
            DisposeController();
            LogOff();
            Reset();
        }

        /// <summary>
        /// Disposes the controller object. This has to be done manually, since COM objects are not
        /// automatically garbage collected. 
        /// </summary>
        /// <returns></returns>
        private bool DisposeController()
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
        /// Upon connection, subscribe to relevant events and handle them.
        /// </summary>
        private void SubscribeToEvents()
        {
            controller.Rapid.ExecutionStatusChanged += OnExecutionStatusChanged;
        }

        /// <summary>
        /// Logs on to the controller with default credentials.
        /// </summary>
        private void LogOn()
        {
            if (isLogged)
            {
                LogOff();
            }
            controller.Logon(UserInfo.DefaultUser);
            isLogged = true;
        }

        /// <summary>
        /// Logs off from current controller.
        /// </summary>
        private void LogOff()
        {
            if (controller != null)
            {
                controller.Logoff();
                isLogged = false;
            }
        }

        /// <summary>
        /// Retrieves main task from the controller. E.g. for ABB robots this would typically be "T_ROB1".
        /// </summary>
        /// <returns></returns>
        private bool RetrieveMainTask()
        {
            ABB.Robotics.Controllers.RapidDomain.Task[] tasks = controller.Rapid.GetTasks();
            if (tasks.Length > 0)
            {
                mainTask = tasks[0];
                isMainTaskRetrieved = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Disposes the task objects. This has to be done manually, since COM objects are not
        /// automatically garbage collected. 
        /// </summary>
        /// <returns></returns>
        private bool DisposeMainTask()
        {
            if (mainTask != null)
            {
                mainTask.Dispose();
                mainTask = null;
                isMainTaskRetrieved = false;
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
            if (!isConnected)
            {
                if (DEBUG) Console.WriteLine("Can't ClearAllModules(), not connected to controller");
                return -1;
            }

            int count = -1;

            Module[] modules = mainTask.GetModules();
            count = modules.Length;

            using (Mastership.Request(controller.Rapid))
            {
                foreach (Module m in modules)
                {
                    if (DEBUG) Console.WriteLine("Deleting module: " + m.Name);
                    m.Delete();
                }
            }

            return count;
        }


        /// <summary>
        /// Loads a module into de controller from a local file. 
        /// @TODO: This is an expensive operation, should probably become threaded. 
        /// @TODO: By default, wipes out all previous modules --> parameterize.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        private bool LoadModuleFromFilename(string filepath)
        {
            if (!isConnected)
            {
                Console.WriteLine("Could not load module '" + filepath + "', not connected to controller");
                return false;
            }

            // For the time being, we will always whipe out previous modules on load
            if (ClearAllModules() < 0) return false;

            // Load the module
            bool success = false;
            using (Mastership.Request(controller.Rapid))
            {
                try
                {
                    // Loads a Rapid module to the task in the robot controller
                    success = mainTask.LoadModuleFromFile(filepath, RapidLoadMode.Replace);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: Could not load module: " + ex);
                }

            }

            // True if loading succeeds without any errors, otherwise false.  
            if (!success)
            {
                // Gets the available categories of the EventLog. 
                foreach (EventLogCategory category in controller.EventLog.GetCategories())
                {
                    if (category.Name == "Common")
                    {
                        if (category.Messages.Count > 0)
                        {
                            foreach (EventLogMessage message in category.Messages)
                            {
                                Console.WriteLine("Program [{1}:{2}({0})] {3} {4}",
                                    message.Name, message.SequenceNumber,
                                    message.Timestamp, message.Title, message.Body);
                            }
                        }
                    }
                }
            }
            else
            {
                if (DEBUG) Console.WriteLine("Sucessfully loaded " + filepath);
            }

            return success;
        }

        /// <summary>
        /// Saves a string representation of a module to a local file. 
        /// </summary>
        /// <param name="module"></param>
        /// <param name="filepath"></param>
        private void SaveModuleToFile(List<string> module, string filepath)
        {
            System.IO.File.WriteAllLines(filepath, module, System.Text.Encoding.ASCII);
        }

        /// <summary>
        /// Resets the program pointer in the controller to the main entry point. Needs to be called
        /// before starting execution of a program, otherwise the controller will throw an error. 
        /// </summary>
        private void ResetProgramPointer()
        {
            if (isMainTaskRetrieved)
            {
                using (Mastership.Request(controller.Rapid))
                {
                    mainTask.ResetProgramPointer();
                }
            }

        }

        /// <summary>
        /// Requests start executing the program in the main task. Remember to call ResetProgramPointer() before. 
        /// </summary>
        private void StartProgram()
        {
            if (isMainTaskRetrieved)
            {
                using (Mastership.Request(controller.Rapid))
                {
                    try
                    {
                        controller.Rapid.Start(true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could not StartProgram(): " + ex);
                        throw ex;
                    }

                }
            }
        }

        /// <summary>
        /// Requests stop executing the program in the main task.
        /// </summary>
        private void StopProgram(bool immediate)
        {
            if (isMainTaskRetrieved)
            {
                using (Mastership.Request(controller.Rapid))
                {
                    controller.Rapid.Stop(immediate ? StopMode.Immediate : StopMode.Cycle);
                    //controller.Rapid.Stop(StopMode.Immediate);
                }
            }
        }

        /// <summary>
        /// Returns a RobTarget object representing the current robot's TCP.
        /// </summary>
        /// <returns></returns>
        private RobTarget GetTCPRobTarget()
        {
            return controller.MotionSystem.ActiveMechanicalUnit.GetPosition(ABB.Robotics.Controllers.MotionDomain.CoordinateSystemType.World);
        }

        /// <summary>
        /// Returns a RobJoint object representing the current values for joint rotations. 
        /// </summary>
        /// <returns></returns>
        private RobJoint GetRobotJoints()
        {
            return controller.MotionSystem.ActiveMechanicalUnit.GetPosition().RobAx;
        }






        /// <summary>
        /// Adds a path to the queue manager.
        /// </summary>
        /// <param name="path"></param>
        private void AddPathToQueue(Path path)
        {
            queue.Add(path);
        }

        /// <summary>
        /// Checks the state of the execution of the robot, and if stopped and if elements 
        /// remaining on the queue, starts executing them.
        /// </summary>
        private void TriggerQueue()
        {
            if (controller.Rapid.ExecutionStatus == ExecutionStatus.Stopped) 
            {
                TriggerQueue(true);
            }
        }

        /// <summary>
        /// An overload to bypass ExecutionStatus check.
        /// </summary>
        /// <param name="robotIsStopped"></param>
        private void TriggerQueue(bool robotIsStopped)
        {
            if (queue.ArePathsPending() && (pathExecuter == null || !pathExecuter.IsAlive))
            {
                Path path = queue.GetNext();
                // RunPath(path);

                // https://msdn.microsoft.com/en-us/library/aa645740(v=vs.71).aspx
                // Thread oThread = new Thread(new ThreadStart(oAlpha.Beta));
                // http://stackoverflow.com/a/3360582
                // Thread thread = new Thread(() => download(filename));

                // This needs to be much better handled, and the trigger queue should not trigger if a thread is running... 
                //Thread runPathThread = new Thread(() => RunPath(path));  // not working for some reason...
                //runPathThread.Start();

                pathExecuter = new Thread(() => RunPath(path));  // http://stackoverflow.com/a/3360582
                pathExecuter.Start();

                
            }
        }


        /// <summary>
        /// Generates a module from a path, loads it to the controller and runs it.
        /// It assumes the robot is stopped (does this even matter anyway...?)
        /// </summary>
        /// <param name="path"></param>
        private void RunPath(Path path)
        {
            if (DEBUG) Console.WriteLine("RUNNING NEW PATH: " + path.Count);
            List<string> module = RAPID.UNSAFEModuleFromPath(path, 100, 5);
            SaveModuleToFile(module, tempBufferFilepath);
            LoadModuleFromFilename(tempBufferFilepath);
            ResetProgramPointer();
            StartProgram();
        }


        /// <summary>
        /// Remove all pending elements from the queue.
        /// </summary>
        private void ClearQueue()
        {
            queue.EmptyQueue();
        }

        /// <summary>
        /// Printlines a "DEBUG" ASCII banner... ;)
        /// </summary>
        private void DebugBanner()
        {
            Console.WriteLine("");
            Console.WriteLine("██████╗ ███████╗██████╗ ██╗   ██╗ ██████╗ ");
            Console.WriteLine("██╔══██╗██╔════╝██╔══██╗██║   ██║██╔════╝ ");
            Console.WriteLine("██║  ██║█████╗  ██████╔╝██║   ██║██║  ███╗");
            Console.WriteLine("██║  ██║██╔══╝  ██╔══██╗██║   ██║██║   ██║");
            Console.WriteLine("██████╔╝███████╗██████╔╝╚██████╔╝╚██████╔╝");
            Console.WriteLine("╚═════╝ ╚══════╝╚═════╝  ╚═════╝  ╚═════╝ ");
            Console.WriteLine("");
        }

        /// <summary>
        /// Dumps a bunch of controller info to the console.
        /// </summary>
        private void DebugControllerDump()
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
            Console.WriteLine("     MainComputerServiceInfo: ");
            Console.WriteLine("         BoardType: " + controller.MainComputerServiceInfo.BoardType);
            Console.WriteLine("         CpuInfo: " + controller.MainComputerServiceInfo.CpuInfo);
            Console.WriteLine("         RamSize: " + controller.MainComputerServiceInfo.RamSize);
            Console.WriteLine("         Temperature: " + controller.MainComputerServiceInfo.Temperature);
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
        }

        private void DebugTaskDump()
        {
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







        //███████╗██╗   ██╗███████╗███╗   ██╗████████╗    ██╗  ██╗ █████╗ ███╗   ██╗██████╗ ██╗     ██╗███╗   ██╗ ██████╗ 
        //██╔════╝██║   ██║██╔════╝████╗  ██║╚══██╔══╝    ██║  ██║██╔══██╗████╗  ██║██╔══██╗██║     ██║████╗  ██║██╔════╝ 
        //█████╗  ██║   ██║█████╗  ██╔██╗ ██║   ██║       ███████║███████║██╔██╗ ██║██║  ██║██║     ██║██╔██╗ ██║██║  ███╗
        //██╔══╝  ╚██╗ ██╔╝██╔══╝  ██║╚██╗██║   ██║       ██╔══██║██╔══██║██║╚██╗██║██║  ██║██║     ██║██║╚██╗██║██║   ██║
        //███████╗ ╚████╔╝ ███████╗██║ ╚████║   ██║       ██║  ██║██║  ██║██║ ╚████║██████╔╝███████╗██║██║ ╚████║╚██████╔╝
        //╚══════╝  ╚═══╝  ╚══════╝╚═╝  ╚═══╝   ╚═╝       ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═════╝ ╚══════╝╚═╝╚═╝  ╚═══╝ ╚═════╝ 

        /// <summary>
        /// What to do when the robot starts running or stops.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExecutionStatusChanged(object sender, ExecutionStatusChangedEventArgs e)
        {
            if (DEBUG) Console.WriteLine("EXECUTION STATUS CHANGED: " + e.Status);

            if (e.Status == ExecutionStatus.Stopped)
            {
                // Tick queue to move forward
                TriggerQueue(true);
            }
        }

    }
}
