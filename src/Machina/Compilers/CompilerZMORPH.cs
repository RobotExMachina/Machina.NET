using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Machina.Types.Data;
using Machina.Types.Geometry;
using Machina.Descriptors.Cursors;

namespace Machina
{
    //   ██████╗ ██████╗ ███╗   ███╗██████╗ ██╗██╗     ███████╗██████╗ 
    //  ██╔════╝██╔═══██╗████╗ ████║██╔══██╗██║██║     ██╔════╝██╔══██╗
    //  ██║     ██║   ██║██╔████╔██║██████╔╝██║██║     █████╗  ██████╔╝
    //  ██║     ██║   ██║██║╚██╔╝██║██╔═══╝ ██║██║     ██╔══╝  ██╔══██╗
    //  ╚██████╗╚██████╔╝██║ ╚═╝ ██║██║     ██║███████╗███████╗██║  ██║
    //   ╚═════╝ ╚═════╝ ╚═╝     ╚═╝╚═╝     ╚═╝╚══════╝╚══════╝╚═╝  ╚═╝
    //
    //  ███████╗███╗   ███╗ ██████╗ ██████╗ ██████╗ ██╗  ██╗
    //  ╚══███╔╝████╗ ████║██╔═══██╗██╔══██╗██╔══██╗██║  ██║
    //    ███╔╝ ██╔████╔██║██║   ██║██████╔╝██████╔╝███████║
    //   ███╔╝  ██║╚██╔╝██║██║   ██║██╔══██╗██╔═══╝ ██╔══██║
    //  ███████╗██║ ╚═╝ ██║╚██████╔╝██║  ██║██║     ██║  ██║
    //  ╚══════╝╚═╝     ╚═╝ ╚═════╝ ╚═╝  ╚═╝╚═╝     ╚═╝  ╚═╝
    //
    /// <summary>
    /// A compiler for ZMorph 3D printers. 
    /// </summary>
    internal class CompilerZMORPH : Compiler
    {
        internal override Encoding Encoding => Encoding.ASCII;

        internal override char CC => ';';

        // A 'multidimensional' Dict to store permutations of (part, wait) to their corresponding GCode command
        // https://stackoverflow.com/a/15826532/1934487
        Dictionary<Tuple<RobotPartType, bool>, String> tempToGCode = new Dictionary<Tuple<RobotPartType, bool>, String>()
        {
            { new Tuple<RobotPartType, bool>(RobotPartType.Extruder, true), "M109" },
            { new Tuple<RobotPartType, bool>(RobotPartType.Extruder, false), "M104" },
            { new Tuple<RobotPartType, bool>(RobotPartType.Bed, true), "M190" },
            { new Tuple<RobotPartType, bool>(RobotPartType.Bed, false), "M140" },
            { new Tuple<RobotPartType, bool>(RobotPartType.Chamber, true), "M191" },
            { new Tuple<RobotPartType, bool>(RobotPartType.Chamber, false), "M141" }
        };

        // On extrusion length reset, keep track of the reset point ("G92")
        double extrusionLengthResetPosition = 0;

        // Every n mm of extrusion, reset "E" to zero: "G92 E0"
        double extrusionLengthResetEvery = 10;

        // Made this class members to be able to insert more than one line of code per Action
        // @TODO: make adding several lines of code per Action more programmatic
        List<string> initializationLines,
                     customDeclarationLines,
                     instructionLines,
                     closingLines;

        // Base constructor
        internal CompilerZMORPH() : base() { }



        /// <summary>
        /// Creates a textual program representation of a set of Actions using native GCode Laguage.
        /// </summary>
        /// <param name="programName"></param>
        /// <param name="writePointer"></param>
        /// <param name="block">Use actions in waiting queue or buffer?</param>
        /// <returns></returns>
        public override RobotProgram UNSAFEFullProgramFromBuffer(string programName, RobotCursor writer, bool block, bool inlineTargets, bool humanComments)
        {
            // The program files to be returned
            RobotProgram robotProgram = new RobotProgram(programName, CC);


            addActionString = humanComments;

            // Which pending Actions are used for this program?
            // Copy them without flushing the buffer.
            List<Action> actions = block ?
                writer.actionBuffer.GetBlockPending(false) :
                writer.actionBuffer.GetAllPending(false);

            // CODE LINES GENERATION
            // TARGETS AND INSTRUCTIONS
            this.initializationLines = new List<string>();
            this.customDeclarationLines = new List<string>();
            this.instructionLines = new List<string>();
            this.closingLines = new List<string>();

            // --> MOVED TO CUSTOM ACTION `Initialize()`
            //// SOME INITIAL BOILERPLATE TO HEAT UP THE PRINTER, CALIBRATE IT, ETC.
            //// ZMorph boilerplate
            //// HEATUP -> For the user, may not want to use the printer as printer...
            //initializationLines.Add("M140 S60");                // set bed temp and move on to next inst
            //initializationLines.Add("M109 S200");               // set extruder bed temp and wait till heat up
            //initializationLines.Add("M190 S60");                // set bed temp and wait till heat up
            //// HOMING
            //this.initializationLines.Add("G91");                     // set rel motion
            //this.initializationLines.Add("G1 Z1.000 F200.000");      // move up 1mm and accelerate printhead to 200 mm/min
            //this.initializationLines.Add("G90");                     // set absolute positioning
            //this.initializationLines.Add("G28 X0.000 Y0.00");        // home XY axes
            //this.initializationLines.Add("G1 X117.500 Y125.000 F8000.000");  // move to bed center for Z homing
            //this.initializationLines.Add("G28 Z0.000");              // home Z
            //this.initializationLines.Add("G92 E0.00000");            // set filament position to zero

            //// Machina bolierplate
            //this.initializationLines.Add("M82");                     // set extruder to absolute mode (this is actually ZMorph, but useful here
            //this.initializationLines.Add($"G1 F{Math.Round(writer.speed * 60.0, MachinaMath.STRING_ROUND_DECIMALS_MM)}");  // initialize feed speed to the writer's state

            this.initializationLines.AddRange(GenerateDisclaimerHeader(programName));

            // DATA GENERATION
            // Use the write RobotCursor to generate the data
            //int it = 0;
            string line = null;
            foreach (Action a in actions)
            {
                // Move writerCursor to this action state
                writer.ApplyNextAction();  // for the buffer to correctly manage them

                if (a.Type == ActionType.CustomCode && (a as ActionCustomCode).isDeclaration)
                {
                    customDeclarationLines.Add((a as ActionCustomCode).statement);
                }

                // GCode is super straightforward, so no need to pre-declare anything
                if (GenerateInstructionDeclaration(a, writer, out line))
                {
                    this.instructionLines.Add(line);
                }

                //// Move on
                //it++;
            }

            // --> MOVED TO CUSTOM ACTION `Terminate()`
            //// END THE PROGRAM AND LEAVE THE PRINTER READY
            //// ZMorph boilerplate
            //this.closingLines.Add("G92 E0.0000");
            //this.closingLines.Add("G91");
            //this.closingLines.Add("G1 E-3.00000 F1800.000");
            //this.closingLines.Add("G90");
            //this.closingLines.Add("G92 E0.00000");
            //this.closingLines.Add("G1 X117.500 Y220.000 Z30.581 F300.000");

            //this.closingLines.Add("T0");         // choose tool 0: is this for multihead? 
            //this.closingLines.Add("M104 S0");    // set extruder temp and move on
            //this.closingLines.Add("T1");         // choose tool 1
            //this.closingLines.Add("M104 S0");    // ibid
            //this.closingLines.Add("M140 S0");    // set bed temp and move on
            //this.closingLines.Add("M106 S0");    // fan speed 0 (off)
            //this.closingLines.Add("M84");        // stop idle hold (?)
            //this.closingLines.Add("M220 S100");  // set speed factor override percentage 

            
            // PROGRAM ASSEMBLY
            // Initialize a module list
            List<string> module = new List<string>();

            // Initializations
            if (this.initializationLines.Count != 0)
            {
                module.AddRange(this.initializationLines);
                module.Add("");
            }

            // Custom declarations
            if (this.customDeclarationLines.Count != 0)
            {
                module.AddRange(this.customDeclarationLines);
                module.Add("");
            }

            // MAIN PROCEDURE
            // Instructions
            if (this.instructionLines.Count != 0)
            {
                module.AddRange(this.instructionLines);
                module.Add("");
            }

            // Wrapping up
            if (this.closingLines.Count != 0)
            {
                module.AddRange(this.closingLines);
                module.Add("");
            }

            RobotProgramFile mainFile = new RobotProgramFile(programName, "gcode", Encoding, CC);
            mainFile.SetContent(module);
            robotProgram.Add(mainFile);

            return robotProgram;
        }




        //  ╦ ╦╔╦╗╦╦  ╔═╗
        //  ║ ║ ║ ║║  ╚═╗
        //  ╚═╝ ╩ ╩╩═╝╚═╝
        internal bool GenerateInstructionDeclaration(
            Action action, RobotCursor cursor,
            out string declaration)
        {
            string dec = null;
            switch (action.Type)
            {
                case ActionType.Speed:
                    dec = string.Format(CultureInfo.InvariantCulture, 
                        "G1 F{0}",
                        Math.Round(60.0 * cursor.speed, MMath.STRING_ROUND_DECIMALS_MM));
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
                    dec = string.Format(CultureInfo.InvariantCulture, 
                        "G4 P{0}",
                        aw.millis);
                    break;

                case ActionType.Comment:
                    ActionComment ac = (ActionComment)action;
                    dec = string.Format("{0} {1}",
                        CC,
                        ac.comment);
                    break;

                // Untested, but oh well...
                // http://reprap.org/wiki/G-code#M42:_Switch_I.2FO_pin
                case ActionType.IODigital:
                    ActionIODigital aiod = (ActionIODigital)action;
                    if (!aiod.isDigit)
                    {
                        dec = $"{CC} ERROR on \"{aiod}\": only integer pin names allowed";
                    }
                    else
                    {
                        dec = $"M42 P{aiod.pinNum} S{(aiod.on ? "1" : "0")}";
                    }
                    break;


                case ActionType.IOAnalog:
                    ActionIOAnalog aioa = (ActionIOAnalog)action;
                    if (!aioa.isDigit)
                    {
                        dec = $"{CC} ERROR on \"{aioa}\": only integer pin names allowed";
                    }
                    else if (aioa.value < 0 || aioa.value > 255)
                    {
                        dec = $"{CC} ERROR on \"{aioa.ToString()}\": value out of range [0..255]";
                    }
                    else
                    {
                        //dec = $"M42 P{aioa.pinNum} S{Math.Round(aioa.value, 0)}";
                        dec = string.Format(CultureInfo.InvariantCulture,
                            "M42 P{0} S{1}",
                            aioa.pinNum,
                            Math.Round(aioa.value, 0));
                    }
                    break;

                case ActionType.Temperature:
                    ActionTemperature at = (ActionTemperature)action;
                    //dec = $"{tempToGCode[new Tuple<RobotPartType, bool>(at.robotPart, at.wait)]} S{Math.Round(cursor.partTemperature[at.robotPart], MMath.STRING_ROUND_DECIMALS_TEMPERATURE)}";
                    dec = string.Format(CultureInfo.InvariantCulture,
                        "{0} S{1}",
                        tempToGCode[new Tuple<RobotPartType, bool>(at.robotPart, at.wait)],
                        Math.Round(cursor.partTemperature[at.robotPart], MMath.STRING_ROUND_DECIMALS_TEMPERATURE));
                    break;

                case ActionType.Extrusion:
                case ActionType.ExtrusionRate:
                    dec = $"{CC} {action.ToString()}";  // has no direct G-code, simply annotate it as a comment
                    break;

                case ActionType.Initialization:
                    ActionInitialization ai = (ActionInitialization)action;
                    if (ai.initialize == true)
                        StartCodeBoilerplate(cursor);
                    else
                        EndCodeBoilerplate(cursor);

                    break;

                case ActionType.CustomCode:
                    ActionCustomCode acc = action as ActionCustomCode;
                    if (!acc.isDeclaration)
                    {
                        dec = $"{acc.statement}";
                    }
                    break;

                // If action wasn't implemented before, then it doesn't apply to this device
                default:
                    dec = $"{CC} ACTION \"{action}\" NOT APPLICABLE TO THIS DEVICE";
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
            if (addActionString && action.Type != ActionType.Comment)
            {
                dec = string.Format("{0}  {1} [{2}]",
                    dec,
                    CC,
                    action.ToString());
            }
            else if (addActionID)
            {
                dec = string.Format("{0}  {1} [{2}]",
                    dec,
                    CC,
                    action.Id);
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
            return string.Format(CultureInfo.InvariantCulture, 
                "X{0} Y{1} Z{2}",
                Math.Round(cursor.position.X, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(cursor.position.Y, MMath.STRING_ROUND_DECIMALS_MM),
                Math.Round(cursor.position.Z, MMath.STRING_ROUND_DECIMALS_MM));
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
            double len = cursor.extrudedLength - this.extrusionLengthResetPosition;

            // If extruded over the limit, reset extrude position and start over
            if (len > extrusionLengthResetEvery) {
                this.instructionLines.Add($"{CC} Homing extrusion length after {cursor.prevExtrudedLength - this.extrusionLengthResetPosition} mm ({this.extrusionLengthResetEvery} mm limit)");
                this.instructionLines.Add($"G92 E0.0000");
                this.extrusionLengthResetPosition = cursor.prevExtrudedLength;
                len = cursor.extrudedLength - this.extrusionLengthResetPosition;
            }

            return string.Format(CultureInfo.InvariantCulture, 
                "E{0}",
                Math.Round(len, 5));
        }

        /// <summary>
        /// Dumps a bunch of initilazation boilerplate
        /// </summary>
        /// <param name="cursor"></param>
        internal void StartCodeBoilerplate(RobotCursor cursor)
        {
            // SOME INITIAL BOILERPLATE TO HEAT UP THE PRINTER, CALIBRATE IT, ETC.
            // ZMorph boilerplate
            // HEATUP -> For the user, may not want to use the printer as printer...
            //instructionLines.Add("M140 S60");                // set bed temp and move on to next inst
            //instructionLines.Add("M109 S200");               // set extruder bed temp and wait till heat up
            //instructionLines.Add("M190 S60");                // set bed temp and wait till heat up
            // HOMING
            this.instructionLines.Add("G91");                     // set rel motion
            this.instructionLines.Add("G1 Z1.000 F200.000");      // move up 1mm and accelerate printhead to 200 mm/min
            this.instructionLines.Add("G90");                     // set absolute positioning
            this.instructionLines.Add("G28 X0.000 Y0.00");        // home XY axes
            this.instructionLines.Add("G1 X117.500 Y125.000 F8000.000");  // move to bed center for Z homing
            this.instructionLines.Add("G28 Z0.000");              // home Z
            this.instructionLines.Add("G92 E0.00000");            // set filament position to zero

            // Machina bolierplate
            this.instructionLines.Add("M82");                     // set extruder to absolute mode (this is actually ZMorph, but useful here
            this.instructionLines.Add(string.Format(CultureInfo.InvariantCulture, 
                "G1 F{0}",
                Math.Round(cursor.speed * 60.0, MMath.STRING_ROUND_DECIMALS_MM)));  // initialize feed speed to the writer's state
        }
        
        /// <summary>
        /// Dumps a bunch of termination boilerplate
        /// </summary>
        /// <param name="cursor"></param>
        internal void EndCodeBoilerplate(RobotCursor cursor)
        {
            // END THE PROGRAM AND LEAVE THE PRINTER READY
            // ZMorph boilerplate
            this.instructionLines.Add("G92 E0.0000");
            this.instructionLines.Add("G91");
            this.instructionLines.Add("G1 E-3.00000 F1800.000");
            this.instructionLines.Add("G90");
            this.instructionLines.Add("G92 E0.00000");
            this.instructionLines.Add("G1 X117.500 Y220.000 Z30.581 F300.000");

            this.instructionLines.Add("T0");         // choose tool 0: is this for multihead? 
            this.instructionLines.Add("M104 S0");    // set extruder temp and move on
            this.instructionLines.Add("T1");         // choose tool 1
            this.instructionLines.Add("M104 S0");    // ibid
            this.instructionLines.Add("M140 S0");    // set bed temp and move on
            this.instructionLines.Add("M106 S0");    // fan speed 0 (off)
            this.instructionLines.Add("M84");        // stop idle hold (?)
            this.instructionLines.Add("M220 S100");  // set speed factor override percentage 
        }

    }
}
