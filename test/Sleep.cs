using LinePutScript;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;
using LinePutScript.Converter;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Shapes;
using System.Text;
using System.Timers;
using static System.Net.Mime.MediaTypeNames;
using static VPet_Simulator.Core.GraphInfo;
using static VPet_Simulator.Core.ToolBar;
using System.Windows.Media;
using System.Reflection.Emit;
using System.Security.AccessControl;

namespace VPET.Evian.Sleep
{
    public class Variable 
    {
        public string Name { get; set; }
        /// <summary>
        /// 初始体力上限
        /// </summary>
        private double StrengthMax = new double();
        /// <summary>
        /// 当前体力上限
        /// </summary>
        private double StrengthNow = new double();
        /// <summary>
        /// 是否需要睡觉
        /// </summary>
        private bool IsSleep = false;
        /// <summary>
        /// 是否显示过提示话语0
        /// </summary>
        private bool IsSay0 = false;
        /// <summary>
        /// 是否显示过提示话语1
        /// </summary>
        private bool IsSay1 = false;
        /// <summary>
        /// 设置体力上限
        /// </summary>
        public void SetSM(double value)
        {
            StrengthMax = value;
        }
        /// <summary>
        /// 获取体力上限
        /// </summary>
        public double GetSM() 
        {
            return StrengthMax;
        }
        /// <summary>
        /// 设置当前体力上限
        /// </summary>
        public void SetNow(double value)
        {
            StrengthNow = value;
        }
        /// <summary>
        /// 修改当前体力上限
        /// </summary>
        public void ChangeNow(double value)
        {
            StrengthNow += value;
        }
        /// <summary>
        /// 获取当前体力上限
        /// </summary>
        public double GetNow()
        {
            return StrengthNow;
        }
        /// <summary>
        /// 设置是否需要睡觉
        /// </summary>
        public void SetIS(bool value)
        {
            IsSleep = value;
        }
        /// <summary>
        /// 获取是否需要睡觉
        /// </summary>
        public bool GetIS()
        {
            return IsSleep;
        }
        /// <summary>
        /// 是否显示过提示话语0
        /// </summary>
        public void SetISy0(bool value)
        {
            IsSay0 = value;
        }
        /// <summary>
        /// 是否显示过提示话语0
        /// </summary>
        public bool GetISy0()
        {
            return IsSay0;
        }
        /// <summary>
        /// 是否显示过提示话语1
        /// </summary>
        public void SetISy1(bool value)
        {
            IsSay1 = value;
        }
        /// <summary>
        /// 是否显示过提示话语1
        /// </summary>
        public bool GetISy1()
        {
            return IsSay1;
        }
    }
    public class Sleep : MainPlugin
    {
        Variable variable = new Variable();
        TextBlock SleepyTextBlock;
        TextBlock SleepyChange;
        ProgressBar SleepyProgressBarMax;
        private double timepass = new double();
        private string percent = new string("");
        private Main mmain;
        private double worlvalue = 0.0;
        private double lastsleepy = 0.0;
        public override string PluginName => "Sleep";

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public Sleep(IMainWindow mainwin) : base(mainwin)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
        }
        /// <summary>
        /// 当桌宠开启，加载mod的时候会调用这个函数
        /// </summary>
        public override void LoadPlugin()
        {
            variable.SetSM(MW.GameSavesData.GameSave.StrengthMax);

            if (MW.GameSavesData["Sleep"].GetString("StrengthNow") != null)
                variable.SetNow(MW.GameSavesData["Sleep"][(gdbe)"StrengthNow"]);
            else
                MW.GameSavesData["Sleep"][(gdbe)"StrengthNow"] = variable.GetSM();
            Grid grid = MW.Main.ToolBar.gdPanel;
            timepass = (0.0417-MW.GameSavesData.GameSave.Level / (1100* 100)) * MW.GameSavesData.GameSave.StrengthMax / 100 ;
            worlvalue = 0.256 * 0.0417 * 0.03;
            AddRow(grid);
            DispatcherTimer mUItimer = new DispatcherTimer();
            mUItimer.Interval = new TimeSpan(0, 0, 0, 5, 3);
            mUItimer.Tick += new EventHandler(MUItimer);
            mUItimer.Start();
            lastsleepy = variable.GetNow();
            mmain = MW.Main;
            ///base.LoadPlugin();
        }

        private void MUItimer(object? sender, EventArgs e)
        {
            ChangeStrength(mmain);
            SleepyChangeUI(mmain);
            StrengthFunctionUI(mmain);
        }

        public winSetting winSetting;
        public override void Setting()
        {
            if (winSetting == null)
            {
                winSetting = new winSetting(this);
                winSetting.Show();
            }
            else
            {
                winSetting.Topmost = true;
            }
        }
        /// <summary>
        /// 改变UI进度条
        /// </summary>
        private void SleepyChangeUI(Main main)
        {
            var TimePass = (variable.GetNow() - lastsleepy)/variable.GetSM();
            lastsleepy = variable.GetNow();
            TimePass = 20000 * TimePass;
            if (Math.Abs(TimePass) > 0.1)
                SleepyChange.Text = $"{TimePass:f2}/kt";
            else if (Math.Abs(TimePass) > 0.01)
                SleepyChange.Text = $"{TimePass:f3}/kt";
            else 
                SleepyChange.Text = $"{TimePass:f4}/kt";
            var value = 1 - variable.GetNow() / variable.GetSM();
            value = 100 * value;
            SleepyProgressBarMax.Value = value;
        }
        /// <summary>
        /// 添加状态条
        /// </summary>
        /// <param name="grid">添加位置</param>
        private void AddRow(Grid grid)
        {
            SleepyTextBlock = new TextBlock();
            SleepyChange = new TextBlock();
            SleepyProgressBarMax = new ProgressBar();
            grid.RowDefinitions.Add(new RowDefinition());
            int newRowIndex = grid.RowDefinitions.Count - 1;
            ///说明文本
            SleepyTextBlock.Text = "困意值".Translate();
            grid.Children.Add(SleepyTextBlock);
            Grid.SetRow(SleepyTextBlock, newRowIndex);
            Grid.SetColumn(SleepyTextBlock, 0);
            ///最大值的条
            SleepyProgressBarMax.Height = 20;
            SleepyProgressBarMax.VerticalAlignment = VerticalAlignment.Center;
            ProgressBarHelper.SetCornerRadius(SleepyProgressBarMax, new CornerRadius(10));
            SleepyProgressBarMax.Background = null;
            SleepyProgressBarMax.FontSize = 20;
            SleepyProgressBarMax.Foreground = GetForeground(1 - (variable.GetNow() / variable.GetSM()));
            SleepyProgressBarMax.Maximum = 100;
            SleepyProgressBarMax.Opacity = 1;
            var value = 1 - (variable.GetNow() / variable.GetSM());
            value = 100 * value;
            SleepyProgressBarMax.Value = value;
            SleepyProgressBarMax.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEEEEE"));
            SleepyProgressBarMax.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BCBCBC"));
            ProgressBarHelper.AddGeneratingPercentTextHandler(SleepyProgressBarMax, PgbSleepy_GeneratingPercentText);
            ProgressBarHelper.SetIsPercentVisible(SleepyProgressBarMax,true);
            grid.Children.Add(SleepyProgressBarMax);
            Grid.SetRow(SleepyProgressBarMax, newRowIndex);
            Grid.SetColumn(SleepyProgressBarMax, 2);
            ///改变文本
            SleepyChange.Text = $"0.00/t";
            SleepyChange.HorizontalAlignment = HorizontalAlignment.Right;
            SleepyChange.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#039BE5"));
            grid.Children.Add(SleepyChange);
            Grid.SetRow(SleepyChange, newRowIndex);
            Grid.SetColumn(SleepyChange, 4);
        }
        /// <summary>
        /// 改变显示的变化值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PgbSleepy_GeneratingPercentText(object sender, GeneratingPercentTextRoutedEventArgs e)
        {
            var progressBar = (ProgressBar)sender;
            progressBar.Foreground = GetForeground(1-(variable.GetNow() / variable.GetSM()));
            var value = 1 - (variable.GetNow() / variable.GetSM());
            value = 100 * value;
            e.Text = $"{value:f2} / {100:f0}";
        }
        /// <summary>
        /// 改变困意值
        /// </summary>
        private void ChangeStrength(Main main)
        {
            if (!MW.Core.Controller.EnableFunction)
            {
                return;
            }
            variable.SetSM(MW.GameSavesData.GameSave.StrengthMax);
            Random random = new Random(DateTime.Now.Millisecond);
            var ransub = 1.0 * ((double)random.Next() % 5 - 2.5) / 100;
            if (MW.Main.State == Main.WorkingState.Nomal)
            {
                StrengthChange(-timepass * (1 + ransub));
            }
            else if (MW.Main.State == Main.WorkingState.Sleep)
            {
                StrengthChange(5 * timepass * (1 + ransub));
            }
            else if (MW.Main.State == Main.WorkingState.Work && MW.Main.NowWork.Type != GraphHelper.Work.WorkType.Play)
            {
                var SCWork = 1.0 * (Math.Abs(MW.Main.NowWork.Feeling) + MW.Main.NowWork.StrengthDrink + MW.Main.NowWork.StrengthFood) / 3;
                StrengthChange(-(timepass * (1 + ransub) + SCWork * worlvalue));
            }
            else if (MW.Main.State == Main.WorkingState.Work && MW.Main.NowWork.Type == GraphHelper.Work.WorkType.Play)
            {
                var SCWork = 1.0 * (-Math.Abs(MW.Main.NowWork.Feeling) + MW.Main.NowWork.StrengthDrink + MW.Main.NowWork.StrengthFood) / 3;
                StrengthChange(-(timepass * (1 + ransub) + SCWork * worlvalue));
            }
        }
        private void StrengthFunctionUI(Main main)
        {
            StrengthFunction(0.05);
        }
        /// <summary>
        /// 改变困意值
        /// </summary>
        /// <param name="TimePass">改变的值</param>
        private void StrengthChange(double TimePass)
        {
            variable.SetSM(MW.GameSavesData.GameSave.StrengthMax);
            if ((variable.GetNow() + TimePass) <= variable.GetSM() && (variable.GetNow() + TimePass) >= 0) 
            {
                variable.ChangeNow(TimePass);
                MW.GameSavesData["Sleep"][(gdbe)"StrengthNow"] = variable.GetNow();
            }
            
        }
        /// <summary>
        /// 获取刷子
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private Brush GetForeground(double value)
        {
            if (value <= 0.3)
            { 
                return MW.Main.FindResource("SuccessProgressBarForeground") as Brush;
            }

            if (value <= 0.8)
            {
                return MW.Main.FindResource("WarningProgressBarForeground") as Brush;
            }

            return MW.Main.FindResource("DangerProgressBarForeground") as Brush;
        }
        /// <summary>
        /// 困意值功能
        /// </summary>
        /// <param name="TimePass"></param>
        private void StrengthFunction(double TimePass)
        {
            double freedrop = (DateTime.Now - MW.Main.LastInteractionTime).TotalMinutes;
            if (freedrop < 1)
                freedrop = 0;
            else
                freedrop = Math.Min(Math.Sqrt(freedrop) * TimePass / 4, MW.GameSavesData.GameSave.FeelingMax / 800);
            if (MW.Main.State != Main.WorkingState.Sleep)
            {
                if (variable.GetNow() / variable.GetSM() >= 0.7)
                {
                    if ((MW.GameSavesData.GameSave.Feeling + 2 * freedrop) < 100) 
                    {
                        MW.GameSavesData.GameSave.FeelingChange(2 * freedrop);
                    }
                    if (MW.GameSavesData.GameSave.Health <= 2 * TimePass + MW.GameSavesData.GameSave.Health) 
                    {
                        MW.GameSavesData.GameSave.Health += Function.Rnd.Next(0, 2) * TimePass;
                    }
                    if (MW.GameSavesData.GameSave.Health > 100)
                    {
                        MW.GameSavesData.GameSave.Health = 100;
                    }
                }
                else if (variable.GetNow() / variable.GetSM() >= 0.5)
                {
                    variable.SetIS(false);
                    variable.SetISy0(false);
                    variable.SetISy1(false);
                }
                else if (variable.GetNow() / variable.GetSM() >= 0.4)
                {
                    if (variable.GetIS() == false)
                    {
                        MW.Main.Say("主人，我有点困了".Translate());
                        variable.SetIS(true);
                    }
                }
                else if (variable.GetNow() / variable.GetSM() >= 0.2)
                {
                    var feelingdouble = Function.Rnd.Next(0, 3) / 10.0;
                    if ((MW.GameSavesData.GameSave.Feeling - feelingdouble * freedrop) > 0)
                    {
                        MW.GameSavesData.GameSave.FeelingChange(-(feelingdouble * freedrop));
                    }
                    if (variable.GetISy0() == false)
                    {
                        variable.SetIS(false);
                        MW.Main.Say("主人，我睡一会".Translate());
                        variable.SetISy0(true);
                        variable.SetIS(true);
                        if (MW.Main.State == Main.WorkingState.Work)
                        {
                            MW.Main.WorkTimer.Stop();
                        }
                        MW.Main.DisplaySleep(true);
                        return;
                    }                    
                }
                else if (variable.GetNow() / variable.GetSM() >= 0.1)
                {
                    var feelingdouble = Function.Rnd.Next(5, 15) / 10.0;
                    if ((MW.GameSavesData.GameSave.Feeling - feelingdouble * freedrop) > 0)
                    {
                        MW.GameSavesData.GameSave.FeelingChange(-(feelingdouble * freedrop));
                    }
                    if (MW.GameSavesData.GameSave.Health - 2 * TimePass > 0) 
                    {
                        MW.GameSavesData.GameSave.Health -= Function.Rnd.Next(0, 2) * TimePass;
                    }
                    if (variable.GetIS() == false)
                    {
                        MW.Main.Say("主人，我睡了".Translate());
                        variable.SetIS(true);
                        if (MW.Main.State == Main.WorkingState.Work)
                        {
                            MW.Main.WorkTimer.Stop();
                        }
                        MW.Main.DisplaySleep(true);
                        return;
                    }
                    if (variable.GetISy1() == false) 
                    {
                        MW.Main.Say("主人，我感觉有点心悸".Translate());
                        variable.SetISy1(true);
                    }
                }
                else
                {
                    if (MW.Main.State == Main.WorkingState.Work)
                    {
                        MW.Main.WorkTimer.Stop();
                    }
                    MW.Main.DisplaySleep(true);
                }
            }
            else
            {
                if(variable.GetNow() / variable.GetSM() >= 0.95)
                {
                    if (MW.Main.State == Main.WorkingState.Sleep)
                    {
                        MW.Main.State = Main.WorkingState.Nomal;
                        MW.Main.DisplayToNomal();
                    }
                }
                if (variable.GetNow() / variable.GetSM() >= 0.7)
                {
                    if ((MW.GameSavesData.GameSave.Feeling + 2 * freedrop) < 100)
                    {
                        MW.GameSavesData.GameSave.FeelingChange(2 * freedrop);
                    }
                    if (MW.GameSavesData.GameSave.Health <= 2 * TimePass + MW.GameSavesData.GameSave.Health)
                    {
                        MW.GameSavesData.GameSave.Health += Function.Rnd.Next(0, 2) * TimePass;
                    }
                }
                else if (variable.GetNow() / variable.GetSM() >= 0.5)
                {
                    variable.SetIS(false);
                    variable.SetISy0(false);
                    variable.SetISy1(false);
                }
            }
        }
    }
}

