﻿
namespace WiiUGamepadToMouse
{
    partial class SizeView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SizeView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(854, 480);
            this.Cursor = System.Windows.Forms.Cursors.SizeAll;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SizeView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "SizeView";
            this.Load += new System.EventHandler(this.SizeView_Load);
            this.ResizeEnd += new System.EventHandler(this.SizeView_ResizeEnd);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.SizeView_MouseClick);
            this.ResumeLayout(false);

        }

        #endregion
    }
}