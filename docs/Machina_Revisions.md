```text
//  ███╗   ███╗ █████╗  ██████╗██╗  ██╗██╗███╗   ██╗ █████╗ 
//  ████╗ ████║██╔══██╗██╔════╝██║  ██║██║████╗  ██║██╔══██╗
//  ██╔████╔██║███████║██║     ███████║██║██╔██╗ ██║███████║
//  ██║╚██╔╝██║██╔══██║██║     ██╔══██║██║██║╚██╗██║██╔══██║
//  ██║ ╚═╝ ██║██║  ██║╚██████╗██║  ██║██║██║ ╚████║██║  ██║
//  ╚═╝     ╚═╝╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝
//                                                          
//  ██████╗ ███████╗██╗   ██╗██╗      ██████╗  ██████╗      
//  ██╔══██╗██╔════╝██║   ██║██║     ██╔═══██╗██╔════╝      
//  ██║  ██║█████╗  ██║   ██║██║     ██║   ██║██║  ███╗     
//  ██║  ██║██╔══╝  ╚██╗ ██╔╝██║     ██║   ██║██║   ██║     
//  ██████╔╝███████╗ ╚████╔╝ ███████╗╚██████╔╝╚██████╔╝     
//  ╚═════╝ ╚══════╝  ╚═══╝  ╚══════╝ ╚═════╝  ╚═════╝      
//                                                          
```

## TODO
- [ ] TEST THAT THE ROBOT MOVES FINE NOW...!



## BUILD 1305 - 0.4.2
- [x] Change Joints/To to Axes/To
- [x] Split Compliers into indiv cs 
- [x] Add `Temperature()`
- [x] Add `Extrude()`
- [x] Add `ExtrusionRate()`
- [x] Test 'Temperature' etc
- [x] Implement GCode compiler for ZMorph
- [x] Basic 3D Printing example

## BUILD 1304 - 0.4.1
- [x] Add `SetIOName()` to customize IO names
- [x] Rename and migrate `Zone` to `Precision`

## BUILD 1303 - 0.4.0
- [x] FREAKING IOs ;)
- [x] Remove id comments
- [x] Add option to include robot actions as comments
- [x] Fix TPWrite length message on ABB compilers


## BUILD 1302
- [x] Add Tooling for KUKA+UR
- [x] Test that Tool attach/detachment works good


## BUILD 1301
- [ ] Vector.Orthoginalize(...)
- [ ] First stub at Plane object:
    - [ ] Constructors with origin and orientation


## BUILD 1300
- _*BRobot*_ is now called _*Machina*_
- [x] Fix big: TransformTo is not working good, only outputs [0,0,0,1] or [1,0,0,0] quaternions for ABB. There was a typo in RM creation.


---
## BUILD 1215
- _Notes on new Reference system_:
    + Right now `.Coordinates()` accepts a `global` (robot base) or `local` (TCP) setting. This means:
        * On `global`, `.MoveTo()` does absolute XYZ in cartesian space and `.Move()` does relative transformations in that system
        * On `local`, `.Move()` uses the dynamic TCP reference frame and `MoveTo()` still does absolute location based on global coordinates. This is probably not very consistent. 
    + For starters, `global` could be renamed to `base` or `robotBase` and `local` to `tool`. This is probably more understandable. After that, the following will apply:
        * On `base`, `.MoveTo()` does absolute XYZ in cartesian space and `.Move()` does relative transformations in that orientation. ABB compilation would use WObj0
        * On `tool`, `.Move()` uses the TCP reference frame. Because of the dynamic nature of the TCP, `MoveTo()` would effectively have the same effect as `.Move()`. ABB compilation would use WObj0 and frame is computed internally? Use `RelTool` instead?
    + On top of this, this would allow for custom reference systems to be entered.
        * Let's say we could do: 
            ```
            ReferenceSystem bench = new ReferenceSystem("bench", 
                new Point(300, 0, 200), 
                new Orientation(1, 0, 0, 0, 1, 0));
            bot.Coordinates(bench);

            // Alternatively:
            ReferenceSystem gig = ReferenceSystem.FromABBWObj("gig", "...wobj declaration string goes here...");
            bot.Coordinates(gig);


            ```
        * From there, postprocessing/compilation could happen with wobjs or the like. 
        * Cursors will have two additional properties: `Enum CoordinateType {Base, Tool, Custom}`, and `CustomCoodinate {null if Base/Tool, Reference obj if Custom}` 
        * Cursors would have two internal frames: one in the chosen reference system, and another one in robot base coordinates. Both representations would be managed simultaneously.




## BUILD 1214
- [x] Add a bunch of public getters to retrieve the state of the robot (useful for testing)


## BUILD 1213
- [ ] Add Tools
    - [x] Tool class
    - [x] Tool Action
    - [x] Tool API
    - [x] Tool to cursors
    - [x] Tool in CompilerABB
    - [x] TEST
    - [x] Implement .Detach()


## BUILD 1212
- [x] Rework the Rotation interface!
- [x] Massive rework of all classes...


## BUILD 1211
- [x] Quaternion cleanup
- [x] AxisAngle now features a Vector Axis object

## BUILD 1210
- [x] Internal rework of classes and files


## BUILD 1209
- [x] Point is now Vector: 
    - [x] All internal instances of Point have been changed to Vector
    - [x] There is still a new Point class. This is just for the sake of the public API, for the sake of presenting a _perceied difference_ between a location and a direction. This is just cosmetic, everything is Vectors internally...


## BUILD 1208
- [x] Euler Angles --> Rename to YawPitchRoll
    - [x] Create class structure
    - [x] EU > Q > EU
    - [x] EU > RM > EU
    - [x] EU > AA > EU
    - [x] EU > RV > EU
- [x] This was really helpful, nice tool: http://danceswithcode.net/engineeringnotes/rotations_in_3d/demo3D/rotations_in_3d_tool.html


## BUILD 1207
- [x] ROTATIONMATRIX
    - [x] Create 3x3 matrix
    - [x] M33 > Q
    - [x] Q > M33
    - [x] TESTS
    - [x] M33 > Q > M33
    - [x] TESTS
    - [x] M33.Inverse()
    - [x] M33.Transpose()
    - [x] Orthogonality checks on creation of a M33
    - [x] Q > M33 > Q
    - [x] Do two opposed quaternions yield the same RM? 
    - [x] M33 > AA
    - [x] AA > M33
    - [x] TEST THE ABOVE
    - [x] M33 > RV
    - [x] RV > M33
- [x] Acknowledge EuclideanSpace 


## BUILD 1206
- [x] ROTATIONVECTOR
    - [x] Simple conversion from AxisAngle
    - [x] ... a lot of unlogged goodness
    - [x] RV > AA > RV
    - [x] RV > Q > RV
- [x] AXISANGLE 
    - [x] Add AxisAngle.IsEquivalent()
    - [x] Implicit conversion to Point

- [x] Add Point.CompareDirections() for parallelism, orthogonality and oppositeness.
- [x] Update readme with KUKA implementation and development levels.


## BUILD 1205
- [ ] Rework all rotation definitions, add different definition modes, write testing suit, make this good once and for all!
    - [x] QUATERNIONS:
        - [x] Quaternion Implementation
        - [x] Quaternions are always normalized on construction for proper rotation representation
        - [x] Add Quaternion Vector-Normalization: if between {-1, 1}, keep scalar component constant and normalize the rotation axis.
        - [x] Add fallback to regular normalization if Vector-Norm is not possible.
        - [x] Add fallback to convert the Quaternion to identity if trying to normalize a zero-length Q   
        - [x] The above fallback takes into account the sign of the rotation to return positive or negative identity quaternions (not sure why, it just feels like it makes sense...)
        - [x] Fixed a bug that made quaternions flip the axis for negative leading member on Vector-normalization...
        - [x] Quaternion -> AaxisAngle -> Quaternion successful conversion
    - [x] AXISANGLES:
        - [x] AxisAngle implementation
        - [x] Auto-normalization on creation
        - [x] .IsZero on zero axis or zero angle
        - [x] Working simple AxisAngle to Quaternion conversion.
        - [x] Add AA -> Q -> AA tests (account for Q returning always positive rotation)
        - [x] AxisAngle.Flip() + .Modulate()


## BUILD 1204
- [x] Add KUKA KRL. Still a lot of testing to do!


## BUILD 1203
- [x] Add inline generation of poses, instead of splitting them into variables.
- [x] Add 'id' count to Actions
- [x] Reduce numerical precison on string exports, we don't need 15 decimals:
    ```
    movej(p[0.2, 0.319848077530122, 0.401736481776669, 0.137046446582579, 1.56644805234647, 0.137046446582579], a=1, v=0.025, r=0.001)
    ```


## BUILD 1202
- [x] RobotCursor for ABBs and URs is pretty much identical, except for the utility functions, which pretty much relate to compilation anyway. Move this to Compiler and keep one unitary Cursor.
- [x] Add string to Robot constructor to determine robot make.
- [x] Add `.Comment()` to generate inline custom comments
- [x] Add `.Comment()` to the Reference.
- [x] On export, add an additional file with human-readable instructions --> Added `.Robot("HUMAN")`! Unrecognized brand names default to this compiler.
    

## BUILD 1201
- [x] Testing and debugging of UR offline code generation


## BUILD 1200
- [x] Project is now targetting .NET framework 4.6.1 (because of the new ABB.Robotics library...)
- [x] BRobot for Dynamo is now a package
- [x] Renamed Rotation.GetRotationVector() to .GetRotationAxis(), this will only return the unit vector corresponding to the rotation axis.
- [x] Added Rotation.GetRotationVector() to return the axis-angle representation of the Quaternion https://en.wikipedia.org/wiki/Axis%E2%80%93angle_representation
- [x] Add Joint.Scale()
- [x] Fix PushPop actions generating targets on compilation
- [x] Updated old TEST_OfflineAPITests to new abs+rel conventions, specially speeds and zones.









---

# PHASE 1

## PENDING 
- [ ] Rework Actions
    - [x] Make a static API that can generate them as objects
    - [ ] Add the possibility of not inputing speed+zone+mType, and stick to the previous cursor state
    - [ ] Merge R+T & T+R into one. 
    - [x] Do Speed(), Zone() and Motion() become Actions as well? How would this work with push+popSettings?
    - [x] If they were Actions, relative and absolute modes could be implemented: .Speed(10) is an increase, .SpeedTo(10) is a setting.
- [x] Add a CS constructor from a Rotation object
- [ ] R+T vs T+R order in Transform isn't working
- [ ] This example doesn't work!
    ```csharp
        Point dir = new Point(0, 5, 0);

        for (var i = 0; i < 36; i++) {
            bot.Move(dir);
            dir.Rotate(0, 0, 1, -10);  // rotate the vector 10 degs around unit Z vector
            Console.WriteLine("DIR" + dir);
        } 
    ```

## BUILD 1118
- [x] Implement Actions in a Dynamo suite of nodes

## BUILD 1117
- [x] Fixed local orientation problem
- [x] Reworked Settings to become Actions: Speed, Zone, Motion and Coordinates.
- [x] Modify walkthoruhg and API with SpeedTo + ZoneTo 
- [x] Add static Action constructors mirroring the main API
- [x] Bring back Push and PopSettings with some simple workaround

## BUILD 1116
- [x] Get stream mode working again, with all available actions
- [x] Improved subscription and disposal methods, looks like it might now be reliable if the client always properly disconnects...
- [x] Robot.ControlMode() is now .Mode()

## BUILD 1115
- [x] ProgramGenerator class is now Compiler
- [x] Deactivated Queue class
- [x] Deactivated Path class and all related methods 
- [ ] Rebirth of Execute mode
    - [x] motionCursor
    - [x] .Execute() instruction
    - [x] Move ActionBuffer and Compiler now to the RobotCursor class
    - [x] Add some queue manager for actions released from the virtualCursor and awaiting in the writerCursor. In other words, find a way to .Execute() several buffered actions, and have the queue manager wait for the robot to stop running the program before sending a new batch of instructions. --> __Do actionBuffers belong to the virtualCursors??__ --> Added this in a rather weird way, not sure if I should rethink this...   
    - [x] Add comments to all of the above, it is kinda confusing right now...
- [x] Add joint position on Execute cursor initilaization.
- [x] Removed device-specific overloads from primitive DataTypes.

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



