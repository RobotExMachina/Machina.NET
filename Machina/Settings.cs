using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    /// <summary>
    /// A class representing a Setting state, to be un/buffered.
    /// </summary>
    class Settings
    {
        public double Acceleration;
        public double Speed;
        public double RotationSpeed;
        public double Precision;
        public MotionType MotionType;
        public ReferenceCS RefCS;
        public double ExtrusionRate;


        public Settings(double acc, double speed, double rotationSpeed, double precision, MotionType mType, ReferenceCS refcs, double extrusionRate)
        {
            this.Acceleration = acc;
            this.Speed = speed;
            this.RotationSpeed = rotationSpeed;
            this.Precision = precision;
            this.MotionType = mType;
            this.RefCS = refcs;
            this.ExtrusionRate = extrusionRate;
        }

        public Settings Clone() => new Settings(Acceleration, Speed, RotationSpeed, Precision, MotionType, RefCS, ExtrusionRate);

        public override string ToString() => $"{RefCS} {MotionType} {Acceleration}-{Speed}-{RotationSpeed} {Precision} {ExtrusionRate}";
    }
}
