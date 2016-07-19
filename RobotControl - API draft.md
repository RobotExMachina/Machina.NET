//   ██████╗ ███████╗███████╗██╗     ██╗███╗   ██╗███████╗
//  ██╔═══██╗██╔════╝██╔════╝██║     ██║████╗  ██║██╔════╝
//  ██║   ██║█████╗  █████╗  ██║     ██║██╔██╗ ██║█████╗  
//  ██║   ██║██╔══╝  ██╔══╝  ██║     ██║██║╚██╗██║██╔══╝  
//  ╚██████╔╝██║     ██║     ███████╗██║██║ ╚████║███████╗
//   ╚═════╝ ╚═╝     ╚═╝     ╚══════╝╚═╝╚═╝  ╚═══╝╚══════╝
//                                                        

---
# OFFLINE

## BASIC SETUP
A basic program with simple instructions.

```csharp
// Setup
Robot arm = new Robot("ABB", "IRB120");         // create a new instance of a robot, pass in brand and model

// Set offline control mode
arm.ControlMode("offline");

// Actions
arm.SetVelocity(200);
arm.SetZone(20);

arm.MoveTo(500, 0, 300);  // a XY square
arm.Move(50, 0, 0);
arm.Move(0, 50, 0);
arm.Move(-50, 0, 0);
arm.Move(0, -50, 0);

// Exports all the actions in the queue to a program file
arm.Export("toolpath.mod");

```

#### NOTES
- In Offline mode, first action should always be absolute, since no robot state can be read from a controller.
- Exporting will flush the buffer at that moment.
- The 'virtual' RobotPointer will always be at the last action, the 'write' one won't exist, and the 'real' one will only move on .Export().
- Paths can also be used and added to the buffer.
```csharp
arm.Follow(circlePath);
```




//  ███████╗██╗  ██╗███████╗ ██████╗██╗   ██╗████████╗███████╗
//  ██╔════╝╚██╗██╔╝██╔════╝██╔════╝██║   ██║╚══██╔══╝██╔════╝
//  █████╗   ╚███╔╝ █████╗  ██║     ██║   ██║   ██║   █████╗  
//  ██╔══╝   ██╔██╗ ██╔══╝  ██║     ██║   ██║   ██║   ██╔══╝  
//  ███████╗██╔╝ ██╗███████╗╚██████╗╚██████╔╝   ██║   ███████╗
//  ╚══════╝╚═╝  ╚═╝╚══════╝ ╚═════╝ ╚═════╝    ╚═╝   ╚══════╝
//                                                            

---
# EXECUTE

## BASIC SETUP
- A basic program with simple instructions.

```csharp
// Setup
Robot arm = new Robot("ABB", "IRB120");         // create a new instance of a robot, pass in brand and model

// Set offline control mode
arm.ControlMode("execute");
arm.Connect();  // .Connect() will try to verify that brand+model are consistent with the connected device, and overrride them if necessary

// Initial settings
arm.SetVelocity(200);
arm.SetZone(20);

// Actions (they get buffered)
arm.MoveTo(500, 0, 300);  // a XY square
arm.Move(50, 0, 0);
arm.Move(0, 50, 0);
arm.Move(-50, 0, 0);
arm.Move(0, -50, 0);

// Executes the buffered actions
arm.Execute();      // flushes the buffer, generates the program, uploads it to the controller and runs it

arm.Disconnect();

```

#### NOTES
- The 'virtual' RobotPointer will be at the last issued action, the 'write' one will be at the last .Execute() issued, and the 'real' one will poll the robot's movements in real-time.


## PATH USE
- Paths can define absolute and relative movements. When relative, they can be reused for a similar motion with different origins.
- This would mean that Frames no longer store absolute coordinates, but contain flags to indicate if position/orientations are relative or absolute...

```csharp
// Setup
Robot arm = new Robot("ABB", "IRB120");         // create a new instance of a robot, pass in brand and model

// Set offline control mode
arm.ControlMode("execute");
arm.Connect();

// Actions (they get buffered)
arm.SetVelocity(200);
arm.SetZone(20);

Path circ = XYCircleFromCenter(100, 12);

// Move somewhere in space
arm.MoveTo(500, 0, 200);

// Do 10 concentric circles
for (int i = 0; i < 10; i++) {
    arm.Follow(circ);
    arm.Move(0, 0, 10);
}

// Executes the buffered actions
arm.Execute();  // flushes the buffer, generates the program, uploads it to the controller and executes it

arm.Disconnect();



// Paths can define abstract actions
public Path XYCircleFromCenter(double r, int def, double vel, double zon) {
    double sideLen = 2 * r * Math.Sin(180.0 / def);  // http://www.mathopenref.com/polygonsides.html

    Path circ = new Path();

    // Settings that will be used for this path
    circ.SetVelocity(vel);
    circ.SetZone(zon);

    // Assuming the TCP is on the center, move to y = r
    circ.Move(0, r, 0);

    // Now describe the circle
    double ang = 0;
    double ancInc = 360 / def;
    for (int i = 0; i < def; i++) {
        ang -= angInc;
        double incX = sideLen * Math.Sin(ang);
        double incY = sideLen * Math.Cos(ang);
        circ.Move(incX, incY, 0);
    }

    // Go back to the center it was on
    circ.Move(0, -r, 0);

    return circ;
} 


```




//  ███████╗████████╗██████╗ ███████╗ █████╗ ███╗   ███╗
//  ██╔════╝╚══██╔══╝██╔══██╗██╔════╝██╔══██╗████╗ ████║
//  ███████╗   ██║   ██████╔╝█████╗  ███████║██╔████╔██║
//  ╚════██║   ██║   ██╔══██╗██╔══╝  ██╔══██║██║╚██╔╝██║
//  ███████║   ██║   ██║  ██║███████╗██║  ██║██║ ╚═╝ ██║
//  ╚══════╝   ╚═╝   ╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝╚═╝     ╚═╝
//                                                      

---
# STREAM

## BASIC SETUP
A basic program with simple instructions.

```csharp
// Setup
Robot arm = new Robot("ABB", "IRB120");         // create a new instance of a robot, pass in brand and model

// Set stream control mode
arm.ControlMode("stream");
arm.Connect();

// Actions
arm.SetVelocity(200);
arm.SetZone(20);

arm.MoveTo(500, 0, 300);  // a XY square
arm.Move(50, 0, 0);
arm.Move(0, 50, 0);
arm.Move(-50, 0, 0);
arm.Move(0, -50, 0);

// Done here, shut down
arm.Disconnect();

```

#### NOTES
- The 'virtual' RobotPointer will be at the last issued action, the 'write' one will be at the last target streamed to the controller, and the 'real' one will poll the robot's movements in real-time.

---
## ADDITIONAL NOTES

- Robot models can be set from a library of makes+models 
```csharp
Robot arm = new Robot(RobotModel.ABB.IRB120);  // overload to read from the models library
```

- Speed & zone settings can be buffered processing-style:
```csharp
// Processing-style settings
arm.PushSettings();  // set a bookmark for settings (vel & zone)
arm.SetVelocity(200);
arm.SetZone(5);

arm.Add(circlePath);

arm.PopSettings();  // revert settings to last push
```

- Start and Stop only affect current online program execution, i.e. they stop current execution, and resume it again. They have nothing to do with starting or stopping streaming any more...
```csharp 
arm.Stop();   // immediate stop of whatever is running 
arm.Start();  // immediate start of whatever was running
```

- Path still works as a valid operaive mode
```csharp
Path path = new Path();
path.MoveTo(...);
path.Move(...);
//... 
arm.Follow(path);  // adds the path to the buffer
```

- Full list of actions:
```csharp
// Linear movements
bot.Move();             // relative movement
bot.MoveTo();           // absolute movement

bot.Rotate();
bot.RotateTo();

bot.Transform();        // movement and rotation combined
bot.TransformTo();      

// Joint movements
bot.JMove();
bot.JMoveTo();

bot.JRotate();
bot.JRotateTo();

bot.JTransform();
bot.JTransformTo();

// Joint manipulation
bot.Joints(incj1..j6);  // issues relative increments of joints
bot.JointsTo(j1..j6);   // issues absolute values for joints

// Full transformation descriptions (all the above transform somehow to these)
bot.FullTransform();
bot.FullJoint();

// IO
bot.IO();
bot.IOToggle();   // whatever it was, !

// Custom ones will come
bot.Pick();
bot.Place();
```

- Overloads
```csharp
arm.Move(double x..z);
arm.Move(double x..z, double vel, double zone);
arm.Move(Point p);
arm.Move(Point p, double vel, double zone);

arm.Rotate(double q1..q4);  // +vel&zone
arm.Rotate(Rotation rot);
arm.Rotate(double xVecX..xVecZ, double yVecX..Z, double zVecX..Z);  // compute rotation based on a 3-vec coordinate system
arm.Rotate(Point vecX, Point vecY, Point vecZ);

arm.Transform(double incX..z, string axisName, double ang); 
arm.Transform(double incX..Z, string axisName1, double ang1, string axisName2, double ang2, string axisName3, double ang3);  // a series of three chained euler rotations

arm.TransformTo(double x..z, double q1..q4);  // +vel&zone
arm.TransformTo(Point p, Rotation rot);

arm.Joints(double incJ1...J6);
arm.JointsTo(double J1...J6);

bot.FullTransform(bool absMove, double x..z, bool absRotat, double q1..q6, bool linMovement);
bot.FullJoint(bool absJoints, params double[] axes);

// IO
bot.IO(int id, bool activate);
bot.IOToggle(int id);   // whatever it was, !
```

