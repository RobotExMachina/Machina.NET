```text
 █████╗ ██████╗ ██╗    ██████╗ ███████╗███████╗██╗ ██████╗ ███╗   ██╗
██╔══██╗██╔══██╗██║    ██╔══██╗██╔════╝██╔════╝██║██╔════╝ ████╗  ██║
███████║██████╔╝██║    ██║  ██║█████╗  ███████╗██║██║  ███╗██╔██╗ ██║
██╔══██║██╔═══╝ ██║    ██║  ██║██╔══╝  ╚════██║██║██║   ██║██║╚██╗██║
██║  ██║██║     ██║    ██████╔╝███████╗███████║██║╚██████╔╝██║ ╚████║
╚═╝  ╚═╝╚═╝     ╚═╝    ╚═════╝ ╚══════╝╚══════╝╚═╝ ╚═════╝ ╚═╝  ╚═══╝
```
---
### 2016.07.26 - FORMAL API DRAFT

- OFFLINE MODE
```csharp
// Initializtion
BRobot arm = new BRobot("ABB", "IRB120");   // initialize brobots with make and model

// Set control mode
arm.ControlMode("offline");     // offline/execute/stream

// Settings
arm.PushSettings();             // buffers current state for speed, zone, modes, etc. To be reverted by .PopSettings();
arm.Speed(200);                 // no args returns current value
arm.Zone(10);                   // idem

// Basic transformations
arm.MoveTo(300, 0, 500);        // absolute movement is always in world coordinates
arm.Move(100, 0, 0);            // relative movement in global coordinates (default). robot is now at (400, 0, 500)

arm.RotateTo(-1, 0, 0, 0, 1, 0);    // absolute orientation based on X and Y vectors. robot is now with XYZ rotated 180 around Y
arm.Rotate(0, 0, 1, 45);        // relative rotation in global coordinates (default). robot rotates 45 degrees around Z axis

arm.TransformTo(new Point(300, 0, 500), new Rotation(-1, 0, 0, 0, 1, 0));  // absolute trans + rot. order doesn't matter here
arm.Transform(new Point(100, 0, 0), new Rotation(1, 0, 0, 45));             // relative trans + rot in world coords (default). Because using world, order still doesn't matter here

// Joint motion
arm.Motion("joint");            // sets current motion type to 'joints' (as opposed to default linear)
arm.MoveTo(300, 0, 500);        // performs a MoveJ action
arm.Motion("linear");           // back to default

// Local transformations
arm.Coordinates("local");       // for relative actions like .Move(), .Rotate() or .Transform(), sets either local or global/world reference system. This is also buffered by PushSettings.  --> How about .Coordinates("robot/base/TCP")
arm.MoveTo(300, 0, 500);        // absolute transformations are unaffected by .Coordinates()
arm.Move(100, 0, 0);            // relative movement in (now) local TCP coordinates
arm.Rotate(0, 0, 1, 45);        // relative rotation around poitive TCP Z axis
arm.Transform(pos, rot);        // same here
arm.Transform(rot, pos);        // note that for local transformations, the order of actions matters ;)

// Joint rotations
arm.JointsTo(0, 0, 0, 0, 90, 0);    // sets absolute joint positions. uses settings speed and zone params
arm.Joints(45, -15, 0, 0, 0, 0);    // adds these values to current joint rotations.  

// Settings
arm.PushSettings();             // reverts settings back to speed, zone, etc state before .Push()

// Other stuff
arm.MoveTo(200, 0, 200);
arm.Message("Waiting...");      // send a message to the device to be displayed. E.g., for ABBs this will show up on the FlexPendant
arm.Wait(2500);                 // wait 2.5 secs before performing next action
arm.Move(100, 0, 0);            // relative movement in (default) global coordinates
```

- EXECUTE MODE
```csharp
// Initializtion
BRobot arm = new BRobot("ABB", "IRB120");   // initialize brobots with make and model

// Set control mode
arm.ControlMode("execute");     // offline/execute/stream

// Connect to a controller
arm.Connect();

// Do some stuff
arm.MoveTo(300, 0, 500);
arm.Move(50, 0);
arm.Move(0, 50);
arm.Move(-50, 0);
arm.Move(0, -50);
arm.Move(0, 0, 50);
arm.JointsTo(0, 0, 0, 0, 90, 0);

arm.Execute();  // flushes all the instructions and sends all pending actions to the controller to be run

// Anytime the program is running, it can be paused with 
arm.Stop();

// And resumed with 
arm.Start();

// And sent to the beginning with
arm.Rewind();

// And toggle looping execution:
arm.Loop();
arm.NoLoop();  // default (is this part of settings?)

// Events can be attached:
arm.Stop += OnStopHandler;

arm.Disconnect();
```

- STREAM MODE
```csharp
// Initializtion
BRobot arm = new BRobot("ABB", "IRB120");   // initialize brobots with make and model

// Set control mode
arm.ControlMode("stream");     // offline/execute/stream

// Connect to a controller
arm.Connect();

// Start rolling!
arm.Start();

// Do some stuff
arm.MoveTo(300, 0, 500);
arm.Move(50, 0);
arm.Move(0, 50);
arm.Move(-50, 0);
arm.Move(0, -50);
arm.Move(0, 0, 50);
arm.JointsTo(0, 0, 0, 0, 90, 0);

// Anytime the program is running, it can be paused with 
arm.Stop();

// kthxbye 
arm.Disconnect();

// The stream queue has an event when the controller requests a new target
arm.NextAction += OnNextAction;

private void OnNextAction(object sender, NextActionEventArgs e) {
    // Infinitely move forward
    arm.PushSettings();
    arm.Coordinates("local");
    arm.Move(10, 0, 0);
    arm.PopSettings();
}

```





- Relative actions have an overload to set their behavior:
```csharp
arm.Move("local");          // sets option for future move actions. It is also the overload for bookmarked positions?
arm.Move(x, y, z);

arm.Move("world");
arm.Move(x, y, z);

arm.MoveTo(x, y, z);        // this is independent from relative settings

arm.Transform("local");     // sets both transform params to local. doesn't affect the independents settings for move or rotate 

```











---
### 2016.07.25
- Added Local and Global overloads for movement/rotation:
```csharp
arm.MoveLocal(x, y, z);     // relative movement in local axes  
arm.MoveGlobal(x, y, z);    // relative movement in global axes
arm.MoveTo(x, y, z);        // absolute movement in global coordinates

arm.RotateLocal(rot);       // relative rotation in local axes  
arm.RotateGlobal(rot);      // relative rotation in global axes
arm.RotateTo(rot);          // absolute rotation in global coordinates
```

- Could this be wrapped into a default simpler instruction, and specific additions:
```csharp
arm.Move(x, y, z);  // this defaults to what?

arm.Move.Local(x, y, z);
arm.Move.Global(x, y, z);
arm.Move.To(x, y, z);
```

- What if movement/rotation types were a state?
```csharp
arm.MovementType("local");
arm.Move(...);                  // relative local movement
arm.MovementType("global");
arm.Move(...);                  // relative global movement
arm.MovementType("absolute");
arm.Move(...);                  // absolute global movement

arm.RotationType("local");
arm.RotationType("global");
arm.RotationType("absolute");
```

- Can both be combined? Aka, the base .Move() instruction follows current state, but specific instructions temporarily bypass it:
```csharp
arm.Movement("local");          // sets current state
arm.Move(...);                  // relative local movement
arm.MoveAbsolute(...);          // performs absolute global mov, keeps state the same
arm.Move(...);                  // still performs relative local movement
arm.Movement("absolute");       // sets state
arm.Move(...);                  // .Move() now behaves absolute 
```

- State-based movement would make .Transform() very straightforward to implement:
```csharp
arm.Movement("local");
arm.Rotation("absolute");
arm.Transform(vec, rot);        // perform local movement, then absolute rotation. Note the param order defines operation order

arm.Movement("absolute");
arm.Rotation("global");
arm.Transform(rot, vec);        // perform global rotation, then absolute movement
```

- Local/Global/Absolute sounds a little confusing. I wonder if the 'relative global' setting is marginal: in most cases, performing a relative operation in global CS is strange. Maybe would just be simpler to keep it pure local (everything relative to local CS) and pure global (everything absolute to global CS...).
```csharp
arm.Movement("local");
arm.Rotation("local");
// Arc-like movement
for (int i = 0; i < 10; i++) {
    arm.Transform(new Rotation(ZAxis, 5), new Vector(10, 0, 0));
}

arm.Movement("absolute");
arm.Rotation("absolute");
// Go back home
arm.Transform(new Vector(300, 0, 500), new CoordinateSystem(YAxis, -XAxis));
```

- Split settings between "absolute/relative" and "global/local"...?

- How does this affect Joints movement? If this was rephrased 'relative/absolute', then it would fit perfectly:
```csharp
arm.Joints("relative");
arm.Joints(0, 0, 0, 30, -45, 10);   // relative incrase

arm.Joints("absolute");  
arm.Joints(0, 0, 0, 0, 0, 0);       // go home

```

- Doesn't 'relative/absolute' at the end of the day read the same as .Move() and .MoveTo(), .Rotate() and .RotateTo(), .Joints() and .JointsTo()...? If we eliminate the relative+global state, we can go back to not using states.
- If so, overloads and flags for .Transform() would be needed?
```csharp
// Using flags:
arm.Transform(vec, bool relativeM, rot, bool relativeR);  // and viceversa with order?

// Keeping the 'To' convention:
arm.Transform(vec, rot);        // relative movements
arm.Transform(rot, vec);        
arm.TransformTo(vec, rot);      // absolute transform (order here doesn't matter)
```

- Hybrid model:
```csharp
// arm.RelativeMovement("local");   // default is global
arm.Move(...);      // issues a relative movement. By default it is in world coordinates
arm.MoveTo(...);    // issues an absolute movement. It is always in world coordinates, no settings for this...

// arm.RelativeRotation("world");
arm.Rotate(...);    // issues a relative rotation. By default it is in local coordinates
arm.RotateTo(...);  // issues an absolute roation. Always world rotation, no settings

arm.Joints(...);    // relative joints, no option for local or global
arm.JointsTo(...);  // absolute joint setting

arm.Transform(...);     // mov + rot in whatever current settings. This might be confusing if one is world and the other is local...
arm.TransformTo(...);   // abs move + rot, no confusion here
```

- Relative actions have an overload to set their behavior:
```csharp
arm.Move("local");          // sets option for future move actions. It is also the overload for bookmarked positions?
arm.Move(x, y, z);

arm.Move("world");
arm.Move(x, y, z);

arm.MoveTo(x, y, z);        // this is independent from relative settings

arm.Transform("local");     // sets both transform params to local. doesn't affect the independents settings for move or rotate 

```



---
```text
 ██████╗ ███████╗███████╗██╗     ██╗███╗   ██╗███████╗
██╔═══██╗██╔════╝██╔════╝██║     ██║████╗  ██║██╔════╝
██║   ██║█████╗  █████╗  ██║     ██║██╔██╗ ██║█████╗  
██║   ██║██╔══╝  ██╔══╝  ██║     ██║██║╚██╗██║██╔══╝  
╚██████╔╝██║     ██║     ███████╗██║██║ ╚████║███████╗
 ╚═════╝ ╚═╝     ╚═╝     ╚══════╝╚═╝╚═╝  ╚═══╝╚══════╝
```

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


---
```text
███████╗██╗  ██╗███████╗ ██████╗██╗   ██╗████████╗███████╗
██╔════╝╚██╗██╔╝██╔════╝██╔════╝██║   ██║╚══██╔══╝██╔════╝
█████╗   ╚███╔╝ █████╗  ██║     ██║   ██║   ██║   █████╗  
██╔══╝   ██╔██╗ ██╔══╝  ██║     ██║   ██║   ██║   ██╔══╝  
███████╗██╔╝ ██╗███████╗╚██████╗╚██████╔╝   ██║   ███████╗
╚══════╝╚═╝  ╚═╝╚══════╝ ╚═════╝ ╚═════╝    ╚═╝   ╚══════╝
```

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


---
```text
███████╗████████╗██████╗ ███████╗ █████╗ ███╗   ███╗
██╔════╝╚══██╔══╝██╔══██╗██╔════╝██╔══██╗████╗ ████║
███████╗   ██║   ██████╔╝█████╗  ███████║██╔████╔██║
╚════██║   ██║   ██╔══██╗██╔══╝  ██╔══██║██║╚██╔╝██║
███████║   ██║   ██║  ██║███████╗██║  ██║██║ ╚═╝ ██║
╚══════╝   ╚═╝   ╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝╚═╝     ╚═╝
```

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

- Can implement time:
```csharp
arm.Wait(2000);  // in ms
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

/*
--> JOINT MOVEMENT BECOMES A SETTING! --> arm.SetMotionType("joint"); arm.SetMotionType("linear");
// Joint movements
bot.JMove();
bot.JMoveTo();

bot.JRotate();
bot.JRotateTo();

bot.JTransform();
bot.JTransformTo();
*/

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





