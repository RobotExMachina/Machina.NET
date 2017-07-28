using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRobot
{

    



    ///// <summary>
    ///// Represents a Coordinate System composed of a triplet of orthogonal XYZ unit vectors
    ///// following right-hand rule orientations. Useful for spatial and rotational orientation
    ///// operations. 
    ///// </summary>
    //public class CoordinateSystem : Geometry
    //{
    //    public Vector XAxis, YAxis, ZAxis;

    //    /// <summary>
    //    /// Creates a global XYZ reference system.
    //    /// </summary>
    //    public CoordinateSystem()
    //    {
    //        XAxis = new Vector(1, 0, 0);
    //        YAxis = new Vector(0, 1, 0);
    //        ZAxis = new Vector(0, 0, 1);
    //    }

    //    /// <summary>
    //    /// Createa a CoordinateSystem based on the specified guiding Vecots. 
    //    /// Vectors don't need to be normalized or orthogonal, the constructor 
    //    /// will generate the best-fitting CoordinateSystem with this information. 
    //    /// </summary>
    //    /// <param name="vecX"></param>
    //    /// <param name="vecY"></param>
    //    public CoordinateSystem(Vector vecX, Vector vecY)
    //    {
    //        // Some sanity
    //        if (Vector.AreParallel(vecX, vecY))
    //        {
    //            throw new Exception("Cannot create a CoordinateSystem with two parallel vectors");
    //        }

    //        // Create unit X axis
    //        XAxis = new Vector(vecX);
    //        XAxis.Normalize();

    //        // Find normal vector to plane
    //        ZAxis = Vector.CrossProduct(vecX, vecY);
    //        ZAxis.Normalize();

    //        // Y axis is the cross product of both
    //        YAxis = Vector.CrossProduct(ZAxis, XAxis);
    //    }

    //    /// <summary>
    //    /// Create a CoordinateSystem based on the specified guiding Vecots. 
    //    /// Vectors don't need to be normalized or orthogonal, the constructor 
    //    /// will generate the best-fitting CoordinateSystem with this information. 
    //    /// </summary>
    //    /// <param name="x0"></param>
    //    /// <param name="x1"></param>
    //    /// <param name="x2"></param>
    //    /// <param name="y0"></param>
    //    /// <param name="y1"></param>
    //    /// <param name="y2"></param>
    //    public CoordinateSystem(double x0, double x1, double x2, double y0, double y1, double y2) :
    //        this(new Vector(x0, x1, x2), new Vector(y0, y1, y2))
    //    { }

    //    /// <summary>
    //    /// A static constructor that returns a CoordinateSystem from specified vector components. 
    //    /// It will return null if provided components do not form a valid 3x3 rotation matrix.
    //    /// </summary>
    //    /// <param name="x0"></param>
    //    /// <param name="x1"></param>
    //    /// <param name="x2"></param>
    //    /// <param name="y0"></param>
    //    /// <param name="y1"></param>
    //    /// <param name="y2"></param>
    //    /// <param name="z0"></param>
    //    /// <param name="z1"></param>
    //    /// <param name="z2"></param>
    //    /// <returns></returns>
    //    public static CoordinateSystem FromComponents(double x0, double x1, double x2, double y0, double y1, double y2, double z0, double z1, double z2)
    //    {
    //        CoordinateSystem cs = new CoordinateSystem();

    //        cs.XAxis.Set(x0, x1, x2);
    //        cs.YAxis.Set(y0, y1, y2);
    //        cs.ZAxis.Set(z0, z1, z2);

    //        if (cs.IsValid())
    //            return cs;

    //        return null;
    //    }


    //    /// <summary>
    //    /// Returns a Quaternion representation of the current CoordinateSystem. 
    //    /// </summary>
    //    /// <returns></returns>
    //    public Quaternion GetQuaternion()
    //    {
    //        // From the ABB Rapid manual p.1151
    //        double w = 0.5 * Math.Sqrt(1 + XAxis.X + YAxis.Y + ZAxis.Z);

    //        double x = 0.5 * Math.Sqrt(1 + XAxis.X - YAxis.Y - ZAxis.Z)
    //            * (YAxis.Z - ZAxis.Y >= 0 ? 1 : -1);
    //        double y = 0.5 * Math.Sqrt(1 - XAxis.X + YAxis.Y - ZAxis.Z)
    //            * (ZAxis.X - XAxis.Z >= 0 ? 1 : -1);
    //        double z = 0.5 * Math.Sqrt(1 - XAxis.X - YAxis.Y + ZAxis.Z)
    //            * (XAxis.Y - YAxis.X >= 0 ? 1 : -1);

    //        return new Quaternion(w, x, y, z, true);
    //    }

    //    /// <summary>
    //    /// Are the three axes unit vectors and orthonormal?
    //    /// </summary>
    //    /// <returns></returns>
    //    public bool IsValid()
    //    {
    //        bool valid = this.XAxis.IsUnit() && this.YAxis.IsUnit() && this.ZAxis.IsUnit();
    //        //Console.WriteLine("Unit axes: " + valid);
    //        //if (!valid) Console.WriteLine("{0}-{1} {2}-{3} {4}-{5}", this.XAxis, this.XAxis.IsUnit(), this.YAxis, this.YAxis.IsUnit(), this.ZAxis, this.ZAxis.IsUnit());

    //        Vector z = Vector.CrossProduct(this.XAxis, this.YAxis);
    //        valid = valid && this.ZAxis.Equals(z);
    //        //Console.WriteLine("Orthonormal axes: " + valid);
    //        //if (!valid) Console.WriteLine("{0} {1}", this.ZAxis, z);

    //        return valid;
    //    }

    //    public override string ToString()
    //    {
    //        return string.Format("[{0}, {1}, {2}]", XAxis, YAxis, ZAxis);
    //    }

    //}
}
