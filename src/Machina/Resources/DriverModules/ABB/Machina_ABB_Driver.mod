MODULE Machina_Driver

    ! ###\   ###\ #####\  ######\##\  ##\##\###\   ##\ #####\
    ! ####\ ####\##\\\##\##\\\\\\##\  ##\##\####\  ##\##\\\##\
    ! ##\####\##\#######\##\     #######\##\##\##\ ##\#######\
    ! ##\\##\\##\##\\\##\##\     ##\\\##\##\##\\##\##\##\\\##\
    ! ##\ \\\ ##\##\  ##\\######\##\  ##\##\##\ \####\##\  ##\
    ! \\\     \\\\\\  \\\ \\\\\\\\\\  \\\\\\\\\  \\\\\\\\  \\\
    !
    !
    ! This file starts a synchronous, single-threaded server on a virtual/real ABB robot,
    ! waits for a TCP client, listens to a stream of formatted string messages,
    ! buffers them parsed into an 'action' struct, and runs a loop to execute them.
    !
    ! IMPORTANT: make sure to adjust {{HOSTNAME}} and {{PORT}} to your current setup
    !
    ! More info on https://github.com/RobotExMachina
    ! A project by https://github.com/garciadelcastillo
    !
    !
    ! MIT License
    !
    ! Copyright (c) 2018 Jose Luis Garcia del Castillo y Lopez
    !
    ! Permission is hereby granted, free of charge, to any person obtaining a copy
    ! of this software and associated documentation files (the "Software"), to deal
    ! in the Software without restriction, including without limitation the rights
    ! to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    ! copies of the Software, and to permit persons to whom the Software is
    ! furnished to do so, subject to the following conditions:
    !
    ! The above copyright notice and this permission notice shall be included in all
    ! copies or substantial portions of the Software.
    !
    ! THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    ! IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    ! FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    ! AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    ! LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    ! OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    ! SOFTWARE.


    !   __  __ __        __    ___  __      __
    !  |  \|_ /  |   /\ |__) /\ | |/  \|\ |(_
    !  |__/|__\__|__/--\| \ /--\| |\__/| \|__)
    !

    ! An abstract representation of a robotic instruction
    RECORD action
        num id;
        num code;
        string s1;
        ! `Records` cannot contain arrays... :(
        num p1; num p2; num p3; num p4; num p5;
        num p6; num p7; num p8; num p9; num p10;
        num p11;
    ENDRECORD

    ! SERVER DATA --> to modify by user
    PERS string SERVER_IP := "{{HOSTNAME}}";     ! Replace "127.0.0.1" with device IP, typically "192.168.125.1" if working with a real robot or "127.0.0.1" if testing a virtual one (RobotStudio)
    CONST num SERVER_PORT := {{PORT}};           ! Replace 7000 with custom port number, like for example 7000

    ! Useful for handshakes and version compatibility checks...
    CONST string MACHINA_SERVER_VERSION := "1.4.1";

    ! Should program exit on any kind of error?
    PERS bool USE_STRICT := TRUE;

    ! TCP stuff
    LOCAL VAR string clientIp;
    LOCAL VAR socketdev serverSocket;
    LOCAL VAR socketdev clientSocket;

    ! A RAPID-code oriented API:
    !                                                 INSTRUCTION P1 P2 P3 P4...
    CONST num INST_MOVEL := 1;                      ! MoveL X Y Z QW QX QY QZ
    CONST num INST_MOVEJ := 2;                      ! MoveJ X Y Z QW QX QY QZ
    CONST num INST_MOVEABSJ := 3;                   ! MoveAbsJ J1 J2 J3 J4 J5 J6
    CONST num INST_SPEED := 4;                      ! (setspeed V_TCP [V_ORI V_LEAX V_REAX])
    CONST num INST_ZONE := 5;                       ! (setzone FINE TCP [ORI EAX ORI LEAX REAX])
    CONST num INST_WAITTIME := 6;                   ! WaitTime T
    CONST num INST_TPWRITE := 7;                    ! TPWrite "MSG"
    CONST num INST_TOOL := 8;                       ! (settool X Y Z QW QX QY QZ KG CX CY CZ)
    CONST num INST_NOTOOL := 9;                     ! (settool tool0)
    CONST num INST_SETDO := 10;                     ! SetDO "NAME" ON
    CONST num INST_SETAO := 11;                     ! SetAO "NAME" V
    CONST num INST_EXT_JOINTS_ALL := 12;            ! (setextjoints a1 a2 a3 a4 a5 a6, applies to both rob and jointtarget) --> send non-string 9E9 for inactive axes
    CONST num INST_ACCELERATION := 13;              ! WorldAccLim \On V (V = 0 sets \Off, any other value sets WorldAccLim \On V)
    CONST num INST_SING_AREA := 14;                 ! SingArea ON (ON = 0 sets SingArea \Off, any other value sets SingArea \Wrist)
    CONST num INST_EXT_JOINTS_ROBTARGET := 15;      ! (setextjoints a1 a2 a3 a4 a5 a6, applies only to robtarget)
    CONST num INST_EXT_JOINTS_JOINTTARGET := 16;    ! (setextjoints a1 a2 a3 a4 a5 a6, applies only to robtarget)
    CONST num INST_CUSTOM_ACTION := 17;             ! This is a wildcard for custom user functions that do not really fit in the Machina API (mainly Yumi gripping right now)

    CONST num INST_STOP_EXECUTION := 100;           ! Stops execution of the server module
    CONST num INST_GET_INFO := 101;                 ! A way to retreive state information from the server (not implemented)
    CONST num INST_SET_CONFIGURATION := 102;        ! A way to make some changes to the configuration of the server
    CONST num INST_SET_MOTION_UPDATE_INTERVAL := 103;  ! Sets the motion update interval in seconds.

    ! (these could be straight strings since they are never used for checks...?)
    PERS num RES_VERSION := 20;                     ! ">20 1 2 1;" Sends version numbers
    PERS num RES_POSE := 21;                        ! ">21 400 300 500 0 0 1 0;" Sends pose
    PERS num RES_JOINTS := 22;                      ! ">22 0 0 0 0 90 0;" Sends joints
    PERS num RES_EXTAX := 23;                       ! ">23 1000 9E9 9E9 9E9 9E9 9E9;" Sends external axes values
    PERS num RES_FULL_POSE := 24;                   ! ">24 X Y Z QW QX QY QZ J1 J2 J3 J4 J5 J6 A1 A2 A3 A4 A5 A6;" Sends all pose and joint info (probably on split messages)

    ! Characters used for buffer parsing
    PERS string STR_MESSAGE_END_CHAR := ";";        ! Marks the end of a message
    PERS string STR_MESSAGE_CONTINUE_CHAR := ">";   ! Marks the end of an unfinished message, to be continued on next message. Useful when the message is too long and needs to be split in chunks
    PERS string STR_MESSAGE_ID_CHAR := "@";         ! Flags a message as an acknowledgment message corresponding to a source id
    PERS string STR_MESSAGE_RESPONSE_CHAR := "$";   ! Flags a message as a response to an information request (acknowledgments do not include it)

    ! Rounding parameters for string representation of values (consistent with current Machina standards...)
    PERS num STR_RND_M := 6;
    PERS num STR_RND_MM := 3;
    PERS num STR_RND_DEGS := 3;
    PERS num STR_RND_QUAT := 4;
    PERS num STR_RND_RADS := 6;
    PERS num STR_RND_VOLT := 3;
    PERS num STR_RND_TEMP := 0;
    PERS num STR_RND_KG := 3;
    PERS num STR_RND_VEC := 5;

    ! Other stuff for parsing
    PERS string STR_DOT := ".";
    PERS string STR_DOUBLE_QUOTES := """";  ! A escaped double quote is written twice

    ! RobotWare 5.x shim
    PERS num WAIT_MAX := 8388608;

    ! State variables representing a virtual cursor of data the robot is instructed to
    PERS tooldata cursorTool;
    PERS wobjdata cursorWObj;
    VAR robtarget cursorRobTarget;
    VAR jointtarget cursorJointTarget;
    VAR extjoint cursorExtJointsRobTarget;  ! extax portion of robtarget
    VAR extjoint cursorExtJointsJointTarget;  ! extax portion of jointtarget
    VAR speeddata cursorSpeed;
    VAR zonedata cursorZone;
    VAR signaldo cursorDO;
    VAR signalao cursorAO;
    VAR num maxAcceleration;

    ! State variables for real-time motion tracking
    LOCAL VAR robtarget nowrt;
    LOCAL VAR jointtarget nowjt;
    LOCAL VAR extjoint nowexj;

    ! Buffer of incoming messages
    CONST num msgBufferSize := 1000;
    VAR string msgBuffer{msgBufferSize};
    VAR num msgBufferReadCurrPos;
    VAR num msgBufferReadPrevPos;
    VAR num msgBufferReadLine;
    VAR num msgBufferWriteLine;
    VAR bool isMsgBufferWriteLineWrapped;
    VAR bool streamBufferPending;

    ! Buffer of pending actions
    CONST num actionsBufferSize := 1000;
    VAR action actions{actionsBufferSize};
    VAR num actionPosWrite;
    VAR num actionPosExecute;
    VAR bool isActionPosWriteWrapped;

    ! Buffer of responses
    LOCAL VAR string response;


    ! SHARED WITH MONITOR MODULE
    ! If monitor module is available, these variables are shared and can be tweaked from this module.
    PERS num monitorUpdateInterval;                 ! Wait time in secs before next update. Is global variable so that it can be changed from Driver.
    PERS bool isMachinaDriverAvailable := TRUE;     ! Let the monitor know that the main task is running a valid Machina driver.



    !
    !  |\/| /\ ||\ |
    !  |  |/--\|| \|
    !

    ! Main entry point
    PROC Main()
        TPErase;

        ! Allow the controller to choose the best configuraation for motion
        ConfJ \Off;
        ConfL \Off;

        CursorsInitialize;
        ServerInitialize;

        MainLoop;
    ENDPROC

    ! The main loop the program will execute endlessly to read incoming messages
    ! and execute pending Actions
    PROC MainLoop()
        VAR action currentAction;
        VAR bool stopExecution := FALSE;

        WHILE stopExecution = FALSE DO
            ! Read the incoming buffer stream until flagges complete
            ! (must be done this way to avoid execution stack overflow through recursion)
            ReadStream;
            WHILE streamBufferPending = TRUE DO
                ReadStream;
            ENDWHILE
            ParseStream;

            ! Once the stream is flushed, execute all pending actions
            WHILE stopExecution = FALSE AND (actionPosExecute < actionPosWrite OR isActionPosWriteWrapped = TRUE) DO
                currentAction := actions{actionPosExecute};

                TEST currentAction.code
                CASE INST_MOVEL:
                    cursorRobTarget := GetRobTarget(currentAction);
                    MoveL cursorRobTarget, cursorSpeed, cursorZone, cursorTool, \WObj:=cursorWObj;

                CASE INST_MOVEJ:
                    cursorRobTarget := GetRobTarget(currentAction);
                    MoveJ cursorRobTarget, cursorSpeed, cursorZone, cursorTool, \WObj:=cursorWObj;

                CASE INST_MOVEABSJ:
                    cursorJointTarget := GetJointTarget(currentAction);
                    MoveAbsJ cursorJointTarget, cursorSpeed, cursorZone, cursorTool, \WObj:=cursorWObj;

                CASE INST_SPEED:
                    cursorSpeed := GetSpeedData(currentAction);

                CASE INST_ZONE:
                    cursorZone := GetZoneData(currentAction);

                CASE INST_WAITTIME:
                    WaitTime currentAction.p1;

                CASE INST_TPWRITE:
                    TPWrite currentAction.s1;

                CASE INST_TOOL:
                    cursorTool := GetToolData(currentAction);

                CASE INST_NOTOOL:
                    cursorTool := tool0;

                CASE INST_SETDO:
                    GetDataVal currentAction.s1, cursorDO;
                    SetDO cursorDO, currentAction.p1;

                CASE INST_SETAO:
                    GetDataVal currentAction.s1, cursorAO;
                    SetAO cursorAO, currentAction.p1;

                CASE INST_EXT_JOINTS_ALL:
                    cursorExtJointsRobTarget := GetExternalJointsData(currentAction);
                    cursorExtJointsJointTarget := cursorExtJointsRobTarget;

                CASE INST_EXT_JOINTS_ROBTARGET:
                    cursorExtJointsRobTarget := GetExternalJointsData(currentAction);

                CASE INST_EXT_JOINTS_JOINTTARGET:
                    cursorExtJointsJointTarget := GetExternalJointsData(currentAction);

                CASE INST_ACCELERATION:
                    maxAcceleration := currentAction.p1;
                    IF maxAcceleration > 0 THEN
                        WorldAccLim \On := maxAcceleration;
                    ELSE
                        WorldAccLim \Off;
                    ENDIF

                CASE INST_SING_AREA:
                    IF currentAction.p1 = 0 THEN
                        SingArea \Off;
                    ELSE
                        SingArea \Wrist;
                    ENDIF

                CASE INST_STOP_EXECUTION:
                    stopExecution := TRUE;

                CASE INST_GET_INFO:
                    SendInformation(currentAction);

                CASE INST_SET_MOTION_UPDATE_INTERVAL:
                    monitorUpdateInterval := currentAction.p1;
                    TPWrite("Monitor update interval set to " + NumToStr(monitorUpdateInterval, 2) + " s.");

                CASE INST_CUSTOM_ACTION:
                    CustomAction currentAction;
                ENDTEST

                ! Send acknowledgement message
                SendAcknowledgement(currentAction);

                !! Update the client with real-time motion data
                !SendPose;
                !SendJoints;

                actionPosExecute := actionPosExecute + 1;
                IF actionPosExecute > actionsBufferSize THEN
                    actionPosExecute := 1;
                    isActionPosWriteWrapped := FALSE;
                ENDIF

            ENDWHILE
        ENDWHILE

        ServerFinalize;

        ERROR
            IF ERRNO = ERR_SYM_ACCESS THEN
                TPWrite "MACHINA ERROR: Could not find signal """ + currentAction.s1 + """";
                TPWrite "Errors will follow";
                IF USE_STRICT THEN EXIT; ENDIF
                STOP;
            ENDIF

    ENDPROC




    !   __ __                   __   ___  __
    !  /  /  \|\/||\/|/  \|\ ||/   /\ | |/  \|\ |
    !  \__\__/|  ||  |\__/| \||\__/--\| |\__/| \|
    !

    ! Start the TCP server
    PROC ServerInitialize()
        TPWrite "Machina Driver: Initializing...";
        ServerRecover;
    ENDPROC

    ! Recover from a disconnection
    PROC ServerRecover()
        SocketClose serverSocket;
        SocketClose clientSocket;

        SocketCreate serverSocket;
        SocketBind serverSocket, SERVER_IP, SERVER_PORT;
        SocketListen serverSocket;

        TPWrite "Machina Driver: Waiting for incoming connection on " + SERVER_IP + ":" + NumToStr(SERVER_PORT, 0);

        SocketAccept serverSocket, clientSocket \ClientAddress:=clientIp \Time:=WAIT_MAX;

        TPWrite "Machina Driver: Connected to client: " + clientIp;
        TPWrite "Machina Driver: Listening to remote instructions...";

        ! Update the client with some data
        SendVersion;
        SendPose;
        SendJoints;
        SendExtAx;

        ERROR
            IF ERRNO = ERR_SOCK_TIMEOUT THEN
                TPWrite "MACHINA DRIVER ERROR: ServerRecover timeout. Retrying...";
                RETRY;
            ELSEIF ERRNO = ERR_SOCK_CLOSED THEN
                RETURN;
            ELSE
                ! No error recovery handling
            ENDIF
    ENDPROC

    ! Close sockets
    PROC ServerFinalize()
        SocketClose serverSocket;
        SocketClose clientSocket;
        WaitTime 2;
    ENDPROC



    !  __  __ __ __  __      __ __ __
    ! |__)|_ (_ |__)/  \|\ |(_ |_ (_
    ! | \ |____)|   \__/| \|__)|____)
    !

    ! Sends a short acknowledgement response to the client with the recently
    ! executed instruction and an optional id
    PROC SendAcknowledgement(action a)
        response := "";  ! acknowledgement responses do not start with the response char

        IF a.id <> 0 THEN
            response := response + STR_MESSAGE_ID_CHAR + NumToStr(a.id, 0) + STR_WHITE;
        ENDIF

        response := response + NumToStr(a.code, 0) + STR_MESSAGE_END_CHAR;

        SocketSend clientSocket \Str:=response;

        ERROR
            IF ERRNO = ERR_SOCK_CLOSED THEN
                TPWrite "MACHINA ERROR: acknowledgement lost, client disconnected.";
                RETURN;
            ELSE
                ! No error recovery handling
                TPWrite "MACHINA ERROR: unknown error";
            ENDIF
    ENDPROC

    ! Responds to an information request by sending a formatted message
    PROC SendInformation(action a)
        response := STR_MESSAGE_RESPONSE_CHAR;  ! only send response char for information requests

        IF a.id <> 0 THEN
            response := response + STR_MESSAGE_ID_CHAR + NumToStr(a.id, 0) + STR_WHITE;
        ENDIF

        response := response + NumToStr(a.code, 0) + STR_WHITE + NumToStr(a.p1, 0) + STR_WHITE;

        TEST a.p1

        CASE 1:  ! Module version
            response := response + STR_DOUBLE_QUOTES + MACHINA_SERVER_VERSION + STR_DOUBLE_QUOTES;

        CASE 2:  ! IP and PORT
            response := response + STR_DOUBLE_QUOTES + SERVER_IP + STR_DOUBLE_QUOTES + STR_WHITE + NumToStr(SERVER_PORT, 0);

        ENDTEST

        response := response + STR_MESSAGE_END_CHAR;

        SocketSend clientSocket \Str:=response;
    ENDPROC


    ! Send the version of this module to the socket
    PROC SendVersion()
        VAR bool ok;
        VAR string major;
        VAR string minor;
        VAR string patch;
        VAR num it := 1;
        VAR num itn;
        VAR num len;

        len := StrLen(MACHINA_SERVER_VERSION);

        itn := StrFind(MACHINA_SERVER_VERSION, it, STR_DOT);
        major := StrPart(MACHINA_SERVER_VERSION, it, itn - it);

        it := itn + 1;
        itn := StrFind(MACHINA_SERVER_VERSION, it, STR_DOT);
        minor := StrPart(MACHINA_SERVER_VERSION, it, itn - it);

        it := itn + 1;
        patch := StrPart(MACHINA_SERVER_VERSION, it, len - it + 1);

        response := STR_MESSAGE_RESPONSE_CHAR + NumToStr(RES_VERSION, 0) + STR_WHITE
            + major + STR_WHITE
            + minor + STR_WHITE
            + patch + STR_MESSAGE_END_CHAR;

        SocketSend clientSocket \Str:=response;
    ENDPROC


    ! Send the value of the current pose to the socket
    PROC SendPose()
        nowrt := CRobT(\Tool:=tool0, \WObj:=wobj0);

        response := STR_MESSAGE_RESPONSE_CHAR + NumToStr(RES_POSE, 0);
        response := response + STR_WHITE
            + NumToStr(nowrt.trans.x, STR_RND_MM) + STR_WHITE
            + NumToStr(nowrt.trans.y, STR_RND_MM) + STR_WHITE
            + NumToStr(nowrt.trans.z, STR_RND_MM) + STR_WHITE
            + NumToStr(nowrt.rot.q1, STR_RND_QUAT) + STR_WHITE
            + NumToStr(nowrt.rot.q2, STR_RND_QUAT) + STR_WHITE
            + NumToStr(nowrt.rot.q3, STR_RND_QUAT) + STR_WHITE
            + NumToStr(nowrt.rot.q4, STR_RND_QUAT) + STR_MESSAGE_END_CHAR;

        SocketSend clientSocket \Str:=response;
    ENDPROC

    ! Send the value of the current joints to the socket
    PROC SendJoints()
        nowjt := CJointT();

        response := STR_MESSAGE_RESPONSE_CHAR + NumToStr(RES_JOINTS, 0);
        response := response + STR_WHITE
            + NumToStr(nowjt.robax.rax_1, STR_RND_DEGS) + STR_WHITE
            + NumToStr(nowjt.robax.rax_2, STR_RND_DEGS) + STR_WHITE
            + NumToStr(nowjt.robax.rax_3, STR_RND_DEGS) + STR_WHITE
            + NumToStr(nowjt.robax.rax_4, STR_RND_DEGS) + STR_WHITE
            + NumToStr(nowjt.robax.rax_5, STR_RND_DEGS) + STR_WHITE
            + NumToStr(nowjt.robax.rax_6, STR_RND_DEGS) + STR_MESSAGE_END_CHAR;

        SocketSend clientSocket \Str:=response;
    ENDPROC

    ! Send the value of the current external axes to the socket
    PROC SendExtAx()
        VAR jointtarget jt;

        jt := CJointT();
        nowexj := jt.extax;

        response := STR_MESSAGE_RESPONSE_CHAR + NumToStr(RES_EXTAX, 0);
        response := response + STR_WHITE
            + NumToStr(nowexj.eax_a, STR_RND_MM \Exp) + STR_WHITE
            + NumToStr(nowexj.eax_b, STR_RND_MM \Exp) + STR_WHITE
            + NumToStr(nowexj.eax_c, STR_RND_MM \Exp) + STR_WHITE
            + NumToStr(nowexj.eax_d, STR_RND_MM \Exp) + STR_WHITE
            + NumToStr(nowexj.eax_e, STR_RND_MM \Exp) + STR_WHITE
            + NumToStr(nowexj.eax_f, STR_RND_MM \Exp) + STR_MESSAGE_END_CHAR;

        SocketSend clientSocket \Str:=response;
    ENDPROC


    !   __      __  __      __
    !  |__) /\ |__)(_ ||\ |/ _
    !  |   /--\| \ __)|| \|\__)
    !

    ! Read string buffer from the client and try to parse it
    PROC ReadStream()
        VAR string strBuffer;
        VAR num strBufferLength;
        SocketReceive clientSocket \Str:=strBuffer \NoRecBytes:=strBufferLength \Time:=WAIT_MAX;
        ParseBuffer strBuffer, strBufferLength;

        ERROR
        IF ERRNO = ERR_SOCK_CLOSED THEN
            ServerRecover;
            RETRY;
        ELSEIF ERRNO = ERR_SOCK_TIMEOUT THEN
            TPWrite "MACHINA ERROR: ReadStream() timeout. Retrying.";
            RETRY;
        ENDIF
    ENDPROC

    ! Parse an incoming string buffer, and decide what to do with it
    ! based on its quality
    PROC ParseBuffer(string sb, num sbl)
        ! Store the buffer
        StoreBuffer sb;

        ! Keep going if the chunk was trimmed
        streamBufferPending := StrFind(sb, sbl, STR_MESSAGE_END_CHAR) > sbl;
    ENDPROC

    ! Add a string buffer to the buffer of received messages
    PROC StoreBuffer(string buffer)
        IF isMsgBufferWriteLineWrapped = TRUE AND msgBufferWriteLine = msgBufferReadLine THEN
            TPWrite "MACHINA WARNING: memory overload. Maximum string buffer size is " + NumToStr(actionsBufferSize, 0);
            TPWrite "Reduce the amount of stream messages while they execute.";
            EXIT;
        ENDIF

        msgBuffer{msgBufferWriteLine} := buffer;
        msgBufferWriteLine := msgBufferWriteLine + 1;

        IF msgBufferWriteLine > msgBufferSize THEN
            msgBufferWriteLine := 1;
            isMsgBufferWriteLineWrapped := TRUE;
        ENDIF
    ENDPROC

    ! Parse the buffer of received messages into the buffer of pending actions
    PROC ParseStream()
        VAR string statement;
        VAR string part;
        VAR num partLength;
        VAR num lineLength;

        VAR bool parsingLongStatement := FALSE;
        VAR string statementParts{10};
        VAR num statementPartsCount := 0;

        ! TPWrite "Parsing buffered stream, actionPosWrite: " + NumToStr(actionPosWrite, 0);

        WHILE msgBufferReadLine < msgBufferWriteLine OR isMsgBufferWriteLineWrapped = TRUE DO
            lineLength := StrLen(msgBuffer{msgBufferReadLine});

            WHILE msgBufferReadCurrPos <= lineLength DO
                msgBufferReadCurrPos := StrFind(msgBuffer{msgBufferReadLine}, msgBufferReadPrevPos, STR_MESSAGE_END_CHAR);

                partLength := msgBufferReadCurrPos - msgBufferReadPrevPos;

                ! Work around RAPID's limitation of 80 chars per string
                ! Long statement?
                IF parsingLongStatement OR partLength + StrLen(part) > 80 THEN

                    ! First encounter with long statement?
                    IF parsingLongStatement = FALSE THEN
                        ! Store previous chunk
                        statementPartsCount := statementPartsCount + 1;
                        statementParts{statementPartsCount} := part;
                        parsingLongStatement := TRUE;
                    ENDIF

                    ! Add new chunk to list of statement lines
                    statementPartsCount := statementPartsCount + 1;
                    statementParts{statementPartsCount} := StrPart(msgBuffer{msgBufferReadLine}, msgBufferReadPrevPos, partLength);

                    ! Sanity
                    IF statementPartsCount > Dim(statementParts, 1) THEN
                        TPWrite "MACHINA ERROR: message exceeds maximum character length.";
                        IF USE_STRICT THEN
                            EXIT;
                        ELSE
                            part := "";
                            parsingLongStatement := FALSE;
                            statementPartsCount := 0;
                        ENDIF

                    ! If cursor is not at end of line, finish this list and parse it
                    ELSEIF msgBufferReadCurrPos <= lineLength THEN
                        statementParts{statementPartsCount} := statementParts{statementPartsCount} + STR_MESSAGE_END_CHAR;  ! quick and dirty add of the end_char... XD
                        ParseLongStatement statementParts, statementPartsCount;
                        part := "";
                        parsingLongStatement := FALSE;
                        statementPartsCount := 0;
                    ENDIF

                ! Othwerwise, parse as regular single string statement
                ELSE
                    part := part + StrPart(msgBuffer{msgBufferReadLine}, msgBufferReadPrevPos, partLength);  ! take the statement without the STR_MESSAGE_END_CHAR

                    IF msgBufferReadCurrPos <= lineLength THEN
                        ParseStatement(part + STR_MESSAGE_END_CHAR);  ! quick and dirty add of the end_char... XD
                        part := "";
                    ENDIF
                ENDIF

                msgBufferReadCurrPos := msgBufferReadCurrPos + 1;
                msgBufferReadPrevPos := msgBufferReadCurrPos;
            ENDWHILE

            msgBufferReadCurrPos := 1;
            msgBufferReadPrevPos := 1;

            msgBufferReadLine := msgBufferReadLine + 1;
            IF msgBufferReadLine > msgBufferSize THEN
                msgBufferReadLine := 1;
                isMsgBufferWriteLineWrapped := FALSE;
            ENDIF
        ENDWHILE
    ENDPROC

    ! Parse a string representation of a statement into an Action
    ! and store it in the buffer.
    PROC ParseStatement(string st)
        ! This assumes a string formatted in the following form:
        ! [@IDNUM ]INSCODE[ "stringParam"][ p0 p1 p2 ... p11]STR_MESSAGE_END_CHAR

        VAR bool ok;
        VAR bool end;
        VAR num pos := 1;
        VAR num nPos;
        VAR string s;
        VAR num len;
        VAR num params{11};
        VAR num paramsPos := 1;
        VAR action a;

        ! Sanity
        len := StrLen(st);
        IF len < 2 THEN
            TPWrite "MACHINA ERROR: received too short of a message:";
            TPWrite st;
            IF USE_STRICT THEN EXIT; ENDIF
        ENDIF

        ! Does the message come with a leading ID?
        IF StrPart(st, 1, 1) = STR_MESSAGE_ID_CHAR THEN
            nPos := StrFind(st, pos, STR_WHITE);
            IF nPos > len THEN
                TPWrite "MACHINA ERROR: incorrectly formatted message:";
                TPWrite st;
                IF USE_STRICT THEN EXIT; ENDIF
            ENDIF

            s := StrPart(st, 2, nPos - 2);
            ok := StrToVal(s, a.id);
            IF NOT ok THEN
                TPWrite "MACHINA ERROR: incorrectly formatted message:";
                TPWrite st;
                IF USE_STRICT THEN EXIT; ENDIF
                RETURN;
            ENDIF

            pos := nPos + 1;
        ENDIF

        ! Read instruction code
        nPos := StrFind(st, pos, STR_WHITE + STR_MESSAGE_END_CHAR);
        s := StrPart(st, pos, nPos - pos);
        ok := StrToVal(s, a.code);

        ! Couldn't read instruction code, discard this message
        IF NOT ok THEN
            TPWrite "MACHINA ERROR: received corrupt message:";
            TPWrite st;
            IF USE_STRICT THEN
              EXIT;
            ENDIF
            RETURN;
        ENDIF

        ! Is there any string param?
        pos := nPos + 1;
        nPos := StrFind(st, pos, STR_DOUBLE_QUOTES);
        IF nPos < len THEN
            pos := nPos + 1;
            nPos := StrFind(st, pos, STR_DOUBLE_QUOTES);  ! Find the matching double quote
            IF nPos < len THEN
                ! Succesful find of a double quote
                a.s1 := StrPart(st, pos, nPos - pos);
                pos := nPos + 2;  ! skip quotes and following char
                ! Reached end of string?
                IF pos > len THEN
                    end := TRUE;
                ENDIF
            ELSE
                TPWrite "MACHINA ERROR: corrupt message, missing closing double quotes.";
                TPWrite st;
                IF USE_STRICT THEN EXIT; ENDIF
                RETURN;
            ENDIF
        ENDIF

        ! Parse rest of numerical characters
        WHILE end = FALSE DO
            nPos := StrFind(st, pos, STR_WHITE + STR_MESSAGE_END_CHAR);
            IF nPos > len THEN
                end := TRUE;
            ELSE
                ! Parameters should be parsed differently depending on code
                ! for example, a TPWrite action will have a string rather than nums...
                s := StrPart(st, pos, nPos - pos);
                ok := StrToVal(s, params{paramsPos});
                IF ok = FALSE THEN
                    end := TRUE;
                    TPWrite "MACHINA ERROR: received corrupt parameter:";
                    TPWrite s;
                    IF USE_STRICT THEN EXIT; ENDIF
                ENDIF
                paramsPos := paramsPos + 1;
                pos := nPos + 1;
            ENDIF
        ENDWHILE

        ! Quick and dity to avoid a huge IF ELSE statement... unassigned vars use zeros
        a.p1 := params{1};
        a.p2 := params{2};
        a.p3 := params{3};
        a.p4 := params{4};
        a.p5 := params{5};
        a.p6 := params{6};
        a.p7 := params{7};
        a.p8 := params{8};
        a.p9 := params{9};
        a.p10 := params{10};
        a.p11 := params{11};

        ! Save it to the buffer
        StoreAction a;

    ENDPROC

    ! Parse a multi-string representation of a statement into an Action
    ! and store it in the buffer.
    PROC ParseLongStatement(string statementList{*}, num lineCount)
        ! This assumes a string formatted in the following form:
        ! [@IDNUM ]INSCODE[ "stringParam"][ p0 p1 p2 ... p11]STR_MESSAGE_END_CHAR
        VAR string arguments{100};
        VAR num argCount := 0;
        VAR num argId := 1;
        VAR string part := "";

        VAR bool ok;
        VAR bool end;
        VAR num pos := 1;
        VAR num nPos;
        VAR string s;
        VAR num len;
        VAR num params{11};
        VAR num paramsPos := 1;
        VAR action a;

        FOR index FROM 1 TO lineCount DO
            len := StrLen(statementList{index});

            WHILE nPos <= len DO
                nPos := StrFind(statementList{index}, pos, STR_WHITE);
                part := part + StrPart(statementList{index}, pos, nPos - pos);

                IF nPos <= len THEN
                    argCount := argCount + 1;
                    arguments{argCount} := part;
                    part := "";
                    pos := nPos + 1;
                ENDIF

            ENDWHILE

            ! Ready to parse next line
            pos := 1;
            nPos := 0;
        ENDFOR

        ! PARSE THE ARGUMENTS
        ! Does the message come with a leading ID?
        IF StrPart(arguments{argId}, 1, 1) = STR_MESSAGE_ID_CHAR THEN
            part := StrPart(arguments{argId}, 2, StrLen(arguments{argId}) - 1);
            ok := StrToVal(part, a.id);
            IF NOT ok THEN
                TPWrite "MACHINA ERROR: incorrectly formatted message:";
                TPWrite part;
                IF USE_STRICT THEN
                    EXIT;
                ENDIF
                RETURN;
            ENDIF

            ! Move to next arg
            argId := argId + 1;
        ENDIF

        ! Read instruction code
        ok := StrToVal(arguments{argId}, a.code);

        ! If couldn't read instruction code, discard this message
        IF NOT ok THEN
            TPWrite "MACHINA ERROR: received corrupt message:";
            TPWrite arguments{argId};
            IF USE_STRICT THEN
              EXIT;
            ENDIF
            RETURN;
        ENDIF

        argId := argId + 1;

        ! Is there any string param?
        len := StrLen(arguments{argId});
        IF StrPart(arguments{argId}, 1, 1) = STR_DOUBLE_QUOTES AND StrPart(arguments{argId}, len, 1) = STR_DOUBLE_QUOTES THEN
            ! Succesful find of a double quote
            a.s1 := StrPart(arguments{argId}, 2, len - 2);
            argId := argId + 1;
        ENDIF

        ! Parse rest of numerical characters
        FOR index FROM argId TO argCount DO
            ok := StrToVal(arguments{index}, params{paramsPos});
            IF NOT ok THEN
                TPWrite "MACHINA ERROR: received corrupt parameter:";
                TPWrite arguments{index};
                IF USE_STRICT THEN
                    EXIT;
                ENDIF
                RETURN;
            ENDIF
            paramsPos := paramsPos + 1;
        ENDFOR

        ! Quick and dity to avoid a huge IF ELSE statement... unassigned vars use zeros
        a.p1 := params{1};
        a.p2 := params{2};
        a.p3 := params{3};
        a.p4 := params{4};
        a.p5 := params{5};
        a.p6 := params{6};
        a.p7 := params{7};
        a.p8 := params{8};
        a.p9 := params{9};
        a.p10 := params{10};
        a.p11 := params{11};

        ! Save it to the buffer
        StoreAction a;
    ENDPROC



    ! Stores this action in the buffer
    PROC StoreAction(action a)
        IF isActionPosWriteWrapped = TRUE AND actionPosWrite = actionPosExecute THEN
            TPWrite "MACHINA WARNING: memory overload. Maximum Action buffer size is " + NumToStr(actionsBufferSize, 0);
            TPWrite "Reduce the amount of stream messages while they execute.";
            EXIT;
        ENDIF

        actions{actionPosWrite} := a;
        actionPosWrite := actionPosWrite + 1;

        IF actionPosWrite > actionsBufferSize THEN
            actionPosWrite := 1;
            isActionPosWriteWrapped := TRUE;
        ENDIF

    ENDPROC





    !          ___     ___      __         _____  __      __
    !      /  \ | ||  | | \_/  |_ /  \|\ |/   | |/  \|\ |(_
    !      \__/ | ||__| |  |   |  \__/| \|\__ | |\__/| \|__)
    !

    ! Initialize robot cursor values to current state and some defaults
    PROC CursorsInitialize()
        msgBufferReadCurrPos := 1;
        msgBufferReadPrevPos := 1;
        msgBufferReadLine := 1;
        msgBufferWriteLine := 1;
        isMsgBufferWriteLineWrapped := FALSE;
        streamBufferPending := FALSE;

        actionPosWrite := 1;
        actionPosExecute := 1;
        isActionPosWriteWrapped := FALSE;

        response := "";

        cursorTool := tool0;
        cursorWObj := wobj0;
        cursorRobTarget := CRobT(\Tool:=tool0, \WObj:=wobj0);
        cursorExtJointsRobTarget := cursorRobTarget.extax;
        cursorJointTarget := CJointT();
        cursorExtJointsJointTarget := cursorJointTarget.extax;
        cursorSpeed := v20;
        cursorZone := z5;
        maxAcceleration := -1;
    ENDPROC

    ! Return the jointtarget represented by an Action
    FUNC jointtarget GetJointTarget(action a)
        RETURN [[a.p1, a.p2, a.p3, a.p4, a.p5, a.p6], cursorExtJointsJointTarget];
    ENDFUNC

    ! Return the robottarget represented by an Action
    FUNC robtarget GetRobTarget(action a)
        RETURN [[a.p1, a.p2, a.p3], [a.p4, a.p5, a.p6, a.p7], [0, 0, 0, 0], cursorExtJointsRobTarget];
    ENDFUNC

    ! Return the speeddata represented by an Action
    FUNC speeddata GetSpeedData(action a)
        ! Fill in the gaps
        IF a.p2 = 0 THEN
            a.p2 := a.p1;
        ENDIF
        IF a.p3 = 0 THEN
            a.p3 := a.p1;
        ENDIF
        IF a.p4 = 0 THEN
            a.p4 := a.p1;
        ENDIF

        RETURN [a.p1, a.p2, a.p3, a.p4];
    ENDFUNC

    ! Return the zonedata represented by an Action
    FUNC zonedata GetZoneData(action a)
        IF a.p1 = 0 THEN
            RETURN fine;
        ENDIF

        ! Fill in some gaps
        IF a.p2 = 0 THEN
            a.p2 := 1.5 * a.p1;
        ENDIF
        IF a.p3 = 0 THEN
            a.p3 := 1.5 * a.p1;
        ENDIF
        IF a.p4 = 0 THEN
            a.p4 := 0.1 * a.p1;
        ENDIF
        IF a.p5 = 0 THEN
            a.p5 := 1.5 * a.p1;
        ENDIF
        IF a.p6 = 0 THEN
            a.p6 := 0.1 * a.p1;
        ENDIF

        RETURN [FALSE, a.p1, a.p2, a.p3, a.p4, a.p5, a.p6];
    ENDFUNC

    ! Return the tooldata represented by an Action
    FUNC tooldata GetToolData(action a)
        ! If missing weight info
        IF a.p8 = 0 THEN
            a.p8 := 1;
        ENDIF

        ! If missing center of gravity info
        IF a.p9 = 0 THEN
            a.p9 := 0.5 * a.p1;
        ENDIF
        IF a.p10 = 0 THEN
            a.p10 := 0.5 * a.p2;
        ENDIF
        IF a.p11 = 0 THEN
            a.p11 := 0.5 * a.p3;
        ENDIF

        RETURN [TRUE, [[a.p1, a.p2, a.p3], [a.p4, a.p5, a.p6, a.p7]],
            [a.p8, [a.p9, a.p10, a.p11], [1, 0, 0, 0], 0, 0, 0]];
    ENDFUNC

    ! Return the external joints represented by an Action
    FUNC extjoint GetExternalJointsData(action a)
      RETURN [a.p1, a.p2, a.p3, a.p4, a.p5, a.p6];
    ENDFUNC

    ! TPWrite a string representation of an Action
    PROC LogAction(action a)
        TPWrite "ACTION: " + NumToStr(a.code, 0) + " "
            + a.s1 + " "
            + NumToStr(a.p1, 0) + " " + NumToStr(a.p2, 0) + " "
            + NumToStr(a.p3, 0) + " " + NumToStr(a.p4, 0) + " "
            + NumToStr(a.p5, 0) + " " + NumToStr(a.p6, 0) + " "
            + NumToStr(a.p7, 0) + " " + NumToStr(a.p8, 0) + " "
            + NumToStr(a.p9, 0) + " " + NumToStr(a.p10, 0) + " "
            + NumToStr(a.p11, 0) + STR_MESSAGE_END_CHAR;
    ENDPROC




    !   __     _____ __        __         _____  __      __
    !  /  /  \(_  | /  \|\/|  |_ /  \|\ |/   | |/  \|\ |(_
    !  \__\__/__) | \__/|  |  |  \__/| \|\__ | |\__/| \|__)
    !
    ! A wildcard function for users to customize particular behavior
    PROC CustomAction(action a)
      ! Write your own behavior here...
      TPWrite "Custom action request";
      LogAction a;
    ENDPROC

ENDMODULE
