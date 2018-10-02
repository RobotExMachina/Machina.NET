using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina;


namespace TEST_Workbench
{
    class Workbench
    {
        static void Main(string[] args)
        {
            Robot bot = Robot.Create("Test", "ABB");

            bot.DebugMode(true);
            bot.ControlMode(ControlType.Online);

            var parameters = new Dictionary<string, string>()
            {
                {"HOSTNAME", "192.168.125.1"},
                {"PORT", "7000"}
            };
            var files = bot.GetDeviceDriverModules(parameters);

            foreach (var entry in files)
            {
                string filename = entry.Key;
                string content = entry.Value;

                System.IO.File.WriteAllText(filename, content);

            }

            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
            
        }

    }
}
