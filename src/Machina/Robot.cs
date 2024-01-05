﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;
using Machina.Attributes;
using Machina.Types;
using Machina.Types.Geometry;
using Machina.Types.Data;

//  ███╗   ███╗ █████╗  ██████╗██╗  ██╗██╗███╗   ██╗ █████╗ 
//  ████╗ ████║██╔══██╗██╔════╝██║  ██║██║████╗  ██║██╔══██╗
//  ██╔████╔██║███████║██║     ███████║██║██╔██╗ ██║███████║
//  ██║╚██╔╝██║██╔══██║██║     ██╔══██║██║██║╚██╗██║██╔══██║
//  ██║ ╚═╝ ██║██║  ██║╚██████╗██║  ██║██║██║ ╚████║██║  ██║
//  ╚═╝     ╚═╝╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝
//                                                          

namespace Machina
{
    //  ██████╗  ██████╗ ██████╗  ██████╗ ████████╗
    //  ██╔══██╗██╔═══██╗██╔══██╗██╔═══██╗╚══██╔══╝
    //  ██████╔╝██║   ██║██████╔╝██║   ██║   ██║   
    //  ██╔══██╗██║   ██║██╔══██╗██║   ██║   ██║   
    //  ██║  ██║╚██████╔╝██████╔╝╚██████╔╝   ██║   
    //  ╚═╝  ╚═╝ ╚═════╝ ╚═════╝  ╚═════╝    ╚═╝   
    //                                             
    /// <summary>
    /// The core Class in Machina. Represents a state and action-based virtual robot, 
    /// and exposes the public API for robot manipulation and control.
    /// </summary>
    public class Robot
    {
        /// <summary>
        /// Build number.
        /// </summary>
        public static readonly int Build = 1600;

        /// <summary>
        /// Version number.
        /// </summary>
        public static readonly string Version = "0.9.0." + Build;

        /// <summary>
        /// A nickname for this Robot.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// What brand of robot is this?
        /// </summary>
        public RobotType Brand { get; internal set; }

        /// <summary>
        /// The main Control object, acts as an interface to all classes that
        /// manage robot control.
        /// </summary>
        private Control c;


        /// <summary>
        /// An internal logging class to be used by children objects to log messages from this Robot.
        /// </summary>
        internal RobotLogger logger;

        public RobotLogger Logger => logger;




        //  ██████╗ ██╗   ██╗██████╗ ██╗     ██╗ ██████╗     █████╗ ██████╗ ██╗
        //  ██╔══██╗██║   ██║██╔══██╗██║     ██║██╔════╝    ██╔══██╗██╔══██╗██║
        //  ██████╔╝██║   ██║██████╔╝██║     ██║██║         ███████║██████╔╝██║
        //  ██╔═══╝ ██║   ██║██╔══██╗██║     ██║██║         ██╔══██║██╔═══╝ ██║
        //  ██║     ╚██████╔╝██████╔╝███████╗██║╚██████╗    ██║  ██║██║     ██║
        //  ╚═╝      ╚═════╝ ╚═════╝ ╚══════╝╚═╝ ╚═════╝    ╚═╝  ╚═╝╚═╝     ╚═╝
        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="make"></param>
        private Robot(string name, RobotType make)
        {
            this.Name = name;
            this.Brand = make;
            this.logger = new RobotLogger(this);

            if (_reflectedAPI == null || _reflectedAPI.Count == 0)
            {
                LoadReflectedAPI();
            }

            c = new Control(this);
        }


        /// <summary>
        /// Create a new instance of a Robot.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="make"></param>
        /// <returns></returns>
        static public Robot Create(string name, RobotType make)
        {
            if (!Utilities.Strings.IsValidVariableName(name))
            {
                Machina.Logger.Error($"\"{name}\" is not a valid robot name, please start with a letter.");
                return null;
            }

            return new Robot(name, make);
        }

        /// <summary>
        /// Create a new instance of a Robot.
        /// </summary>
        /// <returns></returns>
        static public Robot Create() => Robot.Create("Machina", "HUMAN");

        /// <summary>
        /// Create a new instance of a Robot.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="make"></param>
        /// <returns></returns>
        static public Robot Create(string name, string make)
        {
            try
            {
                RobotType rt = Utilities.Parsing.ParseEnumValue<RobotType>(make);
                return new Robot(name, rt);
            }
            catch
            {
                Machina.Logger.Error($"{make} is not a RobotType, please specify one of the following: ");
                foreach (string str in Enum.GetNames(typeof(RobotType)))
                {
                    Machina.Logger.Error(str);
                }
                return null;
            }
        }




        //   ██████╗ ██████╗ ███╗   ██╗███████╗██╗ ██████╗ 
        //  ██╔════╝██╔═══██╗████╗  ██║██╔════╝██║██╔════╝ 
        //  ██║     ██║   ██║██╔██╗ ██║█████╗  ██║██║  ███╗
        //  ██║     ██║   ██║██║╚██╗██║██╔══╝  ██║██║   ██║
        //  ╚██████╗╚██████╔╝██║ ╚████║██║     ██║╚██████╔╝
        //   ╚═════╝ ╚═════╝ ╚═╝  ╚═══╝╚═╝     ╚═╝ ╚═════╝ 
        //                                                 

        /// What was this even for? Exports checks?
        public bool IsBrand(string brandName)
        {
            return this.Brand.ToString().Equals(brandName, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsBrand(RobotType brand)
        {
            return brand == this.Brand;
        }

        /// <summary>
        /// Configure how Actions are streamed to the controller.
        /// </summary>
        /// <param name="minActionOnController">When Machina detects that the controller has these many Actions or less buffered, it will start streaming new Actions.</param>
        /// <param name="maxActionsOnController">When Maxhina detects that the controller has these many Actions or more buffered, it will stop streaming and wait for them to reach minActionOnController to stream more.</param>
        /// <returns></returns>
        public bool StreamConfiguration(int minActionOnController, int maxActionsOnController)
        {
            return c.ConfigureBuffer(minActionOnController, maxActionsOnController);
        }

        /// <summary>
        /// Sets the control type the robot will operate under, like "offline" or "online".
        /// </summary>
        /// <param name="controlType"></param>
        /// <returns></returns>
        public bool ControlMode(ControlType controlType)
        {
            return c.SetControlMode(controlType);
        }

        /// <summary>
        /// Sets the control type the robot will operate under, like "offline" or "online".
        /// </summary>
        /// <param name="controlType"></param>
        /// <returns></returns>
        public bool ControlMode(string controlType)
        {
            ControlType ct;
            try
            {
                ct = (ControlType)Enum.Parse(typeof(ControlType), controlType, true);
                if (Enum.IsDefined(typeof(ControlType), ct))
                {
                    return c.SetControlMode(ct);
                }
            }
            catch
            {
                logger.Error($"{controlType} is not a valid ControlMode type, please specify one of the following:");
                foreach (string str in Enum.GetNames(typeof(ControlType)))
                {
                    logger.Error(str);
                }
            }
            return false;
        }

        /// <summary>
        /// Sets who will be in charge of managing the connection to the device,
        /// i.e. having "Machina" try to load driver modules to the controller or 
        /// leave that task to the "User" (default).
        /// </summary>
        /// <param name="connectionManager">"User" or "Machina"</param>
        /// <returns></returns>
        public bool ConnectionManager(string connectionManager)
        {
            ConnectionType cm;
            try
            {
                cm = (ConnectionType)Enum.Parse(typeof(ConnectionType), connectionManager, true);
                if (Enum.IsDefined(typeof(ConnectionType), cm))
                    return c.SetConnectionMode(cm);
            }
            catch
            {
                logger.Error($"{connectionManager} is not a valid ConnectionManagerType type, please specify one of the following:");
                foreach (string str in Enum.GetNames(typeof(ConnectionType)))
                    logger.Error(str);
            }
            return false;
        }

        /// <summary>
        /// Sets who will be in charge of managing the connection to the device,
        /// i.e. having "Machina" try to load driver modules to the controller or 
        /// leave that task to the "User" (default).
        /// </summary>
        /// <param name="connectionManager">"User" or "Machina"</param>
        /// <returns></returns>
        public bool ConnectionManager(ConnectionType connectionManager)
        {
            return c.SetConnectionMode(connectionManager);
        }

        /// <summary>
        /// If the controller needs special user logging, set the credentials here. 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool SetUser(string name, string password)
        {
            return c.SetUserCredentials(name, password);
        }

        /// <summary>
        /// Scans the network for robotic devices, real or virtual, and performs all necessary 
        /// operations to connect to it. This is necessary for 'online' modes such as 'execute' and 'stream.'
        /// </summary>
        /// <param name="robotId">If multiple devices are connected, choose this id from the list.</param>
        /// <returns></returns>
        public bool Connect(int robotId = 0)
        {
            return c.ConnectToDevice(robotId);
        }

        /// <summary>
        /// Tries to establish connection to a remote device for 'online' modes.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Connect(string ip, int port)
        {
            return c.ConnectToDevice(ip, port);
        }

        /// <summary>
        /// Performs all necessary instructions to disconnect from and dispose a robot device, real or virtual. 
        /// This is necessary before leaving current execution thread.
        /// </summary>
        public bool Disconnect()
        {
            return c.DisconnectFromDevice();
        }

        /// <summary>
        /// Returns a string representation of the IP of the currently connected robot device.
        /// </summary>
        /// <returns></returns>
        public string GetIP()
        {
            return c.GetControllerIP();
        }

        /// <summary>
        /// Create a program in the device's native language with all the buffered Actions and return it as a RobotProgram,
        /// representing the different program files. Note all buffered Actions will be removed from the queue.
        /// </summary>
        /// <param name="inlineTargets">Write inline targets on action statements, or declare them as independent variables?</param>
        /// <param name="humanComments">If true, a human-readable description will be added to each line of code</param>
        /// <returns></returns>
        public RobotProgram Compile(bool inlineTargets = true, bool humanComments = true)
        {
            return c.Export(inlineTargets, humanComments);
        }

        /// <summary>
        /// Saves a RobotProgram to a folder in the system, generating all the required files and extensions.
        /// </summary>
        /// <param name="program"></param>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public bool SaveProgram(RobotProgram program, string folderPath)
        {
            return program.SaveToFolder(folderPath, logger);
        }


        ///// <summary>
        ///// Create a program in the device's native language with all the buffered Actions and save it to files. 
        ///// Note all buffered Actions will be removed from the queue.
        ///// </summary>
        ///// <param name="folderPath">The folder where the program files will be written to.</param>
        ///// <param name="inlineTargets">Write inline targets on action statements, or declare them as independent variables?</param>
        ///// <param name="humanComments">If true, a human-readable description will be added to each line of code</param>
        ///// <returns></returns>
        //public bool CompileToFolder(string folderPath, bool inlineTargets = true, bool humanComments = true)
        //{
        //    return c.Export(folderPath, inlineTargets, humanComments);
        //}




        ///// <summary>
        ///// In 'execute' mode, flushes all pending Actions, creates a program, 
        ///// uploads it to the controller and runs it.
        ///// </summary>
        ///// <returns></returns>
        //public void Execute()
        //{
        //    c.Execute();
        //}



        //  ███████╗███████╗████████╗████████╗██╗███╗   ██╗ ██████╗ ███████╗
        //  ██╔════╝██╔════╝╚══██╔══╝╚══██╔══╝██║████╗  ██║██╔════╝ ██╔════╝
        //  ███████╗█████╗     ██║      ██║   ██║██╔██╗ ██║██║  ███╗███████╗
        //  ╚════██║██╔══╝     ██║      ██║   ██║██║╚██╗██║██║   ██║╚════██║
        //  ███████║███████╗   ██║      ██║   ██║██║ ╚████║╚██████╔╝███████║
        //  ╚══════╝╚══════╝   ╚═╝      ╚═╝   ╚═╝╚═╝  ╚═══╝ ╚═════╝ ╚══════╝

        /// <summary>
        /// Sets the motion type (linear, joint...) for future issued Actions.
        /// </summary>
        /// <param name="motionType"></param>
        public bool MotionMode(MotionType motionType)
        {
            return c.IssueMotionRequest(motionType);
        }

        /// <summary>
        /// Sets the motion type (linear, joint...) for future issued Actions.
        /// </summary>
        /// <param name="motionType">"linear", "joint", etc.</param>
        [ParseableFromString]
        public bool MotionMode(string motionType)
        {
            MotionType mt;
            try
            {
                mt = (MotionType)Enum.Parse(typeof(MotionType), motionType, true);
                if (Enum.IsDefined(typeof(MotionType), mt))
                {
                    return c.IssueMotionRequest(mt);
                }
            }
            catch
            {
                logger.Error($"{motionType} is not a valid target part for motion type changes, please specify one of the following: ");
                foreach (string str in Enum.GetNames(typeof(MotionType)))
                {
                    logger.Error(str);
                }
            }
            return false;
        }

        /// <summary>
        /// Increase the speed at which new Actions will be executed. This value will be applied to linear motion in mm/s, and rotational or angular motion in deg/s.
        /// </summary>
        /// <param name="speedInc">Speed increment in mm/s or deg/s.</param>
        [ParseableFromString]
        public bool Speed(double speedInc)
        {
            return c.IssueSpeedRequest(speedInc, true);
        }

        /// <summary>
        /// Set the speed at which new Actions will be executed. This value will be applied to linear motion in mm/s, and rotational or angular motion in deg/s.
        /// </summary>
        /// <param name="speed">Speed value in mm/s or deg/s.</param>
        [ParseableFromString]
        public bool SpeedTo(double speed)
        {
            return c.IssueSpeedRequest(speed, false);
        }

        /// <summary>
        /// Increase the acceleration at which new Actions will be executed. This value will be applied to linear motion in mm/s^2, and rotational or angular motion in deg/s^2.
        /// </summary>
        /// <param name="accInc">Acceleration increment in mm/s^2 or deg/s^2.</param>
        /// <returns></returns>
        [ParseableFromString]
        public bool Acceleration(double accInc)
        {
            return c.IssueAccelerationRequest(accInc, true);
        }

        /// <summary>
        /// Set the acceleration at which new Actions will be executed. This value will be applied to linear motion in mm/s^2, and rotational or angular motion in deg/s^2.
        /// </summary>
        /// <param name="acceleration">Acceleration value in mm/s^2 or deg/s^2.</param>
        /// <returns></returns>
        [ParseableFromString]
        public bool AccelerationTo(double acceleration)
        {
            return c.IssueAccelerationRequest(acceleration, false);
        }

        /// <summary>
        /// Increase the default precision value new Actions will be given. 
        /// Precision is measured as the radius of the smooth interpolation
        /// between motion targets. This is refered to as "Zone", "Approximate
        /// Positioning" or "Blending Radius" in different platforms. 
        /// </summary>
        /// <param name="radiusInc">Smoothing radius increment in mm.</param>
        [ParseableFromString]
        public bool Precision(double radiusInc)
        {
            return c.IssuePrecisionRequest(radiusInc, true);
        }

        /// <summary>
        /// Set the default precision value new Actions will be given. 
        /// Precision is measured as the radius of the smooth interpolation
        /// between motion targets. This is refered to as "Zone", "Approximate
        /// Positioning" or "Blending Radius" in different platforms. 
        /// </summary>
        /// <param name="radius">Smoothing radius in mm.</param>
        [ParseableFromString]
        public bool PrecisionTo(double radius)
        {
            return c.IssuePrecisionRequest(radius, false);
        }

        // This should be a GetStuff method
        ///// <summary>
        ///// Gets current ReferenceCS setting.
        ///// </summary>
        ///// <returns></returns>
        //public ReferenceCS Coordinates()
        //{
        //    return c.GetCurrentReferenceCS();
        //}

        /// <summary>
        /// Sets the reference system used for relative transformations.
        /// </summary>
        /// <param name="refcs"></param>
        public bool Coordinates(ReferenceCS refcs)
        {
            return c.IssueCoordinatesRequest(refcs);
        }

        /// <summary>
        /// Sets the reference system used for relative transformations ("local", "global", etc.)
        /// </summary>
        /// <param name="type"></param>
        public bool Coordinates(string type)
        {
            ReferenceCS refcs;
            try
            {
                refcs = (ReferenceCS)Enum.Parse(typeof(ReferenceCS), type, true);
                if (Enum.IsDefined(typeof(ReferenceCS), refcs))
                {
                    return Coordinates(refcs);
                }
            }
            catch
            {
               logger.Error($"{type} is not a Coordinate System, please specify one of the following: ");
                foreach (string str in Enum.GetNames(typeof(ReferenceCS)))
                {
                    logger.Error(str);
                }
            }

            return false;
        }




        /// <summary>
        /// Increments the working temperature of one of the device's parts. Useful for 3D printing operations. 
        /// </summary>
        /// <param name="temp">Temperature increment in °C.</param>
        /// <param name="devicePart">Device's part that will change temperature, e.g. "extruder", "bed", etc.</param>
        /// <param name="waitToReachTemp">If true, execution will wait for the part to heat up and resume when reached the target.</param>
        /// <returns></returns>
        [ParseableFromString]
        public bool Temperature(double temp, string devicePart, bool waitToReachTemp = true)
        {
            RobotPartType tt;
            try
            {
                tt = (RobotPartType)Enum.Parse(typeof(RobotPartType), devicePart, true);
                if (Enum.IsDefined(typeof(RobotPartType), tt))
                {
                    return c.IssueTemperatureRequest(temp, tt, waitToReachTemp, true);
                }
            }
            catch
            {
                logger.Error($"{devicePart} is not a valid target part for temperature changes, please specify one of the following: ");
                foreach (string str in Enum.GetNames(typeof(RobotPartType)))
                {
                    logger.Error(str);
                }
            }
            return false;
        }

        /// <summary>
        /// Sets the working temperature of one of the device's parts. Useful for 3D printing operations. 
        /// </summary>
        /// <param name="temp">Temperature increment in °C.</param>
        /// <param name="devicePart">Device's part that will change temperature, e.g. "extruder", "bed", etc.</param>
        /// <param name="waitToReachTemp">If true, execution will wait for the part to heat up and resume when reached the target.</param>
        /// <returns></returns>
        [ParseableFromString]
        public bool TemperatureTo(double temp, string devicePart, bool waitToReachTemp = true)
        {
            RobotPartType tt;
            try
            {
                tt = (RobotPartType)Enum.Parse(typeof(RobotPartType), devicePart, true);
                if (Enum.IsDefined(typeof(RobotPartType), tt))
                {
                    return c.IssueTemperatureRequest(temp, tt, waitToReachTemp, false);
                }
            }
            catch
            {
                logger.Error($"{devicePart} is not a valid target part for temperature changes, please specify one of the following: ");
                foreach (string str in Enum.GetNames(typeof(RobotPartType)))
                {
                    logger.Error(str);
                }
            }
            return false;
        }

        /// <summary>
        /// Increases the extrusion rate of filament for 3D printers.
        /// </summary>
        /// <param name="rateInc">Increment of mm of filament per mm of movement.</param>
        /// <returns></returns>
        [ParseableFromString]
        public bool ExtrusionRate(double rateInc)
        {
            return c.IssueExtrusionRateRequest(rateInc, true);
        }

        /// <summary>
        /// Sets the extrusion rate of filament for 3D printers.
        /// </summary>
        /// <param name="rate">mm of filament per mm of movement.</param>
        /// <returns></returns>
        [ParseableFromString]
        public bool ExtrusionRateTo(double rate)
        {
            return c.IssueExtrusionRateRequest(rate, false);
        }

        /// <summary>
        /// Buffers current state settings (speed, precision, motion type...), and opens up for 
        /// temporary settings changes to be reverted by PopSettings().
        /// </summary>
        [ParseableFromString]
        public bool PushSettings()
        {
            return c.IssuePushPopRequest(true);
        }

        /// <summary>
        /// Reverts the state settings (speed, precision, motion type...) to the previously buffered
        /// state by PushSettings().
        /// </summary>
        [ParseableFromString]
        public bool PopSettings()
        {
            return c.IssuePushPopRequest(false);
        }




        //   █████╗  ██████╗████████╗██╗ ██████╗ ███╗   ██╗███████╗
        //  ██╔══██╗██╔════╝╚══██╔══╝██║██╔═══██╗████╗  ██║██╔════╝
        //  ███████║██║        ██║   ██║██║   ██║██╔██╗ ██║███████╗
        //  ██╔══██║██║        ██║   ██║██║   ██║██║╚██╗██║╚════██║
        //  ██║  ██║╚██████╗   ██║   ██║╚██████╔╝██║ ╚████║███████║
        //  ╚═╝  ╚═╝ ╚═════╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝
        //                                                         

        /// <summary>
        /// Issue a relative movement action request on current coordinate system.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool Move(Vector direction)
        {
            return c.IssueTranslationRequest(direction, true);
        }

        /// <summary>
        /// Issue a relative movement action request on current coordinate system.
        /// </summary>
        /// <param name="incX"></param>
        /// <param name="incY"></param>
        /// <param name="incZ"></param>
        /// <returns></returns>
        [ParseableFromString]
        public bool Move(double incX, double incY, double incZ = 0)
        {
            return Move(new Vector(incX, incY, incZ));
        }

        /// <summary>
        /// Issue an absolute movement action request.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool MoveTo(Point position)
        {
            return c.IssueTranslationRequest(position, false);
        }

        /// <summary>
        /// Issue an absolute movement action request.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        [ParseableFromString]
        public bool MoveTo(double x, double y, double z)
        {
            return MoveTo(new Vector(x, y, z));
        }

        /// <summary>
        /// Issue a RELATIVE rotation action request according to the current reference system.
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public bool Rotate(Rotation rotation)
        {
            return c.IssueRotationRequest(rotation, true);
        }

        /// <summary>
        /// Issue a RELATIVE rotation action request according to the current reference system.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="angDegs"></param>
        /// <returns></returns>
        public bool Rotate(Vector vector, double angDegs)
        {
            return Rotate(new Rotation(vector.X, vector.Y, vector.Z, angDegs, true));
        }

        /// <summary>
        /// Issue a RELATIVE rotation action request according to the current reference system.
        /// </summary>
        /// <param name="rotVecX"></param>
        /// <param name="rotVecY"></param>
        /// <param name="rotVecZ"></param>
        /// <param name="angDegs"></param>
        /// <returns></returns>
        [ParseableFromString]
        public bool Rotate(double rotVecX, double rotVecY, double rotVecZ, double angDegs)
        {
            return Rotate(new Rotation(rotVecX, rotVecY, rotVecZ, angDegs, true));
        }

        /// <summary>
        /// Issue an ABSOLUTE reorientation request according to the current reference system.
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public bool RotateTo(Rotation rotation)
        {
            return c.IssueRotationRequest(rotation, false);
        }

        /// <summary>
        /// Issue an ABSOLUTE reorientation request according to the current reference system.
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        public bool RotateTo(Orientation cs)
        {
            return RotateTo((Rotation)cs);
        }

        /// <summary>
        /// Issue an ABSOLUTE reorientation request according to the current reference system.
        /// </summary>
        /// <param name="vecX"></param>
        /// <param name="vecY"></param>
        /// <returns></returns>
        public bool RotateTo(Vector vecX, Vector vecY)
        {
            return RotateTo((Rotation)new Orientation(vecX, vecY));
        }

        /// <summary>
        /// Issue an ABSOLUTE reorientation request according to the current reference system.
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y0"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        [ParseableFromString]
        public bool RotateTo(double x0, double x1, double x2, double y0, double y1, double y2)
        {
            return RotateTo((Rotation)new Orientation(x0, x1, x2, y0, y1, y2));
        }

        /// <summary>
        /// Issue a compound RELATIVE local Translation + Rotation request
        /// according to the current reference system.
        /// Note that, if using local coordinates, order of Actions will matter. 
        /// TODO: wouldn't they matter too if they were in global coordinates?
        /// TODO II: should this be changed to simply mean a sort of plane to plane transform? 
        /// Such change would make this more intuitive...
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public bool Transform(Vector direction, Rotation rotation)
        {
            // Note the T+R action order
            //return c.IssueTranslationAndRotationRequest(position, true, rotation, true);

            return c.IssueTransformationRequest(direction, rotation, true, true);
        }

        /// <summary>
        /// Issue a compound RELATIVE local Rotation + Translation request
        /// according to the current reference system.
        /// Note that, if using local coordinates, order of Actions will matter. 
        /// TODO: wouldn't they matter too if they were in global coordinates?
        /// TODO II: should this be changed to simply mean a sort of plane to plane transform? 
        /// Such change would make this more intuitive...
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool Transform(Rotation rotation, Vector direction)
        {
            // Note the R+T action order
            //return c.IssueRotationAndTranslationRequest(rotation, true, position, true);

            return c.IssueTransformationRequest(direction, rotation, true, false);
        }

        /// <summary>
        /// Issue a compound ABSOLUTE global Translation + Rotation request
        /// according to the current reference system.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public bool TransformTo(Point position, Orientation orientation)
        {
            // Action order is irrelevant in absolute mode (since translations are applied based on immutable world XYZ)
            //return c.IssueTranslationAndRotationRequest(true, position, false, true, rotation, false);

            return c.IssueTransformationRequest(position, orientation, false, true);
        }

        /// <summary>
        /// Issue a compound ABSOLUTE global Translation + Rotation request
        /// according to the current reference system.
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool TransformTo(Orientation orientation, Point position)
        {
            // Action order is irrelevant in absolute mode (since translations are applied based on immutable world XYZ)
            //return c.IssueTranslationAndRotationRequest(true, position, false, true, rotation, false);

            return c.IssueTransformationRequest(position, orientation, false, false);
        }

        /// <summary>
        /// Issue a compound ABSOLUTE global Translation + Rotation request
        /// according to the current reference system.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="vX0"></param>
        /// <param name="vX1"></param>
        /// <param name="vX2"></param>
        /// <param name="vY0"></param>
        /// <param name="vY1"></param>
        /// <param name="vY2"></param>
        /// <returns></returns>
        [ParseableFromString]
        public bool TransformTo(double x, double y, double z, double vX0, double vX1, double vX2, double vY0, double vY1, double vY2) 
        {
            return c.IssueTransformationRequest(new Vector(x, y, z), new Orientation(vX0, vX1, vX2, vY0, vY1, vY2), false, true);
        }

        /// <summary>
        /// Issue a RELATIVE arc motion request according to the current reference system.
        /// Orientation will remain constant throughout the motion.
        /// </summary>
        /// <param name="through">Point to fit the arc motion through.</param>
        /// <param name="end">Point to end the arc motion at.</param>
        /// <returns></returns>
        public bool ArcMotion(Point through, Point end)
        {
            Plane throughP = new Plane(
                through.X, through.Y, through.Z,
                1, 0, 0,
                0, 1, 0);  // world XY, won't be used anyway

            Plane endP = new Plane(
                end.X, end.Y, end.Z,
                1, 0, 0,
                0, 1, 0);  // world XY, won't be used anyway

            return c.IssueArcMotionRequest(throughP, endP, true, true);
        }

        /// <summary>
        /// Issue a RELATIVE arc motion request according to the current reference system.
        /// Orientation will remain constant throughout the motion.
        /// </summary>
        /// <param name="throughX"></param>
        /// <param name="throughY"></param>
        /// <param name="throughZ"></param>
        /// <param name="endX"></param>
        /// <param name="endY"></param>
        /// <param name="endZ"></param>
        /// <returns></returns>
        [ParseableFromString]
        public bool ArcMotion(
            double throughX, double throughY, double throughZ,
            double endX, double endY, double endZ)
        {
            Plane through = new Plane(
                throughX, throughY, throughZ,
                1, 0, 0,
                0, 1, 0);  // world XY, won't be used anyway

            Plane end = new Plane(
                endX, endY, endZ,
                1, 0, 0,
                0, 1, 0);  // world XY, won't be used anyway

            return c.IssueArcMotionRequest(through, end, true, true);
        }

        /// <summary>
        /// Issue an ABSOLUTE arc motion request according to the current reference system.
        /// Orientation will remain constant throughout the motion.
        /// </summary>
        /// <param name="through">Point to fit the arc motion through.</param>
        /// <param name="end">Point to end the arc motion at.</param>
        /// <returns></returns>
        public bool ArcMotionTo(Point through, Point end)
        {
            Plane throughP = new Plane(
                through.X, through.Y, through.Z,
                1, 0, 0,
                0, 1, 0);  // world XY, won't be used anyway

            Plane endP = new Plane(
                end.X, end.Y, end.Z,
                1, 0, 0,
                0, 1, 0);  // world XY, won't be used anyway

            return c.IssueArcMotionRequest(throughP, endP, false, true);
        }

        /// <summary>
        /// Issue an ABSOLUTE arc motion request according to the current reference system.
        /// Orientation will remain constant throughout the motion.
        /// </summary>
        /// <param name="throughX"></param>
        /// <param name="throughY"></param>
        /// <param name="throughZ"></param>
        /// <param name="endX"></param>
        /// <param name="endY"></param>
        /// <param name="endZ"></param>
        /// <returns></returns>
        [ParseableFromString]
        public bool ArcMotionTo(
            double throughX, double throughY, double throughZ,
            double endX, double endY, double endZ)
        {
            Plane through = new Plane(
                throughX, throughY, throughZ,
                1, 0, 0, 
                0, 1, 0);  // world XY, won't be used anyway

            Plane end = new Plane(
                endX, endY, endZ,
                1, 0, 0,
                0, 1, 0);  // world XY, won't be used anyway

            return c.IssueArcMotionRequest(through, end, false, true);
        }

        /// <summary>
        /// Issue an ABSOLUTE arc motion request according to the current reference system.
        /// Orientation will also change through the fit planes.
        /// </summary>
        /// <param name="through">Plane to fit the arc motion through.</param>
        /// <param name="end">Plane to end the arc motion at.</param>
        /// <returns></returns>
        public bool ArcMotionTo(Plane through, Plane end)
        {
            // Use deep copies to avoid reference conflicts 
            // @TODO: really need to convert geo types to structs soon!
            return c.IssueArcMotionRequest(through.Clone(), end.Clone(), false, false);
        }

        /// <summary>
        /// Issue an ABSOLUTE arc motion request according to the current reference system.
        /// Orientation will also change through the fit planes.
        /// </summary>
        /// <param name="throughX"></param>
        /// <param name="throughY"></param>
        /// <param name="throughZ"></param>
        /// <param name="throughVX0"></param>
        /// <param name="throughVX1"></param>
        /// <param name="throughVX2"></param>
        /// <param name="throughVY0"></param>
        /// <param name="throughVY1"></param>
        /// <param name="throughVY2"></param>
        /// <param name="endX"></param>
        /// <param name="endY"></param>
        /// <param name="endZ"></param>
        /// <param name="endVX0"></param>
        /// <param name="endVX1"></param>
        /// <param name="endVX2"></param>
        /// <param name="endVY0"></param>
        /// <param name="endVY1"></param>
        /// <param name="endVY2"></param>
        /// <returns></returns>
        public bool ArcMotionTo(
            double throughX, double throughY, double throughZ,
            double throughVX0, double throughVX1, double throughVX2, 
            double throughVY0, double throughVY1, double throughVY2,
            double endX, double endY, double endZ, 
            double endVX0, double endVX1, double endVX2, 
            double endVY0, double endVY1, double endVY2)
        {
            //This would be nice to have [ParseableFromString] too, but we currently cannot discriminate between multiple functions with the same name... 
            Plane through = new Plane(
                throughX, throughY, throughZ,
                throughVX0, throughVX1, throughVX2,
                throughVY0, throughVY1, throughVY2);

            Plane end = new Plane(
                endX, endY, endZ,
                endVX0, endVX1, endVX2,
                endVY0, endVY1, endVY2);

            return c.IssueArcMotionRequest(through, end, false, false);
        }


        /// <summary>
        /// Issue a request to increment the angular values of the robot joint axes rotations.
        /// Values expressed in degrees.
        /// </summary>
        /// <param name="incJoints"></param>
        /// <returns></returns>
        public bool Axes(Joints incJoints)
        {
            return c.IssueJointsRequest(incJoints, true);
        }

        /// <summary>
        /// Issue a request to increment the angular values of the robot joint axes rotations.
        /// Values expressed in degrees.
        /// </summary>
        /// <param name="incJ1"></param>
        /// <param name="incJ2"></param>
        /// <param name="incJ3"></param>
        /// <param name="incJ4"></param>
        /// <param name="incJ5"></param>
        /// <param name="incJ6"></param>
        /// <returns></returns>
        [ParseableFromString]
        public bool Axes(double incJ1, double incJ2, double incJ3, double incJ4, double incJ5, double incJ6)
        {
            return c.IssueJointsRequest(new Joints(incJ1, incJ2, incJ3, incJ4, incJ5, incJ6), true);
        }

        /// <summary>
        /// Issue a request to set the angular values of the robot joint axes rotations.
        /// Values expressed in degrees.
        /// </summary>
        /// <param name="joints"></param>
        /// <returns></returns>
        /// 
        public bool AxesTo(Joints joints)
        {
            return c.IssueJointsRequest(joints, false);
        }

        /// <summary>
        /// Issue a request to set the angular values of the robot joint axes rotations.
        /// Values expressed in degrees.
        /// </summary>
        /// <param name="j1"></param>
        /// <param name="j2"></param>
        /// <param name="j3"></param>
        /// <param name="j4"></param>
        /// <param name="j5"></param>
        /// <param name="j6"></param>
        /// <returns></returns>
        [ParseableFromString]
        public bool AxesTo(double j1, double j2, double j3, double j4, double j5, double j6)
        {
            return c.IssueJointsRequest(new Joints(j1, j2, j3, j4, j5, j6), false);
        }

        /// <summary>
        /// Increase the value of one of the robot's external axis. 
        /// Values expressed in degrees or milimeters, depending on the nature of the external axis.
        /// Note that the effect of this change of external axis will go in effect on the next motion Action.
        /// </summary>
        /// <param name="axisNumber">Axis number from 1 to 6.</param>
        /// <param name="increment">Increment value in mm or degrees.</param>
        [ParseableFromString]
        public bool ExternalAxis(int axisNumber, double increment)
        {
            if (axisNumber == 0)
            {
                logger.Error("Please enter an axis number between 1-6");
                return false;
            }
            return c.IssueExternalAxisRequest(axisNumber, increment, ExternalAxesTarget.All, true);
        }

        /// <summary>
        /// Increase the value of one of the robot's external axis. 
        /// Values expressed in degrees or milimeters, depending on the nature of the external axis.
        /// Note that the effect of this change of external axis will go in effect on the next motion Action.
        /// </summary>
        /// <param name="axisNumber">Axis number from 1 to 6.</param>
        /// <param name="increment">Increment value in mm or degrees.</param>
        /// <param name="externalAxesTarget">Apply this change to all external axes, or only cartesian/joint targets.</param>
        /// <returns></returns>
        public bool ExternalAxis(int axisNumber, double increment, string externalAxesTarget)
        {
            ExternalAxesTarget eat;
            try
            {
                eat = (ExternalAxesTarget)Enum.Parse(typeof(ExternalAxesTarget), externalAxesTarget, true);
                if (Enum.IsDefined(typeof(ExternalAxesTarget), eat))
                {
                    return ExternalAxis(axisNumber, increment, eat);
                }
            }
            catch
            {
                logger.Error($"{externalAxesTarget} is not a valid ExternalAxesTarget type, please specify one of the following:");
                foreach (string str in Enum.GetNames(typeof(ExternalAxesTarget)))
                {
                    logger.Error(str);
                }
            }
            return false;
        }

        /// <summary>
        /// Increase the value of one of the robot's external axis. 
        /// Values expressed in degrees or milimeters, depending on the nature of the external axis.
        /// Note that the effect of this change of external axis will go in effect on the next motion Action.
        /// </summary>
        /// <param name="axisNumber">Axis number from 1 to 6.</param>
        /// <param name="increment">Increment value in mm or degrees.</param>
        /// <param name="externalAxesTarget">Apply this change to all external axes, or only cartesian/joint targets.</param>
        public bool ExternalAxis(int axisNumber, double increment, ExternalAxesTarget externalAxesTarget)
        {
            if (axisNumber == 0)
            {
                logger.Error("Please enter an axis number between 1-6");
                return false;
            }
            return c.IssueExternalAxisRequest(axisNumber, increment, externalAxesTarget, true);
        }

        /// <summary>
        /// Set the value of one of the robot's external axis. 
        /// Values expressed in degrees or milimeters, depending on the nature of the external axis.
        /// Note that the effect of this change of external axis will go in effect on the next motion Action.
        /// </summary>
        /// <param name="axisNumber">Axis number from 1 to 6.</param>
        /// <param name="value">Axis value in mm or degrees.</param>
        [ParseableFromString]
        public bool ExternalAxisTo(int axisNumber, double value)
        {
            if (axisNumber == 0)
            {
                logger.Error("Please enter an axis number between 1-6");
                return false;
            }
            return c.IssueExternalAxisRequest(axisNumber, value, ExternalAxesTarget.All, false);
        }

        /// <summary>
        /// Set the value of one of the robot's external axis. 
        /// Values expressed in degrees or milimeters, depending on the nature of the external axis.
        /// Note that the effect of this change of external axis will go in effect on the next motion Action.
        /// </summary>
        /// <param name="axisNumber">Axis number from 1 to 6.</param>
        /// <param name="value">Axis value in mm or degrees.</param>
        /// <param name="externalAxesTarget">Apply this change to all external axes, or only cartesian/joint targets.</param>
        /// <returns></returns>
        public bool ExternalAxisTo(int axisNumber, double value, string externalAxesTarget)
        {
            ExternalAxesTarget eat;
            try
            {
                eat = (ExternalAxesTarget)Enum.Parse(typeof(ExternalAxesTarget), externalAxesTarget, true);
                if (Enum.IsDefined(typeof(ExternalAxesTarget), eat))
                {
                    return ExternalAxisTo(axisNumber, value, eat);
                }
            }
            catch
            {
                logger.Error($"{externalAxesTarget} is not a valid ExternalAxesTarget type, please specify one of the following:");
                foreach (string str in Enum.GetNames(typeof(ExternalAxesTarget)))
                {
                    logger.Error(str);
                }
            }
            return false;
        }

        /// <summary>
        /// Set the value of one of the robot's external axis. 
        /// Values expressed in degrees or milimeters, depending on the nature of the external axis.
        /// Note that the effect of this change of external axis will go in effect on the next motion Action.
        /// </summary>
        /// <param name="axisNumber">Axis number from 1 to 6.</param>
        /// <param name="value">Axis value in mm or degrees.</param>
        /// <param name="externalAxesTarget">Apply this change to all external axes, or only cartesian/joint targets.</param>
        public bool ExternalAxisTo(int axisNumber, double value, ExternalAxesTarget externalAxesTarget)
        {
            if (axisNumber == 0)
            {
                logger.Error("Please enter an axis number between 1-6");
                return false;
            }
            return c.IssueExternalAxisRequest(axisNumber, value, externalAxesTarget, false);
        }

        // At the moment, allow only absolute setting, since the controller may change his value to find an IK solution to the target.
        // @TODO: bring back as soon as Machina does the IK.
        //public bool ArmAngle(double increment)
        //{
        //    return c.IssueArmAngleRequest(increment, true);
        //}

        /// <summary>
        /// Set the value of the arm-angle parameter.
        /// This value represents the planar offset around the 7th axis for 7-dof robotic arms.
        /// </summary>
        /// <param name="value">Angular value in degrees.</param>
        /// <returns></returns>
        public bool ArmAngleTo(double value)
        {
            return c.IssueArmAngleRequest(value, false);
        }


        /// <summary>
        /// Issue a request to wait idle before moving to next action. 
        /// </summary>
        /// <param name="timeMillis">Time expressed in milliseconds</param>
        /// <returns></returns>
        [ParseableFromString]
        public bool Wait(long timeMillis)
        {
            return c.IssueWaitRequest(timeMillis);
        }

        /// <summary>
        /// Send a string message to the device, to be displayed based on device's capacities.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [ParseableFromString]
        public bool Message(string message)
        {
            return c.IssueMessageRequest(message);
        }

        /// <summary>
        /// Display an internal comment in the compilation code. 
        /// Useful for internal annotations, reminders, etc. 
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        [ParseableFromString]
        public bool Comment(string comment)
        {
            return c.IssueCommentRequest(comment);
        }

        /// <summary>
        /// Insert a line of custom code directly into a compiled program. 
        /// This is useful for obscure instructions that are not covered by Machina's API. 
        /// Note that this Action cannot be checked for validity by Machina, and you are responsible for correct syntax.
        /// This Action is non-streamable. 
        /// </summary>
        /// <param name="statement">Code in the machine's native language.</param>
        /// <param name="isDeclaration">Is this a declaration, like a variable or a workobject? If so, this statement will be placed at the beginning of the program.</param>
        /// <returns></returns>
        public bool CustomCode(string statement, bool isDeclaration = false) =>
                c.IssueCustomCodeRequest(statement, isDeclaration);

        /// <summary>
        /// Define a Tool object on the Robot's internal library to make it avaliable for future Attach/Detach actions.
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        public bool DefineTool(Tool tool)
        {
            if (!Utilities.Strings.IsValidVariableName(tool.name))
            {
                logger.Error($"\"{tool.name}\" is not a valid tool name, please start with a letter.");
                return false;
            }

            Tool copy = Tool.Create(tool);
            return c.IssueDefineToolRequest(copy);
        }

        /// <summary>
        /// Define a Tool object on the Robot's internal library to make it avaliable for future Attach/Detach actions.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="TCPPosition"></param>
        /// <param name="TCPOrientation"></param>
        /// <returns></returns>
        public bool DefineTool(string name, Point TCPPosition, Orientation TCPOrientation)
        {
            if (!Utilities.Strings.IsValidVariableName(name))
            {
                logger.Error($"\"{name}\" is not a valid tool name, please start with a letter.");
                return false;
            }

            Tool tool = Tool.Create(name, TCPPosition, TCPOrientation);
            return c.IssueDefineToolRequest(tool);
        }

        /// <summary>
        /// Define a Tool object on the Robot's internal library to make it avaliable for future Attach/Detach actions.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="TCPPosition"></param>
        /// <param name="TCPOrientation"></param>
        /// <param name="weightKg"></param>
        /// <param name="centerOfGravity"></param>
        /// <returns></returns>
        public bool DefineTool(string name, Point TCPPosition, Orientation TCPOrientation, double weightKg, Point centerOfGravity)
        {
            if (!Utilities.Strings.IsValidVariableName(name))
            {
                logger.Error($"\"{name}\" is not a valid tool name, please start with a letter.");
                return false;
            }

            Tool tool = Tool.Create(name, TCPPosition, TCPOrientation, weightKg, centerOfGravity);
            return c.IssueDefineToolRequest(tool);
        }

        /// <summary>
        /// Define a Tool object on the Robot's internal library to make it avaliable for future Attach/Detach actions.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tcpX"></param>
        /// <param name="tcpY"></param>
        /// <param name="tcpZ"></param>
        /// <param name="tcp_vX0"></param>
        /// <param name="tcp_vX1"></param>
        /// <param name="tcp_vX2"></param>
        /// <param name="tcp_vY0"></param>
        /// <param name="tcp_vY1"></param>
        /// <param name="tcp_vY2"></param>
        /// <param name="weight"></param>
        /// <param name="cogX"></param>
        /// <param name="cogY"></param>
        /// <param name="cogZ"></param>
        /// <returns></returns>
        [ParseableFromString]
        public bool DefineTool(string name, double tcpX, double tcpY, double tcpZ,
            double tcp_vX0, double tcp_vX1, double tcp_vX2, double tcp_vY0, double tcp_vY1, double tcp_vY2,
            double weight, double cogX, double cogY, double cogZ)
        {
            if (!Utilities.Strings.IsValidVariableName(name))
            {
                logger.Error($"\"{name}\" is not a valid tool name, please start with a letter.");
                return false;
            }

            Tool tool = Tool.Create(name,
                tcpX, tcpY, tcpZ,
                tcp_vX0, tcp_vX1, tcp_vX2,
                tcp_vY0, tcp_vY1, tcp_vY2,
                weight,
                cogX, cogY, cogZ);
            return c.IssueDefineToolRequest(tool);
        }

        /// <summary>
        /// Attach a Tool to the flange of this Robot. From this moment on, 
        /// all actions will refer to the new Tool Center Point (TCP) of this Tool.
        /// Note that the Tool must have been previously defined via "DefineTool".
        /// </summary>
        /// <param name="toolName"></param>
        /// <returns></returns>
        [ParseableFromString]
        public bool AttachTool(string toolName)
        {
            if (!Utilities.Strings.IsValidVariableName(toolName))
            {
                logger.Error($"\"{toolName}\" is not a valid tool name, please start with a letter.");
                return false;
            }

            return c.IssueAttachRequest(toolName);
        }

        /// <summary>
        /// Detach all Tools from the flange of this Robot. From this moment on, 
        /// all actions will refer to the flange as the Tool Center Point (TCP).
        /// </summary>
        /// <returns></returns>
        [ParseableFromString]
        public bool DetachTool()
        {
            return c.IssueDetachRequest();
        }


        /// <summary>
        /// Attach a Tool to the flange of this Robot.
        /// From this moment, all Actions like Move or Rotate will refer
        /// to the Tool Center Point (TCP).
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        [System.Obsolete("Use AttachTool() instead.")]
        public bool Attach(Tool tool)
        {
            logger.Warning("Attach is deprecated, Use AttachTool() instead.");

            bool success = c.IssueDefineToolRequest(tool);
            success &= c.IssueAttachRequest(tool.name);
            return success;

            //return c.IssueAttachRequest(tool);
        }

        /// <summary>
        /// Detach all Tools from the flange of this Robot.
        /// From this moment, all Actions like Move or Rotate will refer
        /// to the Flange Center Point (FCP).
        /// </summary>
        /// <returns></returns>
        [System.Obsolete("Detach is deprecated, Use DetachTool() instead.")]
        public bool Detach()
        {
            logger.Warning("Detach is deprecated, Use DetachTool() instead.");
            return c.IssueDetachRequest();
        }


        /// <summary>
        /// Writes to the digital IO pin.
        /// </summary>
        /// <param name="pinNumber"></param>
        /// <param name="isOn"></param>
        /// <param name="toolPin">Is this pin on the tool?</param>
        public bool WriteDigital(int pinNumber, bool isOn, bool toolPin = false)
        {
            return c.IssueWriteToDigitalIORequest(pinNumber.ToString(), isOn, toolPin);
        }

        /// <summary>
        /// Writes to the digital IO pin.
        /// </summary>
        /// <param name="pinId">Pin name.</param>
        /// <param name="isOn"></param>
        /// <param name="toolPin">Is this pin on the tool?</param>
        [ParseableFromString]
        public bool WriteDigital(string pinId, bool isOn, bool toolPin = false)
        {
            return c.IssueWriteToDigitalIORequest(pinId, isOn, toolPin);
        }

        /// <summary>
        /// Writes to the analog IO pin.
        /// </summary>
        /// <param name="pinNumber"></param>
        /// <param name="value"></param>
        /// <param name="toolPin">Is this pin on the tool?</param>
        public bool WriteAnalog(int pinNumber, double value, bool toolPin = false)
        {
            return c.IssueWriteToAnalogIORequest(pinNumber.ToString(), value, toolPin);
        }

        /// <summary>
        /// Writes to the analog IO pin.
        /// </summary>
        /// <param name="pinId">Pin name.</param>
        /// <param name="value"></param>
        /// <param name="toolPin">Is this pin on the tool?</param>
        [ParseableFromString]
        public bool WriteAnalog(string pinId, double value, bool toolPin = false)
        {
            return c.IssueWriteToAnalogIORequest(pinId, value, toolPin);
        }

        /// <summary>
        /// Turns extrusion in 3D printers on/off.
        /// </summary>
        /// <param name="extrude">True/false for on/off.</param>
        /// <returns></returns>
        [ParseableFromString]
        public bool Extrude(bool extrude = true)
        {
            return c.IssueExtrudeRequest(extrude);
        }

        /// <summary>
        /// Initialize this device for action. Initialization uses device-specific
        /// common initialization routines, like homing and calibration, to set the 
        /// device ready for typical procedures like 3D printing. 
        /// </summary>
        /// <returns></returns>
        [ParseableFromString]
        public bool Initialize()
        {
            return c.IssueInitializationRequest(true);
        }

        /// <summary>
        /// Terminate this device. Termination uses device-specific
        /// common termination routines, like cooling or turning fans off, to prepare
        /// the device for idleness.
        /// </summary>
        /// <returns></returns>
        [ParseableFromString]
        public bool Terminate()
        {
            return c.IssueInitializationRequest(false);
        }

        /// <summary>
        /// Issue an Action represented by a string in the Machina Common Language, such as `Do("Move(100,0,0);")`.
        /// The Action will parse the string and use reflection to figure out the most suitable Action
        /// to associate to this. 
        /// </summary>
        /// <param name="actionStatement"></param>
        /// <returns></returns>
        public bool Do(string actionStatement)
        {
            // Should `Do` just generate the corresponding Action, or should it be an Action in itself...?
            // If it was, it would be hard to by-pass creating another Action when the reflected method is called...
            // So keep it as a "Setting" method for the time being...? Although it should prob be it's own Action,
            // just by design...

            // Also, should this method also be `[ParseableFromString]`?? So meta...! lol

            return c.IssueApplyActionRequestFromStringStatement(actionStatement);
        }


        /// <summary>
        /// Issues an Action object to this robot. This is useful when a list of Actions
        /// is already available, and needs to be applied to this Robot.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool Issue(Action action)
        {
            return c.IssueApplyActionRequest(action);
        }







        //   ██████╗ ███████╗████████╗████████╗███████╗██████╗ ███████╗
        //  ██╔════╝ ██╔════╝╚══██╔══╝╚══██╔══╝██╔════╝██╔══██╗██╔════╝
        //  ██║  ███╗█████╗     ██║      ██║   █████╗  ██████╔╝███████╗
        //  ██║   ██║██╔══╝     ██║      ██║   ██╔══╝  ██╔══██╗╚════██║
        //  ╚██████╔╝███████╗   ██║      ██║   ███████╗██║  ██║███████║
        //   ╚═════╝ ╚══════╝   ╚═╝      ╚═╝   ╚══════╝╚═╝  ╚═╝╚══════╝
        //               

        /// <summary>
        /// Returns the module files necessary to load on the device for a successful connection.
        /// </summary>
        /// <param name="parameters">Values necessary to be replaced on the modules, such as {"HOSTNAME","192.168.125.1"} or {"PORT","7000"}.</param>
        /// <returns>A dict with filename-filecontent pairs.</returns>
        public Dictionary<string, string> GetDeviceDriverModules(Dictionary<string, string> parameters) => c.GetDeviceDriverModules(parameters);

        /// <summary>
        /// Returns a Point representation of the Robot's TCP position in mm and World coordinates.
        /// </summary>
        /// <returns></returns>
        public Point GetCurrentPosition() => c.GetCurrentPosition();

        /// <summary>
        /// Returns a Rotation representation of the Robot's TCP orientation in quaternions.
        /// </summary>
        /// <returns></returns>
        public Rotation GetCurrentRotation() => c.GetCurrentRotation();

        /// <summary>
        /// Returns a Joint object representing the rotations in the robot axes.
        /// </summary>
        /// <returns></returns>
        public Joints GetCurrentAxes() => c.GetCurrentAxes();

        /// <summary>
        /// Retuns an ExternalAxes object representing the values of the external axes. If a value is null, that axis is not valid.
        /// </summary>
        /// <returns></returns>
        public ExternalAxes GetCurrentExternalAxes() => c.GetCurrentExternalAxes();

        public double GetCurrentSpeed() => c.GetCurrentSpeedSetting();

        public double GetCurrentAcceleration() => c.GetCurrentAccelerationSetting();

        public double GetCurrentPrecision() => c.GetCurrentPrecisionSetting();

        public MotionType GetCurrentMotionMode() => c.GetCurrentMotionTypeSetting();
        
        /// <summary>
        /// Returns the Tool object currently attached to this Robot, null if none.
        /// </summary>
        /// <returns>The Tool object currently attached to this Robot, null if none.</returns>
        public Tool GetCurrentTool() => c.GetCurrentTool();

        ///// <summary>
        ///// Returns a Point represnting the current location of the Tool Center Point
        ///// (if there is a Tool attached) or the Flange Center Point (if there isn't).
        ///// </summary>
        ///// <returns></returns>
        //public Point GetVirtualPosition() => c.GetVirtualPosition();

        ///// <summary>
        ///// Return a Orientation object representing the current orientation of the Tool Center Point
        ///// (if there is a Tool attached) or the Flange Center Point (if there isn't).
        ///// </summary>
        ///// <returns></returns>
        //public Orientation GetVirtualRotation() => c.GetVirtualRotation();

        ///// <summary>
        ///// Returns a Joint object representing the rotations in the robot axes.
        ///// </summary>
        ///// <returns></returns>
        //public Joints GetVirtualAxes() => c.GetVirtualAxes();

        ///// <summary>
        ///// Returns the Tool object currently attached to this Robot, null if none.
        ///// </summary>
        ///// <returns>The Tool object currently attached to this Robot, null if none.</returns>
        //public Tool GetVirtualTool() => c.GetVirtualTool();


        public override string ToString() => $"Robot[\"{this.Name}\", {this.Brand}]";





        //  ██████╗ ███████╗██████╗ ██╗   ██╗ ██████╗ 
        //  ██╔══██╗██╔════╝██╔══██╗██║   ██║██╔════╝ 
        //  ██║  ██║█████╗  ██████╔╝██║   ██║██║  ███╗
        //  ██║  ██║██╔══╝  ██╔══██╗██║   ██║██║   ██║
        //  ██████╔╝███████╗██████╔╝╚██████╔╝╚██████╔╝
        //  ╚═════╝ ╚══════╝╚═════╝  ╚═════╝  ╚═════╝ 
        //                                            
        /// <summary>
        /// Dumps a bunch of information to the console about the controller, the main task, etc.
        /// </summary>
        public void DebugDump()
        {
            c.DebugDump();
        }

        /// <summary>
        /// Dumps a list of the remaining buffered Actions.
        /// </summary>
        public void DebugBuffers()
        {
            c.DebugBuffers();
        }

        /// <summary>
        /// Dumps the state of the internal RobotPointers
        /// </summary>
        public void DebugRobotCursors()
        {
            c.DebugRobotCursors();
        }

        /// <summary>
        /// Dumps current Settings values
        /// </summary>
        public void DebugSettingsBuffer()
        {
            //c.DebugSettingsBuffer();
        }

        /// <summary>
        /// Sets Machina to dump all log messages to the Console.
        /// </summary>
        public void DebugMode(bool on)
        {
            if (on)
            {
                Machina.Logger.WriteLine += Console.WriteLine;
                Machina.Logger.SetLogLevel(LogLevel.DEBUG);
            } 
            else
            {
                Machina.Logger.WriteLine -= Console.WriteLine;
                Machina.Logger.SetLogLevel(LogLevel.INFO);
            }
        }









        //  ███████╗██╗   ██╗███████╗███╗   ██╗████████╗███████╗
        //  ██╔════╝██║   ██║██╔════╝████╗  ██║╚══██╔══╝██╔════╝
        //  █████╗  ██║   ██║█████╗  ██╔██╗ ██║   ██║   ███████╗
        //  ██╔══╝  ╚██╗ ██╔╝██╔══╝  ██║╚██╗██║   ██║   ╚════██║
        //  ███████╗ ╚████╔╝ ███████╗██║ ╚████║   ██║   ███████║
        //  ╚══════╝  ╚═══╝  ╚══════╝╚═╝  ╚═══╝   ╚═╝   ╚══════╝
        //                                                      
        /// <summary>
        /// Will be raised whenever an Action has been successfully issued and is scheduled for release to the device or compilation. 
        /// </summary>
        public event ActionIssuedHandler ActionIssued;
        public delegate void ActionIssuedHandler(object sender, ActionIssuedArgs args);
        internal virtual void OnActionIssued(ActionIssuedArgs args) => ActionIssued?.Invoke(this, args);

        /// <summary>
        /// Will be raised whenever an Action has been released to the device and is scheduled for execution.
        /// </summary>
        public event ActionReleasedHandler ActionReleased;
        public delegate void ActionReleasedHandler(object sender, ActionReleasedArgs args);
        internal virtual void OnActionReleased(ActionReleasedArgs args) => ActionReleased?.Invoke(this, args);

        /// <summary>
        /// Will be raised whenever an Action has completed execution on the device. 
        /// </summary>
        public event ActionExecutedHandler ActionExecuted;
        public delegate void ActionExecutedHandler(object sender, ActionExecutedArgs args);
        internal virtual void OnActionExecuted(ActionExecutedArgs args) => ActionExecuted?.Invoke(this, args);

        /// <summary>
        /// Will be raised whenever new information is available about the real-time information about the state of the device.
        /// </summary>
        public event MotionUpdateHandler MotionUpdate;
        public delegate void MotionUpdateHandler(object sender, MotionUpdateArgs args);
        internal virtual void OnMotionUpdate(MotionUpdateArgs args) => MotionUpdate?.Invoke(this, args);





        //  ██╗███╗   ██╗████████╗███████╗██████╗ ███╗   ██╗ █████╗ ██╗     
        //  ██║████╗  ██║╚══██╔══╝██╔════╝██╔══██╗████╗  ██║██╔══██╗██║     
        //  ██║██╔██╗ ██║   ██║   █████╗  ██████╔╝██╔██╗ ██║███████║██║     
        //  ██║██║╚██╗██║   ██║   ██╔══╝  ██╔══██╗██║╚██╗██║██╔══██║██║     
        //  ██║██║ ╚████║   ██║   ███████╗██║  ██║██║ ╚████║██║  ██║███████╗
        //  ╚═╝╚═╝  ╚═══╝   ╚═╝   ╚══════╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝╚══════╝
        //                                                                  
        /// <summary>
        /// A <Name, MethodInfo> dict of reflected methods from the main Robot class, 
        /// that can be invoked from parsed strings from primitive values. 
        /// Such methods are flagged with attributes, and loaded at runtime. 
        /// </summary>
        internal static Dictionary<string, MethodInfo> _reflectedAPI;

        /// <summary>
        /// A <Name, MethodInfo> dict of CASE-INSENSITIVE reflected methods from the main Robot class, 
        /// that can be invoked from parsed strings from primitive values. 
        /// Such methods are flagged with attributes, and loaded at runtime. 
        /// </summary>
        internal static Dictionary<string, MethodInfo> _reflectedAPICaseInsensitive;

        ///// <summary>
        ///// Stores how many overrides a particular method has. Useful to perform extra checks 
        ///// when a call might be ambiguous.  
        ///// </summary>
        //private Dictionary<string, int> _reflectedAPIOverrides;


        /// <summary>
        /// Used to load all methods from the API that can be parseable from a string, using reflection. 
        /// </summary>
        internal static void LoadReflectedAPI()
        {
            // https://stackoverflow.com/a/14362272/1934487
            Type robotType = typeof(Robot);
            _reflectedAPI = robotType
                .GetMethods()
                .Where(x => x.GetCustomAttributes().OfType<Attributes.ParseableFromString>().Any())
                .ToDictionary(y => y.Name);

            // This one is to issue warnings for badly cased instructions.  
            _reflectedAPICaseInsensitive = robotType
                .GetMethods()
                .Where(x => x.GetCustomAttributes().OfType<Attributes.ParseableFromString>().Any())
                .ToDictionary(y => y.Name, StringComparer.InvariantCultureIgnoreCase);

            Machina.Logger.Debug("Loaded reflected API");
            foreach (var pair in _reflectedAPI)
            {
                Machina.Logger.Debug(pair.Key + " --> " + pair.Value);
            }
        }

    }


}
