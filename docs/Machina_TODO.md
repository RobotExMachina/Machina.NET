```text
//  ███╗   ███╗ █████╗  ██████╗██╗  ██╗██╗███╗   ██╗ █████╗ 
//  ████╗ ████║██╔══██╗██╔════╝██║  ██║██║████╗  ██║██╔══██╗
//  ██╔████╔██║███████║██║     ███████║██║██╔██╗ ██║███████║
//  ██║╚██╔╝██║██╔══██║██║     ██╔══██║██║██║╚██╗██║██╔══██║
//  ██║ ╚═╝ ██║██║  ██║╚██████╗██║  ██║██║██║ ╚████║██║  ██║
//  ╚═╝     ╚═╝╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝
//                                                          
//  ████████╗ ██████╗ ██████╗  ██████╗                      
//  ╚══██╔══╝██╔═══██╗██╔══██╗██╔═══██╗                     
//     ██║   ██║   ██║██║  ██║██║   ██║                     
//     ██║   ██║   ██║██║  ██║██║   ██║                     
//     ██║   ╚██████╔╝██████╔╝╚██████╔╝                     
//     ╚═╝    ╚═════╝ ╚═════╝  ╚═════╝                      
//                                                          
```

# PHASE 2

## HIGH-LEVEL
- [x] RENAME THE PROJECT!
- [x] Add support for KUKA + UR compilation
- [x] Add Tools 
- [x] Improve BRobot for Dynamo, create package, publish
- [x] Deactivate action# display, and replace with the human version?
- [x] Rename Zone to Precision
- [x] Redo github banner
- [x] Post links to some Machina videos on YouTube
- [x] Create Grasshopper library

- [ ] Implement 'Temperature'
- [ ] Implement 'FeedRate'
- [ ] Implement 'Extrude'
- [ ] Implement GCode compiler for ZMorph

- [ ] REMOVE THE REGULAR/TO MODEL, and add a ActionMode("absolute"/"relative") to substitute it
- [ ] Rename "MotionType" to "MotionMode"
- [ ] Rename 'Motion' to 'MotionType' here and Dyn (GH is changed)
- [ ] Rename 'Attach' to 'AttachTool', and 'Detach' to 'DetachTools'...?
- [ ] Rename 'PushSettings' to 'SettingsPush' and same for Pop?
- [ ] Print a disclaimer header for exported code

- [ ] Add enhanced CoordSys selection and WObj use

- [ ] Rework the ABB real-time connection

- [ ] Verify program names and IOnames cannot start with a digit
- [ ] Add Acceleration
- [ ] Add Action constructors that take atomic primitives (x,y,z instead of a point object): they shallow copy it anyway... optimization
    - [ ] Use these constructors in Dynamo
    - [ ] Use these constructors in Grasshopper
- [ ] Are the Action... static constructors still necessary?
- [ ] Add 'null' checks for Action lists before compile (Dynamo + GH?)
- [ ] Remove TurnOn/Off

- [ ] Create `Program` as a class that contains a list of actions? It could be interesting as a way to enforce the idea of Programs as a list of Actions, especially in VPL interfaces. Also, it would allow to do things such as adding an `Instruction` (like a function) to the scope of a program, that could be called from the Program itself.




## LOW-LEVEL
- [ ] Rotation problem: the following R construction returns a CS system with the Y inverted!:
    ```csharp
        > Rotation r = new Rotation(-1, 0, 0, 0, 0, -1);
        > r
        [[0,0,0.70710678,0.70710678]]
        > r.GetCoordinateSystem()
        [[[-1,0,0],[0,0,1],[0,1,0]]]  // --> Notice the inverted Y axis!

        // It doesn't happen with this one for example:
        > Rotation r1 = new Rotation(-1, 0, 0, 0, 1, 0);
        > r1
        [[0,0,1,0]]
        > r1.GetCoordinateSystem()
        [[[-1,0,0],[0,1,0],[0,0,-1]]]
    
        // Another way to see this is the following:
        > CoordinateSystem cs = new CoordinateSystem(-1, 0, 0, 0, 0, -1);
        > cs
        [[[-1,0,0],[0,0,-1],[0,-1,0]]]
        > 
        > cs.GetQuaternion().GetCoordinateSystem()
        [[[-1,0,0],[0,0,1],[0,1,0]]]  // It is inverted!

        // However, this works well:
        > Rotation r = new Rotation(-1, 0, 0, 0, 1, -1);
        > r
        [[0,0,0.92387953,0.38268343]]
        > r.GetCoordinateSystem()
        [[[-1,0,0],[0,0.70710678,0.70710678],[0,0.70710678,-0.70710678]]]

        > Rotation r2 = new Rotation(-1, 0, 0, 0, 1, 0);
        > r2.GetCoordinateSystem()
        [[[-1,0,0],[0,1,0],[0,0,-1]]]
        > r2.RotateLocal(new Rotation(1, 0, 0, 45));
        > r2
        [[0,0,0.92387953,-0.38268343]]
        > r2.GetCoordinateSystem()
        [[[-1,0,0],[0,0.70710678,-0.70710678],[0,-0.70710678,-0.70710678]]]
        > r2.RotateLocal(new Rotation(1, 0, 0, 45));
        > r2.GetCoordinateSystem()
        [[[-1,0,0],[0,0,-1],[0,-1,0]]]
        > 
        // Maybe the problem is in the initial Vectors to Quaternion conversion?
        
    ```
    --> check https://github.com/westphae/quaternion/blob/master/quaternion.go ?
    --> Write some unit tests and test the library to figure this out
    --> Is this a problem inherent to Quaternion to Axis-Angle convertion, and the fact that the latter always returns positive rotations? 
- [ ] The dependency tree makes BRobot not work right now if the user doesn't have RobotStudio in the system. And the library is pretty much only used for comm, not used at all for offlien code generation. A way to figure this out, and have the library work in offline mode without the libraries should be implemented. 
- [ ] Coordinates(): this should:
    - [ ] Accept a CS object to use as a new reference frame
    - [ ] Use workobjects when compiled
    - [ ] Perform coordinate/orientation transforms when changing from one CS to the next
- [ ] Add `.Motion("circle")` for `movec` commands?
- [ ] Add `.Acceleration()` and `.AccelerationTo()` for UR robots?
- [ ] Rethink API names to be more 'generic' and less 'ABBish'
- [ ] This happens, should it be fixed...?
    ```csharp
    arm.Rotate(1, 0, 0, 225);  // interesting (and obvious): because internally this only adds a new target, the result is the robot getting there in the shortest way possible (performing a -135deg rotation) rather than the actual 225 rotation over X as would intuitively come from reading he API...
    ```
- [ ] UR simulator is doing weird things with linear vs. joint movements... --> RoboDK doesn't do it, but follows a different path on `movej`...
- [ ] Named CSs could be "world", "base", "flange", and "tool"/"tcp"
- [ ] Add `bot.Units("meters");` to set which units are used from a point on.
- [ ] Compilers are starting to look pretty similar. Abstract them into the superclass, and only override instruction-specific string generation?
- [ ] In KRL, user may export the file with a different name from the module... how do we fix this?
- [ ] Apparently, messages in KRL are kind fo tricky, with several manuals just dedicated to it. Figure this out.

- [ ] ROTATION REWORK:
    - [x] Create individual classes for AxisAngle (main), Quaternion, RotationVector (UR), Matrix33, and EulerZYX
    - [x] Create constructors for each one. 
    - [ ] Create conversions between them:
        * [ ] AxisAngle -> Quaternion
        * [ ] Quaternion -> AxisAngle
    - [ ]   


---

# PHASE 1

## HIGH-LEVEL
- [x] Merged ConnectionMode & OnlineMode into ControlMode
- [x] Restructured library
- [x] Redesigned API
- [x] Abstracted TCPPOsition, TCPRotation and stuff into a VirtualRobot object
- [x] Ported Util methods as static to their appropriate geometry class 
- [x] Rename the project to BRobot ;)
- [x] All the connection properties, runmodes and stuff should belong to the VirtualRobot?
- [ ] Created Debug() & Error() utility functions
- [ ] Detect out of position and joint errors and incorporate a soft-restart.
- [ ] Make changes in ControlMode at runtime possible, i.e. resetting controllers and communication, flushing queues, etc.
- [ ] Streamline 'bookmarked' positions with a dictionary in Control or similar 
- [ ] Implement .PointAt() (as in 'look' at somewhere)
- [ ] Rename Point to Vector (or add an empty subclass of sorts)
- [ ] Split off Quaternion from Rotation? API-wise, make the difference more explicit between a Rotation and a Quaternion...?



## LOW-LEVEL
- [ ] Low-level methods in Communication should not check for !isConnected, but rather just the object they need to perform their function. Only high-level functions should operate based on connection status.
- [ ] Fuse Path and Frame Queue into the same thing --> Rethink the role of the Queue manager
- [ ] Clarify the role of queue manager andits relation to Control and Comm.
- [ ] Unsuscribe from controller events on .Disconnect()



## FUTURE WISHLIST
- [ ] Bring back 'bookmarked' absolute positions.

## SOMETIME...
- [ ] Get my self a nice cold beer and a pat on the back... ;)


## DONE

