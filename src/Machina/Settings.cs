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
<<<<<<< HEAD
        public double Speed;
        public double Acceleration;
=======
        //public double Speed;
        //public double Acceleration;
        //public double RotationSpeed;
        //public double JointSpeed;
        //public double JointAcceleration;

        public Dictionary<SpeedType, double> SpeedValues;
>>>>>>> 2ce80f32d646ca2ed599525ba68a2bd47278da4e
        public double Precision;
        public MotionType MotionType;
        public ReferenceCS ReferenceCS;
        public double ExtrusionRate;

<<<<<<< HEAD
        public Settings(double speed, double acc, double precision, MotionType mType, ReferenceCS refcs, double extrusionRate)
        {
            this.Speed = speed;
            this.Acceleration = acc;
=======
        public Settings(Dictionary<SpeedType, double> speedValues, double precision, MotionType motionType, ReferenceCS referenceCS, double extrusionRate)
        {
            this.SpeedValues = Util.CopyGenericDictionary<SpeedType, double>(speedValues);
>>>>>>> 2ce80f32d646ca2ed599525ba68a2bd47278da4e
            this.Precision = precision;
            this.MotionType = motionType;
            this.ReferenceCS = referenceCS;
            this.ExtrusionRate = extrusionRate;
        }

<<<<<<< HEAD
        public Settings Clone() => new Settings(Speed, Acceleration, Precision, MotionType, RefCS, ExtrusionRate);

        public override string ToString() => $"{RefCS} {MotionType} {Speed}-{Acceleration}-{Precision} {ExtrusionRate}";
=======
        public Settings Clone() => new Settings(
            Util.CopyGenericDictionary<SpeedType, double>(this.SpeedValues),
            this.Precision,
            this.MotionType,
            this.ReferenceCS,
            this.ExtrusionRate);

        public override string ToString()
        {
            return $"{ReferenceCS} {MotionType} {SpeedValues[SpeedType.Global]} {Precision} {ExtrusionRate}";
        }

        //public Settings(double speed, double acc, double rotationSpeed, double jointSpeed, double jointAcceleration,
        //    double precision, MotionType mType, ReferenceCS refcs, double extrusionRate)
        //{
        //    this.Speed = speed;
        //    this.Acceleration = acc;
        //    this.RotationSpeed = rotationSpeed;
        //    this.JointSpeed = jointSpeed;
        //    this.JointAcceleration = jointAcceleration;
        //    this.Precision = precision;
        //    this.MotionType = mType;
        //    this.RefCS = refcs;
        //    this.ExtrusionRate = extrusionRate;
        //}

        //public Settings Clone() => new Settings(Speed, Acceleration, RotationSpeed, JointSpeed, JointAcceleration, Precision, MotionType, RefCS, ExtrusionRate);

        //public override string ToString() => $"{RefCS} {MotionType} {Acceleration}-{Speed}-{RotationSpeed}-{JointSpeed}-{JointAcceleration} {Precision} {ExtrusionRate}";
>>>>>>> 2ce80f32d646ca2ed599525ba68a2bd47278da4e
    }
}
