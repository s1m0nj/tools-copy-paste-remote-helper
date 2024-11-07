
// frmMain.cs
using System;
using System.IO;
using System.Windows.Forms;
using System.Configuration;
using System.Drawing;
using System.Management;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace cfx.tools.copy_paste_remote_helper
{
    public partial class frmMain : Form
    {
        private FileSystemWatcher fileWatcher;
        
        private bool isMonitoring = false;

        public frmMain()
        {
            InitializeComponent();
            LoadSettings();
            if (string.IsNullOrEmpty(txtClipboardFile.Text) || string.IsNullOrEmpty(txtAhkFile.Text) || string.IsNullOrEmpty(txtAhkFile.Text)) return;
            FileWatcher_Changed(null, null);
            BtnStartStop_Click(null, null);

        }

        private void BtnBrowseClipboard_Click(object sender, EventArgs e)
        {
            BrowseFile(txtClipboardFile);
        }

        private void BtnBrowseAhk_Click(object sender, EventArgs e)
        {
            BrowseFile(txtAhkFile);
        }

        private void BtnStartStop_Click(object sender, EventArgs e)
        {
            ToggleMonitoring(txtClipboardFile.Text, txtAhkFile.Text, btnStartStop);
        }

        private void BrowseFile(TextBox targetTextBox)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    targetTextBox.Text = openFileDialog.FileName;
                }
            }
        }

        private void ToggleMonitoring(string clipboardPath, string ahkPath, Button toggleButton)
        {
            if (isMonitoring)
            {
                // Stop Monitoring
                fileWatcher.EnableRaisingEvents = false;
                toggleButton.Text = "Start Monitoring";
                isMonitoring = false;
            }
            else
            {
                // Validate file paths
                if (!File.Exists(clipboardPath))
                {
                    MessageBox.Show("Clipboard file path is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (string.IsNullOrWhiteSpace(ahkPath))
                {
                    MessageBox.Show("Please specify a valid output file for the AutoHotkey script.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Start Monitoring
                txtClipboardFile.Text = clipboardPath;
                txtAhkFile.Text = ahkPath;
                fileWatcher.Path = Path.GetDirectoryName(txtClipboardFile.Text);
                fileWatcher.Filter = Path.GetFileName(txtClipboardFile.Text);
                fileWatcher.EnableRaisingEvents = true;
                toggleButton.Text = "Stop Monitoring";
                isMonitoring = true;
            }
        }

        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => FileWatcher_Changed(sender, e)));
                return;
            }

            try
            {
                // Read the content from the clipboard file with read sharing to prevent lock issues
                using (var stream = new FileStream(txtClipboardFile.Text, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream))
                {
                    string clipboardContent = reader.ReadToEnd();

                    // Generate AHK script based on clipboard content
                    string ahkScript = GenerateAhkScript(clipboardContent);

                    // Save the AHK script to the output file
                    File.WriteAllText(txtAhkFile.Text, ahkScript);

                    // Provide subtle visual feedback
                    this.BackColor = Color.LightGreen;
                    var timer = new Timer { Interval = 500 }; // Change back color after 500ms
                    timer.Tick += (s, ev) => {
                        this.BackColor = SystemColors.Control;
                        timer.Stop();
                        timer.Dispose();
                    };
                    timer.Start();

                    // Reload the AHK script
                    ReloadAhkScript(txtAhkFile.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing file change: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private string GenerateAhkScript(string clipboardContent)
        {
            // Split the clipboard content by newlines to handle each line separately.
            string[] lines = clipboardContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            StringBuilder ahkScript = new StringBuilder();
            ahkScript.AppendLine($"{txtHotkey.Text}::");
            ahkScript.AppendLine($"Sleep, 2000");


           
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    // If the line is blank, send an Enter
                    ahkScript.AppendLine("    Send, {Enter}");
                }
                else
                {
                    // Escape braces explicitly to prevent nested braces causing issues
                    string escapedLine = EscapeForSend(line);
                    ahkScript.AppendLine($"    Send, {escapedLine}{{Enter}}");
             
                }
            }

            ahkScript.AppendLine("return");

            return ahkScript.ToString();
        }

        // Helper function to escape special characters for AutoHotkey's Send command.
        private string EscapeForSend(string input)
        {
            StringBuilder escaped = new StringBuilder();
            foreach (char c in input)
            {
                switch (c)
                {
                    case '{':
                        escaped.Append("{{}");
                        break;
                    case '}':
                        escaped.Append("{}}");
                        break;
                    case '^':
                        escaped.Append("{^}");
                        break;
                    case '!':
                        escaped.Append("{!}");
                        break;
                    case '+':
                        escaped.Append("{+}");
                        break;
                    case '#':
                        escaped.Append("{#}");
                        break;
                    case '%':
                        escaped.Append("`%"); // Corrected to remove additional braces
                        break;
                    case '`':
                        escaped.Append("``"); // Removed extra braces for literal backticks
                        break;
                    case ' ':
                        escaped.Append("{Space}"); // Handle spaces explicitly
                        break;
                    case '\t':
                        escaped.Append("{Tab}"); // Handle tabs explicitly
                        break;
                    default:
                        escaped.Append(c);
                        break;
                }
            }
            return escaped.ToString();
        }


        //private string EscapeForAHK(string input)
        //{
        //    StringBuilder escaped = new StringBuilder();
        //    foreach (char c in input)
        //    {
        //        switch (c)
        //        {
        //            case '{':
        //                escaped.Append("{{}");
        //                break;
        //            case '}':
        //                escaped.Append("{}}");
        //                break;
        //            case '^':
        //                escaped.Append("{^}");
        //                break;
        //            case '!':
        //                escaped.Append("{!}");
        //                break;
        //            case '+':
        //                escaped.Append("{+}");
        //                break;
        //            case '#':
        //                escaped.Append("{#}");
        //                break;
        //            case '%':
        //                escaped.Append("`%"); // Corrected to remove additional braces
        //                break;
        //            case '`':
        //                escaped.Append("``"); // Removed extra braces for literal backticks
        //                break;
        //            case '$':
        //                escaped.Append("{$}");
        //                break;
        //            case '&':
        //                escaped.Append("{&}");
        //                break;
        //            case '<':
        //                escaped.Append("{<}");
        //                break;
        //            case '>':
        //                escaped.Append("{>}");
        //                break;
        //            case '(':
        //                escaped.Append("{(}");
        //                break;
        //            case ')':
        //                escaped.Append("{)}");
        //                break;
        //            case '[':
        //                escaped.Append("{[}");
        //                break;
        //            case ']':
        //                escaped.Append("{]}");
        //                break;
        //            case '"':
        //                escaped.Append("{\"}");
        //                break;
        //            case '\\':
        //                escaped.Append("{\\}");
        //                break;
        //            case ';':
        //                escaped.Append("{;}");
        //                break;
        //            case ',':
        //                escaped.Append("{,}");
        //                break;
        //            case '.':
        //                escaped.Append("{.}");
        //                break;
        //            case '?':
        //                escaped.Append("{?}");
        //                break;
        //            case ':':
        //                escaped.Append("{:}");
        //                break;
        //            case '=':
        //                escaped.Append("{=}");
        //                break;
        //            case '-':
        //                escaped.Append("{-}");
        //                break;
        //            case '_':
        //                escaped.Append("{_}");
        //                break;
        //            case '@':
        //                escaped.Append("{@}");
        //                break;
        //            default:
        //                escaped.Append(c);
        //                break;
        //        }
        //    }
        //    return escaped.ToString();
        //}



        private void ReloadAhkScript(string ahkFilePath)
        {
            try
            {
                // Ensure the file path is properly enclosed in quotes to handle spaces
                string formattedPath = $"\"{ahkFilePath}\"";

                // Check if the AutoHotkey process is already running
                var processes = System.Diagnostics.Process.GetProcessesByName("autohotkey");
                foreach (var process in processes)
                {
                    try
                    {
                        // Get the command line arguments of the running process
                        string processPath = GetCommandLine(process.Id);
                        if (processPath.Contains(ahkFilePath))
                        {
                            // Kill only if the process is running our script
                            process.Kill();
                        }
                    }
                    catch
                    {
                        // Ignore processes we can't access or get command line arguments for
                    }
                }

                // Start the script
                System.Diagnostics.Process.Start("autohotkey", formattedPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reloading AHK script: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetCommandLine(int processId)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processId}"))
                using (var objects = searcher.Get())
                {
                    foreach (var obj in objects)
                    {
                        return obj["CommandLine"]?.ToString() ?? string.Empty;
                    }
                }
            }
            catch
            {
                // Ignore any issues accessing the command line
            }
            return string.Empty;
        }




        private void LoadSettings()
        {
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.ClipboardFilePath)) 
                txtClipboardFile.Text = Properties.Settings.Default.ClipboardFilePath;
            else
                txtClipboardFile.Text = "C:\\Users\\SimonJackson\\OneDrive - Corefocus Consultancy Limited\\Desktop\\clipboard.txt";

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.AhkOutputFilePath)) 
                txtAhkFile.Text = Properties.Settings.Default.AhkOutputFilePath;
            else
                txtAhkFile.Text =  "C:\\Users\\SimonJackson\\OneDrive - Corefocus Consultancy Limited\\Desktop\\clipboard.ahk";

            //if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.Hotkey))
            //    txtHotkey.Text = Properties.Settings.Default.Hotkey;
            //else 
                txtHotkey.Text = "+!v";
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.ClipboardFilePath = txtClipboardFile.Text;
            Properties.Settings.Default.AhkOutputFilePath = txtAhkFile.Text;
            Properties.Settings.Default.Hotkey = txtHotkey.Text;
            Properties.Settings.Default.Save();
        }


    }
}
