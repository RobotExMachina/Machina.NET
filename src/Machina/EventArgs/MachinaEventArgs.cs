using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    //  ███████╗██╗   ██╗███████╗███╗   ██╗████████╗ █████╗ ██████╗  ██████╗ ███████╗
    //  ██╔════╝██║   ██║██╔════╝████╗  ██║╚══██╔══╝██╔══██╗██╔══██╗██╔════╝ ██╔════╝
    //  █████╗  ██║   ██║█████╗  ██╔██╗ ██║   ██║   ███████║██████╔╝██║  ███╗███████╗
    //  ██╔══╝  ╚██╗ ██╔╝██╔══╝  ██║╚██╗██║   ██║   ██╔══██║██╔══██╗██║   ██║╚════██║
    //  ███████╗ ╚████╔╝ ███████╗██║ ╚████║   ██║   ██║  ██║██║  ██║╚██████╔╝███████║
    //  ╚══════╝  ╚═══╝  ╚══════╝╚═╝  ╚═══╝   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝ ╚══════╝
    //                                                                              
    public abstract class MachinaEventArgs : EventArgs
    {
        /// <summary>
        /// The arguments on this event must be serializable to a JSON object.
        /// </summary>
        /// <returns></returns>
        public abstract string ToJSONString();


        //public string SerializeToJSON()
        //{
        //    MemoryStream ms = new MemoryStream();
        //    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MachinaEventArgs));
        //    ser.WriteObject(ms, this);
        //    byte[] json = ms.ToArray();
        //    ms.Close();
        //    return Encoding.UTF8.GetString(json, 0, json.Length);
        //}

        //public MachinaEventArgs DeserializeFromJSON(string json)
        //{
        //    MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
        //    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MachinaEventArgs));
        //    MachinaEventArgs e = ser.ReadObject(ms) as MachinaEventArgs;
        //    ms.Close();
        //    return e;
        //}

    }
}
