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

using RobotControl;
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
            arm.Disconnect();
            this.Close();

 		} else if ( e.KeyData == Keys.V ) {
 			verbose=!verbose;
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
        InitializePath(o);
    }



    public void updateTuioObject(TuioObject o) {
		lock(objectSync) {
			objectList[o.SessionID].update(o);
		}
		if (verbose) Console.WriteLine("set obj "+o.SymbolID+" "+o.SessionID+" "+o.X+" "+o.Y+" "+o.Angle+" "+o.MotionSpeed+" "+o.RotationSpeed+" "+o.MotionAccel+" "+o.RotationAccel);

        // ROBOT
        AddTargetToPath(o);
    }


    public void removeTuioObject(TuioObject o) {
		lock(objectSync) {
			objectList.Remove(o.SessionID);
		}
		if (verbose) Console.WriteLine("del obj "+o.SymbolID+" ("+o.SessionID+")");

        // ROBOT
        AddTargetToPath(o);
        SendPathToRobot();
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

        Console.WriteLine("OnPaintBackground");
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
    private List<double> xpos = new List<double>();
    private List<double> ypos = new List<double>();

    private void InitializeRobot()
    {
        // ROBOT
        arm = new Robot();

        //arm.Connect("instruct");
        arm.Connect();
        Console.WriteLine(arm.IP);
        Console.WriteLine(arm.GetPosition());
        Console.WriteLine(arm.GetOrientation());
        Console.WriteLine(arm.GetJoints());
    }

    private void InitializePath(TuioObject o)
    {
        // Clear the lists and add position
        xpos = new List<double>();
        ypos = new List<double>();
        xpos.Add(o.X);
        ypos.Add(o.Y);
    }

    private void AddTargetToPath(TuioObject o)
    {
        // Add position
        xpos.Add(o.X);
        ypos.Add(o.Y);
    }

    private void SendPathToRobot()
    {
        Console.WriteLine("--> SENDING PATH");

        Path currentPath = new Path();
        List<double> transX = MapCoordiantes(ypos, 200, 240);
        List<double> transY = MapCoordiantes(xpos, 0, 320);
        for (int i = 0; i < transX.Count; i++)
        {
            currentPath.Add(transX[i], transY[i], 200);
        }
        arm.ExecutePath(currentPath);

    }

    private List<double> MapCoordiantes(List<double> normVal, double newOrigin, double newDim)
    {
        List<double> mapped = new List<double>();
        for (int i = 0; i < normVal.Count; i++)
        {
            mapped.Add(newOrigin + normVal[i] * newDim);
        }

        return mapped;
    }

}
