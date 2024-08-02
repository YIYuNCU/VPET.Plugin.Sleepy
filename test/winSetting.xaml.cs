using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using VPet_Simulator.Core;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Panuon.WPF.UI;
using System.Diagnostics;
using System.Windows.Shapes;
using LinePutScript;

namespace VPET.Evian.Sleep
{
    /// <summary>
    /// winSetting.xaml 的交互逻辑
    /// </summary>
    public partial class winSetting : Window
    {
        Sleep vts;

        public winSetting(Sleep vts)
        {///前两行代码不可缺少
            InitializeComponent();
            this.vts = vts;   
            Mul.Text = vts.variable.GetMul().ToString();
            Mode.IsChecked = vts.Mode;
            SwitchOn.IsChecked = vts.Enable;
            if(vts.Mode)
            {
                SleepText.Visibility = Visibility.Visible;
                SleepTime.Visibility = Visibility.Visible;
                WakeText.Visibility = Visibility.Visible;
                WakeTime.Visibility = Visibility.Visible;
                SleepTime.Text = vts.SleepHour.ToString();
                WakeTime.Text = vts.AwakeHour.ToString();
            }
            else
            {
                Mul.Visibility = Visibility.Visible;
                MulText.Visibility = Visibility.Visible;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            vts.winSetting = null;
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (Mode.IsChecked.Value != vts.Mode)
            {
                vts.Mode = Mode.IsChecked.Value;
                vts.MW.GameSavesData["Sleep"][(gbol)"Mode"] = vts.Mode; 
            }
            if (Mode.IsChecked == true)
            {
                if (vts.AwakeHour.ToString() != WakeTime.Text.ToString() && WakeTime.Text.Length != 0) 
                {
                    vts.AwakeHour = Convert.ToDouble(WakeTime.Text);
                    vts.MW.GameSavesData["Sleep"][(gdbe)"AwakeHour"] = vts.AwakeHour;
                }
                if (vts.SleepHour.ToString() != SleepTime.Text.ToString() && SleepTime.Text.Length != 0)
                {
                    vts.SleepHour = Convert.ToDouble(SleepTime.Text);
                    vts.MW.GameSavesData["Sleep"][(gdbe)"SleepHour"] = vts.SleepHour;
                }
            }
            else if(Mul.Text != "")
            {
                if(Math.Abs(Convert.ToInt32(Mul.Text)) <= 1)
                {
                    vts.variable.SetMul(1);
                }
                else
                {
                    vts.variable.SetMul(Math.Abs(Convert.ToInt32(Mul.Text)));
                }
                vts.MW.GameSavesData["Sleep"][(gint)"Multiple"] = vts.variable.GetMul();
                vts.timepass = (0.0417 - vts.MW.GameSavesData.GameSave.Level / (1100 * 100)) * (vts.MW.GameSavesData.GameSave.StrengthMax / 100) / vts.variable.GetMul();
            }
            if(SwitchOn.IsChecked.Value != vts.Enable)
            {
                vts.Enable = SwitchOn.IsChecked.Value;
                vts.MW.GameSavesData["Sleep"][(gbol)"Enable"] = vts.Enable;
                if(vts.Enable)
                {
                    vts.mUItimer.Start();
                }
                else
                {
                    vts.mUItimer.Stop();
                    vts.SleepyChange.Text = $"0.0/ht";
                }
            }
            Close();
        }

        private void Mode_Checked(object sender, RoutedEventArgs e)
        {
            if(Mode.IsChecked == true)
            {
                SleepText.Visibility = Visibility.Visible;
                SleepTime.Visibility = Visibility.Visible;
                WakeText.Visibility = Visibility.Visible;
                WakeTime.Visibility = Visibility.Visible;
                SleepTime.Text = vts.SleepHour.ToString();
                WakeTime.Text = vts.AwakeHour.ToString();
                Mul.Visibility = Visibility.Collapsed;
                MulText.Visibility = Visibility.Collapsed;
            }
            else
            {
                SleepText.Visibility = Visibility.Collapsed;
                SleepTime.Visibility = Visibility.Collapsed;
                WakeText.Visibility = Visibility.Collapsed;
                WakeTime.Visibility = Visibility.Collapsed;
                Mul.Visibility = Visibility.Visible;
                MulText.Visibility = Visibility.Visible;
            }
        }

        private void WakeTime_TextChanged(object sender, RoutedEventArgs e)
        {
            if(Convert.ToDouble(WakeTime.Text) < 0.17)
            {
                WakeTime.Text = (0.17).ToString();
            }
        }

        private void SleepTime_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Convert.ToDouble(SleepTime.Text) < 0.17)
            {
                SleepTime.Text = (0.17).ToString();
            }
        }
    }
}
