using System;
using System.Collections.Generic;

namespace Machina
{
    //   ██████╗ ██████╗ ███╗   ███╗██████╗ ██╗██╗     ███████╗██████╗ 
    //  ██╔════╝██╔═══██╗████╗ ████║██╔══██╗██║██║     ██╔════╝██╔══██╗
    //  ██║     ██║   ██║██╔████╔██║██████╔╝██║██║     █████╗  ██████╔╝
    //  ██║     ██║   ██║██║╚██╔╝██║██╔═══╝ ██║██║     ██╔══╝  ██╔══██╗
    //  ╚██████╗╚██████╔╝██║ ╚═╝ ██║██║     ██║███████╗███████╗██║  ██║
    //   ╚═════╝ ╚═════╝ ╚═╝     ╚═╝╚═╝     ╚═╝╚══════╝╚══════╝╚═╝  ╚═╝

    //  ███████╗███╗   ███╗ ██████╗ ██████╗ ██████╗ ██╗  ██╗
    //  ╚══███╔╝████╗ ████║██╔═══██╗██╔══██╗██╔══██╗██║  ██║
    //    ███╔╝ ██╔████╔██║██║   ██║██████╔╝██████╔╝███████║
    //   ███╔╝  ██║╚██╔╝██║██║   ██║██╔══██╗██╔═══╝ ██╔══██║
    //  ███████╗██║ ╚═╝ ██║╚██████╔╝██║  ██║██║     ██║  ██║
    //  ╚══════╝╚═╝     ╚═╝ ╚═════╝ ╚═╝  ╚═╝╚═╝     ╚═╝  ╚═╝

    /// <summary>
    /// A compiler for ZMorph 3D printers. 
    /// </summary>
    internal class CompilerZMORPH : Compiler
    {
        // A 'multidimensional' Dict to store permutations of (part, wait) to their corresponding GCode command
        // https://stackoverflow.com/a/15826532/1934487
        Dictionary<Tuple<RobotPart, bool>, String> tempToGCode;  
        

        internal CompilerZMORPH() : base(";")
        {
            tempToGCode = new Dictionary<Tuple<RobotPart, bool>, String>();
            tempToGCode.Add(new Tuple<RobotPart, bool>(RobotPart.Extruder, true), "M109");
            tempToGCode.Add(new Tuple<RobotPart, bool>(RobotPart.Extruder, false), "M104");
            tempToGCode.Add(new Tuple<RobotPart, bool>(RobotPart.Bed, true), "M190");
            tempToGCode.Add(new Tuple<RobotPart, bool>(RobotPart.Bed, false), "M140");
            tempToGCode.Add(new Tuple<RobotPart, bool>(RobotPart.Chamber, true), "M191");
            tempToGCode.Add(new Tuple<RobotPart, bool>(RobotPart.Chamber, false), "M141");
        }

        /// <summary>
        /// Creates a textual program representation of a set of Actions using native RAPID Laguage.
        /// WARNING: this method is EXTREMELY UNSAFE; it performs no IK calculations, assigns default [0,0,0,0] 
        /// robot configuration and assumes the robot controller will figure out the correct one.
        /// </summary>
        /// <param name="programName"></param>
        /// <param name="writePointer"></param>
        /// <param name="block">Use actions in waiting queue or buffer?</param>
        /// <returns></returns>
        //public override List<string> UNSAFEProgramFromBuffer(string programName, RobotCursor writePointer, bool block)
        public override List<string> UNSAFEProgramFromBuffer(string programName, RobotCursor writer, bool block, bool inlineTargets, bool humanComments)
        {
            ADD_ACTION_STRING = humanComments;

            // Which pending Actions are used for this program?
            // Copy them without flushing the buffer.
            List<Action> actions = block ?
                writer.actionBuffer.GetBlockPending(false) :
                writer.actionBuffer.GetAllPending(false);

            // CODE LINES GENERATION
            // TARGETS AND INSTRUCTIONS
            List<string> initializationLines = new List<string>();
            List<string> instructionLines = new List<string>();
            List<string> closingLines = new List<string>();

            // SOME INITIAL BOILERPLATE TO HEAT UP THE PRINTER, CALIBRATE IT, ETC.
            // ZMorph boilerplate
            //initializationLines.Add("M140 S60");                // set bed temp and move on to next inst
            //initializationLines.Add("M109 S200");               // set extruder bed temp and wait till heat up
            //initializationLines.Add("M190 S60");                // set bed temp and wait till heat up
            initializationLines.Add("G91");                     // set rel motion
            initializationLines.Add("G1 Z1.000 F200.000");      // move up 1mm and set printhead speed to 200 mm/min
            initializationLines.Add("G90");                     // set absolute positioning
            initializationLines.Add("G28 X0.000 Y0.00");        // home XY axes
            initializationLines.Add("G1 X117.500 Y125.000 F8000.000");  // move to bed center for Z homing
            initializationLines.Add("G28 Z0.000");              // home Z
            initializationLines.Add("G92 E0.00000");            // home filament
            initializationLines.Add("M82");                     // set extruder to absolute mode

            // Machina bolierplate
            initializationLines.Add(string.Format("G1 F{0}", Math.Round(writer.speed * 60.0, Geometry.STRING_ROUND_DECIMALS_MM)));  // initialize feed speed to the writer's state

            // DATA GENERATION
            // Use the write RobotCursor to generate the data
            //int it = 0;
            string line = null;
            foreach (Action a in actions)
            {
                // Move writerCursor to this action state
                writer.ApplyNextAction();  // for the buffer to correctly manage them

                // GCode is super straightforward, so no need to pre-declare anything
                if (GenerateInstructionDeclaration(a, writer, out line))
                {
                    instructionLines.Add(line);
                }

                //// Move on
                //it++;
            }

            // END THE PROGRAM AND LEAVE THE PRINTER READY
            // ZMorph
            closingLines.Add("G92 E0.0000");
            //closingLines.Add("G91");
            //closingLines.Add("G1 E-3.00000 F1800.000");
            //closingLines.Add("G90");
            //closingLines.Add("G92 E0.00000");
            //closingLines.Add("G1 X117.500 Y220.000 Z30.581 F300.000");

            closingLines.Add("T0");         // choose tool 0: is this for multihead? 
            closingLines.Add("M104 S0");    // set extruder temp and move on
            closingLines.Add("T1");         // choose tool 1
            closingLines.Add("M104 S0");    // ibid
            closingLines.Add("M140 S0");    // set bed temp and move on
            closingLines.Add("M106 S0");    // fan speed 0 (off)
            closingLines.Add("M84");        // stop idle hold (?)
            closingLines.Add("M220 S100");  // set speed factor override percentage 



            // PROGRAM ASSEMBLY
            // Initialize a module list
            List<string> module = new List<string>();

            // Initializations
            if (initializationLines.Count != 0)
            {
                module.AddRange(initializationLines);
                module.Add("");
            }

            // MAIN PROCEDURE
            // Instructions
            if (instructionLines.Count != 0)
            {
                module.AddRange(instructionLines);
                module.Add("");
            }

            // Wrapping up
            if (closingLines.Count != 0)
            {
                module.AddRange(closingLines);
                module.Add("");
            }

            return module;
        }




        //  ╦ ╦╔╦╗╦╦  ╔═╗
        //  ║ ║ ║ ║║  ╚═╗
        //  ╚═╝ ╩ ╩╩═╝╚═╝
        internal bool GenerateInstructionDeclaration(
            Action action, RobotCursor cursor,
            out string declaration)
        {
            string dec = null;
            switch (action.type)
            {
                case ActionType.Speed:
                    dec = string.Format("G1 F{0}",
                        Math.Round(60.0 * cursor.speed, Geometry.STRING_ROUND_DECIMALS_MM));
                    break;

                case ActionType.Translation:
                case ActionType.Transformation:
                    dec = string.Format("G1 {0}{1}",
                        GetPositionTargetValue(cursor),
                        cursor.isExtruding ? " " + GetExtrusionTargetValue(cursor) : "");
                    break;

                // Only available in MakerBot? http://reprap.org/wiki/G-code#M70:_Display_message
                case ActionType.Message:
                    ActionMessage am = (ActionMessage)action;
                    dec = string.Format("M70 P1000 ({0})",
                        am.message);
                    break;

                // In GCode, this is called "Dwell"
                case ActionType.Wait:
                    ActionWait aw = (ActionWait)action;
                    dec = string.Format("G4 P{0}",
                        aw.millis);
                    break;

                case ActionType.Comment:
                    ActionComment ac = (ActionComment)action;
                    dec = string.Format("{0} {1}",
                        commentCharacter,
                        ac.comment);
                    break;

                // Untested, but oh well...
                // http://reprap.org/wiki/G-code#M42:_Switch_I.2FO_pin
                case ActionType.IODigital:
                    ActionIODigital aiod = (ActionIODigital)action;
                    dec = $"M42 P{aiod.pin} S{(cursor.digitalOutputs[aiod.pin] ? "1" : "0")}";
                    break;

                case ActionType.IOAnalog:
                    ActionIOAnalog aioa = (ActionIOAnalog)action;
                    if (aioa.value < 0 || aioa.value > 255)
                    {
                        dec = $"{commentCharacter} ERROR on \"{aioa.ToString()}\": value out of range [0..255]";
                    }
                    else
                    {
                        dec = $"M42 P{aioa.pin} S{Math.Round(cursor.analogOutputs[aioa.pin], 0)}";
                    }
                    break;

                case ActionType.Temperature:
                    ActionTemperature at = (ActionTemperature)action;
                    dec = $"{tempToGCode[new Tuple<RobotPart, bool>(at.robotPart, at.wait)]} S{Math.Round(cursor.partTemperature[at.robotPart], Geometry.STRING_ROUND_DECIMALS_TEMPERATURE)}";
                    break;

                case ActionType.Extrusion:
                case ActionType.ExtrusionRate:
                    dec = $"{commentCharacter} {action.ToString()}";  // has no direct G-code, simply annotate it as a comment
                    break;

                // If action wasn't implemented before, then it doesn't apply to this device
                default:
                    dec = $"{commentCharacter} ACTION \"{action}\" NOT APPLICABLE TO THIS DEVICE";
                    break;

                //case ActionType.Rotation:
                //case ActionType.Zone:
                //case ActionType.Joints:
                //case ActionType.Attach:
                //case ActionType.Detach:
                //    dec = string.Format("; ACTION \"{0}\" NOT IMPLEMENTED IN THIS DEVICE", action);
                //    break;
            }

            // Add trailing comments or ids if speficied
            if (ADD_ACTION_STRING && action.type != ActionType.Comment)
            {
                dec = string.Format("{0}  {1} [{2}]",
                    dec,
                    commentCharacter,
                    action.ToString());
            }
            else if (ADD_ACTION_ID)
            {
                dec = string.Format("{0}  {1} [{2}]",
                    dec,
                    commentCharacter,
                    action.id);
            }

            declaration = dec;
            return dec != null;
        }

        /// <summary>
        /// Returns a simple XYZ position.
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        internal string GetPositionTargetValue(RobotCursor cursor)
        {
            return string.Format("X{0} Y{1} Z{2}",
                Math.Round(cursor.position.X, Geometry.STRING_ROUND_DECIMALS_MM),
                Math.Round(cursor.position.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                Math.Round(cursor.position.Z, Geometry.STRING_ROUND_DECIMALS_MM));
        }

        /// <summary>
        /// Computes how much the cursor has moved in this action, and returns how much
        /// filament it should extrude based on extrusion rate.
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        internal string GetExtrusionTargetValue(RobotCursor cursor)
        {
            return $"E{Math.Round(cursor.extrusionRate * cursor.position.DistanceTo(cursor.prevPosition), 6)}";
        }

    }
}
