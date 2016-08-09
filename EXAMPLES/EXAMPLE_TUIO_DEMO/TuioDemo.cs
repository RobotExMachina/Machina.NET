/*
 * ROBOTCONTROL - TUIO_DEMO
 * A quick implementation of the BRobot library using ReacTIVision.
 * This example reads Tuio packages sent from a running instance of ReacTIVision,
 * generates 2D paths for each fiducial and uploads them to a Robot Controller
 * in real time to be immediate executed.
 * 
 * USAGE:
 * - Connect the computer to a real (e.g. IRC5) or virtual (e.g. RobotStudio) controller
 * - Run an instance of ReacTIVision in the system
 * - Run this app
 * - Use fiducial markers to generate 2D paths 
 */ 
 

/*
	TUIO C# Demo - part of the reacTIVision project
	Copyright (c) 2005-2014 Martin Kaltenbrunner <martin@tuio.org>

	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using TUIO;

using BRobot;
using System.Text;

public class TuioDemo : Form , TuioListener
{
	private TuioClient client;
	private Dictionary<long,TuioDemoObject> objectList;
	private Dictionary<long,TuioCursor> cursorList;
	private Dictionary<long,TuioBlob> blobList;
	private object cursorSync = new object();
	private object objectSync = new object();
	private object blobSync = new object();

	public static int width, height;
	private int window_width =  640;
	private int window_height = 480;
	private int window_left = 0;
	private int window_top = 0;
	private int screen_width = Screen.PrimaryScreen.Bounds.Width;
	private int screen_height = Screen.PrimaryScreen.Bounds.Height;

	private bool fullscreen;
	private bool verbose;

	SolidBrush blackBrush = new SolidBrush(Color.Black);
	SolidBrush whiteBrush = new SolidBrush(Color.White);

	SolidBrush grayBrush = new SolidBrush(Color.Gray);
	Pen fingerPen = new Pen(new SolidBrush(Color.Blue), 1);
        

    public TuioDemo(int port) {
		
		verbose = true;
		fullscreen = false;
		width = window_width;
		height = window_height;

		this.ClientSize = new System.Drawing.Size(width, height);
		this.Name = "TuioDemo";
		this.Text = "TuioDemo";
			
		this.Closing+=new CancelEventHandler(Form_Closing);
		this.KeyDown +=new KeyEventHandler(Form_KeyDown);

		this.SetStyle( ControlStyles.AllPaintingInWmPaint |
						ControlStyles.UserPaint |
						ControlStyles.DoubleBuffer, true);

		objectList = new Dictionary<long,TuioDemoObject>(128);
		cursorList = new Dictionary<long,TuioCursor>(128);
			
		client = new TuioClient(port);
		client.addTuioListener(this);

		client.connect();

        // ROBOT
        InitializeRobot();
    }



    private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {

 		if (e.KeyData == Keys.F1) {
	 		if (fullscreen == false) {

				width = screen_width;
				height = screen_height;

				window_left = this.Left;
				window_top = this.Top;

				this.FormBorderStyle = FormBorderStyle.None;
		 		this.Left = 0;
		 		this.Top = 0;
		 		this.Width = screen_width;
		 		this.Height = screen_height;

		 		fullscreen = true;
	 		} else {

				width = window_width;
				height = window_height;

		 		this.FormBorderStyle = FormBorderStyle.Sizable;
		 		this.Left = window_left;
		 		this.Top = window_top;
		 		this.Width = window_width;
		 		this.Height = window_height;

		 		fullscreen = false;
	 		}
 		} else if ( e.KeyData == Keys.Escape) {
            // ROBOT
            arm.Disconnect();

            this.Close();

 		} else if ( e.KeyData == Keys.V ) {
 			verbose=!verbose;

        } else if ( e.KeyData == Keys.S ) {
            // ROBOT
            RequestStopAfterCurrentProgram();

        } else if ( e.KeyData == Keys.R )
        {
            // ROBOT
            //MakeRobotSleep();
            currentFiduID = -1;
        }
        

 	}

	private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
	{
		client.removeTuioListener(this);

		client.disconnect();
		System.Environment.Exit(0);
	}

	public void addTuioObject(TuioObject o) {
		lock(objectSync) {
			objectList.Add(o.SessionID,new TuioDemoObject(o));
		}
        if (verbose) Console.WriteLine("add obj "+o.SymbolID+" ("+o.SessionID+") "+o.X+" "+o.Y+" "+o.Angle);

        // ROBOT
        if (cMode == ControlMode.Execute)
        {
            InitializePath(o);
            AddTargetToPath(o, 0);
        }
        else if (cMode == ControlMode.Stream)
        {     
            if (UseThisTUIOObject(o))
            {
                //MakeRobotWakeUp();
                //MoveRobotTo(o, 1);
                MoveRobotTo(o, 0);
            }
        }
        
    }

    // NOTE: this only gets invoked when there is significant change in the object.
    // If the object stays in place with no movement/rotation, it doesn't get called
    public void updateTuioObject(TuioObject o) {
		lock(objectSync) {
			objectList[o.SessionID].update(o);
		}
		if (verbose) Console.WriteLine("set obj "+o.SymbolID+" "+o.SessionID+" "+o.X+" "+o.Y+" "+o.Angle+" "+o.MotionSpeed+" "+o.RotationSpeed+" "+o.MotionAccel+" "+o.RotationAccel);

        // ROBOT
        if (cMode == ControlMode.Execute)
        {
            AddTargetToPath(o, 0);
        }
        else if (cMode == ControlMode.Stream && UseThisTUIOObject(o))
        {
            if (UseThisTUIOObject(o))
            {
                MoveRobotTo(o, 0);
            }
        }
        
    }

    public void removeTuioObject(TuioObject o) {
		lock(objectSync) {
			objectList.Remove(o.SessionID);
		}
		if (verbose) Console.WriteLine("del obj "+o.SymbolID+" ("+o.SessionID+")");

        // ROBOT
        // Sending the path to the robot or flagging current ID as inactive
        // is taken care of by TimeTick()
        if (cMode == ControlMode.Execute)
        {
            AddTargetToPath(o, 0);
        }
        else if (cMode == ControlMode.Stream && UseThisTUIOObject(o))
        {
            if (UseThisTUIOObject(o))
            {
                MoveRobotTo(o, 0);
            }
        }
    }

	public void addTuioCursor(TuioCursor c) {
		lock(cursorSync) {
			cursorList.Add(c.SessionID,c);
		}
		if (verbose) Console.WriteLine("add cur "+c.CursorID + " ("+c.SessionID+") "+c.X+" "+c.Y);
	}

	public void updateTuioCursor(TuioCursor c) {
		if (verbose) Console.WriteLine("set cur "+c.CursorID + " ("+c.SessionID+") "+c.X+" "+c.Y+" "+c.MotionSpeed+" "+c.MotionAccel);
	}

	public void removeTuioCursor(TuioCursor c) {
		lock(cursorSync) {
			cursorList.Remove(c.SessionID);
		}
		if (verbose) Console.WriteLine("del cur "+c.CursorID + " ("+c.SessionID+")");
 	}

	public void addTuioBlob(TuioBlob b) {
		lock(blobSync) {
			blobList.Add(b.SessionID,b);
		}
	if (verbose) Console.WriteLine("add blb "+b.BlobID + " ("+b.SessionID+") "+b.X+" "+b.Y+" "+b.Angle+" "+b.Width+" "+b.Height+" "+b.Area);
	}

	public void updateTuioBlob(TuioBlob b) {
	if (verbose) Console.WriteLine("set blb "+b.BlobID + " ("+b.SessionID+") "+b.X+" "+b.Y+" "+b.Angle+" "+b.Width+" "+b.Height+" "+b.Area+" "+b.MotionSpeed+" "+b.RotationSpeed+" "+b.MotionAccel+" "+b.RotationAccel);
	}

	public void removeTuioBlob(TuioBlob b) {
		lock(blobSync) {
			blobList.Remove(b.SessionID);
		}
		if (verbose) Console.WriteLine("del blb "+b.BlobID + " ("+b.SessionID+")");
	}

	public void refresh(TuioTime frameTime) {
        //Console.WriteLine("Refreshing");
        //Console.WriteLine("secs: " + frameTime.Seconds);                    // seconds elapsed since program start
        //Console.WriteLine("totalsecs: " + frameTime.TotalMilliseconds);     // millis elapsed since program start
        //Console.WriteLine("micros: " + frameTime.Microseconds);             // micros elapsed since last whole second

        // ROBOT
        TimeTick(frameTime);

        Invalidate();
	}

	protected override void OnPaintBackground(PaintEventArgs pevent)
	{
		// Getting the graphics object
		Graphics g = pevent.Graphics;
		g.FillRectangle(whiteBrush, new Rectangle(0,0,width,height));

		// draw the cursor path
		if (cursorList.Count > 0) {
 			lock(cursorSync) {
			foreach (TuioCursor tcur in cursorList.Values) {
				List<TuioPoint> path = tcur.Path;
				TuioPoint current_point = path[0];

				for (int i = 0; i < path.Count; i++) {
					TuioPoint next_point = path[i];
					g.DrawLine(fingerPen, current_point.getScreenX(width), current_point.getScreenY(height), next_point.getScreenX(width), next_point.getScreenY(height));
					current_point = next_point;
				}
				g.FillEllipse(grayBrush, current_point.getScreenX(width) - height / 100, current_point.getScreenY(height) - height / 100, height / 50, height / 50);
				Font font = new Font("Arial", 10.0f);
				g.DrawString(tcur.CursorID + "", font, blackBrush, new PointF(tcur.getScreenX(width) - 10, tcur.getScreenY(height) - 10));
			}
		}
		}

		// draw the objects
		if (objectList.Count > 0)
		{
 			lock(objectSync) {
				foreach (TuioDemoObject tobject in objectList.Values) {
					tobject.paint(g);
				}
			}
		}

	}

	public static void Main(String[] argv) {
	 	int port = 0;
		switch (argv.Length) {
			case 1:
				port = int.Parse(argv[0],null);
				if(port==0) goto default;
				break;
			case 0:
				port = 3333;
				break;
			default:
				Console.WriteLine("usage: java TuioDemo [port]");
				System.Environment.Exit(0);
				break;
		}
	
		TuioDemo app = new TuioDemo(port);
		Application.Run(app);
	}




    //██████╗  ██████╗ ██████╗  ██████╗ ████████╗
    //██╔══██╗██╔═══██╗██╔══██╗██╔═══██╗╚══██╔══╝
    //██████╔╝██║   ██║██████╔╝██║   ██║   ██║   
    //██╔══██╗██║   ██║██╔══██╗██║   ██║   ██║   
    //██║  ██║╚██████╔╝██████╔╝╚██████╔╝   ██║   
    //╚═╝  ╚═╝ ╚═════╝ ╚═════╝  ╚═════╝    ╚═╝   

    // ROBOT STUFF
    private Robot arm;

    // Motion settings for both modes
    private int velocity = 100;              // For instruct mode, use standard RAPID velocities
    private int sleepVelocity = 25;          // velocity at which it will retreat to sleep
    private int zone = 5;                    // For instruct mode, use standard RAPID zones
    private bool pathSimpHQ = false;            // use high quality path simplication?
    private double pathSimpResolution = 0.1;    // simplication precision in world mm 

    // The coordinates, dimensions and reorientations of the physical 3D box
    // the normalized coordinates will be remapped to
    private bool flipXY = true;
    private double worldX = 200;
    private double worldY = 100;
    private double worldZ = 200;
    private double boxX = 240;
    private double boxY = 320;
    private double boxZ = 100;

    //private double worldX = 300;
    //private double worldY = 150;
    //private double worldZ = 500;
    //private double boxX = 1;
    //private double boxY = 320;
    //private double boxZ = 240;

    private bool calibrateMotionArea = false;

    // In "stream" mode, marker movement will be replicated by the robot in near real-time.
    // In "instruct" mode, the whole stroke will be sent as a path to the robot.
    //private string onlineMode = "instruct";
    private ControlMode cMode = ControlMode.Execute;

    // In "stream" mode, which fiducial ID the app is reading 
    // (to avoid jumping between multiple simultaneous fiducials
    private int currentFiduID = -1;

    // In "stream" module, the distance from previous target to trigger sending
    // a new one to the robot
    private double thresholdDistance = 10;     // in real-world mm
    
    private Frame lastTarget = new Frame(0, 0, 0);
    private bool awake = false;
    
       
    // Buffer Paths in "instruct" mode
    private Dictionary<int, Path> fiduPaths;
    private Dictionary<int, long> fiduTimes;  // stores the time elapsed since last target was added
    private int strokeCount = 0;

    // Allow a time buffer before sending the path to the robot
    private long lastTimeTick = 0;
    private long idleTime = 0;                // how long has it been since TUIO has requested to add an object
    private long maxTimeInc = 1000;  // if a path hasn't received a target before this much time, it will be sent to the robot
    private long resetTime = 10000;           // after this much idle time, the robot will go to sleep

    private long minTimeBetweenTargets = 250;  // in ms
    private long lastTargetAddedTimestamp = 0;


    private bool fastTest = false;
    private object movementLock = new object();

    private void InitializeRobot()
    {
        // ROBOT
        arm = new Robot();
        arm.ControlMode(cMode);

        arm.Connect();
        arm.DebugDump();
        //Console.WriteLine(arm.GetIP());
        //Console.WriteLine(arm.GetCurrentPosition());
        //Console.WriteLine(arm.GetCurrentOrientation());
        //Console.WriteLine(arm.GetCurrentJoints());

        arm.Speed(velocity);
        arm.Zone(zone);

        if (cMode == ControlMode.Execute)
        {
            fiduPaths = new Dictionary<int, Path>();
            fiduTimes = new Dictionary<int, long>();
        }
        else if (cMode == ControlMode.Stream)
        {
            arm.Start();

            //arm.RotateTo(Rotation.FlippedAroundY);
            arm.RotateTo(0, 1, 0, 0, 0, 1);

            MoveRobotTo(0, 0, 0);
            if (calibrateMotionArea)
            {
                MoveRobotTo(1, 0, 0);
                MoveRobotTo(1, 1, 0);
                MoveRobotTo(0, 1, 0);
                MoveRobotTo(0, 0, 0);
            }

            // A test on adding a bunch of targets really fast
            if (fastTest)
            {
                for (var i = 0; i < 10; i++)
                {
                    for (var j = 0; j < 10; j++)
                    {
                        Console.WriteLine("{0} {1} {2}", i, j, DateTime.Now);
                        MoveRobotTo(0.1 * j, 0.1 * i, 0);
                    }
                }
            }
        }

    }

    /// <summary>
    /// Invoked any time the window is refreshed. 
    /// For every Path, it updates timestamps since last frame was added, 
    /// and triggers executions if over a certain threshold.
    /// </summary>
    private void TimeTick(TuioTime frameTime)
    {
        long timeInc = frameTime.TotalMilliseconds - lastTimeTick;

        if (cMode == ControlMode.Execute)
        {
            var keys = new List<int>(fiduTimes.Keys);       // http://stackoverflow.com/a/2260472
            foreach (var key in keys)
            {
                fiduTimes[key] += timeInc;
                if (fiduTimes[key] > maxTimeInc)
                {
                    SendPathToRobot(key);
                }
            }
        }
        else if (cMode == ControlMode.Stream)
        {
            //idleTime += timeInc;
            //if (awake && idleTime > maxTimeInc)
            //{
            //    MakeRobotSleep();
            //}
            
        }

        //Console.WriteLine("IDLE TIME: " + idleTime);
        lastTimeTick = frameTime.TotalMilliseconds;
    }




    // _____ _  __ _
    //(_  | |_)|_ |_||V|
    //__) | | \|__| || |
    private bool UseThisTUIOObject(TuioObject o)
    {
        // If no fiducial is on right now, mark this as current
        if (currentFiduID == -1)
        {
            currentFiduID = o.SymbolID;
            return true;
        }

        // If was unsing this ID, keep using it
        else if (currentFiduID == o.SymbolID)
        {
            return true;
        }

        // Otherwise, this marker is a different one from current, and shall not be used
        return false;
    }

    //private void MakeRobotWakeUp()
    //{
    //    arm.Stop();
    //    arm.Start();
    //    awake = true;
    //    arm.SetVelocity(velocity);
    //    if (lastTarget == null)
    //    {
    //        lastTarget = new Frame(0, 0, 0);
    //    }
    //}

    private void MoveRobotTo(TuioObject o, double normZ)
    {
        long inc = o.TuioTime.TotalMilliseconds - lastTargetAddedTimestamp;
        if (inc > minTimeBetweenTargets)
        {
            Console.WriteLine("MOVEMENT WENT THROUGH");
            lastTargetAddedTimestamp = o.TuioTime.TotalMilliseconds;
            MoveRobotTo(o.X, o.Y, normZ);
        }
        else
        {
            Console.WriteLine("Movement skipped");
        }
       
    }

    private void MoveRobotTo(double normX, double normY, double normZ)
    {

        lock (movementLock)
        {
            idleTime = 0;

            //// Remap the tuioObj to flat 2D
            //Frame target = new Frame(o.X, o.Y, z);
            //if (flipXY) target.FlipXY();
            //target.RemapAxis("x", 0, 1, worldX, worldX + boxX);
            //target.RemapAxis("y", 0, 1, worldY, worldY + boxY);
            //target.RemapAxis("z", 0, 1, worldZ, worldZ + boxZ);

            Frame target = new Frame(normX, normY, normZ);
            target.FlipXY();
            target.FlipXZ();
            target.ReverseZ();
            target.RemapAxis("x", 0, 1, worldX, worldX + boxX);
            target.RemapAxis("y", 0, 1, worldY, worldY + boxY);
            target.RemapAxis("z", 0, 1, worldZ, worldZ + boxZ);

            if (Frame.DistanceBetween(lastTarget, target) > thresholdDistance)
            {
                Console.WriteLine("--> SENDING MOVE REQUEST!");
                Console.WriteLine("     " + target);
                arm.MoveTo(target.Position.X, target.Position.Y, target.Position.Z);  // TODO: should implement a .MoveTo(Frame target) method... also, in this case velocity and zones are inferred from the initial setup on initialization
                lastTarget = target;
            }
        }
    }

    //private double worldX = 200;
    //private double worldY = 100;
    //private double worldZ = 200;
    //private double boxX = 100;
    //private double boxY = 320;
    //private double boxZ = 240;


    //private void MakeRobotSleep()
    //{
    //    Console.WriteLine("PUTTING ROBOT TO SLEEP");
    //    // Release current fiducial ID
    //    currentFiduID = -1;

    //    // TODO: implement some sort of 'slowly retreat back to home position'
    //    if (lastTarget != null)
    //    {
    //        //arm.MoveTo(lastTarget.Position.X, lastTarget.Position.Y, worldZ + boxZ);
    //        arm.SetVelocity(sleepVelocity);
    //        arm.MoveTo(worldX, 0, worldZ + boxZ);
    //    }

    //    awake = false;
    //}



    //___    _____ _     _____
    // | |\|(_  | |_)| |/   | 
    //_|_| |__) | | \|_|\__ | 

    private void InitializePath(TuioObject o)
    {
        //fiduPaths[o.SymbolID] = new Path();

        if (fiduPaths.ContainsKey(o.SymbolID))
        {
            //fiduPaths[o.SymbolID] = new Path("Stroke_" + strokeCount++);
            //fiduTimes[o.SymbolID] = 0;
        } 
        else
        {
            fiduPaths.Add(o.SymbolID, new Path("Stroke_" + strokeCount++));
            fiduTimes.Add(o.SymbolID, 0);
            AddTargetToPath(o, 1);
        }
    }

    private void AddTargetToPath(TuioObject o, double z)
    {
        // Add a position
        fiduPaths[o.SymbolID].Add(o.X, o.Y, z);
        fiduTimes[o.SymbolID] = 0;
    }

    private void SendPathToRobot(int objID)
    {

        Path targetPath = fiduPaths[objID];

        // Replicate last target at height 1
        targetPath.Add(targetPath.GetLastTarget().Position.X, targetPath.GetLastTarget().Position.Y, 1);

        Console.WriteLine("--> SENDING PATH " + targetPath.Name);

        if (flipXY)
        {
            targetPath.FlipXY();
        }
        targetPath.RemapAxis("x", 0, 1, worldX, worldX + boxX);
        targetPath.RemapAxis("y", 0, 1, worldY, worldY + boxY);
        targetPath.RemapAxis("z", 0, 1, worldZ, worldZ + boxZ);
        targetPath.Simplify(pathSimpResolution, pathSimpHQ);

        arm.LoadPath(targetPath);
        RemovePathFromDicts(objID);
        
    }


    private void RequestStopAfterCurrentProgram()
    {
        arm.StopAfterProgram();
    }

    

    private void RemovePathFromDicts(int objID)
    {
        fiduPaths.Remove(objID);
        fiduTimes.Remove(objID);
    }

    
}
