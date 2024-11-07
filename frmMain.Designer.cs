// frmMain.Designer.cs
using System;
using System.IO;
using System.Windows.Forms;
using System.Configuration;
using System.Drawing;

namespace cfx.tools.copy_paste_remote_helper
{
    partial class frmMain
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
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 250);
            this.Text = "Clipboard to AutoHotkey Generator";
            this.FormClosing += FrmMain_FormClosing;

            // Clipboard File Label
            Label lblClipboardFile = new Label() { Text = "Clipboard File:", Left = 10, Top = 20, Width = 100 };
            this.Controls.Add(lblClipboardFile);

            // Clipboard File TextBox
            txtClipboardFile = new TextBox() { Left = 120, Top = 20, Width = 350, Name = "txtClipboardFile" };
            this.Controls.Add(txtClipboardFile);

            // Clipboard File Browse Button
            btnBrowseClipboard = new Button() { Text = "Browse", Left = 480, Top = 18, Width = 75 };
            btnBrowseClipboard.Click += BtnBrowseClipboard_Click;
            this.Controls.Add(btnBrowseClipboard);

            // AHK Output File Label
            Label lblAhkFile = new Label() { Text = "AHK Output File:", Left = 10, Top = 60, Width = 100 };
            this.Controls.Add(lblAhkFile);

            // AHK Output File TextBox
            txtAhkFile = new TextBox() { Left = 120, Top = 60, Width = 350, Name = "txtAhkFile" };
            this.Controls.Add(txtAhkFile);

            // AHK Output File Browse Button
            btnBrowseAhk = new Button() { Text = "Browse", Left = 480, Top = 58, Width = 75 };
            btnBrowseAhk.Click += BtnBrowseAhk_Click;
            this.Controls.Add(btnBrowseAhk);

            // Hotkey Label
            Label lblHotkey = new Label() { Text = "Hotkey:", Left = 10, Top = 100, Width = 100 };
            this.Controls.Add(lblHotkey);

            // Hotkey TextBox
            txtHotkey = new TextBox() { Left = 120, Top = 100, Width = 100, Name = "txtHotkey" };
            this.Controls.Add(txtHotkey);

            // Start/Stop Monitoring Button
            btnStartStop = new Button() { Text = "Start Monitoring", Left = 10, Top = 140, Width = 150 };
            btnStartStop.Click += BtnStartStop_Click;
            this.Controls.Add(btnStartStop);

            // FileSystemWatcher initialization
            fileWatcher = new FileSystemWatcher();
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            fileWatcher.Changed += FileWatcher_Changed;
        }

        private TextBox txtClipboardFile;
        private Button btnBrowseClipboard;
        private TextBox txtAhkFile;
        private Button btnBrowseAhk;
        private TextBox txtHotkey;
        private Button btnStartStop;

        #endregion
    }
}