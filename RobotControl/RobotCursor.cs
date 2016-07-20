using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotControl
{
    //   ██████╗██╗   ██╗██████╗ ███████╗ ██████╗ ██████╗ 
    //  ██╔════╝██║   ██║██╔══██╗██╔════╝██╔═══██╗██╔══██╗
    //  ██║     ██║   ██║██████╔╝███████╗██║   ██║██████╔╝
    //  ██║     ██║   ██║██╔══██╗╚════██║██║   ██║██╔══██╗
    //  ╚██████╗╚██████╔╝██║  ██║███████║╚██████╔╝██║  ██║
    //   ╚═════╝ ╚═════╝ ╚═╝  ╚═╝╚══════╝ ╚═════╝ ╚═╝  ╚═╝
    //                                                    
    /// <summary>
    /// Represents an abstraction of the state of a robotic device. 
    /// Keeps track of things such as position, orientation, joint configuration,
    /// current speed, velocity, etc.
    /// Useful as virtual representation of a simulated or controller robot actuator. 
    /// </summary>
    abstract class RobotCursor
    {
        // Public props
        public string name;
        public Point position;
        public Rotation rotation;
        public Joints joints;
        public int velocity;
        public int zone;
        public MotionType motionType;
        protected bool initialized = false;

        public RobotCursor(string name)
        {
            this.name = name;
        }

        public abstract bool ApplyAction(ActionTranslation translation);

        /// <summary>
        /// Minimum information necessary to initialize a robot object.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        public bool Initialize(Point pos, Rotation rot)
        {
            position = new Point(pos);
            rotation = new Rotation(rot);
            velocity = -1;
            zone = -1;
            motionType = MotionType.Undefined;

            initialized = true;
            return initialized;
        }

        /// <summary>
        /// Applies the directives of an Action to this Cursor.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(Action action)
        {
            // @TODO: ask @PAN
            //return ApplyAction((action.GetType())action);

            // This will need a lot of maintenance, add new cases for each actiontype. 
            // Better way to do it?
            switch (action.type)
            {
                case ActionType.Translation: return ApplyAction((ActionTranslation)action);
            }
            return false;
        } 

    }




    //   █████╗ ██████╗ ██████╗ 
    //  ██╔══██╗██╔══██╗██╔══██╗
    //  ███████║██████╔╝██████╔╝
    //  ██╔══██║██╔══██╗██╔══██╗
    //  ██║  ██║██████╔╝██████╔╝
    //  ╚═╝  ╚═╝╚═════╝ ╚═════╝ 
    //                          
    internal class RobotCursorABB : RobotCursor
    {
        
        public RobotCursorABB(string name) : base( name) { }

        public override bool ApplyAction(ActionTranslation action)
        {
            // @TODO: this must be more programmatically implemented 
            if (Control.SAFETY_CHECK_TABLE_COLLISION)
            {
                double newZ = action.relative ? position.Z + action.translation.Z : action.translation.Z;
                if (Control.IsBelowTable(newZ))
                {
                    if (Control.SAFETY_STOP_ON_TABLE_COLLISION)
                    {
                        Console.WriteLine("Cannot perform action: too close to base XY plane --> TCP.z = {0}", newZ);
                        return false;
                    }
                    else
                    {
                        Console.WriteLine("WARNING: too close to base XY plane, USE CAUTION! --> TCP.z = {0}", newZ);
                    }
                }
            }

            if (action.relative)
                position.Add(action.translation);
            else
                position.Set(action.translation);

            // If valid inputs, update, otherwise stick with previous values
            if (action.velocity != -1) velocity = action.velocity;
            if (action.zone != -1) zone = action.zone;
            if (action.motionType != MotionType.Undefined) motionType = action.motionType;

            return true;
        }

        //public override bool RevertAction(ActionTranslation action)
        //{

        //}

        /// <summary>
        /// Returns an ABB robtarget representation of the current state of the cursor.
        /// WARNING: this method is EXTREMELY UNSAFE; it performs no IK calculations, assigns default [0,0,0,0] 
        /// robot configuration and assumes the robot controller will figure out the correct one.
        /// </summary>
        /// <returns></returns>
        public string GetUNSAFERobTargetDeclaration()
        {
            return string.Format("[{0},{1},[0,0,0,0],[9E9,9E9,9E9,9E9,9E9,9E9]]",
                position,
                rotation
            );
        }


        public override string ToString()
        {
            return string.Format("{5}: {0} {1} {2} v{3} z{4}", motionType, position, rotation, velocity, zone, name);
        }

    }
}
