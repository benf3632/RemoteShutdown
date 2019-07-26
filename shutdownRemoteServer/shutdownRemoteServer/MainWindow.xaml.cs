using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace shutdownRemoteServer
{
    public partial class MainWindow : Window
    {

        private enum Mode

        {
            Shutdown,
            Restart,
            Hibrnate,
        }

        private int time = 5;
        private DispatcherTimer timer;
        private Mode mode;
        private System.Windows.Forms.NotifyIcon trayIcon;

        private System.Diagnostics.Process process;
        private System.Diagnostics.ProcessStartInfo startInfo;
        private BackgroundWorker background = new BackgroundWorker();
        private bool started = false;


        public MainWindow()
        {
            InitializeComponent();

            trayIcon = new System.Windows.Forms.NotifyIcon();
            trayIcon.Icon = Resource.shutdown;
            trayIcon.MouseDoubleClick += TrayIcon_MouseDoubleClick;
            trayIcon.MouseDown += trayIcon_MouseDown;
            trayIcon.Visible = true;

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;

            process = new System.Diagnostics.Process();
            startInfo = new System.Diagnostics.ProcessStartInfo();

            start.Click += Start_Click;

            SocketListener.StartListening();

            Thread thread = new Thread(new ThreadStart(SocketListener.Accept));
            thread.IsBackground = true;
            thread.Start();
       
            background.WorkerReportsProgress = true;
            background.DoWork += HandleMessages;
            background.ProgressChanged += Start_Click;
            background.RunWorkerAsync();

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                string value = key.GetValue("RemoteShutdownServer") as string;
                bool check = value != null ? true : false;
          
                DockStartup.IsChecked = check;
                ContextMenu menu = this.Resources["TrayMenu"] as ContextMenu;
                MenuItem item = LogicalTreeHelper.FindLogicalNode(menu, "TrayStartup") as MenuItem;
                item.IsChecked = check;
            }
        }

        private void trayIcon_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ContextMenu menu = (ContextMenu)this.FindResource("TrayMenu");
                menu.IsOpen = true;
            }
        }

        private void TrayIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }

        private void Winodw_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                trayIcon.BalloonTipTitle = "Minimize Successful";
                trayIcon.BalloonTipText = "Minimize the app";
                trayIcon.ShowBalloonTip(400);
                trayIcon.Visible = true;
            }
            else if (this.WindowState == WindowState.Normal)
            {
                trayIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        private void HandleMessages(object sender, DoWorkEventArgs e)
        {
            byte[] msg = new byte[1];
            while (true)
            {
                byte[] data = SocketListener.recv();
                switch(data[0])
                {
                    case 100:
                        if (started)
                        {
                            msg[0] = 50;
                            break;
                        }
                        byte[] timeByte = new byte[4];
                        mode = (Mode)data[1];
                        Array.Copy(data, 2, timeByte, 0, 4);
                        Array.Reverse(timeByte);
                        time = BitConverter.ToInt32(timeByte, 0);
                        msg[0] = 200;
                        background.ReportProgress(0);
                        break;
                    case 150:
                        if (!started)
                        {
                            msg[0] = 50;
                            break;
                        }
                        background.ReportProgress(1);
                        break;
                }

                SocketListener.Sent(msg);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (time <= 0)
            {
                timer.Stop();
                execute();
            }

            if (time <= 10)
            {
                TBCountDown.Foreground = Brushes.Red;

            }
            else
            {
                TBCountDown.Foreground = Brushes.Black;
            }
            int sec = time % 60;
            int min = (time / 60) % 60;
            int hr = time / 3600;
            string timeFor = string.Format("{0}:{1}:{2}", 
                hr < 10 ? "0" + hr.ToString() : hr.ToString(),
                min < 10 ? "0" + min.ToString() : min.ToString(), 
                sec < 10 ? "0" + sec.ToString() : sec.ToString());
            TBCountDown.Text = timeFor;
            time--;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rad = (RadioButton)sender;
            switch (rad.Name)
            {
                case "Shut":
                    mode = Mode.Shutdown;
                    break;
                case "Res":
                    mode = Mode.Restart;
                    break;
                case "Hib":
                    mode = Mode.Hibrnate;
                    break;
            }
        }



        private void execute()
        {
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            switch(mode)
            {
                case Mode.Shutdown:
                    startInfo.Arguments = "/C shutdown /s /hybrid";
                    break;
                case Mode.Restart:
                    startInfo.Arguments = "/C shutdown /r";
                    break;
                case Mode.Hibrnate:
                    startInfo.Arguments = "/C shutdown /h";
                    break;
            }

            process.StartInfo = startInfo;
            process.Start();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            
            int seconds = !IsTextAllowed(S.Text) || S.Text == "" ? 0 : int.Parse(S.Text);
            int minuts = !IsTextAllowed(M.Text) || M.Text == "" ? 0 : int.Parse(M.Text) * 60;
            int hours = !IsTextAllowed(H.Text) || H.Text == "" ? 0 : int.Parse(H.Text) * 3600;
            time = hours + minuts + seconds;
            H.IsEnabled = false;
            M.IsEnabled = false;
            S.IsEnabled = false;
            timer.Start();
            start.Content = "Stop!";
            start.Click -= Start_Click;
            start.Click += Stop_Click;
            started = true;
        }

        private void Start_Click(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 1)
            {
                Stop_Click();
                return;
            }
            H.IsEnabled = false;
            M.IsEnabled = false;
            S.IsEnabled = false;
            timer.Start();
            start.Content = "Stop!";
            start.Click -= Start_Click;
            start.Click += Stop_Click;
            started = true;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            H.IsEnabled = true;
            M.IsEnabled = true;
            S.IsEnabled = true;
            H.Text = "0";
            M.Text = "0";
            S.Text = "0";
            start.Content = "Start!";
            start.Click += Start_Click;
            start.Click -= Stop_Click;
            started = false;
        }

        private void Stop_Click()
        {
            timer.Stop();
            H.IsEnabled = true;
            M.IsEnabled = true;
            S.IsEnabled = true;
            H.Text = "0";
            M.Text = "0";
            S.Text = "0";
            start.Content = "Start!";
            start.Click += Start_Click;
            start.Click -= Stop_Click;
            started = false;
        }

        private new void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static readonly Regex _regex = new Regex("[^0-9]+");
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void Show_IP(object sender, RoutedEventArgs e)
        {
            string ip = SocketListener.getServerIp();
            ip = ip.Substring(0,ip.IndexOf(':'));
            MessageBox.Show(ip, "IP", MessageBoxButton.OK);
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Help(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("For Starting local Shutdown:\n" +
                            "Pick the length of the timer (H:M:S) and a shutdown option\n\n" +
                            "For Starting Remote Shutdown:\n" +
                            "In the app chose the time as above and a shutdown option\n" +
                            "Enter the ip of the computer you want to shutdown\n (Can be seen in File -> Show IP or icon tray -> Show IP)", 
                            "Help", MessageBoxButton.OK);
        }

        private void Startup(object sender, RoutedEventArgs e)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue("RemoteShutdownServer", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
            }
            DockStartup.IsChecked = true;
            ContextMenu menu = this.Resources["TrayMenu"] as ContextMenu;
            MenuItem item = LogicalTreeHelper.FindLogicalNode(menu, "TrayStartup") as MenuItem;
            item.IsChecked = true;
        }

        private void UStartup(object sender, RoutedEventArgs e)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.DeleteValue("RemoteShutdownServer", false);
            }
            DockStartup.IsChecked = false;
            ContextMenu menu = this.Resources["TrayMenu"] as ContextMenu;
            MenuItem item = LogicalTreeHelper.FindLogicalNode(menu, "TrayStartup") as MenuItem;
            item.IsChecked = true;
        }
    }
}
