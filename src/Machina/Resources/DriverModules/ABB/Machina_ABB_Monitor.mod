MODULE Machina_Monitor

    ! ###\   ###\ #####\  ######\##\  ##\##\###\   ##\ #####\
    ! ####\ ####\##\\\##\##\\\\\\##\  ##\##\####\  ##\##\\\##\
    ! ##\####\##\#######\##\     #######\##\##\##\ ##\#######\
    ! ##\\##\\##\##\\\##\##\     ##\\\##\##\##\\##\##\##\\\##\
    ! ##\ \\\ ##\##\  ##\\######\##\  ##\##\##\ \####\##\  ##\
    ! \\\     \\\\\\  \\\ \\\\\\\\\\  \\\\\\\\\  \\\\\\\\  \\\
    !
    !
    ! This file starts a synchronous, single-threaded server on a virtual/real ABB robot,
    ! waits for a TCP client, and upon connection, streams periodic update messages with
    ! information about the real-time status of the device.
    ! This module is meant to be executed on a Multi-Task enabled robot running parallel
    ! to the "Machina_Driver", as it uses some of its variables and conventions.
    !
    ! IMPORTANT: make sure to adjust {{HOSTNAME}} and {{PORTs}} to your current setup
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

    ! SERVER DATA --> to modify by user
    CONST num MONITOR_PORT := {{PORT}};           ! Replace {{PORT}} with custom port number, like for example 7001

    ! Useful for handshakes and version compatibility checks...
    CONST string MACHINA_MONITOR_VERSION := "1.0.0";

    ! Refresh rate
    PERS num monitorUpdateInterval := 0.1;    ! Wait time in secs before next update. Is global variable so that it can be changed from Driver.

    ! SHARED FROM DRIVER MODULE
    ! Should program exit on any kind of error?
    PERS bool USE_STRICT;
    PERS bool isMachinaDriverAvailable;       ! If this is false, no Machina Driver is running and this module won't work either.

    ! TCP stuff
    PERS string SERVER_IP;

    ! API (see Driver module for updated info)
    PERS num RES_VERSION;
    PERS num RES_POSE;
    PERS num RES_JOINTS;
    PERS num RES_EXTAX;
    PERS num RES_FULL_POSE;

    PERS string STR_MESSAGE_END_CHAR;
    PERS string STR_MESSAGE_CONTINUE_CHAR;
    PERS string STR_MESSAGE_ID_CHAR;
    PERS string STR_MESSAGE_RESPONSE_CHAR;

    ! Rounding parameters for string representation of values (consistent with current Machina standards...)
    PERS num STR_RND_M;
    PERS num STR_RND_MM;
    PERS num STR_RND_DEGS;
    PERS num STR_RND_QUAT;
    PERS num STR_RND_RADS;
    PERS num STR_RND_VOLT;
    PERS num STR_RND_TEMP;
    PERS num STR_RND_KG;
    PERS num STR_RND_VEC;

    ! Other stuff for parsing
    PERS string STR_DOT;
    PERS string STR_DOUBLE_QUOTES;

    ! RobotWare 5.x shim
    PERS num WAIT_MAX;

    ! Cursor parameters used by the Driver
    PERS tooldata cursorTool;
    PERS wobjdata cursorWObj;

    ! State variables for real-time motion tracking
    LOCAL VAR robtarget nowrt;
    LOCAL VAR jointtarget nowjt;
    LOCAL VAR extjoint nowexj;


    ! LOCAL VARS
    LOCAL VAR string clientIp;
    LOCAL VAR socketdev monServerSocket;
    LOCAL VAR socketdev monClientSocket;

    ! Buffer of responses
    LOCAL VAR string response;




    !
    !  |\/| /\ ||\ |
    !  |  |/--\|| \|
    !
    ! Main entry point
    PROC Main()
        IF isMachinaDriverAvailable = FALSE THEN
            TPWrite("ERROR: Machina Driver not available, shutting Monitor down...");
            EXIT;
        ENDIF

        ServerInitialize;

        MainLoop;
    ENDPROC

    ! The main loop the program will execute endlessly to stream tracking updates
    PROC MainLoop()
        VAR bool stopExecution := FALSE;

        WHILE stopExecution = FALSE DO
            ! Streaming stuff goes here
            SendFullPose;

            WaitTime monitorUpdateInterval;
        ENDWHILE

        ServerFinalize(2);

        ERROR
            TPWrite("ERROR: Something went wrong...");
            IF USE_STRICT THEN
              EXIT;
            ENDIF
            STOP;
    ENDPROC


    !   __ __                   __   ___  __
    !  /  /  \|\/||\/|/  \|\ ||/   /\ | |/  \|\ |
    !  \__\__/|  ||  |\__/| \||\__/--\| |\__/| \|
    !

    ! Start the TCP server
    PROC ServerInitialize()
        TPWrite "Initializing...";
        ServerRecover;
    ENDPROC

    ! Recover from a disconnection
    PROC ServerRecover()
        ServerFinalize(0);

        SocketCreate monServerSocket;
        SocketBind monServerSocket, SERVER_IP, MONITOR_PORT;
        SocketListen monServerSocket;

        TPWrite "Waiting for incoming connection on " + SERVER_IP + ":" + NumToStr(MONITOR_PORT, 0);

        SocketAccept monServerSocket, monClientSocket \ClientAddress:=clientIp \Time:=WAIT_MAX;

        TPWrite "Connected to client: " + clientIp;
        TPWrite "Streaming real-time pose every " + NumToStr(monitorUpdateInterval, 2) + " s";

        ERROR
            IF ERRNO = ERR_SOCK_TIMEOUT THEN
                TPWrite "ERROR: ServerRecover timeout. Retrying...";
                RETRY;
            ELSEIF ERRNO = ERR_SOCK_CLOSED THEN
                TPWrite "ERROR: socket closed.";
                RETURN;
            ELSE
                ! No error recovery handling
                TPWrite "ERROR: something went wrong...";
            ENDIF
    ENDPROC

    ! Close sockets
    PROC ServerFinalize(num pauseTime)
        SocketClose monServerSocket;
        SocketClose monClientSocket;
        WaitTime pauseTime;
    ENDPROC

    ! Send joints, pose and extax info in one message to the client
    PROC SendFullPose()
        ! If using two CJointT and CRobT queries, the poses they will be slightly
        ! off from each other due to the time difference between the queries.
        ! Use FK instead to compute the pose; more expensive (?) but more precise too.
        nowjt := CJointT();
        nowrt := CalcRobT(nowjt, cursorTool \WObj:=cursorWObj);

        ! String vars can only hold 80 chars. Split the message in three chunks
        ! Pose
        response := STR_MESSAGE_RESPONSE_CHAR + NumToStr(RES_FULL_POSE, 0);
        response := response + STR_WHITE
            + NumToStr(nowrt.trans.x, STR_RND_MM) + STR_WHITE
            + NumToStr(nowrt.trans.y, STR_RND_MM) + STR_WHITE
            + NumToStr(nowrt.trans.z, STR_RND_MM) + STR_WHITE
            + NumToStr(nowrt.rot.q1, STR_RND_QUAT) + STR_WHITE
            + NumToStr(nowrt.rot.q2, STR_RND_QUAT) + STR_WHITE
            + NumToStr(nowrt.rot.q3, STR_RND_QUAT) + STR_WHITE
            + NumToStr(nowrt.rot.q4, STR_RND_QUAT) + STR_MESSAGE_CONTINUE_CHAR;
        SendMessage(response);

        ! Joints
        response := NumToStr(nowjt.robax.rax_1, STR_RND_DEGS) + STR_WHITE
            + NumToStr(nowjt.robax.rax_2, STR_RND_DEGS) + STR_WHITE
            + NumToStr(nowjt.robax.rax_3, STR_RND_DEGS) + STR_WHITE
            + NumToStr(nowjt.robax.rax_4, STR_RND_DEGS) + STR_WHITE
            + NumToStr(nowjt.robax.rax_5, STR_RND_DEGS) + STR_WHITE
            + NumToStr(nowjt.robax.rax_6, STR_RND_DEGS) + STR_MESSAGE_CONTINUE_CHAR;
        SendMessage(response);

        ! ExtAxes
        response := NumToStr(nowjt.extax.eax_a, STR_RND_MM \Exp) + STR_WHITE
            + NumToStr(nowjt.extax.eax_b, STR_RND_MM \Exp) + STR_WHITE
            + NumToStr(nowjt.extax.eax_c, STR_RND_MM \Exp) + STR_WHITE
            + NumToStr(nowjt.extax.eax_d, STR_RND_MM \Exp) + STR_WHITE
            + NumToStr(nowjt.extax.eax_e, STR_RND_MM \Exp) + STR_WHITE
            + NumToStr(nowjt.extax.eax_f, STR_RND_MM \Exp) + STR_MESSAGE_END_CHAR;
        SendMessage(response);
    ENDPROC

    ! Send a message to the client socket
    PROC SendMessage(string msg)
        SocketSend monClientSocket \Str:=msg;

        ERROR
            IF ERRNO = ERR_SOCK_CLOSED THEN
                TPWrite "ERROR: client disconnected.";
                ServerRecover;
                RETURN;
            ELSE
                ! No error recovery handling
                TPWrite "ERROR: unknown error";
            ENDIF

            IF USE_STRICT THEN
              EXIT;
            ENDIF
    ENDPROC

ENDMODULE
