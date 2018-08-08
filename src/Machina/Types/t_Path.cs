using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    // THIS WAS LONG FORGOTTEN, BUT MAY MAKE ITS APPEARANCE BACK SOMETIME... 


    ////██████╗  █████╗ ████████╗██╗  ██╗
    ////██╔══██╗██╔══██╗╚══██╔══╝██║  ██║
    ////██████╔╝███████║   ██║   ███████║
    ////██╔═══╝ ██╔══██║   ██║   ██╔══██║
    ////██║     ██║  ██║   ██║   ██║  ██║
    ////╚═╝     ╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝
    ///// <summary>
    ///// Represents an ordered sequence of target Frames
    ///// </summary>
    //public class Path : Geometry
    //{
    //    public string Name;
    //    private List<Frame> Targets;
    //    public int Count { get; private set; }

    //    public Path() : this("defaultPath") { }

    //    public Path(string name)
    //    {
    //        this.Name = name;
    //        this.Targets = new List<Frame>();
    //        Count = 0;
    //    }

    //    public void Add(Frame target)
    //    {
    //        this.Targets.Add(target);
    //        Count++;
    //    }

    //    public void Add(Vector position)
    //    {
    //        this.Add(new Frame(position));
    //    }

    //    public void Add(double x, double y, double z)
    //    {
    //        this.Add(new Frame(x, y, z));
    //    }

    //    public void Add(Vector position, Rotation orientation)
    //    {
    //        this.Add(new Frame(position, orientation));
    //    }

    //    public Frame GetTarget(int index)
    //    {
    //        return this.Targets[index];
    //    }

    //    public Frame GetFirstTarget()
    //    {
    //        return this.Targets[0];
    //    }

    //    public Frame GetLastTarget()
    //    {
    //        return this.Targets[this.Targets.Count - 1];
    //    }

    //    private void UpadateTargetCount()
    //    {
    //        Count = Targets.Count;
    //    }

    //    /// <summary>
    //    /// Flips the XY coordinates of all target frames.
    //    /// </summary>
    //    public void FlipXY()
    //    {
    //        foreach (Frame f in Targets)
    //        {
    //            //double x = f.Position.X;
    //            //f.Position.X = f.Position.Y;
    //            //f.Position.Y = x;
    //            f.FlipXY();
    //        }
    //    }

    //    /// <summary>
    //    /// Remaps the coordinates of all target frames from a source to a target domain.
    //    /// </summary>
    //    /// <param name="axis"></param>
    //    /// <param name="prevMin"></param>
    //    /// <param name="prevMax"></param>
    //    /// <param name="newMin"></param>
    //    /// <param name="newMax"></param>
    //    /// <returns></returns>
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
    //                foreach (Frame f in Targets)
    //                {
    //                    f.Position.X = Util.Remap(f.Position.X, prevMin, prevMax, newMin, newMax);
    //                }
    //                break;
    //            case 1:
    //                foreach (Frame f in Targets)
    //                {
    //                    f.Position.Y = Util.Remap(f.Position.Y, prevMin, prevMax, newMin, newMax);
    //                }
    //                break;
    //            default:
    //                foreach (Frame f in Targets)
    //                {
    //                    f.Position.Z = Util.Remap(f.Position.Z, prevMin, prevMax, newMin, newMax);
    //                }
    //                break;
    //        }

    //        return true;
    //    }

    //    /// <summary>
    //    /// Simplifies the path using a combination of radial distance and 
    //    /// Ramer–Douglas–Peucker algorithm. 
    //    /// </summary>
    //    /// <ref>Adapted from https://github.com/imshz/simplify-net </ref>
    //    /// <param name="tolerance"></param>
    //    /// <param name="highQuality"></param>
    //    /// <returns></returns>
    //    public bool Simplify(double tolerance, bool highQuality)
    //    {

    //        if (Count < 1)
    //        {
    //            Console.WriteLine("Path contains no targets.");
    //            return false;
    //        }

    //        int prev = Count;

    //        double sqTolerance = tolerance * tolerance;

    //        if (!highQuality)
    //        {
    //            SimplifyRadialDistance(sqTolerance);
    //        }

    //        SimplifyDouglasPeucker(sqTolerance);

    //        Console.WriteLine("Path " + this.Name + " simplified from " + prev + " to " + Count +" targets.");

    //        return true;
    //    }

    //    /// <summary>
    //    /// The RDP algorithm.
    //    /// </summary>
    //    /// <ref>Adapted from https://github.com/imshz/simplify-net </ref>
    //    /// <param name="sqTolerance"></param>
    //    /// <returns></returns>
    //    private void SimplifyDouglasPeucker(double sqTolerance)
    //    {
    //        var len = Count;
    //        var markers = new int?[len];
    //        int? first = 0;
    //        int? last = len - 1;
    //        int? index = 0;
    //        var stack = new List<int?>();
    //        var newTargets = new List<Frame>();

    //        markers[first.Value] = markers[last.Value] = 1;

    //        while (last != null)
    //        {
    //            var maxSqDist = 0.0d;

    //            for (int? i = first + 1; i < last; i++)
    //            {
    //                var sqDist = Vector.SqSegmentDistance(Targets[i.Value].Position,
    //                    Targets[first.Value].Position, Targets[last.Value].Position);

    //                if (sqDist > maxSqDist)
    //                {
    //                    index = i;
    //                    maxSqDist = sqDist;
    //                }
    //            }

    //            if (maxSqDist > sqTolerance)
    //            {
    //                markers[index.Value] = 1;
    //                stack.AddRange(new[] { first, index, index, last });
    //            }

    //            if (stack.Count > 0)
    //            {
    //                last = stack[stack.Count - 1];
    //                stack.RemoveAt(stack.Count - 1);
    //            }
    //            else
    //            {
    //                last = null;
    //            }

    //            if (stack.Count > 0)
    //            {
    //                first = stack[stack.Count - 1];
    //                stack.RemoveAt(stack.Count - 1);
    //            }
    //            else
    //            {
    //                first = null;
    //            }
    //        }


    //        for (int i = 0; i < len; i++)
    //        {
    //            if (markers[i] != null)
    //            {
    //                newTargets.Add(Targets[i]);
    //            }
    //        }

    //        Targets = newTargets;
    //        UpadateTargetCount();
    //    }

    //    /// <summary>
    //    /// Simple distance-based simplification. Consecutive points under 
    //    /// threshold distance are removed. 
    //    /// </summary>
    //    /// <ref>Adapted from https://github.com/imshz/simplify-net </ref>
    //    /// <param name="sqTolerance"></param>
    //    /// <returns></returns>
    //    private void SimplifyRadialDistance(double sqTolerance)
    //    {
    //        Frame prevFrame = Targets[0];
    //        List<Frame> newTargets = new List<Frame> { prevFrame };
    //        Frame frame = null;

    //        for (int i = 1; i < Targets.Count; i++)
    //        {
    //            frame = Targets[i];

    //            if (Vector.SqDistance(frame.Position, prevFrame.Position) > sqTolerance)
    //            {
    //                newTargets.Add(frame);
    //                prevFrame = frame;
    //            }
    //        }

    //        // Add the last frame of the path (?)
    //        if (frame != null && !prevFrame.Position.Equals(frame.Position))
    //        {
    //            newTargets.Add(frame);
    //        }

    //        Targets = newTargets;
    //        UpadateTargetCount();
    //    }
    //}
}
