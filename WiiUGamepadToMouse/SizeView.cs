using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WiiUGamepadToMouse
{
    public partial class SizeView : Form
    {
        public Form1 form1; // Reference to Form1.cs
        public SizeView()
        {
            InitializeComponent();
        }

        private void SizeView_Load(object sender, EventArgs e)
        {

        }

        private void SizeView_MouseClick(object sender, MouseEventArgs e)
        {
            Close();
            form1.aspectX = Size.Width;
            form1.aspectY = Size.Height;
            form1.xOffset = Location.X;
            form1.yOffset = Location.Y;
        }

        private void SizeView_ResizeEnd(object sender, EventArgs e)
        {
            form1.aspectX = Size.Width;
            form1.aspectY = Size.Height;
            form1.xOffset = Location.X;
            form1.yOffset = Location.Y;
        }
    }
}
