﻿/*
 * Copyright (C) 2019-2020 FIX94
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Runtime.Serialization.Json;
using System.Xml;
using System.Xml.Linq;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Globalization;

namespace WiiUGamepadToMouse
{
    public partial class Form1 : Form
    {
        private bool started = false;
        private Thread thr;

        private double multi = 1.0;
        private bool touchleft = false;
        private bool prevleft = false;
        private bool prevright = false;
        private bool prevmiddle = false;
        public int xOffset = 0;
        public int yOffset = 0;

        System.Timers.Timer ourTimer = null;
        private int curTicks = 0;
        private bool wasScrolling = false;
        private bool trackpadMode;
        public int aspectX = 854;
        public int aspectY = 480;

        public Form1()
        {
            InitializeComponent();
            //clamp down settings if they were tampered with
            decimal refDec = Properties.Settings.Default.xOffset;
            clamp(ref refDec, -9999, 9999);
            numericUpDown1.Value = Properties.Settings.Default.xOffset = refDec;

            refDec = Properties.Settings.Default.yOffset;
            clamp(ref refDec, -9999, 9999);
            numericUpDown2.Value = Properties.Settings.Default.yOffset = refDec;

            checkBox1.Checked = Properties.Settings.Default.touchLeft;
            checkBox2.Checked = Properties.Settings.Default.autoStart;
            checkBox3.Checked = Properties.Settings.Default.trackpadMode;
            numericUpDown3.Value = Properties.Settings.Default.aspectX;
            numericUpDown4.Value = Properties.Settings.Default.aspectY;
            trackpadMode = checkBox3.Checked;
            aspectX = (int)numericUpDown3.Value;
            aspectY = (int)numericUpDown4.Value;

            FormClosing += DoExit;
            label2.Text = "Stopped";
            updateMulti(); //first update
            // trackBar1.ValueChanged += trackBar1_ValueChanged;
            numericUpDown1.KeyUp += numericUpDown1_ValueChanged;
            numericUpDown2.KeyUp += numericUpDown2_ValueChanged;

            ourTimer = new System.Timers.Timer();
            ourTimer.AutoReset = true;
            ourTimer.Elapsed += TimerTick;
            ourTimer.Interval = 80;

            if (Properties.Settings.Default.autoStart)
                serverStart();
        }

        private void clamp(ref int value, int min, int max)
        {
            value = (value < min) ? min : (value > max) ? max : value;
        }
        private void clamp(ref decimal value, decimal min, decimal max)
        {
            value = (value < min) ? min : (value > max) ? max : value;
        }

        private void DoExit(Object sender, EventArgs e)
        {
            serverEnd();
            if (ourTimer != null)
                ourTimer.Close();
            Properties.Settings.Default.Save();
        }

        private void TimerTick(Object sender, EventArgs e)
        {
            curTicks++;
        }

        private void updateMulti()
        {
            /* multi = (double)(trackBar1.Value) * 12.5 / 100.0;
            label4.Text = multi.ToString() + "x (" + (int)(aspectX * multi) + "x" + (int)(480.0 * multi) + ")"; */
            multi = 1.0;
        }

        /* private void trackBar1_ValueChanged(Object sender, EventArgs e)
        {
            Properties.Settings.Default.multi = trackBar1.Value;
            updateMulti();
        } */

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.touchLeft = checkBox1.Checked;
            touchleft = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.autoStart = checkBox2.Checked;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.xOffset = numericUpDown1.Value;
            xOffset = (int)numericUpDown1.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.yOffset = numericUpDown2.Value;
            yOffset = (int)numericUpDown2.Value;
        }

        private void serverStart()
        {
            if (!started)
            {
                thr = new Thread(new ThreadStart(Serverthread));
                thr.Priority = ThreadPriority.Highest;
                thr.Start();
                started = true;
                label2.Text = "Started";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            serverStart();
        }

        private void serverEnd()
        {
            if (started)
            {
                started = false;
                thr.Join();
                label2.Text = "Stopped";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            serverEnd();
        }

        //just some helper code for scroll wheel repeat
        private bool checkWheelScroll(double inY)
        {
            bool ret = false;
            //in case of it previously shut off
            if (!ourTimer.Enabled)
                ourTimer.Enabled = true;
            else //already enabled, lets check
            {
                int cmpTicks = 6;
                if (inY > 0.9) cmpTicks = 1;
                else if (inY > 0.65) cmpTicks = 2;
                else if (inY > 0.4) cmpTicks = 4;
                if (curTicks >= cmpTicks)
                {
                    ret = true;
                    curTicks = 0;
                }
            }
            return ret;
        }

        //mouse input code from https://gist.github.com/Harmek/1263705
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint numInputs, Input[] inputs, int size);

        internal struct MouseInput
        {
            public int X;
            public int Y;
            public int MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        internal struct Input
        {
            public int Type;
            public MouseInput MouseInput;
        }

        private const int InputMouse = 0;

        private const int MouseEventMove = 0x01;
        private const int MouseEventLeftDown = 0x02;
        private const int MouseEventLeftUp = 0x04;
        private const int MouseEventRightDown = 0x08;
        private const int MouseEventRightUp = 0x10;
        private const int MouseEventMiddleDown = 0x20;
        private const int MouseEventMiddleUp = 0x40;
        private const int MouseEventWheel = 0x0800;
        private const int MouseEventAbsolute = 0x8000;

        //gamepad buttons
        private const int VPAD_BUTTON_SYNC = 0x00000001,
            VPAD_BUTTON_HOME = 0x00000002,
            VPAD_BUTTON_MINUS = 0x00000004,
            VPAD_BUTTON_PLUS = 0x00000008,
            VPAD_BUTTON_R = 0x00000010,
            VPAD_BUTTON_L = 0x00000020,
            VPAD_BUTTON_ZR = 0x00000040,
            VPAD_BUTTON_ZL = 0x00000080,
            VPAD_BUTTON_DOWN = 0x00000100,
            VPAD_BUTTON_UP = 0x00000200,
            VPAD_BUTTON_RIGHT = 0x00000400,
            VPAD_BUTTON_LEFT = 0x00000800,
            VPAD_BUTTON_Y = 0x00001000,
            VPAD_BUTTON_X = 0x00002000,
            VPAD_BUTTON_B = 0x00004000,
            VPAD_BUTTON_A = 0x00008000,
            VPAD_BUTTON_TV = 0x00010000,
            VPAD_BUTTON_STICK_R = 0x00020000,
            VPAD_BUTTON_STICK_L = 0x00040000;

        private void button3_Click(object sender, EventArgs e)
        {
            // Open the size view window and resize it
            SizeView sizeView = new SizeView();
            sizeView.form1 = this;
            sizeView.Width = aspectX;
            sizeView.Height = aspectY;
            sizeView.Location = new Point(xOffset, yOffset);
            sizeView.Show();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            aspectX = (int)numericUpDown3.Value;
            Properties.Settings.Default.aspectX = aspectX;
            updateMulti();
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            aspectY = (int)numericUpDown4.Value;
            Properties.Settings.Default.aspectY = aspectY;
            updateMulti();
        }

        // timer to handle updating modified settings
        // 2 timers are running; the 80ms timer and this 100ms timer
        private void timer1_Tick(object sender, EventArgs e)
        {
            checkBox3.Checked = trackpadMode;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            trackpadMode = checkBox3.Checked;
            Properties.Settings.Default.trackpadMode = trackpadMode;
        }

        private void Serverthread()
        {
            //wait on port 4242 with a 200ms timeout for data receives
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 4242);
            UdpClient udpClient = new UdpClient(ipep);
            udpClient.Client.ReceiveTimeout = 200;
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            int oldTpX = 0, oldTpY = 0; // Initial touch points, used for trackpad mode
            int prevTpX = 0, prevTpY = 0; // Last touch points, used for trackpad mode
            int cursorX = 0, cursorY = 0; // Virtual cursor position, used for trackpad mode
            int currentCX = 0, currentCY = 0; // Current (new) cursor position
            bool oldTouched = false; // Whether or not touch input happened on the last UDP packet
            bool willClick = false; // Whether or not we should send a click event to the input system
            int previousHold = 0; // Last button state for the Wii U gamepad

            double touchTraversal = 0.0; // How far the touchpoint has moved from the original touchpoint, used for trackpad mode

            while (started)
            {
                byte[] data = null;
                try
                {
                    data = udpClient.Receive(ref sender);
                }
                catch (System.Net.Sockets.SocketException)
                {
                }
                //just try receive again
                if (data == null)
                    continue;

                //parse JSON that was given to us and read data we want to use
                XmlDictionaryReader jsonReader = JsonReaderWriterFactory.CreateJsonReader(data, 
                                                                new XmlDictionaryReaderQuotas());
                XElement wiiUGamePad = XElement.Load(jsonReader).Element("wiiUGamePad");
                int hold = Int32.Parse(wiiUGamePad.Element("hold").Value);
                int tpTouch = Int32.Parse(wiiUGamePad.Element("tpTouch").Value);
                int tpX = (int)(Int32.Parse(wiiUGamePad.Element("tpX").Value)*(aspectX/854.0));
                int tpY = (int)(Int32.Parse(wiiUGamePad.Element("tpY").Value)*(aspectY/480.0));
                Double lStickY = Double.Parse(wiiUGamePad.Element("lStickY").Value, CultureInfo.InvariantCulture);
                Double rStickY = Double.Parse(wiiUGamePad.Element("rStickY").Value, CultureInfo.InvariantCulture);

                //get our basic mouse data ready
                Input[] input = new Input[1];
                input[0].Type = InputMouse;
                input[0].MouseInput.Flags = 0;
                input[0].MouseInput.Time = 0;
                input[0].MouseInput.ExtraInfo = IntPtr.Zero;

                bool left = false, right = false, middle = false;
                //first, translate touchscreen info to mouse position
                if (tpTouch != 0)
                {
                    if (!oldTouched)
                    {
                        oldTpX = tpX;
                        oldTpY = tpY;
                        cursorX = Cursor.Position.X;
                        cursorY = Cursor.Position.Y;
                        willClick = trackpadMode;
                        touchTraversal = 0.0;
                        if (!trackpadMode)
                        {
                            oldTpX = 0;
                            oldTpY = 0;
                            cursorX = xOffset;
                            cursorY = yOffset;
                        }
                    }

                    // input[0].MouseInput.Flags |= MouseEventAbsolute | MouseEventMove;
                    // double inputXscale = (65535.0 / (double)SystemInformation.VirtualScreen.Width);
                    // double inputYscale = (65535.0 / (double)SystemInformation.VirtualScreen.Height);
                    // double touchXbase = ((tpX - oldTpX + cursorX) * multi) + xOffset;
                    // double touchYbase = ((tpY - oldTpY + cursorY) * multi) + yOffset;
                    touchTraversal += Math.Sqrt(((tpX - oldTpX) * (tpX - oldTpX)) + ((tpY - oldTpY) * (tpY - oldTpY)));
                    if (touchTraversal > 100.0 || !trackpadMode)
                    {
                        currentCX = tpX - oldTpX + cursorX;
                        currentCY = tpY - oldTpY + cursorY;
                    }
                    // input[0].MouseInput.X = (int)(touchXbase * inputXscale);
                    // input[0].MouseInput.Y = (int)(touchYbase * inputYscale);
                    Cursor.Position = new Point(currentCX, currentCY);

                    if (touchTraversal >= 10.0)
                    {
                        willClick = false;
                    }

                    //also allow left clicks with touchscreen if requested
                    if (touchleft) left = true;
                } 
                else if (oldTouched)
                {
                    if (willClick && checkBox4.Checked)
                    {
                        left = true;
                    }
                    cursorX = currentCX;
                    cursorY = currentCY;
                }
                oldTouched = (tpTouch != 0);

                // toggle trackpad mode if x is pressed
                if (((hold & (previousHold ^ 0xffffffff)) & VPAD_BUTTON_X) != 0)
                {
                    trackpadMode = !trackpadMode;
                }

                // toggle always touch mode if y is pressed
                if (((hold & (previousHold ^ 0xffffffff)) & VPAD_BUTTON_Y) != 0)
                {
                    trackpadMode = !trackpadMode;
                }

                //process buttons for left/right/middle clicks
                if ((hold & (VPAD_BUTTON_A | VPAD_BUTTON_ZL | VPAD_BUTTON_ZR)) != 0)
                    left = true;
                if ((hold & (VPAD_BUTTON_B | VPAD_BUTTON_L | VPAD_BUTTON_R)) != 0)
                    right = true;
                if ((hold & (VPAD_BUTTON_STICK_L | VPAD_BUTTON_STICK_R)) != 0)
                    middle = true;

                //set mouse left/right/middle click status
                if (!prevleft && left)
                    input[0].MouseInput.Flags |= MouseEventLeftDown;
                else if(prevleft && !left)
                    input[0].MouseInput.Flags |= MouseEventLeftUp;

                if (!prevright && right)
                    input[0].MouseInput.Flags |= MouseEventRightDown;
                else if (prevright && !right)
                    input[0].MouseInput.Flags |= MouseEventRightUp;
                
                if (!prevmiddle && middle)
                    input[0].MouseInput.Flags |= MouseEventMiddleDown;
                else if (prevmiddle && !middle)
                    input[0].MouseInput.Flags |= MouseEventMiddleUp;

                prevleft = left;
                prevright = right;
                prevmiddle = middle;

                //set up mouse scroll wheel using analog sticks
                if (lStickY > 0.15 || rStickY > 0.15)
                {
                    double inY = Math.Max(lStickY, rStickY);
                    if (checkWheelScroll(inY) || !wasScrolling)
                    {
                        input[0].MouseInput.Flags |= MouseEventWheel;
                        input[0].MouseInput.MouseData = 120;
                        wasScrolling = true;
                    }

                }
                else if (lStickY < -0.15 || rStickY < -0.15)
                {
                    double inY = Math.Max(-lStickY, -rStickY);
                    if (checkWheelScroll(inY) || !wasScrolling)
                    {
                        input[0].MouseInput.Flags |= MouseEventWheel;
                        input[0].MouseInput.MouseData = -120;
                        wasScrolling = true;
                    }
                }
                else if(wasScrolling) //reset to slow interval
                {
                    wasScrolling = false;
                    ourTimer.Enabled = false;
                    curTicks = 0;
                }

                //finally send our mouse data to OS
                SendInput(1, input, Marshal.SizeOf(input[0]));

                previousHold = hold;
            }
            udpClient.Close();
        }
    }
}
