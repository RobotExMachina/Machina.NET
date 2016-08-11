# BRobot Walkthrough

This section will introduce you to the basics of using BRobot to control mechanical actuators, and walk you through different examples that cover most of the aspects that make this library easy, powerful and fun to work with. 


## Disclaimer

__Working with robots is dangerous.__ Robotic actuators are very powerful machines, but for the most part extremely unaware of their environment; if it collides with something, including yourself, it will not detect it and try to keep going, posing a threat to itself and the operators surrounding it. This is particularly relevant when running in 'automatic' mode, where several security measures are bypassed for the sake of performance.

When using robots in a real-time interactive environment, plase make sure:
- You have been __adequately trained__ to use that particular machine,
- you are in __good physical and mental condition__,
- you are operating the robot under the __utmost security measures__,
- you are following the facility's and facility staff's __security protocols__,
- and the robot has the __appropriate guarding__ in place, including, but not reduced to, e-stops, physical barriers, light courtains, etc. 

__BRobot is in a very early stage of development.__ You are using this software at your own risk, no warranties are provided herewith, and unexpected results/bugs may arise during its use. Always test and simulate your applications thoroughly before running them in a real device. The author/s shall not be liable for any injuries, damages or losses consequence of using this software in any way.


## Introduction

BRobot is a .NET library for real-time manipulation and control of mechanical actuators. It exposes a human-relatable high-level API of abstract actions, which are managed and streamed to connected devices in real time. BRobot is particularly suited for building interactive applications that make the most of sensing and acting on the environment through physical devices.

As of version 0.1.0, BRobot can only interface with ABB robotic arms, and this will be the main focus of the examples provided in this walkthrough. More devices will hopefully come soon, such as Kuka robots, Universal Robots, Arduinos, 3D printers, drones, etc... If you are interested in helping with this, please check the Contribute section.


## Setup

Setting a BRobot project up with extremely easy:

- First, clone this repo to your local machine and open it in Visual Studio. The project comes with the core library, as well as many app examples. 

- If you have not installed RobotStudio in your machine, make sure you do so by [reading this guide](https://github.com/garciadelcastillo/BRobot/blob/master/docs/Setting_up_RobotStudio.md). As specified in the guide, create a station with your choice of robot manipulator, and make sure it is started and running in auto mode. 

- To make sure everything is working, run the 'EXAMPLE_ConnectionCheck' project. If you see a long debug dump with a lot of information from this station, it means connection is live and we can start working!


## Hello World!

At this point, you should have the BRobot project correctly installed in your platform and connected to a virtual ABB robot. The fun is about to start!

Let's run a .NET interactive shell (REPL) with the referenced BRobot assembly. In Visual Studio, go to the Solution Explorer, right click on the BRobot project and choose "Initialize Interactive with Project:"

![](https://github.com/garciadelcastillo/BRobot/blob/master/docs/VS_REPL_01.png)

A C# interactive window should pop up looking like this:

![](https://github.com/garciadelcastillo/BRobot/blob/master/docs/VS_REPL_02.png)

You can think of this as a command line window that has BRobot loaded in it, and through which you can start taking to the robot. 

First thing we need to do is to instantiate a new Robot object to start working with. In the C# interactive, type this:

```csharp
Robot bot = new Robot();
```

`bot` is our new Robot object, which we will use to interface with the virtual device and through which we will issue all instructions. 

There is some setup we need to do before moving on: specify which interaction mode we will be using ([more details later](#modes)), connecting to the controller and starting it. We will start working in [stream](#stream) mode. In the C# interactive, type:

```csharp
bot.Mode("stream");
```

Now, connect to the controller by typing:

```csharp
bot.Connect();
```

If everything went well, you should see a bunch of logs describing the state of the controller. To start running stream mode, type:

```csharp
bot.Start();
```

A log message saying `EXECUTION STATUS CHANGED: Running` is the sign that this worked. Also, if you go to the RAPID tab in RobotStudio, you will observe that the Output window now displays 'Program started', and that there is a big stop button now on the top ribbon bar. The robot is now connected in [stream](#stream) mode, and listening to your instructions! Well done! :)

At this point, we can start issuing actions to the virtual robot, and they will be executed as soon as they get priority. Since the robot is idle right now, anything we send will be executed immediately. Let's move the robot somewhere in the positive XYZ octant. The measures I will be using in this example are suited for a small IRB120, please scale them accordingly if you are simulating a bigger device:

```csharp
bot.MoveTo(300, 100, 200);
```

`MoveTo` is an movement action in absolute global coordinates. You should see the virtual robot start moving slowly to this location in global coordinates. By default, the robot moves at 20 mm/s. We can set the speed for new instructions a bit higher:

```csharp
bot.Speed(100);
```

And issue a new instruction so that the robot moves 200 mm in the positive Z direction:

```csharp
bot.Move(0, 0, 200);
```

`Move` is a relative translation action based on the current location of the robot. You will notice that actions are not immediate, but they happen as soon as all the previously issued ones are complete. The robot was slowly moving to [300, 100, 200], and it wasn't until it reached this point that it increased speed and started moving up. This is what we call _priority-based instruction_. 

Finally, let's move the robot back to a 'home' position by directly setting the rotation angles of the joints. Since this is not a linear movement, we want to be cautious and slow the robot down a little:

```csharp
bot.Speed(50);
bot.Joints(0, 0, 0, 0, 90, 0);
```

As soon as these actions get priority, the robot will start moving back to its initial rest position. 

Before we close the C# interactive window, it is very important to properly disconnect from the controller. Otherwise, memory and subscriptions may not be properly handled, and it may impede our ability to successfuly connect later on:

```csharp
bot.Disconnect();
```

Now you are good to close the C# REPL and RobotStudio if you want ;)


## A simple .NET app

The example above is taking advantage of the C# interactive window capabilities to perform live control of a robotic device. However, the exact same functionality can be achieved inside any .NET app, including console applications, WPF or any other platform that supports .NET (Dynamo, Grasshopper, Unity, etc.). 

For example, the above code can be wrapped in a simple console application. If you create a new console application project in Visual Studio, and add BRobot as a reference, you can get the same result with this code:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BRobot;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            // Instantiate a new Robot object
            Robot bot = new Robot();

            // Set it to work in live streaming mode
            bot.Mode("stream");

            // Connect to the (virtual) controller and start listening to actions
            bot.Connect();
            bot.Start();

            // The robot is listening now! We can start issuing actions!
            // Let's display a message on the FlexPendant
            bot.Message("Let's start rocking!");

            // Now let's move it somewhere in absolute world coordinates
            bot.MoveTo(300, 100, 200);

            // Let's set a higher speed for the next movement
            bot.Speed(100);

            // Let's issue a new order to move 200 mm in the Z direction
            bot.Move(0, 0, 200);

            // And finally, let's bring it back to a 'home' position
            bot.JointsTo(0, 0, 0, 0, 90, 0);

            // Let's halt the program here and let the robot work
            // before exiting the program
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();

            // Remember to disconnect from the controller before leaving!
            bot.Disconnect();
        }
    }
}

```


## The Action-State Model

Before we move on, it is probably helpful at this point to discuss the way BRobot is built and works. If you don't care much about abstract jibber jabber, just skip to the last paragraph of this section ;)

BRobot is built following what I would like to describe as an _**action-state model**_. In a few words, the way a user interfaces with physical devices through BRobot is by _issuing actions_ that change the _state_ of different _internal representations_, and which get _released_ and executed whenever they get _priority_. Let's illustrate this with a simple example. 

Imagine you open your email client and you have 50 unread emails. You will most likely start replying one at a time, starting from the oldest (unless you realize there are certain urgent ones). As you respond, your mental state changes, and you may actually give different responses to newer emails based on your reaction to previous ones. Furthermore, as you go through your long queue of messages, new ones keep arriving to your inbox in response to your own responses, or completely unrelated. You probably decide to keep responding in order, and deal with them when you reach the top of your list. In a way, this is similar to what is happening inside BRobot. 

In Brobot, an _**action**_ is any kind of instruction we wish the device to follow, such as moving somewhere, rotating, waiting or displaying a message. We _**issue**_ actions by submitting a request to the device to follow this action, a request which may or may not be accepted for several reasons. If the action is accepted, it gets stored in an internal _**action queue**_. We do expect such action to be executed as soon as all the previously pending actions have been completed, unless we have otherwise specified. BRobot takes care of maganing this queue, and _**releasing**_ its actions to the different connected devices, real or virtual, whenever _**priority**_ dictates. 

The effect of actions on the device is strongly determined by its current _**state**_ at that moment, especially for _**relative**_ actions. For example, if we issue a `Move(100, 0, 0)` action, the device will move 100 mm in the X direction from wherever it was before, while if we tell it to `MoveTo(300, 100, 200)` it doesn't matter where it were before, it will just move to that absolute position. However, both movements will be affected by the current state of speed value, zone, type of motion, etc.  

Due to the inherent asynchronous nature of communicating with physically actuated devices, BRobot manages a series of internal _**state**_ representations, to properly keep track of such states. For example, there is a _**virtual cursor**_ which represents the state of a virtual device immediately after valid actions are issued, a _**write cursor**_ which keeps track of the actions that have been released to a connected device, and a _**motion cursor**_ which tries to keep up with the actual state of the connected device at any given time. BRobot uses these representations to make sure it handles the action queue as accurately as possible, and gives you proper notice of when things are happening so that you can react to them. 

BRobot's action-state model is strongly inspired by the work of [Seymour Papert](https://en.wikipedia.org/wiki/Seymour_Papert) et al. with the [LOGO language](https://en.wikipedia.org/wiki/Logo_(programming_language)) and its application in [turtle graphics and robotics for children](https://en.wikipedia.org/wiki/Turtle_(robot)), with further inspiration drawn from the syntactic sleekness of the [Processing language](https://processing.org/) and the motivation behind [SAM patterns](http://sam.js.org/).

So in a nutshell, the takeaway of the model BRobot offers you is that you can request the device to execute actions at any time, such as moving, rotating or stopping, with the effect of those actions depending on the state of the device whenever those actions happen, such as its location, speed, motion type, etc. Furthermore, those actions won't happen immediately, but will be stored in a queue and released to the device when they get priority, one at a time. The way these actions are released to the device depends on your choice of operating [Modes](#modes) available, which are explained in the next section.


## Modes

BRobot offers three main modes of control: [stream](#stream), [execute](#execute) and [offline](#offline). 


### Stream

__Stream__ mode is as close as you can get to pure real-time interaction with a connected device. In stream mode, any action you issue will be immediately sent to the queue, and released to the device for execution as soon as the device is ready to receive it. 

A basic stream example would look like this:

```csharp
// Initialize
Robot bot = new Robot();

// Set stream mode, connect and start
bot.Mode("stream");
bot.Connect();
bot.Start();

// Do stuff
bot.MoveTo(500, 500, 500);  // this action is released to the device immediately
bot.Move(0, -100, 0);       // this action will be executed immediately after the previous one is done
bot.Move(0, 0, -100);       // this one is also executed right after the previous one is over

// ... allow some time in your app for the above to execute ...

// When done, remember to always disconnect
bot.Disconnect();           // will stop the device and disconnect
```

Stream mode is useful for highly responsive applications such as motion tracking, interactive installation, communication with other devices... you name it. 

Note that due to the nature of network communications and live motion control, and depending on the range of your demands, the device may not be able to properly keep up with the app's demands, or conversely execute your actions faster that new ones can be supplied. This can result in undesired laggy responsiveness and/or abrupt motion. Sometimes the situation can be improved by tweaking motion parameters such as speed, zone, distance between targets, or subscribing to certain events. However, and depending on the nature of your application, you may want to consider using [execute mode](#mode).


### Execute

__Execute__ mode is a nice compromise between responsiveness and performance. In execute mode, actions get buffered as soon as the are issued, but are not released until the application explicitly requests so. In that moment BRobot compiles a program with such instructions in the device's native language, uploads it to the controller and runs it. This process is usually costly and takes some time to process, but once the program is up and running, you are ensured its execution at the machine's full capacity and smoothness. 

In this mode, use the `Execute` method to release all pending actions as a program to the controller. Note that consecutive calls to the `Execute` method will queue blocks of pending actions, which will be released as full programs whenever the previous one ends:

```csharp
// Setup
Robot bot = new Robot();
bot.Mode("execute");
bot.Connect();

// Draw a horizontal XY square
bot.Speed(100);
bot.Zone(2);
bot.MoveTo(300, 300, 100);
bot.Move(50, 0);
bot.Move(0, 50);
bot.Move(-50, 0);
bot.Move(0, -50);

// Release all actions issued so far as a program to the controller
bot.Execute();  // note this will clear all the above actions from the 'pending' buffer

// Issue new actions: a vertical XZ square
bot.Move(50, 0, 0);
bot.Move(0, 0, 50);
bot.Move(-50, 0, 0);
bot.Move(0, 0, -50);

// This call will queue the above four actions as a block
// and release them as a program whenever the previous one ends
bot.Execute();

// ... allow some time in your app for the above to execute ....

bot.Disconnect();  // remember to disconnect before leaving
```

Execute mode is useful for application where responsiveness is not a top priority, but smooth and reliable motion are desired, such as drawing applications, path planning, etc. It is also very convenient for target-intensive applications where the device must follow a great number of waypoints. 


### Offline

You may not always have a device available with you, or may simply just want to generate programs in the native language the device runs. __Offline__ mode is designed to give you access to non-real-time robotics under the same API, and be able to export programs that can be manually loaded to devices. 

Offline mode works very similar to [execute mode](#execute), releasing all pending actions into a full program whenever requested. However, instead of uploading the program to the controller, use `Export` to write it directly to a file on your system. Similar to the example before:

```csharp
// Setup
Robot bot = new Robot();
bot.Mode("offline");

// Draw a flat XY square
bot.Speed(100);
bot.Zone(2);
bot.MoveTo(300, 300, 100);
bot.Move(50, 0);
bot.Move(0, 50);
bot.Move(-50, 0);
bot.Move(0, -50);

// Release all actions issued so far as a program to a file
bot.Export(@"C:\XY_square.mod");  // note this will clear all the above actions from the 'pending' buffer

// Issue new actions, a XZ square
bot.Move(50, 0, 0);
bot.Move(0, 0, 50);
bot.Move(-50, 0, 0);
bot.Move(0, 0, -50);

// Release the newly issued actions as a program
bot.Export(@"C:\XZ_square.mod");
```

For an ABB robot, the program in the `XY_square.mod` file will look like this:

```text
MODULE BRobotProgram

  CONST speeddata vel100:=[100,100,5000,1000];

  CONST zonedata zone2:=[FALSE,2,3,3,0.3,3,0.3];

  CONST robtarget target0:=[[300,300,100],[0,0,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]];
  CONST robtarget target1:=[[350,300,100],[0,0,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]];
  CONST robtarget target2:=[[350,350,100],[0,0,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]];
  CONST robtarget target3:=[[300,350,100],[0,0,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]];
  CONST robtarget target4:=[[300,300,100],[0,0,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]];

  PROC main()
    ConfJ \Off;
    ConfL \Off;

    MoveL target0,vel100,zone2,Tool0\WObj:=WObj0;
    MoveL target1,vel100,zone2,Tool0\WObj:=WObj0;
    MoveL target2,vel100,zone2,Tool0\WObj:=WObj0;
    MoveL target3,vel100,zone2,Tool0\WObj:=WObj0;
    MoveL target4,vel100,zone2,Tool0\WObj:=WObj0;

  ENDPROC

ENDMODULE
```

Offline mode is useful for testing purposes, to get acquaintanced with your device's native language, and to generate full programs in case you don't have access to real-time communication with your machine. For safety reasons, it is also the default control mode.


## Examples





