using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Types.Geometry
{

    // TO REVIEW ITS NEW ROLE IN BROBOT
    // THIS IS ONLY USED IN THE STREAMQUEUE, REVIEW THIS WHEN BACK TO ONLINE MODE...



    ////███████╗██████╗  █████╗ ███╗   ███╗███████╗
    ////██╔════╝██╔══██╗██╔══██╗████╗ ████║██╔════╝
    ////█████╗  ██████╔╝███████║██╔████╔██║█████╗  
    ////██╔══╝  ██╔══██╗██╔══██║██║╚██╔╝██║██╔══╝  
    ////██║     ██║  ██║██║  ██║██║ ╚═╝ ██║███████╗
    ////╚═╝     ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝     ╚═╝╚══════╝                                      
    ///// <summary>
    ///// Represents a location and rotation in 3D space, with some additional
    ///// metadata representing speeds, zones, etc.
    ///// </summary>
    //public class Frame : Geometry
    //{
    //    /// <summary>
    //    /// This is the default rotation that will be assigned to Frames constructed only with location properties.
    //    /// </summary>
    //    public static Rotation DefaultOrientation = Rotation.FlippedAroundY;
    //    public static double DefaultSpeed = 10;
    //    public static double DefaultZone = 5;

    //    public static double DistanceBetween(Frame f1, Frame f2)
    //    {
    //        double dx = f2.Position.X - f1.Position.X;
    //        double dy = f2.Position.Y - f1.Position.Y;
    //        double dz = f2.Position.Z - f1.Position.Z;

    //        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    //    }

    //    public Vector Position;
    //    public Rotation Orientation;
    //    public double Speed;
    //    public double Zone;

    //    public Frame(double x, double y, double z)
    //    {
    //        this.Position = new Vector(x, y, z);
    //        this.Orientation = DefaultOrientation;
    //        this.Speed = DefaultSpeed;
    //        this.Zone = DefaultZone;
    //    }

    //    public Frame(double x, double y, double z, double speed, double zone)
    //    {
    //        this.Position = new Vector(x, y, z);
    //        this.Orientation = DefaultOrientation;
    //        this.Speed = speed;
    //        this.Zone = zone;
    //    }

    //    public Frame(double x, double y, double z, double qw, double qx, double qy, double qz)
    //    {
    //        this.Position = new Vector(x, y, z);
    //        this.Orientation = new Rotation(qw, qx, qy, qz, true);
    //        this.Speed = DefaultSpeed;
    //        this.Zone = DefaultZone;
    //    }

    //    public Frame(double x, double y, double z, double qw, double qx, double qy, double qz, double speed, double zone)
    //    {
    //        this.Position = new Vector(x, y, z);
    //        this.Orientation = new Rotation(qw, qx, qy, qz, true);
    //        this.Speed = speed;
    //        this.Zone = zone;
    //    }

    //    public Frame(Vector position)
    //    {
    //        this.Position = new Vector(position.X, position.Y, position.Z);  // shallow copy
    //        this.Orientation = DefaultOrientation;
    //        this.Speed = DefaultSpeed;
    //        this.Zone = DefaultZone;
    //    }

    //    public Frame(Vector position, double speed, double zone)
    //    {
    //        this.Position = new Vector(position.X, position.Y, position.Z);  // shallow copy
    //        this.Orientation = DefaultOrientation;
    //        this.Speed = speed;
    //        this.Zone = zone;
    //    }

    //    public Frame(Vector position, Rotation orientation)
    //    {
    //        this.Position = new Vector(position.X, position.Y, position.Z);  // shallow copy
    //        this.Orientation = new Rotation(orientation.W, orientation.X, orientation.Y, orientation.Z, true);  // shallow copy
    //        this.Speed = DefaultSpeed;
    //        this.Zone = DefaultZone;
    //    }

    //    public Frame(Vector position, Rotation orientation, double speed, double zone)
    //    {
    //        this.Position = new Vector(position.X, position.Y, position.Z);  // shallow copy
    //        this.Orientation = new Rotation(orientation.W, orientation.X, orientation.Y, orientation.Z, true);  // shallow copy
    //        this.Speed = speed;
    //        this.Zone = zone;
    //    }

    //    public string GetPositionDeclaration()
    //    {
    //        return string.Format("[{0},{1},{2}]", Position.X, Position.Y, Position.Z);
    //    }

    //    public string GetOrientationDeclaration()
    //    {
    //        return string.Format("[{0},{1},{2},{3}]", Orientation.W, Orientation.X, Orientation.Y, Orientation.Z);
    //    }

    //    /// <summary>
    //    /// WARNING: This library still doesn't perform IK calculation, and will always use
    //    /// a default [0,0,0,0] axis configuration.
    //    /// </summary>
    //    /// <returns></returns>
    //    public string GetUNSAFEConfigurationDeclaration()
    //    {
    //        return string.Format("[{0},{1},{2},{3}]", 0, 0, 0, 0);
    //    }

    //    /// <summary>
    //    /// WARNING: no external axes are taken into account here... 
    //    /// </summary>
    //    /// <returns></returns>
    //    public string GetExternalAxesDeclaration()
    //    {
    //        return "[9E9,9E9,9E9,9E9,9E9,9E9]";
    //    }

    //    public string GetSpeedDeclaration()
    //    {
    //        return string.Format("[{0},{1},{2},{3}]", Speed, Speed, 5000, 1000);  // default speed declarations in ABB always use 500 deg/s as rot speed, but it feels too fast (and scary). using the same value as lin motion
    //    }

    //    public string GetZoneDeclaration()
    //    {
    //        double high = 1.5 * Zone;
    //        double low = 0.15 * Zone;
    //        return string.Format("[FALSE,{0},{1},{2},{3},{4},{5}]", Zone, high, high, low, high, low);
    //    }

    //    /// <summary>
    //    /// WARNING: This library still doesn't perform IK calculation, and will always return
    //    /// a default [0,0,0,0] axis configuration.
    //    /// </summary>
    //    /// <returns></returns>
    //    public string GetUNSAFERobTargetDeclaration()
    //    {
    //        return string.Format("[{0},{1},{2},{3}]",
    //            GetPositionDeclaration(),
    //            GetOrientationDeclaration(),
    //            GetUNSAFEConfigurationDeclaration(),
    //            GetExternalAxesDeclaration()
    //        );
    //    }

    //    public void FlipXY()
    //    {
    //        double x = this.Position.X;
    //        this.Position.X = this.Position.Y;
    //        this.Position.Y = x;
    //    }

    //    public void FlipYZ()
    //    {
    //        double y = this.Position.Y;
    //        this.Position.Y = this.Position.Z;
    //        this.Position.Z = y;
    //    }

    //    public void FlipXZ()
    //    {
    //        double x = this.Position.X;
    //        this.Position.X = this.Position.Z;
    //        this.Position.Z = x;
    //    }

    //    public void ReverseX()
    //    {
    //        this.Position.X = -this.Position.X;
    //    }

    //    public void ReverseY()
    //    {
    //        this.Position.Y = -this.Position.Y;
    //    }

    //    public void ReverseZ()
    //    {
    //        this.Position.Z = -this.Position.Z;
    //    }

    //    public bool RemapAxis(string axis, double prevMin, double prevMax, double newMin, double newMax)
    //    {
    //        string a = axis.ToLower();
    //        //Some sanity
    //        if (!a.Equals("x") && !a.Equals("y") && !a.Equals("z"))
    //        {
    //            Console.WriteLine("Please use 'x', 'y' or 'z' as arguments");
    //            return false;
    //        }

    //        int axid = a.Equals("x") ? 0 : a.Equals("y") ? 1 : 2;

    //        switch (axid)
    //        {
    //            case 0:
    //                this.Position.X = Numeric.Remap(this.Position.X, prevMin, prevMax, newMin, newMax);
    //                break;
    //            case 1:
    //                this.Position.Y = Numeric.Remap(this.Position.Y, prevMin, prevMax, newMin, newMax);
    //                break;
    //            default:
    //                this.Position.Z = Numeric.Remap(this.Position.Z, prevMin, prevMax, newMin, newMax);
    //                break;
    //        }

    //        return true;
    //    }


    //    public override string ToString()
    //    {
    //        //return this.Position + "," + this.Orientation;
    //        return string.Format("{0},{1},{2},{3}", this.Position, this.Orientation, this.Speed, this.Zone);
    //    }
    //}
}
