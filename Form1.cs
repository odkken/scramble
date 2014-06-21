using System;
using System.Windows.Forms;
using RegawMOD.Android;
using System.Collections.Generic;

namespace SimpleButtonCheck
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        AndroidController android;
        Device device;

        private void Form1_Load(object sender, EventArgs e)
        {
            //Usually, you want to load this at startup, may take up to 5 seconds to initialize/set up resources/start server
            android = AndroidController.Instance;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            string serial;

            //Always call UpdateDeviceList() before using AndroidController on devices to get the most updated list
            android.UpdateDeviceList();

            if (android.HasConnectedDevices)
            {
                serial = android.ConnectedDevices[0];
                device = android.GetConnectedDevice(serial);
                //var words = Scramble.Solve(TextBox1.Text.ToCharArray());
                //foreach (var word in words)
                //{
                //    var moves = new List<AdbCommand>();
                //    foreach (var letterSqr in word.Key.Letters)
                //    {
                //        var pos = letterSqr.Pos;
                //        var x = 170 + pos.X * (920 - 170) / 3f;
                //        var y = 630 + pos.Y * (1370 - 630) / 3f;
                //        var comm = Adb.FormAdbShellCommand(device, false, "input tap " + (int)x + " " + (int)y);
                //        moves.Add(comm);
                //    }

                //    Adb.ExecuteAdbCommands(moves);
                //    var chek = Adb.FormAdbShellCommand(device, false, @"input", "tap 950 390");
                //    Adb.ExecuteAdbCommand(chek);
                //}
                var comm = Adb.FormAdbShellCommand(device, false, "input tap " + 800 + " " + 1560);
                while(true)
                {
                    Adb.ExecuteAdbCommand(comm);
                }
            }
            else
            {
                this.TextBox1.Text = "Error - No Devices Connected";
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //ALWAYS remember to call this when you're done with AndroidController.  It removes used resources
            android.Dispose();
        }
    }
}
