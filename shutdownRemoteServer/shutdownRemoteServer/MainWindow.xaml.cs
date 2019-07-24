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

        private System.Diagnostics.Process process;
        private System.Diagnostics.ProcessStartInfo startInfo;
        private BackgroundWorker background = new BackgroundWorker();
        private bool started = false;


        public MainWindow()
        {
            InitializeComponent();
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
            Thread t1 = new Thread(new ThreadStart(HandleMessages));
            t1.IsBackground = true;
            t1.Start();
            background.WorkerReportsProgress = true;
            background.DoWork += Background_DoWork;
            background.ProgressChanged += Start_Click;
            background.RunWorkerAsync();
        }

        private void HandleMessages()
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

        private void Background_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true) ;
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

        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
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
    }
}
