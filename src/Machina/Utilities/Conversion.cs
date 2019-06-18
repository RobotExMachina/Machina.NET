using System;

//  ██╗   ██╗████████╗██╗██╗     ██╗████████╗██╗███████╗███████╗                      
//  ██║   ██║╚══██╔══╝██║██║     ██║╚══██╔══╝██║██╔════╝██╔════╝                      
//  ██║   ██║   ██║   ██║██║     ██║   ██║   ██║█████╗  ███████╗                      
//  ██║   ██║   ██║   ██║██║     ██║   ██║   ██║██╔══╝  ╚════██║                      
//  ╚██████╔╝   ██║   ██║███████╗██║   ██║   ██║███████╗███████║                      
//   ╚═════╝    ╚═╝   ╚═╝╚══════╝╚═╝   ╚═╝   ╚═╝╚══════╝╚══════╝                      
//                                                                                    
//   ██████╗ ██████╗ ███╗   ██╗██╗   ██╗███████╗██████╗ ███████╗██╗ ██████╗ ███╗   ██╗
//  ██╔════╝██╔═══██╗████╗  ██║██║   ██║██╔════╝██╔══██╗██╔════╝██║██╔═══██╗████╗  ██║
//  ██║     ██║   ██║██╔██╗ ██║██║   ██║█████╗  ██████╔╝███████╗██║██║   ██║██╔██╗ ██║
//  ██║     ██║   ██║██║╚██╗██║╚██╗ ██╔╝██╔══╝  ██╔══██╗╚════██║██║██║   ██║██║╚██╗██║
//  ╚██████╗╚██████╔╝██║ ╚████║ ╚████╔╝ ███████╗██║  ██║███████║██║╚██████╔╝██║ ╚████║
//   ╚═════╝ ╚═════╝ ╚═╝  ╚═══╝  ╚═══╝  ╚══════╝╚═╝  ╚═╝╚══════╝╚═╝ ╚═════╝ ╚═╝  ╚═══╝
//                                                                                    


namespace Machina.Utilities
{
    /// <summary>
    /// Utility functions for data conversions.
    /// </summary>
    public static class Conversion
    {
        /// <summary>
        /// Converts an array of signed int32 to a byte array. Useful for buffering. 
        /// </summary>
        /// <param name="intArray"></param>
        /// <param name="littleEndian">Set endianness. Windows systems are little endian, while most network communication is bigendian.</param>
        /// <returns></returns>
        public static byte[] Int32ArrayToByteArray(int[] intArray, bool littleEndian = false)
        {
            byte[] buffer = new byte[4 * intArray.Length];

            // Windows systems are little endian, but the UR takes bigendian... :(
            if (BitConverter.IsLittleEndian == littleEndian)
            {
                Buffer.BlockCopy(intArray, 0, buffer, 0, buffer.Length);
            }

            // If the system stores data differently than requested, must manually reverse each byte!
            else
            {
                byte[] bint;
                for (var i = 0; i < intArray.Length; i++)
                {
                    bint = BitConverter.GetBytes(intArray[i]);
                    Array.Reverse(bint);
                    for (var j = 0; j < 4; j++)
                    {
                        buffer[4 * i + j] = bint[j];
                    }
                }
            }

            return buffer;
        }

        /// <summary>
        /// Converts an array of bytes to an array of signed int32. 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="byteCount">If 0, the whole byte array will be used.</param>
        /// <param name="bytesAreLittleEndian">Is the byte array little endian? This will be used to define how to translate the buffer to this system's endianness.</param>
        /// <returns></returns>
        public static int[] ByteArrayToInt32Array(byte[] bytes, int byteCount = 0, bool bytesAreLittleEndian = false)
        {
            if (byteCount == 0)
            {
                byteCount = bytes.Length;
            }

            //// Sanity -> for the sake of performance, let's trust the user knows what s/he is doing... 
            //if (byteCount % 4 != 0) throw new Exception("byteCount must be multiple of 4");
            //if (byteCount > bytes.Length) throw new Exception("byteCount is larger than array size");
            int[] ints = new int[byteCount / 4];

            // Windows systems are little endian... 
            if (BitConverter.IsLittleEndian == bytesAreLittleEndian)
            {
                Buffer.BlockCopy(bytes, 0, ints, 0, byteCount);
            }

            // ...but network communication is usually bigendian.
            // If the system stores data differently than the array to process, must manually reverse each 4 bytes!
            else
            {
                byte[] clone = new byte[ints.Length * 4];  // don't reverse the order of the passed array
                byte first, second;
                for (int i = 0; i < ints.Length; i++)
                {
                    first = bytes[4 * i];
                    second = bytes[4 * i + 1];

                    clone[4 * i] = bytes[4 * i + 3];
                    clone[4 * i + 1] = bytes[4 * i + 2];
                    clone[4 * i + 2] = second;
                    clone[4 * i + 3] = first;
                }

                Buffer.BlockCopy(clone, 0, ints, 0, byteCount);
            }

            return ints;
        }

        /// <summary>
        /// Takes an array of objects and converts it into an array of nullable doubles.
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static double?[] NullableDoublesFromObjects(object[] objs)
        {
            if (objs == null) return null;

            double?[] d = new double?[objs.Length];

            for (int i = 0; i < d.Length; i++)
            {
                try
                {
                    d[i] = Convert.ToDouble(objs[i]);
                }
                catch
                {
                    d[i] = null;
                }
            }

            return d;
        }


        /// <summary>
        /// Quick conversion for wobjs, but totally dislike this here. Mmmm...
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="vx0"></param>
        /// <param name="vx1"></param>
        /// <param name="vx2"></param>
        /// <param name="vy0"></param>
        /// <param name="vy1"></param>
        /// <param name="vy2"></param>
        /// <returns></returns>
        public static double[] PlaneToABBPose(double x, double y, double z, double vx0, double vx1, double vx2, double vy0, double vy1, double vy2)
        {
            Orientation o = new Orientation(vx0, vx1, vx2, vy0, vy1, vy2);
            Quaternion q = o.Q;

            return new double[]
            {
                x, y, z,
                q.W, q.X, q.Y, q.Z
            };
        }
    }    
}
