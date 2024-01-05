``` text
//  ███╗   ███╗ █████╗  ██████╗██╗  ██╗██╗███╗   ██╗ █████╗ 
//  ████╗ ████║██╔══██╗██╔════╝██║  ██║██║████╗  ██║██╔══██╗
//  ██╔████╔██║███████║██║     ███████║██║██╔██╗ ██║███████║
//  ██║╚██╔╝██║██╔══██║██║     ██╔══██║██║██║╚██╗██║██╔══██║
//  ██║ ╚═╝ ██║██║  ██║╚██████╗██║  ██║██║██║ ╚████║██║  ██║
//  ╚═╝     ╚═╝╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝
//                                                          
//     ███╗   ██╗███████╗████████╗                          
//     ████╗  ██║██╔════╝╚══██╔══╝                          
//     ██╔██╗ ██║█████╗     ██║                             
//     ██║╚██╗██║██╔══╝     ██║                             
//  ██╗██║ ╚████║███████╗   ██║                             
//  ╚═╝╚═╝  ╚═══╝╚══════╝   ╚═╝                             
//                                                          
//   ██████╗██╗  ██╗ █████╗ ███╗   ██╗ ██████╗ ███████╗██╗      ██████╗  ██████╗
//  ██╔════╝██║  ██║██╔══██╗████╗  ██║██╔════╝ ██╔════╝██║     ██╔═══██╗██╔════╝
//  ██║     ███████║███████║██╔██╗ ██║██║  ███╗█████╗  ██║     ██║   ██║██║  ███╗
//  ██║     ██╔══██║██╔══██║██║╚██╗██║██║   ██║██╔══╝  ██║     ██║   ██║██║   ██║
//  ╚██████╗██║  ██║██║  ██║██║ ╚████║╚██████╔╝███████╗███████╗╚██████╔╝╚██████╔╝
//   ╚═════╝╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝ ╚═════╝ ╚══════╝╚══════╝ ╚═════╝  ╚═════╝
```
# TODO:
- [ ] Hopefully turn this changelog into something more [orthodox](http://keepachangelog.com/en/1.0.0/)
- [ ] Make `Machina` types JSON-de/serializable via `DataContract`s
- [ ] The `HUMAN` compiler doesn't export actions with the real/abs and axis/cartesian problem, even though it should since it doesn't really need to apply the actions to the writer... Same for 'MACHINA' compiler...
- [ ] Add 'EASE' mode to motion, as an option for UR robots to do MoveL
- [ ] `noTool` gets declared on every ABB program, even if not used. Fix this, and probably use `tool0` on compilation?
- [ ] Review if second attachments produce a TCP transformation without undoing the previous tool.


# v0.9.0
## BUILD 1600
- Added `KUKA` online control thanks to @Arastookhajehee! 🤓


# v0.8.14
## BUILD 1508
- Added `ArcMotion/To` action.
- `ArcMotion` action compiles to ABB offline
- `ArcMotion` action streams to ABB online
- Fixed ABB variable-base compiler, id numbers are now correlative. 

## BUILD 1507
- Improvements to the `KUKA` compiler from @Arastookhajehee! 😄


# v0.8.13b
## BUILD 1506
- Fixed bug that wouldn't allow `WriteAnalog` and `WriteDigital` to be exected on the same program.

# v0.8.13
## BUILD 1505
- Fixed a bug with `WriteAnalog` on `UR` devices.

# v0.8.12b
## BUILD 1504
`Action.Id` generation comes from a `static` counter in the `Action` class. This had a couple problems:
    - The counter is unique to the library. So, if a `Robot` was Disposed and a new one created, the first action for the second one would pick up from where the previous robot left (like resetting the Bridge multiple times). Alternatively, if the same assembly manages two robots, their action ids would alternate or be entwined. 
    - Can't do it instance, because it would require factory methods from the robot to create `actions`.
    - Solution: new `Actions` are created with an `id` of -1 (or id-less). This maintains the flexibility of creating robot-agnostic actions. However, when they get issued to a `Robot` instance, the instance adds a rolling `id` coming from an internal counter. 
    - Let's see how many things I break by doing this... XD
    - In any case, in the future, migration to non-numeric ids would be preferred... 
- [x] Shift `Action.Id` generation to `OnIssue`.

## BUILD 1503
- [x] Softened exceptions for `RobotStudioManager`: they are now Machina Logger Errors.
- [x] `ActionExecuted` events are now raised for `Actions` that are non-streamable (like `MotionMode`). This is a good improvement, and solves a problem with the Bridge were such actions would not get acknowledged.

# v0.8.12
## BUILD 1502
- Compilers now have embedded abstract comment characters and encoding
- Compilers now return `MachinaFile` objects instead of `List<string>`. This helps with multi-file program creation.
- `MachinaFiles` serialize to different string lists...
- `KUKA` compiler now returns `dat` and `src` files.
- - `MachinaFiles` is now `RobotProgramFile`
- [x] Added `RobotProgram` as an aggregator of `RobotProgramFiles`.
- [x] Adapt the `ABB` compiler to spit out two files
- [x] Adapt the `UR` compiler to spit out the `script` file
- [x] Adapt all the other compilers to reflect these changes
- [x] Check that all changes work
- [x] Split `Types` namespace into `Types.Geometry` and `Types.Data` or similar
- [x] Will need to change the GH + Dynamo compilers to adapt to these changes...

## BUILD 1501
- Changed the KUKA compiler with edits suggested by Alexander and Matty from RMIT.


# v0.8.11
## BUILD 1500
- Picked the project up after a long hiatus...
- Added `[ParseableFromString]` attribute for those methods in the API that can be parsed from primitive values.
- Changed `Do()`: it now parses a string and tries to turn it into an action using reflection.
- Added `Issue(Action)`: does what `Do()` used to do, take an Action and issue it.

# v0.8.10
## BUILD 1430
- Add `wobj` code option for ABB driver.
- Add quick conversion utility which I will totally revert...

# v0.8.9
## BUILD 1429
- Fix Example files.

# v0.8.8
## BUILD 1428
- Revert `RobotCursor` to default `null` tool; this was giving a full other set of problems... Let's see what I break now by doing this...
- Fix bug where tools changes would accumulate rather than undo...
- New build version to keep up with Dynamo app update.

# v0.8.7
## BUILD 1426
- `CustomAction` on ABB now adds action id and statemenet terminator automatically.
- Add quick custom action on ABB for Yumi gripping
- Fix check on UR protocol

## BUILD 1427
- Helper methods are now under `Machina.Utilites` namespace.
- Add utility parsing functions.

# v0.8.6
## BUILD 1424
- Added more getters for robot state
- More Type stringification
- `RobotCursor` now has the `"noTool"` tool attached by default.
- Unstreamable action now is logged at `Verbose` level

## BUILD 1425
- Workaround to SW3.0 crash problem: https://github.com/RobotExMachina/Machina.NET/issues/7
- Relative actions now log absolute state on Verbose


# v0.8.5 - UR fixes
## BUILD 1423
- UR now uses `movep` for linear motion, to ensure constant speed and blending radius
- Fix UR tool rounding error on weight.
- Fix UR analog out not existing for tool
- Fix UR DO for tool not streaming.

## BUILD 1424
- UR now initializes state (no need to start with a `Transform`...)
- Improved native type stringification.
- TCP position and Axes are now displayed upon connection.
- Fix typos on UR compiler
- Reverted back to UR `movel` motion on online due to SW3.0 problems: https://github.com/RobotExMachina/Machina.NET/issues/7
- Additional error handling for UR dis/connection.
- Reverted default `ToString()` to no labels (must extend this behavior to all the other types)


---
# v0.8.4
## BUILD 1421
- Made number stringification on compilers `CultureInvariant`

## BUILD 1422
- Add `IInstructionable` interface and implement it on `Tool` and all Actions
- Fix `Tool` not attaching correctly when TCP info is missing
- The ABB driver now accepts messages longer than 80 chars.


---
# v0.8.3
## BUILD 1420 - The YuMi fix
- Change ABB driver to account for different robtarget and jointtarget ext axes.
- Add `ArmAngle` action to set the value of the arm-angle for 7-dof robotic arms.
- Add `ArmAngleTo` to the API. For the time being, we will only have absolute modal for this.
- `RobotCursor` now keeps track of cartesian and joint external axes separately.
- `ExternalAxis` now accepts a third parameter as the target for this action: cartesian targets, joint targets or all.
- Changes to drivers, added motion update control and `CustomCode` for real-time streaming


## BUILD 1419
- Fix typos on UR modules
- Add `.prg` program init for ABB modules
- Add `GetDeviceDriverModules(params)` as a way of retrieving module files
- Quick and dirty fixing of the UR manager not being able to reconnect after disconnection
- Fix tool detachment when robot was coming from axis motion.


---
# v0.8.2
## BUILD 1418
- Logger functions are public now, so that external clients can log. Not great, but looking at making the Bridge great again!
- `id` on `MachinaEventArgs` `json` fixed to number.
- Remove several throws in favor of error logs and unsuccessful connections.
- Adding same tool name was giving errors, fixed.
- Add main task loader for `RobotStudioManager` as quick fix to interface to Yumi robots...
- Multiple threads were using the same `_responseChunks` array, fixed using a local variable now.
- Started stringifying numbers with `Culture.Invariant`, must find a better, more programmatic and less pain in the ass way.


# v0.8.1
## BUILD 1417
- Added monitoring module for real-time streaming of full poses.
- Add TCP listener for monitor on `TCPCommunicationManagerABB`
- `MotionUpdate` event is raised if real-time data is available on ABB robot.

# v0.8.0
## BUILD 1416
- Rename main `Control` `RobotCursor`s to match the issue/release/execute pattern.
- New `ActionExecuted` event, combines former `MotionCursorUpdated` and `ActionCompleted` in the same event.
- New `ActionReleased` event, raised any time an `Action` is sent to the device for execution.
- New `ActionIssued` event, raised any time an `Action` is successfully issued and scheduled for release or compilation.
- Base scaffolding for `MotionUpdate` event.


## BUILD 1415
- Created `DefineTool`, `AttachTool` and `DetachTool` as the new tool-related actions.
- Flagged `Attach` and `Detach` for deprecation.

## BUILD 1414
- Removal of all `JointAcceleration`, `JointSpeed` and `RotationSpeed` actions.
- Add version check for ABB robots
- Add `ExternalAxis` init msg from the ABB driver
- Make sure correct speed values are being sent and parsed by the drivers...
  - [x] ABB
  - [x] UR

## BUILD 1413
- Massive API deprecation.

## BUILD 1412
- Split `Action` classes
- Removed static `Action` constructors.
- Add `:base()` to several children `Action` classes
- `ActionType` is not an abstract property of `Action`

## BUILD 1411
- Add `Machina.Logger` for global message logging.
- Subscription to `Machina.Logger.WriteLine` broadcasts formatted messages below the set `Machina.Logger.SetLevel(int)`.
- [x] Improved Logging system
- [x] Console dump levels: DEBUG (5), VERBOSE (4), INFO (3), WARNING (2), ERROR (1) and NONE (0).


---
# v0.7.0
## BUILD 1410
- Create `ToolCreated` event (intern request ;)
- Add `MACHINA` compiler: it serializes each Action into its `ToInstruction` form.


## BUILD 1409
- `TCPCommunicationManagerABB` now listens to incoming messages from the driver
- `TCPCommunicationManagerABB` now is initialized with pose + joint data from the controller, which means being able to start with any relative/abs motion.
- `TCPCommunicationManagerABB` now waits for initialization data from the driver before successfully connecting, and fails to do so on timeout.


## BUILD 1408
- Create `MachinaEventArgs` abstract class and derivates per `Event`
- Each evenArgs now serializes itself into a `JSON` object
- Raising all events is now handled by `Control`
- Improve `OnActionCompleted`: it now returns the correct remaining action count, plus the buffer.





---
# v0.6.4

## BUILD 1407
- [x] Add `ExternalAxes` Action
- [x] Add `eternalAxes` as cursor state
- [x] Add `ExternalAxes(double? ext1 = null, double? ext2 = null, double? ext3 = null, double? ext4 = null, double? ext5 = null, double? ext6 = null)`
- [x] Update ABB driver to 1.0.2
- [x] Update ABB compiler
- [x] Update `ABBCommunicationProtocol`
- [x] Add `ExternalAxes` data type to replace `double?[]`
- [x] Add `.ToArrayString()` method to a bunch of data types, to generate array-like string representations: `Vector(500, 200, 300)` --> `"[500,200,300]"`
- [x] Fix a lot of rounding for String representation.
- [x] Change `ExternalAxes(double? a1...a6)` to `ExternalAxis(axisNumber, value)`
- [x] Add `CustomCode(string statement, bool isDeclaration)` Action




---
# v0.6.3

## BUILD 1405
- [x] Remove EOL chars and add double quotes to name in `Tool.ToInstruction()`
- [x] Update ABB driver

## BUILD 1404
- [x] Deprecate direct `Tool` constructors, substituted with static `Tool.Create(...)`
- [x] Add a private `Tool` constructor with all parameters as primitives.
- [x] Add `Tool.ToInstruction()` method to produce _streamable_ message
- [x] Fix typos in `Actions`

## BUILD 1403
- [x] Add safe program name check to avoid
- [x] IOs can now be named with `string`
- [x] Remove `Robot.SetIOName()`
- [x] Add `toolPin` option for IOs (UR robots)


---
# v0.6.2

## BUILD 1402
- [x] Add `ToInstruction()` override to Action:
    Generates a string representing a "serialized" instruction representing the Machina-API command that would have generated this action. Useful for generating actions to send to the Bridge.

---
# v0.6.1

## BUILD 1401
- [x] `Speed/To` can now be a `double`
- [x] `Precision/To` can now be a `double`
- [x] Tweaks to ABB compiler to accept the above.
- [x] Add `Acceleration` and `AccelerationTo` actions: add to Actions, Cursor, Settings
  - [x] Add acceleration params to UR compiler
  - [x] Acceleration values of zero or less reset it back to the robot's default.
- [x] Add `RotationSpeed/To()` option
  - [x] Add ABB correct compilation with defaults
  - [x] Add UR warning compilation message
- [x] Add `JointSpeed/To()` and `JointAcceleration/To()` for UR robots
  - [x] Add UR compilation
  - [x] Add ABB compilation warnings
- [x] UR streaming


# v0.6.0

## BUILD 1400
This release focuses on reworking the Streaming mode to base it off TCP connection with the controller server.
- [x] New factory constructor: `new Robot(...)` is now `Robot.Create(...)`
- [x] `Offline` mode working with new architecture
- [x] Add `ConnectionManager()` with options `user` (they are in charge of setting up communication server/firmatas) or `machina` (the library will try to make its best to figure out connection).
- [x] Created a lot of utility classes and managers of different kinds, trying to make the library [SOLID](https://en.wikipedia.org/wiki/SOLID_(object-oriented_design))
- [x] So many other untracked changes...
- [x] Fix C:/mod permissions for non-admins, use ENV system path
- [x] Rework the ABB real-time connection
- [x] Add an overload for TransformTo that takes single values as x, y, z, xvec0, xvec1, xvec2, yvec0, yvec1, yvec2.
- [x] Add BufferEmpty event
- [x] Add MotionCursorUpdated event
- [x] Add `GetCurrentPosition()` vs. `GetVirtualPsotion()`, drawing from state/virtual cursor.
- [x] Same for orientation and axes.
- [x] Add `SetUser(name, password)` to specify special logging credentials.
- [x] RobotWare options are now checked after log-on: for restricted accounts, `system.xml` is not accessible.


## BUILD 1308
- [x] Remove Undefined from MotionMode
- [ ] Add some kind of descriptive text for each MotionMode
- [x] Add `ExtrusionRate` to `Settings`


## BUILD 0.5.0.1307
- [ ] ~~REMOVE THE REGULAR/TO MODEL, and add a ActionMode("absolute"/"relative") to substitute it!~~
    - ActionMode becomes a property of the cursor.
    - Under this, Actions cannot be independently defined, but their meaning varies depending on when/where in the program they have been issued! :( This detracts from the conceptual independence of the Action and its platform-agnosticity... This may make sense in command-line environments, but will be quite shitty in VPLs
    - Make `Push/PopSettings()` store `ActionMode` too?
--> Decided not to go for this. The focus of this project is the CORE library, not the VPLs APIS... And when writting Machina code, the ...To() suffix is quite convenient and literal to quickly switch between modes, makes different explicit, is faster to type/read, and works better with auto completion in dev IDEs. Furtehrmore, it is interesting to keep the idea that Actions are agnostic to the medium; it would be weird if the same line of code would mean different things depending on the state of the cursor: the action should be absolute or relative on its own.
--> VPLs will have selector tabs to change the mode, or additional parameters to set abs/rel mode (the most typical usage scenario is using abs mode anyway...).

- [x] Add `ExtrusionRateTo()` and `TemperatureTo()`
- [x] Add `ExtrusionRateTo()` and `TemperatureTo()`
- [x] Rename "MotionType" to "MotionMode": action API and enum value
- [x] Rename `Mode()` to `ControlMode()`
- [x] Rename `RunMode()` to `CycleMode()`
- [ ] ~~Rename '`Attach`' to '`AttachTool`', and '`Detach`' to '`DetachTools`'...?~~
- [ ] ~~Rename '`PushSettings`' to '`SettingsPush`' and same for `Pop`?~~
- [x] Print a disclaimer header for exported code
- [x] Print a disclaimer header for exported code
    - [x] Fix ASCII art --> It is bad when writting text from UTF-8 to ASCII (every filetype but human...)
- [x] Rename `Zone` and `Joints` Actions in actions
- [x] Fix OfflineAPIs


## BUILD 1306 - 0.4.3
- [x] Make sure Extrusion Actions don't cause weird effects in non-3D printer compilers and viceversa
- [x] Rethink what the 3D printer does automatically and what needs to be managed by the user: temperature, calibration, homing... --> The philosophy of the library is that it is a very low-level 3D printer interface as a result of the ibject being a machine that can move in 3D space. It is for simple custom operations, not really for hi-end printing (user would be much better off using a slicer software).
    - [x] Focus on the ZMorph for now; if at some point I use other printer, will expand functionality.
    - [x] Add `Initialize()` and `Terminate()` for custom initialization and ending boilerplates.
    - [ ] ~~Change `Extrude(bool)` to `Extrude(double)` to include ExtrusionRate, and remove `ExtrusionRate`~~ --> let's keep it like this for the moment, might be confusing/tyring to combine them. --> Perhaps add a `Extrude(double)` overload tht combines them both?

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
