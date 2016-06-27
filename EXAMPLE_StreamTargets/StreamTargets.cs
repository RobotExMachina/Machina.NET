using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RobotControl;

using ABB.Robotics;
using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers.RapidDomain;  // This is for the Task Class
using ABB.Robotics.Controllers.EventLogDomain;

// STILL NEEDS TO BE CONVERTED TO PURE ROBOTCONTROL API

namespace EXAMPLE_StreamTargets
{
    class StreamTargets
    {
        static string moduleName = "RemoteMain";
        static string moduleFilepath = @"D:\temp\";
        static string moduleFilename = "remote_robot_v3_slow.mod";

        static Robot arm;

        static RapidData RD_aborted,
            RD_pnum;

        static int virtualRDCount = 4;
        static RapidData[] RD_p = new RapidData[virtualRDCount],
            RD_pset = new RapidData[virtualRDCount];



        [MTAThread]
        static void Main(string[] args)
        {
            //// The main controller
            ////Controller controller;
            //while ((controller = CreateController()) == null)
            //{
            //}
            //controller.Logon(UserInfo.DefaultUser);  // logon to the system
            //controller.EventLog.ClearAll();


            arm = new Robot();
            arm.Connect();

            // Get task with specified name
            //Task tRob1 = controller.Rapid.GetTask("T_ROB1");
            //if (!tRob1.Enabled) tRob1.Enabled = true;

            //// Clear previous modules
            //ClearAllModules(controller, tRob1);

            //// Some debug stuff
            //Console.WriteLine("ExecutionStatus: " + tRob1.ExecutionStatus);
            //Console.WriteLine("ExecutionStatus (Rapid): " + controller.Rapid.ExecutionStatus);
            //Console.WriteLine("ControllerOperatingMode: " + controller.OperatingMode);

            arm.DebugDump();

            // Load module
            //Console.WriteLine("");
            //Console.WriteLine("Press any key to load " + moduleFilePath + " and run the task...");
            //Console.ReadKey();

            //bool moduleIsLoaded = LoadModuleFromFile(controller, moduleFilePath, tRob1);
            //Console.WriteLine("    success loading module: " + moduleIsLoaded);

            arm.LoadModule(moduleFilename, moduleFilepath);


            //// Load RapidData control variables
            //RD_aborted = LoadRapidDataVariable(tRob1, moduleName, "aborted");
            //RD_pnum = LoadRapidDataVariable(tRob1, moduleName, "pnum");
            //RD_pnum.ValueChanged += RD_pnum_ValueChanged;  // Add an eventhandler to 'pnum'

            // Load RapidData control variables
            RD_aborted = LoadRapidDataVariable(arm.mainTask, moduleName, "aborted");
            RD_pnum = LoadRapidDataVariable(arm.mainTask, moduleName, "pnum");
            RD_pnum.ValueChanged += RD_pnum_ValueChanged;  // Add an eventhandler to 'pnum'

            // Load and set the first four targets
            for (int i = 0; i < virtualRDCount; i++)
            {
                RD_p[i] = LoadRapidDataVariable(arm.mainTask, moduleName, "p" + i);
                RD_pset[i] = LoadRapidDataVariable(arm.mainTask, moduleName, "pset" + i);
                AddVirtualTarget();
            }

            // STOP AND EXIT
            Console.WriteLine(" ");
            Console.WriteLine("Press any key to START the task...");
            Console.ReadKey();

            //// Run the task
            //using (Mastership.Request(.Rapid))
            //{
            //    controller.Rapid.Cycle = ExecutionCycle.Once;
            //    Console.WriteLine("    running mode: " + controller.Rapid.Cycle);

            //    if (true)
            //    {
            //        Console.WriteLine("    starting Rapid Module...");

            //        try
            //        {
            //            // Reset program pointer
            //            arm.mainTask.ResetProgramPointer();
            //            controller.Rapid.Start(true);  // https://webcache.googleusercontent.com/search?q=cache:-My5hfxyY0IJ:https://forums.robotstudio.com/discussion/9376/simple-pc-sdk-application-not-starting-tasks+&cd=1&hl=en&ct=clnk&gl=us
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine("    --> Exception: " + ex);
            //            Console.ReadKey();
            //        }
            //    }
            //}

            arm.Start();

            // Loop infinitely? I don't think this will work, will get stuck, eventhandling is synchronous...             
            // Change to a simple ReadKey()?
            while (!targetsExhausted)
            {
                Console.WriteLine(" ");
                Console.WriteLine("Looping targets, press any key when done...");
                Console.ReadKey();
                //AddVirtualTarget();
            }


            // STOP AND EXIT
            Console.WriteLine(" ");
            Console.WriteLine("Press any key to STOP the task...");
            Console.ReadKey();

            //if (true)
            //{
            //    Console.WriteLine("Stopping Rapid Module...");
            //    using (Mastership.Request(controller.Rapid))
            //    {
            //        controller.Rapid.Stop(StopMode.Immediate);  // by default stops by the end of the cycle
            //    }
            //}

            arm.Stop();

            // Dispose and log off
            //Console.WriteLine("Disposing...");
            //arm.mainTask.Dispose();
            //arm.mainTask = null;
            //controller.Logoff();
            //controller.Dispose();
            //controller = null;

            arm.Disconnect();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();

        }

        private static void RD_pnum_ValueChanged(object sender, DataValueChangedEventArgs e)
        {
            //throw new NotImplementedException();
            RapidData rd = (RapidData)sender;
            Console.WriteLine("   variable '" + rd.Name + "' changed: " + rd.StringValue);

            // Do not add target if pnum is being reset to initial value
            if (!rd.StringValue.Equals("-1"))
            {
                AddVirtualTarget();
            }
        }


        static bool targetsExhausted = false;
        static int programPointer = 0;  // this keeps track of what is the next target index that needs to be assigned. it should asynchronously be ~4 units ahead of the 'pnum' rapid counter

        private static void AddVirtualTarget()
        {
            // Make sure we don't overwrite prev set targets
            if (programPointer < targets.Length)
            {
                if (RD_pset[programPointer % virtualRDCount].StringValue.Equals("FALSE"))
                {
                    Console.WriteLine("    setting target " + programPointer);
                    SetRapidDataVar(arm.controller, RD_pset[programPointer % virtualRDCount], true);
                    SetRapidDataRobTarget(arm.controller, RD_p[programPointer % virtualRDCount], targets[programPointer]);

                    programPointer++;
                    if (programPointer >= targets.Length)
                    {
                        targetsExhausted = true;
                    }
                }
                else
                {
                    Console.WriteLine("--> GOING TOO FAST BUDDY!");
                }
            }
            else
            {
                Console.WriteLine("Past the target length");
            }

        }



        /// <summary>
        /// Returns a RapidData object representing a persistent variable in the Rapid execution
        /// </summary>
        /// <param name="task"></param>
        /// <param name="modName"></param>
        /// <param name="varName"></param>
        /// <returns></returns>
        static RapidData LoadRapidDataVariable(ABB.Robotics.Controllers.RapidDomain.Task task, string modName, string varName)
        {
            RapidData @var = null;
            try
            {
                @var = task.GetModule(modName).GetRapidData(varName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return @var;
        }

        /// <summary>
        /// Sets the value of a RapidData object
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="rdvar"></param>
        /// <param name="varValue"></param>
        static void SetRapidDataVar(Controller controller, RapidData rdvar, bool varValue)
        {
            using (Mastership.Request(controller.Rapid))
            {
                try
                {
                    Console.WriteLine("    current value for '" + rdvar.Name + "': " + rdvar.StringValue);
                    rdvar.Value = new Bool(varValue);
                    Console.WriteLine("    new value for '" + rdvar.Name + "': " + rdvar.StringValue);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("    RapidData setting exception: " + ex);
                }
            }
        }


        /// <summary>
        /// Sets the passed RapidData variable to a RobTarget from its string representation
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="rdvar"></param>
        /// <param name="robTargetDeclaration"></param>
        static void SetRapidDataRobTarget(Controller controller, RapidData rdvar, string robTargetDeclaration)
        {
            using (Mastership.Request(controller.Rapid))
            {
                try
                {
                    Console.WriteLine("    current value for '" + rdvar.Name + "': " + rdvar.StringValue);
                    RobTarget rt = new RobTarget();
                    rt.FillFromString2(robTargetDeclaration);
                    rdvar.Value = rt;
                    Console.WriteLine("    new value for '" + rdvar.Name + "': " + rdvar.StringValue);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("    RapidData setting exception: " + ex);
                }
            }
        }


        //████████╗ █████╗ ██████╗  ██████╗ ███████╗████████╗███████╗
        //╚══██╔══╝██╔══██╗██╔══██╗██╔════╝ ██╔════╝╚══██╔══╝██╔════╝
        //   ██║   ███████║██████╔╝██║  ███╗█████╗     ██║   ███████╗
        //   ██║   ██╔══██║██╔══██╗██║   ██║██╔══╝     ██║   ╚════██║
        //   ██║   ██║  ██║██║  ██║╚██████╔╝███████╗   ██║   ███████║
        //   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝ ╚══════╝   ╚═╝   ╚══════╝                                       
        // 50 targets defining a circle in space
        static string[] targets =
        {
            "[[377.22,4.21,546.99],[0,-0.0056,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[376.24,4.21,562.63],[0,-0.0056,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[373.3,4.21,578.01],[0,-0.0056,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[368.46,4.21,592.91],[0,-0.0057,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[361.79,4.21,607.08],[0,-0.0058,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[353.4,4.21,620.31],[0,-0.006,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[343.42,4.21,632.38],[0,-0.0061,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[332,4.21,643.1],[0,-0.0063,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[319.33,4.21,652.3],[0,-0.0066,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[305.6,4.21,659.85],[0,-0.0069,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[291.04,4.21,665.62],[0,-0.0072,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[275.87,4.21,669.51],[0,-0.0076,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[260.33,4.21,671.48],[0,-0.0081,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[244.66,4.21,671.48],[0,-0.0086,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[229.12,4.21,669.51],[0,-0.0092,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[213.95,4.21,665.62],[0,-0.0098,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[199.39,4.21,659.85],[0,-0.0106,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[185.66,4.21,652.3],[0,-0.0113,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[172.99,4.21,643.1],[0,-0.0122,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[161.57,4.21,632.38],[0,-0.013,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[151.59,4.21,620.31],[0,-0.0139,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[143.19,4.21,607.08],[0,-0.0147,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[136.52,4.21,592.91],[0,-0.0154,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[131.68,4.21,578.01],[0,-0.016,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[128.75,4.21,562.63],[0,-0.0163,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[127.76,4.21,546.99],[0,-0.0165,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[128.75,4.21,531.36],[0,-0.0163,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[131.68,4.21,515.97],[0,-0.016,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[136.52,4.21,501.08],[0,-0.0154,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[143.19,4.21,486.9],[0,-0.0147,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[151.59,4.21,473.68],[0,-0.0139,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[161.57,4.21,461.61],[0,-0.013,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[172.99,4.21,450.89],[0,-0.0122,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[185.66,4.21,441.68],[0,-0.0113,0.9999,0],[0,0,-1,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[199.39,4.21,434.14],[0,-0.0106,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[213.95,4.21,428.37],[0,-0.0098,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[229.12,4.21,424.47],[0,-0.0092,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[244.66,4.21,422.51],[0,-0.0086,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[260.33,4.21,422.51],[0,-0.0081,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[275.87,4.21,424.47],[0,-0.0076,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[291.04,4.21,428.37],[0,-0.0072,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[305.6,4.21,434.14],[0,-0.0069,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[319.33,4.21,441.68],[0,-0.0066,1,0],[0,0,-1,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[332,4.21,450.89],[0,-0.0063,1,0],[0,0,-1,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[343.42,4.21,461.61],[0,-0.0061,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[353.4,4.21,473.68],[0,-0.006,1,0],[0,0,-1,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[361.79,4.21,486.9],[0,-0.0058,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[368.46,4.21,501.08],[0,-0.0057,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[373.3,4.21,515.97],[0,-0.0056,1,0],[0,0,-1,0],[0,9E9,9E9,9E9,9E9,9E9]]",
            "[[376.24,4.21,531.36],[0,-0.0056,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]",
        };
    }
}
