```text
██████╗ ██████╗  ██████╗ ██████╗  ██████╗ ████████╗
██╔══██╗██╔══██╗██╔═══██╗██╔══██╗██╔═══██╗╚══██╔══╝
██████╔╝██████╔╝██║   ██║██████╔╝██║   ██║   ██║   
██╔══██╗██╔══██╗██║   ██║██╔══██╗██║   ██║   ██║   
██████╔╝██║  ██║╚██████╔╝██████╔╝╚██████╔╝   ██║   
╚═════╝ ╚═╝  ╚═╝ ╚═════╝ ╚═════╝  ╚═════╝    ╚═╝   
                                                                                                        
██████╗ ███████╗██╗   ██╗██╗      ██████╗  ██████╗ 
██╔══██╗██╔════╝██║   ██║██║     ██╔═══██╗██╔════╝ 
██║  ██║█████╗  ██║   ██║██║     ██║   ██║██║  ███╗
██║  ██║██╔══╝  ╚██╗ ██╔╝██║     ██║   ██║██║   ██║
██████╔╝███████╗ ╚████╔╝ ███████╗╚██████╔╝╚██████╔╝
╚═════╝ ╚══════╝  ╚═══╝  ╚══════╝ ╚═════╝  ╚═════╝ 
```

## PENDING 
- [ ] Get stream mode working again
- [ ] Get Execute mode working again

## BUILD 1115
- [x] ProgramGenerator class is now Compiler
- [x] Deactivated Queue class
- [x] Deactivated Path class and all related methods 
- [ ] Rebirth of Execute mode
    - [x] motionCursor
    - [x] .Execute() instruction
    - [x] Move ActionBuffer and Compiler now to the RobotCursor class
    - [x] Add some queue manager for actions released from the virtualCursor and awaiting in the writerCursor. In other words, find a way to .Execute() several buffered actions, and have the queue manager wait for the robot to stop running the program before sending a new batch of instructions. --> __Do actionBuffers belong to the virtualCursors??__ --> Added this in a rather weird way, not sure if I should rethink this...   
    - [ ] Add comments to all of the above, it is kinda confusing right now...
- [ ] Add joint position on Execute cursor initilaization.

## BUILD 1114
- [x] Quick Dynamo ZeroTouchNodes test

## BUILD 1113
- [x] Rename the project ;) 

## BUILD 1112
- [x] List<string> .Export()
- [x] API implementations:  
    - [x] .Coordinates()
    - [x] back to .Move() relying on current CS and .MoveTo() as absolute
    - [x] same with .Rotate() and .Transform()
    - [ ] ~~transform J options~~ --> will use .Motion(strType) as a setting instead
- [x] Fixed ProgramGenerator bug running actions twice on writeCursor.
- [x] API consolidation: make a decision about the final syntax of rel/abs/local/world transforms 
- [ ] Write a full 'unit test' program to verify functionality doesn't break

## BUILD 1111
- [x] Add comments to all missing actions
- [x] Rename all syntax instances of 'velocity' (vector) to 'speed' (scalar)
- [x] Refactor .setvel and .setzone to .speed and .zone
- [x] Remove MointType.Joints: there is only one type of ActionJoint, so there is no need to specify this
- [x] Improved ProgramGenerator workflow 
- [ ] ~~Resolve inconsistencies between degree and radian angle representation (make everything radians by standard, with special overloads for degrees?)~~ --> Hybrid: all inputs are degs, all return values are radians... 

## BUILD 1110
- [x] Add .Message(string) action ;) (will be a good test for program generation with non-movement actions)
- [x] Add .Wait(long millis) action
- [x] Rewrite program generation to accept non-movement actions (or even the ones that don't apply at all) --> This could be done more programmatically, but moving on... :)

## BUILD 1109
- [x] Add .Joints() and .JointsTo() actions
- [x] Refine Actions checks on first type issued as absolute... 
- [x] Test Z table limitations with RT + TR transformations

## BUILD 1108
- [x] Port Util methods as static to their appropriate geometry class 
- [x] Rewrite relative .Move actions:
    - [x] Implement .MoveGlobal() --> moves the TCP this increment in World coordinates
    - [x] Implement .MoveLocal() --> moves the TCP this increment in TCP coordinates
- [x] Add Transform() actions: a combination of Move and Rotate at the same time
    - [x] .TransformTo()
    - [x] .TransformLocal()
    - [x] .TransformGlobal()
    - [x] Inverse Rot + Trans action

## BUILD 1107
- [x] Implement bot.PushSettings() & bot.PopSettings();
- [x] Necessary geometry implementations for Rotations:
    - [x] Quaternion unitization
    - [x] Quaternion from vector and angle, including unit vector checks
    - [x] Quaternion multiplication
    - [x] Transformation of a Point by a Quaternion rotation.
    - [x] Get Vector and Angle from a rotation: always return the positive angle solution?
    - [x] CHANGE FUCKING Q1..Q4 CONVENTION FOR ABB ROBOTS... XD
    - [x] CoordinateSystem class, representing a 3x3 rotation matrix
    - [x] A robust Quaternion from vecX, vecY with internal checks
    - [x] And back to CS from Quaternion! ;)
- [x] Add Rotate() actions
    - [ ] RotateTo(q1..q4);                     // hardcode quat rotation --> NO, not intuitive for the user... 
    - [x] RotateTo(vecX, vecY);                 // note this method should take care of normalizing and orthogonizing the vectors, with Z being unnecessary
    - [x] RotateTo(Rotation)
    - [x] RotateTo(CoordinateSystem)
    - [x] RotateGlobal();  // rotates current TCP around world axis
    - [x] RotateLocal();   // rotates current TCP aroung local axis

## BUILD 1106
- [x] Code cleanup and commenting
- [x] Create a basic offline example
- [x] Fix broken examples
- [x] Added placeholder API actions
- [x] Add internal names to VirtualPointers
- [x] Renamed RobotPointer to RobotCursor
- [ ] Restructure crappy barfed code more programmatically --> Wait till a more complete scope arises.

## BUILD 1105
- [x] Develop 'Offline' mode with the new framework 
    + Still shaky and narrow, but the main foundation is there... :)

## BUILD 1104
- [x] Implement RobotPointers class
    - [x] Low-level state description of Virtual, Streaming and Real robots
    - [x] Link the virtual one to API-level instructions

## BUILD 1103
- [ ] Implement API-level instructions
    - [x] .Move()
    - [x] .MoveTo()
    - [ ] .Rotate()
    - [ ] .RotateTo()
    - [ ] .Transform()  // movement and rotation combined
    - [ ] .TRansformTo()
    - [ ] All the above in J mode (joint movement)
    - [ ] .Joints()    // relative joint movement
    - [ ] .JointsTo()  // absolute joint movement
    - [ ] .FullTransform()  // a full constructor with all 
    - [ ] .FullJoint()      // a full constructor

## BUILD 1102
- [x] Create a framework for abstract Actions
    - [x] Create Action class, very low-level: mostly just primitive properties.
    - [x] Design it to be either two types (transform vs joints) or a single one (how?)
    - [x] Add properties for abs/rel Pos, abs/rel Rot, abs/rel Joint, movement type (lin/joint), vel, zone.
    - [x] Create an ActionBuffer class where issued actions get buffered until released somewhere (to a file as a module, to the controller as an uploaded program, to he controller as individual targets).

## BUILD 1101
- [x] New class structure: 
    - [x] Split all the public Robot API from all the internal actual operations
    - [x] Centralize private methods into a Control class
    - [x] Add placeholder classes for future developments (Tool, Library, Solvers, etc.)
    - [x] Create a dedicated Communication class to handle connections to real/virtual controllers
- [x] Port all current functionality into the new structure
- [x] Make sure everything works as before with the new structure
- [x] Add Build number 
- [x] Merged ConnectionMode & OnlineMode into ControlMode 
- [x] Split off the Comm object
- [x] Remove all references to ABB objects from Control class.
- [x] Big comments review
- [x] Deep port of all functionality (make the Robot class basically a very shallow middleware for API purposes)
- [x] Dead code cleanup

## BUILD 1100
- [x] Recheck all examples are working correctly and nothing is broken before branching

## BUILDS 10xx
- Previous test and prototyping builds
- Transitioning to a split class architecture and more programmatic implementation



