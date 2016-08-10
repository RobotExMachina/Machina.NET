## QUICK API REFERENCE - v0.1.0

### Constructors
- [Robot](#robot)

### Properties
- [Version](#version)

### Methods
- [Mode](#mode)  
&nbsp;
- [Connect](#connect)
- [Disconnect](#disconnect)  
&nbsp;
- [Start](#start)
- [Stop](#stop)  
&nbsp;
- [Export](#export)
- [Execute](#execute)  
&nbsp;
- [Motion](#motion)
- [Coordinates](#coordinates)
- [Speed](#speed)
- [Zone](#zone)
- [PushSettings](#pushsettings)
- [PopSettings](#popsettings)  
&nbsp;
- [Move](#move)
- [MoveTo](#moveto)
- [Rotate](#rotate)
- [RotateTo](#rotateto)
- [Transform](#transform)
- [TransformTo](#transformto)
- [Joints](#joints)
- [JointsTo](#jointsto)
- [Wait](#wait)
- [Message](#message)


### Events
- [BufferEmpty](#bufferempty)


--
## Constructors

### Robot

_Robot()_

Creates a new instance of a Robot object.

```csharp
Robot bot = new Robot();
```


--
## Properties

## Version

_static string_

Represents current version number.

```csharp
Console.WriteLine(Robot.Version);  // "0.1.0"
```


--
## Methods

### Mode

_bool Mode(ControlMode mode)_

_bool Mode(string mode)_

Sets the control mode the robot will operate under: __"offline"__ (default), __"execute"__ or __"stream."__

```csharp
Robot arm = new Robot();

// Sets the robot instance to work in 'stream' mode
bot.Mode("stream");  
```


--
### Connect

_bool Connect()_

Scans the network for robotic devices, real or virtual, and performs all necessary operations to connect to it. Necessary for 'online' modes such as 'execute' and 'stream.'

```csharp
Robot bot = new Robot();
bot.Mode("stream");

// Connects to a real or virtual controller
bot.Connect();  
```


### Disconnect

_bool Disconnect()_

Performs all necessary operations to safely disconnect from connected devices. Necessary for 'online' modes such as 'execute' and 'stream.' 

```csharp
Robot bot = new Robot();
bot.Mode("stream");
bot.Connect();

// ... do awesome stuff

// Disconnect from the controller before closing the client
bot.Disconnect();  
```

_NOTE: it is **extremely important to disconnect from devices** upon client closure to ensure proper disposal of all COM objects. Failure to do so may prevent the client from further successful connections and the need to restart the controller._


--
### Start

_bool Start()_

Runs the program loaded in the device or resumes a [stopped](#stop) execution.

```csharp
Robot bot = new Robot();
bot.Mode("stream");
bot.Connect();

// Start running the streaming program and listen for actions
bot.Start();
```

_NOTE: works only in 'execute' and 'stream' modes._


### Stop

_bool Stop()_

Stops the currently running program. Program execution can be resumed with [Start()](#start)

```csharp
Robot bot = new Robot();
bot.Mode("stream");
bot.Connect();
bot.Start();

// ... do stuff

// Stop program execution
bot.Stop();

// ... do more stuff in between

bot.Start();  // resume execution
```

_NOTE: works only in 'execute' and 'stream' modes._


--
### Export
_List<string> Export()_
_bool Export(string filepath)_

Converts all pending actions into device-specific code, and returns it as a string representation or saves it to a text file. All actions issued to the Robot instance so far will be flushed from the buffer.

```csharp
Robot bot = new Robot();
bot.Mode("offline");

// ... issue a bunch of actions

// Create a program file with all issued actions so far (and flush them from the buffer)
bot.Export(@"C:\roboticProgram.mod");
```

```csharp
Robot bot = new Robot();
bot.Mode("offline");

// ... issue a bunch of actions

List<string> code = bot.Export();
```

_NOTE: works only in 'offline' mode._


### Execute

_void Execute()_

Converts all pending actions into device-specific code, uploads it to the connected controller and starts execution of this program. All actions issued to the Robot instance so far will be flushed from the buffer. 

```csharp
Robot bot = new Robot();
bot.Mode("execute");

// ... issue a bunch of actions

// Upload all pending actions as a program to the controller and run it!
bot.Execute();
```

_NOTE: works only in 'execute' mode._


--
### Motion 

_MotionType Motion()_

_void Motion(MotionType type)_

_void Motion(string type)_

Gets or sets the current type of motion to be applied to future translation actions. This can be __"linear"__ (default) for straight line movements in euclidean space, or __"joint"__ for linear transitions between joint angles (linear movement in robot configuration space).

```csharp
// Change to "joint" movement
bot.Motion("joint");
bot.Move(100, 0, 0);  // will perform a relative joint movement

// Revert back to "linear" movement
bot.Motion("linear");
bot.Move(-100, 0, 0);  // will perform a relative linear movement
```

_NOTE: "joint" movement is easier and gentler to the mechanics of a robotic arm, but trajectories are often less intuitive, resulting in unexpected interferences and collisions. Use extreme caution when executing "joint" translations._


### Coordinates

_ReferenceCS Coordinates()_

_void Coordinates(ReferenceCS refcs)_

_void Coordinates(string type)_

Gets or sets the coordinate system that will be used for future relative actions. This can be __"global"__ or __"world"__ (default) to refer to the system's global reference coordinates, or __"local"__ to refer to the device's local reference frame. For example, for a robotic arm, the "global" coordinate system will be the robot's base, and the "local" one will be the coordinates of the end effector, after all translation and rotation transformations. 

```csharp
bot.Move(100, 0);      // moves 100 mm in global (default) X direction
bot.Rotate(1, 0, 0, 45);  // rotates 45 degs around global X axis

// Sets relative actions to use the device's local reference frame
bot.Coordinates("local");
bot.Move(100, 0, 0);      // move 100 mm in local X direction
bot.Rotate(1, 0, 0, 45);  // rotates 45 degs around local X axis
```


### Speed

_int Speed()_

_void Speed(int speed)_

Gets or sets the speed in mm/s at which future transformation actions will run. Default value is 20.

```csharp
bot.Speed(100);
bot.Move(100, 0);  // will move at 100 mm/s

bot.Speed(25);
bot.Move(100, 0);  // will move at 25 mm/s
```


### Zone

_int Zone()_

_void Zone(int zone)_

Gets or sets the zone radius in mm at which the device will start transitioning to its next target transformation. You can think of this as a 'proximity precision' parameter to blend movement along consecutive waypoints. Default value is 5 mm.

```csharp
// Set a 10 mm proximity radius to targets
bot.Zone(10);

// The corners of this square movement will look 'rounded' with a 10 mm radius
bot.Move(50, 0);  
bot.Move(0, 50);
bot.Move(-50, 0);
bot.Move(0, -50);
```


### PushSettings

_void PushSettings()_

Stores current state settings to a buffer, so that temporary changes can be made, and settings can be reverted to the stored state later with [PopSettings()](#popsettings). 

```csharp
bot.PushSettings();  // store current settings
bot.Speed(200);
bot.Zone(10);
bot.Move(100, 0);    // will move at 200 mm/s with 10 mm zone
bot.PopSettings();   // revert to previously stored settings

bot.Move(100, 0);    // will move at the speed and zone settings before .PushSettings()
```

_NOTE: State settings include [motion type](#motion), [reference coordinate system](#coordinates), [speed](#speed) and [zone](#zone)._


### PopSettings

_void PopSettings()_

Reverts current settings to the state store by the last call to [PushSettings()](#pushsettings).  

```csharp
bot.PushSettings();  // store current settings
bot.Speed(200);
bot.Zone(10);
bot.Move(100, 0);    // will move at 200 mm/s with 10 mm zone
bot.PopSettings();   // revert to previously stored settings

bot.Move(100, 0);    // will move at the speed and zone settings before .PushSettings()
```

_NOTE: State settings include [motion type](#motion), [reference coordinate system](#coordinates), [speed](#speed) and [zone](#zone)._

--
### Move

_bool Move(Point direction)_

_bool Move(double incX, double incY)_

_bool Move(double incX, double incY, double incZ)_

Moves the device along a speficied vector relative to its current position. 

```csharp
bot.MoveTo(300, 0, 500);  // device moves to global coordinates [300, 0, 500]
bot.Move(0, 0, 100);      // device is now at [300, 0, 600]
```

_NOTE: the direction of this relative movement is defined by the current [reference coordinate system](#coordinates)._


### MoveTo

_bool MoveTo(Point position)_

_bool MoveTo(double x, double y, double z)_

Moves the device to an absolute position in global coordinates. 

```csharp
bot.MoveTo(300, 0, 500);  // device moves to global coordinates [300, 0, 500]
bot.Move(0, 0, 100);      // device is now at [300, 0, 600]
```


### Rotate

_bool Rotate(Rotation rotation)_

_bool Rotate(Point vector, double angDegs)_

_bool Rotate(double rotVecX, double rotVecY, double rotVecZ, double angDegs)_

Rotates the device a specified angle in degrees along the specified vector.

```csharp
bot.Rotate(0, 0, 1, 45);  // rotates the device 45 degrees around Z axis  
```

_NOTE: the orientation of this relative rotation is defined by the current [reference coordinate system](#coordinates)._


### RotateTo

_bool RotateTo(Rotation rotation)_

_bool RotateTo(CoordinateSystem cs)_

_bool RotateTo(Point vecX, Point vecY)_

_bool RotateTo(double x0, double x1, double x2, double y0, double y1, double y2)_

Rotate the devices to an absolute orientation defined by the two main X and Y axes.

```csharp
bot.RotateTo(-1, 0, 0, 0, 1, 0);  // rotates to a coordinate system with flipped Z axis
```


### Transform

_bool Transform(Point position, Rotation rotation)_

_bool Transform(Rotation rotation, Point position)_

Performs a compound relative rotation + translation transformation in a single action. Note that when performing relative transformations, the R+T versus T+R order matters. The overloads are designed to take this order into account. 

```csharp
// Move the device 100 mm in X and then rotate 90 degs around Z axis
bot.Transform(new Point(100, 0, 0), new Rotation(0, 0, 1, 90));
```

_NOTE: the direction and orientation of this relative transformation is defined by the current [reference coordinate system](#coordinates)._


### TransformTo

_bool TransformTo(Point position, Rotation rotation)_

_bool TransformTo(Rotation rotation, Point position)_

Performs a compound absolute rotation + translation transformation, or in other words, sets both a new absolute position and orientation for the device in the same action.

```csharp
// The device moves to [300, 0, 500] and is rotated 180 degs in the Y axis from the global reference CS
bot.TransformTo(new Point(300, 0, 500), new Rotation(0, 1, 0, 180)); 
```

_NOTE: R+T order isn't relevant for absolute transformations, the overloads are here just for [orthogonality](https://en.wikipedia.org/wiki/Orthogonal_instruction_set)._


### Joints

_bool Joints(Joints incJoints)_

_bool Joints(double incJ1, double incJ2, double incJ3, double incJ4, double incJ5, double incJ6)_

Increase the rotation angle in degrees of the joints in mechanical devices, specially robotic arms. 

```csharp
bot.Joints(-15, 0, 0, 0, 0, 0);  // rotate joint 1 (base) -15 degs
bot.Joints(0, 0, 0, 0, 0, 180);  // rotate joint 6 (end effector) 180 degs
```


### JointsTo

_bool JointsTo(Joints joints)_

_bool JointsTo(double j1, double j2, double j3, double j4, double j5, double j6)_

Sets the rotation angle in degrees of the joints in mechanical devices, specially robotic arms. 

```csharp
bot.JointsTo(0, 0, 0, 0, 0, 0);   // sets the robot to move to home position
bot.JointsTo(0, 0, 0, 0, 90, 0);  // set joint 5 to rotate 90 degs
```


### Wait

_bool Wait(long timeMillis)_

Pause program execution for specified milliseconds. 

```csharp
bot.Move(100, 0);
bot.Wait(1500);    // wait 1.5 s before executing next action
bot.Move(0, 100);
```


### Message

_bool Message(string message)_

Displays a text message on the device. This will depend on the device's configuration. For example, for ABB robots it will display it on the FlexPendant's log window.

```csharp
bot.Move(100, 0);
bot.Message("Moving up!");
bot.Move(0, 0, 100);
```


--
## Events

### BufferEmpty

_event BufferEmptyHandler BufferEmpty_

This event rises whenever the client has released all pending actions to the controller, and there are no more instructions to be issued. Think of this as an event that signals a 'demand' to issue new orders to the connected device. This is useful when developing time-sensitive apps whose actions depend on the state of the environment, like motion controllers, interactive installations and so on...

```csharp
// Subscribe to BufferEmptyEvents 
bot.BufferEmpty += new BufferEmptyHandler(IssueNewMovement);

void IssueNewMovement(object sender, EventArgs args) {
    // Handle the event by moving to the most current target
    bot.MoveTo(currentTarget);
}
```

_NOTE: works only in 'stream' mode._




