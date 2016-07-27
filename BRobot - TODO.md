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

## HIGH-LEVEL
- [x] Merged ConnectionMode & OnlineMode into ControlMode
- [x] Restructured library
- [x] Redesigned API
- [x] Abstracted TCPPOsition, TCPRotation and stuff into a VirtualRobot object
- [x] Ported Util methods as static to their appropriate geometry class 
- [ ] Created Debug() & Error() utility functions
- [ ] All the connection properties, runmodes and stuff should belong to the VirtualRobot?
- [ ] Detect out of position and joint errors and incorporate a soft-restart.
- [ ] Make changes in ControlMode at runtime possible, i.e. resetting controllers and communication, flushing queues, etc.
- [ ] Streamline 'bookmarked' positions with a dictionary in Control or similar 
- [ ] Implement .PointAt() (as in 'look' at somewhere)
- [ ] Rename the project to BroBot ;)
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

