# BRobot

![](https://github.com/garciadelcastillo/BRobot/blob/master/docs/brobot_banner_888x500.png)

BRobot is a .NET library for action-based real-time control of mechanical actuators. 

BRobot simplifies all the hassle of connecting to a mechanical controller, writing programs in platform-specific languages and managing the asynchronous communication between a machine and a connected application, by providing a [human-relatable API](https://github.com/garciadelcastillo/BRobot/blob/master/docs/Reference.md) of Actions, unified for all the different devices. 

As of v0.1.0, BRobot only works for ABB robotic arms. Other devices are currently under active development. 


## Disclaimer

__Working with robots is dangerous.__ Robotic actuators are very powerful machines, but for the most part extremely unaware of their environment; if it collides with something, including yourself, it will not detect it and try to keep going, posing a threat to itself and the operators surrounding it. This is particularly relevant when running in 'automatic' mode, where several security measures are bypassed for the sake of performance.

When using robots in a real-time interactive environment, plase make sure:
- You have been __adequately trained__ to use that particular machine,
- you are in __good physical and mental condition__,
- you are operating the robot under the __utmost security measures__,
- you are following the facility's and facility staff's __security protocols__,
- and the robot has the __appropriate guarding__ in place, including, but not reduced to, e-stops, physical barriers, light courtains, etc. 

__BRobot is in a very early stage of development.__ You are using this software at your own risk, no warranties are provided herewith, and unexpected results/bugs may arise during its use. Always test and simulate your applications thoroughly before running them in a real device. The author/s shall not be liable for any injuries, damages or losses consequence of using this software in any way whatsoever.


## Hello World

Assuming your computer is [connected to a real or virtual robotic arm](https://github.com/garciadelcastillo/BRobot/blob/master/docs/Setting_up_RobotStudio.md), here is a simple .NET program that traces a vertical 50 x 50 mm square:

```csharp
// Load assembly
using BRobot;

// Instantiate a new Robot object
Robot bot = new Robot();

// Do real-time streaming
bot.Mode("stream");

// Connect to the controller and start running
bot.Connect();
bot.Start();

// Display a message on the handheld pendant
bot.Message("Hello world!");

// Move it to positive XYZ octant, face front and trace the square
bot.Speed(100);
bot.Zone(2);
bot.MoveTo(300, 300, 300);
bot.Rotate(0, 1, 0, -90);
bot.Move(0, 0, 50);
bot.Move(0, 50, 0);
bot.Move(0, 0, -50);
bot.Move(0, -50, 0);

// ... let the robot complete these actions before shutting down

// kthxbye
bot.Disconnect();
```


## Documentation and Reference

BRobot is its infancy, and rapid changes are happening with every commit! Your most reliable and up-to-date source of documentation is probably the code itself, and the comments within. 

To learn how to start using BRobot, and examples of some of its features, please take a look at the [Walkthrough page](https://github.com/garciadelcastillo/BRobot/blob/master/docs/Walkthrough.md). 

Documentation on BRobot's API can be found in the [Reference](https://github.com/garciadelcastillo/BRobot/blob/master/docs/Reference.md).


## Related Projects

BRobot comes with a bunch of sample projects that may help you understand its possibilities. Take a look at them, and please don't blame me if they broke during development... :sweat_smile:


## Current Limitations

Much of the development has focused on making the best of what BRobot can do, rather than fixing what it can't yet. There are several known limitations:
- No forward/inverse kinematics solvers. This means that robotic programs coming out of BRobot include no arm configurations, and it is up to the controller to decide them. It also means that for motion actions, switching from `JointsTo` to any `TransformTo` action and viceversa requires an absolute action first, which can then be followed by any relative actions of the same kind. 
- Closing BRobot applications without properly disposing COM objects causes ABB controllers to silently reject variable subscriptions. Remember to always properly `Disconnect`, and if failed to do so... you may have to restart the machine.
- Many, many other things that you will discover along the way ;)


## Contribute

BRobot is an open-source project, with a lot of room for improvement and collaboration. Furthermore, its internal modular architecture is designed to allow developers to implement their own drivers and compilers in a very straightforward way. If you would like to use BRobot to control some [obscure Basic-based parallel robot](http://www.geva-roboter.at/files/rp_1adh_3adh_5adh_setup_and_maintenance.pdf), ping me and I will explain to you how. A proper collaboration page should be up sometime soon... 


## Acknowledgments

The development of BRobot was sponsored by [Autodesk, Inc](https://www.autodesk.com/), as part of a summer research residency project in Boston, 2016. Please take a look at the [detailed list of acknowledgments](https://github.com/garciadelcastillo/BRobot/blob/master/docs/Acknowledgments.md)

BRobot was created and is maintained by [Jose Luis Garcia del Castillo](http://www.garciadelcastillo.es).


## License

[MIT License](https://github.com/garciadelcastillo/BRobot/blob/master/LICENSE.md)