using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotControl
{
    /// <summary>
    /// Represents an abstraction of the state of a robotic device. 
    /// Keeps track of things such as position, orientation, joint configuration,
    /// current speed, velocity, etc.
    /// Useful as virtual representation of a simulated or controller robot actuator. 
    /// </summary>
    abstract class RobotPointer
    {
        // Public props
        public Point position;
        public Rotation rotation;
        public Joints joints;
        public int velocity;
        public int zone;
        public MotionType motionType;
        protected bool initialized = false;

        public abstract bool ApplyAction(ActionTranslation translation);

        /// <summary>
        /// Minimum information necessary to initialize a robot object
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        public bool Initialize(Point pos, Rotation rot)
        {
            position = pos;
            rotation = rot;
            velocity = -1;
            zone = -1;
            motionType = MotionType.Undefined;

            initialized = true;
            return initialized;
        }
    }





    internal class RobotPointerABB : RobotPointer
    {
        
        public RobotPointerABB() { }

        public override bool ApplyAction(ActionTranslation action)
        {
            if (action.relative)
                position.Add(action.translation);
            else
                position = action.translation;

            // If valid inputs, update, otherwise stick with previous values
            if (action.velocity != -1) velocity = action.velocity;
            if (action.zone != -1) zone = action.zone;
            if (action.motionType != MotionType.Undefined) motionType = action.motionType;

            return true;
        }

        public override string ToString()
        {
            return string.Format("pointer: {0} {1},{2},{3}\\{4}", motionType, position, rotation, velocity, zone);
        }

    }
}
