using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;
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


            TBCountDown.Text = string.Format("{0}:{1}:{2}", time/3600, (time / 60) % 60, time % 60);
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
            switch(mode)
            {
                case Mode.Shutdown:
                    break;
                case Mode.Restart:
                    break;
                case Mode.Hibrnate:
                    break;
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            int seconds = int.Parse(S.Text) * 60;
            int minuts = int.Parse(M.Text) * 60;
            int hours = int.Parse(H.Text);
            seconds = seconds == 0 ? 1 : seconds;
            minuts = minuts == 0 ? 1 : minuts;
            hours = hours == 0 ? 1 : hours;
            time = hours * minuts * seconds;
            H.IsEnabled = false;
            M.IsEnabled = false;
            S.IsEnabled = false;
            timer.Start();
            start.Content = "Stop!";
            start.Click += Stop_Click;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            H.IsEnabled = true;
            M.IsEnabled = true;
            S.IsEnabled = true;
            H.Text = "Hours";
            M.Text = "Minutes";
            S.Text = "Seconds";
            start.Content = "Start!";
            start.Click += Start_Click;
        }
    }
}
