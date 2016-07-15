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

        //// Abstract methods
        //public abstract bool LogOff();
        //public abstract bool Disconnect();
        //public abstract bool DisposeMainTask();
        //public abstract bool LoadIP();

        // Base constructor
        public Communication()
        {
            Reset();
        }

        public virtual void Reset()
        {

        }

        public bool IsConnected()
        {
            return isConnected;
        }

        public string GetIP()
        {
            return IP;
        }

        /// <summary>
        /// Reverts the comm object to a clean state.
        /// </summary>
        //public void Reset()
        //{
        //    LogOff();
        //    Disconnect();
        //    DisposeMainTask();
        //    LoadIP();
        //}
        


    }

    class CommunicationABB : Communication
    {
        // ABB stuff and flags
        private Controller controller;
        private ABB.Robotics.Controllers.RapidDomain.Task mainTask;
        private bool isLogged = false;
        //private bool isMainTaskRetrieved = false;                         // just do null check on the mainTask object
        private static string localBufferPathname = @"C:\";                 // Route names to be used for file handling
        private static string localBufferFilename = "buffer.mod";
        private static string remoteBufferDirectory = "RobotControl";

        /// <summary>
        /// Main constructor
        /// </summary>
        public CommunicationABB() : base() 
        {
            Reset();
        }

        public override void Reset()
        {
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
        public bool ConnectToDevice(int deviceId)
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
        public bool DisconnectFromDevice()
        {
            //if (SafetyStopImmediateOnDisconnect) StopProgram(true);

            // All these guys were incorporated to Reset();
            //LogOff();
            //DisposeMainTask();
            //DisposeController();
            
            Reset();
            return true;
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

    }

}
