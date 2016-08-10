## REFERENCE

### System.Object - BRobot.Robot

####Constructors
Robot

#### Properties
Build (static)

#### Methods

- [Mode](#mode)

+ Connect
+ Disconnect


+ Start
+ Stop

Motion
Coordinates
Speed
Zone
PushSettings
PopSettings

Move
MoveTo
Rotate
RotateTo
Transform
TransformTo
Joints
JointsTo
Wait
Message

Execute
Export

Events
    BufferEmpty





## Methods

### Mode

_Mode(ControlMode mode)_

_Mode(string mode)_

Sets the control mode the robot will operate under: "offline" (default), "execute" or "stream".

```csharp
Robot arm = new Robot();
arm.Mode("stream");  // sets the robot instance to work in 'stream' mode

```


