```text
██████╗ ██████╗  ██████╗ ██████╗  ██████╗ ████████╗
██╔══██╗██╔══██╗██╔═══██╗██╔══██╗██╔═══██╗╚══██╔══╝
██████╔╝██████╔╝██║   ██║██████╔╝██║   ██║   ██║   
██╔══██╗██╔══██╗██║   ██║██╔══██╗██║   ██║   ██║   
██████╔╝██║  ██║╚██████╔╝██████╔╝╚██████╔╝   ██║   
╚═════╝ ╚═╝  ╚═╝ ╚═════╝ ╚═════╝  ╚═════╝    ╚═╝   
                                                                                                        
████████╗ ██████╗ ██████╗  ██████╗ 
╚══██╔══╝██╔═══██╗██╔══██╗██╔═══██╗
   ██║   ██║   ██║██║  ██║██║   ██║
   ██║   ██║   ██║██║  ██║██║   ██║
   ██║   ╚██████╔╝██████╔╝╚██████╔╝
   ╚═╝    ╚═════╝ ╚═════╝  ╚═════╝ 
```

# PHASE 2

## HIGH-LEVEL
- [ ] RENAME THE PROJECT!
- [ ] Add support for KUKA compilation
- [ ] Add Tools 
- [ ] Add enhanced CS selection and WObj use
- [ ] Improve BRobot for Dynamo, create package, publish
- [ ] Create Grasshopper library


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

