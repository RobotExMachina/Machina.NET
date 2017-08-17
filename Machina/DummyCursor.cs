using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Machina
{
    public class DummyCursor
    {
        public Matrix4x4 Base;
        //public Matrix4x4 CoordinateSystem;
        public Matrix4x4 TCP;

        public DummyCursor()
        {
            Base = Matrix4x4.Identity;
            //CoordinateSystem = new Matrix4x4();
            TCP = Matrix4x4.Identity;
        }

        public Point GetPosition()
        {
            Vector3 v = TCP.Translation;
            return new Point(v.X, v.Y, v.Z);
        }


        public void MoveGlobal(float x, float y, float z)
        {
            Matrix4x4 xform = Matrix4x4.CreateTranslation(x, y, z);

            /* Usually this would be premultiplied, 
             * but since System.Numerics.Matrix4x4 uses row-major representation 
             * (transposed from regular mathematical notation), 
             * multiplication oreder must be inverted. 
             * For instance, a regular 2D translation matrix is
             * | 1  0  5 |
             * | 0  1  9 |
             * | 0  0  1 |
             * 
             * while in Matrix4x4 is represented as:
             * | 1  0  0 |
             * | 0  1  0 |
             * | 5  9  1 |
             * 
             * More info on:
             * http://seanmiddleditch.com/matrices-handedness-pre-and-post-multiplication-row-vs-column-major-and-notations/
             * https://github.com/Microsoft/referencesource/blob/master/System.Numerics/System/Numerics/Matrix4x4.cs
             */

            //TCP = xform * TCP;  // premultiply (column-major)
            TCP = TCP * xform;      // post-multiply (column-major)
        }

        public void MoveGlobal(Vector dir)
        {
            MoveGlobal((float)dir.X, (float)dir.Y, (float)dir.Z);
        }


        public void MoveLocal(float x, float y, float z)
        {
            Matrix4x4 xform = Matrix4x4.CreateTranslation(x, y, z);
            //TCP = TCP * xform;  // postmultiply
            TCP = xform * TCP;  // see above 
        }
        public void MoveLocal(Vector dir)
        {
            MoveLocal((float)dir.X, (float)dir.Y, (float)dir.Z);
        }

        public void RotateGlobal(float x, float y, float z, float angDegs)
        {
            Matrix4x4 xform = Matrix4x4.CreateFromAxisAngle(new Vector3(x, y, z), (float) (angDegs * Geometry.TO_RADS));
            //TCP = xform * TCP;
            TCP = TCP * xform; 
        }

        public void RotateLocal(float x, float y, float z, float angDegs)
        {
            Matrix4x4 xform = Matrix4x4.CreateFromAxisAngle(new Vector3(x, y, z), (float)(angDegs * Geometry.TO_RADS));
            //TCP = TCP * xform;
            TCP = xform * TCP;
        }



        public void ExtractAxes(Matrix4x4 m, out Vector x, out Vector y, out Vector z)
        {
            x = new Vector(m.M11, m.M12, m.M13);
            y = new Vector(m.M21, m.M22, m.M23);
            z = new Vector(m.M31, m.M32, m.M33);
        }



        public override string ToString()
        {
            Vector loc = TCP.Translation;
            Vector x, y, z;
            ExtractAxes(TCP, out x, out y, out z);

            return string.Format("loc: {0} X: {1} Y: {2} Z: {3}",
                loc, x, y, z);
        }



    }
}
