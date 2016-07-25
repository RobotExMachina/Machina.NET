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

        // Abstract methods
        public abstract bool ApplyAction(ActionTranslation action);
        public abstract bool ApplyAction(ActionRotation action);
        public abstract bool ApplyAction(ActionTranslationAndRotation action);
        public abstract bool ApplyAction(ActionRotationAndTranslation action);


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
            // @TOPAN: is there a better way to choose the subclass?
            //return ApplyAction((action.GetType())action);

            // This will need a lot of maintenance, add new cases for each actiontype. 
            // Better way to do it?
            switch (action.type)
            {
                case ActionType.Translation:
                    return ApplyAction((ActionTranslation)action);

                case ActionType.Rotation:
                    return ApplyAction((ActionRotation)action);

                case ActionType.TranslationAndRotation:
                    return ApplyAction((ActionTranslationAndRotation)action);

                case ActionType.RotationAndTranslation:
                    return ApplyAction((ActionRotationAndTranslation)action);

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
            Point newPosition = new Point();

            if (action.relativeTranslation)
            {
                if (action.worldTranslation)
                {
                    newPosition = position + action.translation;
                }
                else
                {
                    Point worldVector = Point.Rotation(action.translation, Rotation.Conjugate(this.rotation));
                    newPosition = position + worldVector;
                }
            }
            else
            {
                newPosition.Set(action.translation);
            }

            // @TODO: this must be more programmatically implemented 
            if (Control.SAFETY_CHECK_TABLE_COLLISION)
            {
                if (Control.IsBelowTable(newPosition.Z))
                {
                    if (Control.SAFETY_STOP_ON_TABLE_COLLISION)
                    {
                        Console.WriteLine("Cannot perform action: too close to base XY plane --> TCP.z = {0}", newPosition.Z);
                        return false;
                    }
                    else
                    {
                        Console.WriteLine("WARNING: too close to base XY plane, USE CAUTION! --> TCP.z = {0}", newPosition.Z);
                    }
                }
            }

            position = newPosition;

            // If valid inputs, update, otherwise stick with previous values
            if (action.velocity != -1) velocity = action.velocity;
            if (action.zone != -1) zone = action.zone;
            if (action.motionType != MotionType.Undefined) motionType = action.motionType;

            return true;
        }

        //public override bool RevertAction(ActionTranslation action)
        //{

        //}


        public override bool ApplyAction(ActionRotation action)
        {
            // @TODO: implement some kind of security check here...
            if (action.relativeRotation)
            {
                if (action.worldRotation)
                {
                    rotation.PreMultiply(action.rotation);
                }
                else
                {
                    rotation.Multiply(action.rotation);
                }
            }
            else
            {
                rotation.Set(action.rotation);
            }

            // If valid inputs, update, otherwise stick with previous values
            if (action.velocity != -1) velocity = action.velocity;
            if (action.zone != -1) zone = action.zone;
            if (action.motionType != MotionType.Undefined) motionType = action.motionType;

            return true;
        }


        public override bool ApplyAction(ActionTranslationAndRotation action)
        {
            Point newPos = new Point();

            if (action.relativeTranslation)
            {
                if (action.worldTranslation)
                {
                    newPos = position + action.translation;
                }
                else
                {
                    Point worldVector = Point.Rotation(action.translation, Rotation.Conjugate(this.rotation));
                    newPos = position + worldVector;
                }
            }
            else
            {
                newPos.Set(action.translation);
            }

            // @TODO: this must be more programmatically implemented 
            if (Control.SAFETY_CHECK_TABLE_COLLISION)
            {
                if (Control.IsBelowTable(newPos.Z))
                {
                    if (Control.SAFETY_STOP_ON_TABLE_COLLISION)
                    {
                        Console.WriteLine("Cannot perform action: too close to base XY plane --> TCP.z = {0}", newPos.Z);
                        return false;
                    }
                    else
                    {
                        Console.WriteLine("WARNING: too close to base XY plane, USE CAUTION! --> TCP.z = {0}", newPos.Z);
                    }
                }
            }

            position = newPos;

            // @TODO: implement some kind of security check here...
            if (action.relativeRotation)
            {
                if (action.worldRotation)
                {
                    rotation.PreMultiply(action.rotation);
                }
                else
                {
                    rotation.Multiply(action.rotation);
                }
            }
            else
            {
                rotation.Set(action.rotation);
            }

            // If valid inputs, update, otherwise stick with previous values
            if (action.velocity != -1) velocity = action.velocity;
            if (action.zone != -1) zone = action.zone;
            if (action.motionType != MotionType.Undefined) motionType = action.motionType;

            return true;
        }


        public override bool ApplyAction(ActionRotationAndTranslation action)
        {
            // @TODO: implement some kind of security check here...
            Rotation newRot = new Rotation();
            if (action.relativeRotation)
            {
                if (action.worldRotation)
                {
                    //rotation.PreMultiply(action.rotation);
                    newRot = Rotation.Multiply(action.rotation, rotation);
                }
                else
                {
                    //rotation.Multiply(action.rotation);
                    newRot = Rotation.Multiply(rotation, action.rotation);
                }
            }
            else
            {
                newRot.Set(action.rotation);
            }

            Point newPos = new Point();
            if (action.relativeTranslation)
            {
                if (action.worldTranslation)
                {
                    newPos = position + action.translation;
                }
                else
                {
                    Point worldVector = Point.Rotation(action.translation, Rotation.Conjugate(newRot));
                    newPos = position + worldVector;
                }
            }
            else
            {
                newPos.Set(action.translation);
            }

            // @TODO: this must be more programmatically implemented 
            if (Control.SAFETY_CHECK_TABLE_COLLISION)
            {
                if (Control.IsBelowTable(newPos.Z))
                {
                    if (Control.SAFETY_STOP_ON_TABLE_COLLISION)
                    {
                        Console.WriteLine("Cannot perform action: too close to base XY plane --> TCP.z = {0}", newPos.Z);
                        return false;
                    }
                    else
                    {
                        Console.WriteLine("WARNING: too close to base XY plane, USE CAUTION! --> TCP.z = {0}", newPos.Z);
                    }
                }
            }

            rotation = newRot;
            position = newPos;

            // If valid inputs, update, otherwise stick with previous values
            if (action.velocity != -1) velocity = action.velocity;
            if (action.zone != -1) zone = action.zone;
            if (action.motionType != MotionType.Undefined) motionType = action.motionType;

            return true;
        }







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
