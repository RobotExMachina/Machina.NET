using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Net.Sockets;

using System.Threading;

using Machina.Drivers.Communication;

namespace Machina.Drivers
{

    //   █████╗ ██████╗ ██████╗ 
    //  ██╔══██╗██╔══██╗██╔══██╗
    //  ███████║██████╔╝██████╔╝
    //  ██╔══██║██╔══██╗██╔══██╗
    //  ██║  ██║██████╔╝██████╔╝
    //  ╚═╝  ╚═╝╚═════╝ ╚═════╝ 
    //                          
    class DriverABB : Driver
    {
        //private RobotStudioManager _rsBridge;

        private TcpClient clientSocket = new TcpClient();
        private NetworkStream clientNetworkStream;
        private Thread receivingThread;
        private Thread sendingThread;
        static public TCPConnectionStatus Status { get; private set; }

        // From the Machina_Server.mod file, must be consistent!
        const string STR_MESSAGE_END_CHAR = ";";
        const string STR_MESSAGE_ID_CHAR = "@";

        // A RAPID-code oriented API:
        //                                     // INSTRUCTION P1 P2 P3 P4...
        const int INST_MOVEL = 1;              // MoveL X Y Z QW QX QY QZ
        const int INST_MOVEJ = 2;              // MoveJ X Y Z QW QX QY QZ
        const int INST_MOVEABSJ = 3;           // MoveAbsJ J1 J2 J3 J4 J5 J6
        const int INST_SPEED = 4;              // (setspeed V_TCP[V_ORI V_LEAX V_REAX])
        const int INST_ZONE = 5;               // (setzone FINE TCP[ORI EAX ORI LEAX REAX])
        const int INST_WAITTIME = 6;           // WaitTime T
        const int INST_TPWRITE = 7;            // TPWrite "MSG"
        const int INST_TOOL = 8;               // (settool X Y Z QW QX QY QZ KG CX CY CZ)
        const int INST_NOTOOL = 9;             // (settool tool0)
        const int INST_SETDO = 10;             // SetDO "NAME" ON
        const int INST_SETAO = 11;             // SetAO "NAME" V


        private int sentMessages = 0;
        private int receivedMessages = 0;
        private int maxStreamCount = 10;
        private int sendNewBatchOn = 2;



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
        public DriverABB(Control ctrl) : base(ctrl) { }


        public override bool ConnectToDevice(string ip, int port)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Performs all necessary actions to establish a connection to a real/virtual device, 
        /// including connecting to the controller, loggin in, checking required states, etc.
        /// </summary>
        /// <param name="deviceId"></param>
        public override bool ConnectToDevice(int deviceId)
        {
            //_isConnected = false;

            //if (this.masterControl.connectionMode == ConnectionManagerType.Machina)
            //{

            //    _rsBridge = new RobotStudioManager(this);
            //    _rsBridge.Connect(deviceId);


            //    //// If here, everything went well and successfully connected 
            //    //isConnected = true;

            //    // If on 'stream' mode, set up stream connection flow
            //    if (masterControl.GetControlMode() == ControlType.Stream)
            //    {
            //        if (!SetupStreamingMode())
            //        {
            //            Console.WriteLine("Could not initialize 'stream' mode in controller");
            //            Reset();
            //            return false;
            //        }
            //    }
            //    else
            //    {
            //        // if on Execute mode on _rsBridge, do nothing (programs will be uploaded in batch)
            //    }


            //}
            //else
            //{
            //    if (!SetupStreamingMode())
            //    {
            //        Console.WriteLine("Could not initialize 'stream' mode in controller");
            //        Reset();
            //        return false;
            //    }
            //}


            //DebugDump();

            //return _isConnected;

            throw new NotImplementedException();
        }


        /// <summary>
        /// Forces disconnection from current controller and manages associated logoffs, disposals, etc.
        /// </summary>
        /// <returns></returns>
        public override bool DisconnectFromDevice()
        {
            //Reset();
            //_rsBridge.Disconnect();

            // Disconnect from TCP sockets here... 

            return true;
        }

        public override Joints GetCurrentJoints()
        {
            throw new NotImplementedException();
        }

        public override Rotation GetCurrentOrientation()
        {
            throw new NotImplementedException();
        }

        public override Vector GetCurrentPosition()
        {
            throw new NotImplementedException();
        }

        public override void DebugDump()
        {
            throw new NotImplementedException();
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }





        //███████╗████████╗██████╗ ███████╗ █████╗ ███╗   ███╗██╗███╗   ██╗ ██████╗ 
        //██╔════╝╚══██╔══╝██╔══██╗██╔════╝██╔══██╗████╗ ████║██║████╗  ██║██╔════╝ 
        //███████╗   ██║   ██████╔╝█████╗  ███████║██╔████╔██║██║██╔██╗ ██║██║  ███╗
        //╚════██║   ██║   ██╔══██╗██╔══╝  ██╔══██║██║╚██╔╝██║██║██║╚██╗██║██║   ██║
        //███████║   ██║   ██║  ██║███████╗██║  ██║██║ ╚═╝ ██║██║██║ ╚████║╚██████╔╝
        //╚══════╝   ╚═╝   ╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝╚═╝     ╚═╝╚═╝╚═╝  ╚═══╝ ╚═════╝ 

        /// <summary>
        /// Performs necessary operations to set up 'stream' control mode in the controller
        /// </summary>
        /// <returns></returns>
        private bool SetupStreamingMode()
        {
            //// If manager is Machina, try to rely on the RobotStudio bridge to set up the streaming module
            //if (this.masterControl.connectionMode == ConnectionManagerType.Machina)
            //{
            //    if (!_rsBridge.LoadStreamingModule())
            //    {
            //        Console.WriteLine("Could not load streaming module");
            //        return false;
            //    }

            //    if (!_rsBridge.ResetProgramPointer())
            //    {
            //        Console.WriteLine("Could not reset the program pointer");
            //        return false;
            //    }

            //    if (!_rsBridge.StartProgramExecution())
            //    {
            //        Console.WriteLine("Could not load start the streaming module");
            //        return false;
            //    }

            //}
            
            if (!EstablishTCPConnection())
            {
                Console.WriteLine("Could not connect to server in controller");
                return false;
            }

            // Hurray!
            return true;
        }











        ///// <summary>
        ///// This function will look at the state of the program pointer, the streamQueue, 
        ///// and if necessary will add a new target to the stream. This is meant to be called
        ///// to initiate the stream update chain, like when adding a new target, or pnum event handling.
        ///// </summary>
        //public override void TickStreamQueue(bool hasPriority)
        //{
        //    //Console.WriteLine($"TICKING StreamQueue: {WriteCursor.ActionsPending()} actions pending");
        //    //if (WriteCursor.AreActionsPending())
        //    //{
        //    //    Console.WriteLine("About to set targets");
        //    //    //SetNextVirtualTarget(hasPriority);
        //    //    //SendActionAsMessage(hasPriority);  // this is now watched by the thread
        //    //    //TickStreamQueue(hasPriority);  // call this in case there are more in the queue...
        //    //}
        //    //else
        //    //{
        //    //    Console.WriteLine($"Not setting targets, actions pending {WriteCursor.ActionsPending()}");
        //    //}
        //}

        









        //██████╗ ██████╗ ██╗██╗   ██╗ █████╗ ████████╗███████╗
        //██╔══██╗██╔══██╗██║██║   ██║██╔══██╗╚══██╔══╝██╔════╝
        //██████╔╝██████╔╝██║██║   ██║███████║   ██║   █████╗  
        //██╔═══╝ ██╔══██╗██║╚██╗ ██╔╝██╔══██║   ██║   ██╔══╝  
        //██║     ██║  ██║██║ ╚████╔╝ ██║  ██║   ██║   ███████╗
        //╚═╝     ╚═╝  ╚═╝╚═╝  ╚═══╝  ╚═╝  ╚═╝   ╚═╝   ╚══════╝

        








        

        private bool EstablishTCPConnection()
        {
            try
            {
                clientSocket = new TcpClient();
                clientSocket.Connect(this.IP, this.Port);
                Status = TCPConnectionStatus.Connected;
                clientNetworkStream = clientSocket.GetStream();
                clientSocket.ReceiveBufferSize = 1024;
                clientSocket.SendBufferSize = 1024;

                sendingThread = new Thread(SendingMethod);
                sendingThread.IsBackground = true;
                sendingThread.Start();

                receivingThread = new Thread(ReceivingMethod);
                receivingThread.IsBackground = true;
                receivingThread.Start();


                return clientSocket.Connected;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: could not establish TCP connection");
                Console.WriteLine(ex);
            }

            return false;
        }

        private bool hasRobotBufferFilled = false;
        private bool ShouldSend()
        {
            if (hasRobotBufferFilled)
            {
                if (sentMessages - receivedMessages < sendNewBatchOn)
                {
                    hasRobotBufferFilled = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (sentMessages - receivedMessages < maxStreamCount)
                {
                    return true;
                }
                else
                {
                    hasRobotBufferFilled = true;
                    return false;
                }
            }
        }


        private void SendingMethod(object obj)
        {
            // Expire the thread on disconnection
            while (Status != TCPConnectionStatus.Disconnected)
            {
                //while (this.WriteCursor.actionBuffer.AreActionsPending())
                while (this.ShouldSend() && this.WriteCursor.actionBuffer.AreActionsPending())
                {
                    if (SendActionAsMessage(true)) sentMessages++;
                }

                Thread.Sleep(30);
            }
        }

        private void ReceivingMethod(object obj)
        {
            // Expire the thread on disconnection
            while (Status != TCPConnectionStatus.Disconnected)
            {
                if (clientSocket.Available > 0)
                {
                    var buffer = new byte[1024];
                    var byteCount = clientSocket.GetStream().Read(buffer, 0, buffer.Length);
                    var response = Encoding.UTF8.GetString(buffer, 0, byteCount);

                    //Console.WriteLine($"  RES: Server response was {response}");
                    var messages = response.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var res in messages)
                    {
                        Console.WriteLine($"  RES: Server response was {res};");
                        receivedMessages++;
                    }
                }

                Thread.Sleep(30);
            }
        }


        private bool CloseTCPConnection()
        {
            //if (controller == null) return true;

            if (clientSocket != null)
            {
                Status = TCPConnectionStatus.Disconnected;
                clientSocket.Client.Disconnect(false);
                clientSocket.Close();
                if (clientNetworkStream != null) clientNetworkStream.Dispose();
                return true;
            }

            return false;
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

            string msg = GetActionMessage(a, WriteCursor);
            if (msg == "")
            {
                Console.WriteLine("Applied action to cursor, but not necessary to stream to robot");
                return false;
            }

            byte[] msgBytes = Encoding.ASCII.GetBytes(msg);

            clientNetworkStream.Write(msgBytes, 0, msgBytes.Length);

            Console.WriteLine("Sending mgs: " + msg);

            return true;
        }


        private string GetActionMessage(Action action, RobotCursor cursor)
        {
            string msg = "";

            switch (action.type)
            {
                case ActionType.Translation:
                case ActionType.Rotation:
                case ActionType.Transformation:
                    //// MoveL/J X Y Z QW QX QY QZ
                    msg = string.Format("{0}{1} {2} {3} {4} {5} {6} {7} {8} {9}{10}",
                        STR_MESSAGE_ID_CHAR,
                        action.id,
                        cursor.motionType == MotionType.Linear ? INST_MOVEL : INST_MOVEJ,
                        Math.Round(cursor.position.X, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(cursor.position.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(cursor.position.Z, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(cursor.rotation.Q.W, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        Math.Round(cursor.rotation.Q.X, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        Math.Round(cursor.rotation.Q.Y, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        Math.Round(cursor.rotation.Q.Z, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        STR_MESSAGE_END_CHAR);
                    break;

                case ActionType.Axes:
                    // MoveAbsJ J1 J2 J3 J4 J5 J6
                    msg = string.Format("{0}{1} {2} {3} {4} {5} {6} {7} {8}{9}",
                        STR_MESSAGE_ID_CHAR,
                        action.id,
                        INST_MOVEABSJ,
                        Math.Round(cursor.joints.J1, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.joints.J2, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.joints.J3, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.joints.J4, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.joints.J5, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        Math.Round(cursor.joints.J6, Geometry.STRING_ROUND_DECIMALS_DEGS),
                        STR_MESSAGE_END_CHAR);
                    break;

                case ActionType.Speed:
                    // (setspeed V_TCP[V_ORI V_LEAX V_REAX])
                    msg = $"{STR_MESSAGE_ID_CHAR}{action.id} {INST_SPEED} {cursor.speed}{STR_MESSAGE_END_CHAR}";  // this accepts more velocity params, but those are still not implemented in Machina... 
                    break;

                case ActionType.Precision:
                    // (setzone FINE TCP[ORI EAX ORI LEAX REAX])
                    msg = $"{STR_MESSAGE_ID_CHAR}{action.id} {INST_ZONE} {cursor.precision}{STR_MESSAGE_END_CHAR}";  // this accepts more zone params, but those are still not implemented in Machina... 
                    break;

                case ActionType.Wait:
                    // !WaitTime T
                    ActionWait aw = (ActionWait)action;
                    msg = $"{STR_MESSAGE_ID_CHAR}{action.id} {INST_WAITTIME} {0.001 * aw.millis}{STR_MESSAGE_END_CHAR}";
                    break;

                case ActionType.Message:
                    // !TPWrite "MSG"
                    ActionMessage am = (ActionMessage)action;
                    msg = $"{STR_MESSAGE_ID_CHAR}{action.id} {INST_TPWRITE} \"{am.message}\"{STR_MESSAGE_END_CHAR}";
                    break;

                case ActionType.Attach:
                    // !(settool X Y Z QW QX QY QZ KG CX CY CZ)
                    ActionAttach aa = (ActionAttach)action;
                    Tool t = aa.tool;

                    msg = string.Format("{0}{1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}{14}",
                        STR_MESSAGE_ID_CHAR,
                        action.id,
                        INST_TOOL,
                        Math.Round(t.TCPPosition.X, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(t.TCPPosition.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(t.TCPPosition.Z, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(t.TCPOrientation.Q.W, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        Math.Round(t.TCPOrientation.Q.X, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        Math.Round(t.TCPOrientation.Q.Y, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        Math.Round(t.TCPOrientation.Q.Z, Geometry.STRING_ROUND_DECIMALS_QUAT),
                        Math.Round(t.weight, Geometry.STRING_ROUND_DECIMALS_KG),
                        Math.Round(t.centerOfGravity.X, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(t.centerOfGravity.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                        Math.Round(t.centerOfGravity.Z, Geometry.STRING_ROUND_DECIMALS_MM),
                        STR_MESSAGE_END_CHAR);
                    break;

                case ActionType.Detach:
                    // !(settool0)
                    msg = $"{STR_MESSAGE_ID_CHAR}{action.id} {INST_NOTOOL}{STR_MESSAGE_END_CHAR}";
                    break;

                case ActionType.IODigital:
                    // !SetDO "NAME" ON
                    ActionIODigital aiod = (ActionIODigital)action;
                    msg = $"{STR_MESSAGE_ID_CHAR}{action.id} {INST_SETDO} \"{cursor.digitalOutputNames[aiod.pin]}\" {(aiod.on ? 1 : 0)}{STR_MESSAGE_END_CHAR}";
                    break;

                case ActionType.IOAnalog:
                    // !SetAO "NAME" V
                    ActionIOAnalog aioa = (ActionIOAnalog)action;
                    msg = $"{STR_MESSAGE_ID_CHAR}{action.id} {INST_SETAO} \"{cursor.digitalOutputNames[aioa.pin]}\" {aioa.value}{STR_MESSAGE_END_CHAR}";
                    break;
            }

            return msg;
        }












    }
}
