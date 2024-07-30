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
using static VPet_Simulator.Core.GraphHelper;
using System.Windows.Navigation;
using System.Windows.Media.Converters;

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
        /// 一桌宠日的时间是3的多少倍
        /// </summary>
        private int multiple = 1;
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
        /// 设置倍数
        /// </summary>
        public void SetMul(int value)
        {
            multiple = value;
        }
        /// <summary>
        /// 获取倍数
        /// </summary>
        public int GetMul()
        {
            return multiple;
        }
    }
    enum State
    {
        NSleepy = 0,
        LSleepy = 1,
        MSleepy = 2,
        HSleepy = 3,
        NSleep = 4,
        Sleep = 5,
        Sleepless = 6
    };
    public class Sleep : MainPlugin
    {
        public Variable variable = new Variable();
        TextBlock SleepyTextBlock;
        TextBlock SleepyChange;
        ProgressBar SleepyProgressBarMax;
        public double timepass = new double();
        private string percent = new string("");
        private Main mmain;
        private double worlvalue = 0.0;
        private double lastsleepy = 0.0;
        public override string PluginName => "Sleep";
        /// <summary>
        /// 无困意值对话
        /// </summary>
        private List<ClickText> mnostext = new List<ClickText>();
        /// <summary>
        /// 低困意值对话
        /// </summary>
        private List<ClickText> mlstext = new List<ClickText>();
        /// <summary>
        /// 中困意值对话
        /// </summary>
        private List<ClickText> mmstext = new List<ClickText>();
        /// <summary>
        /// 高困意值对话
        /// </summary>
        private List<ClickText> mhstext = new List<ClickText>();
        /// <summary>
        /// 需要睡觉对话
        /// </summary>
        private List<ClickText> mnestext = new List<ClickText>();
        /// <summary>
        /// 梦话
        /// </summary>
        private List<ClickText> msltext = new List<ClickText>();
        /// <summary>
        /// 失眠抱怨
        /// </summary>
        private List<ClickText> msleepless = new List<ClickText>();
        /// <summary>
        /// 睡眠过多抱怨
        /// </summary>
        private List<ClickText> moversleep = new List<ClickText>();
        private long TextNum = 300;
        private long ERRNUM = 200;
        private bool SleepErrState = false;
        private State nstate = State.Sleep;
        private long sleeperr = -1;
        private long nsleeperr = -1;
        State mstate = State.Sleep;
        /// <summary>
        /// 文本文档
        /// </summary>
        public LpsDocument MTexts;
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

            if (!string.IsNullOrEmpty(MW.GameSavesData["Sleep"].GetString("StrengthNow")))
                variable.SetNow(MW.GameSavesData["Sleep"][(gdbe)"StrengthNow"]);
            else
                MW.GameSavesData["Sleep"][(gdbe)"StrengthNow"] = variable.GetSM();
            if (!string.IsNullOrEmpty(MW.GameSavesData["Sleep"].GetString("SleepErrState")))
                SleepErrState = MW.GameSavesData["Sleep"][(gbol)"SleepErrState"];
            else
                SleepErrState = false;
            if (!string.IsNullOrEmpty(MW.GameSavesData["Sleep"].GetString("Multiple")))
            {
                variable.SetMul(MW.GameSavesData["Sleep"][(gint)"Multiple"]);
            }
            else
            {
                MW.GameSavesData["Sleep"][(gint)"Multiple"] = variable.GetMul();
            }
            ///添加列表项
            MenuItem modset = MW.Main.ToolBar.MenuMODConfig;
            modset.Visibility = Visibility.Visible;
            var menuItem = new MenuItem()
            {
                Header = "Sleep".Translate(),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            menuItem.Click += (s, e) => { Setting(); };
            modset.Items.Add(menuItem);
            //////读取文本文件
            mlstext = MW.ClickTexts.FindAll(x => x.Working == "LowSleepyText");
            mmstext = MW.ClickTexts.FindAll(x => x.Working == "MidSleepyText");
            mhstext = MW.ClickTexts.FindAll(x => x.Working == "HighSleepyText");
            msltext = MW.ClickTexts.FindAll(x => x.Working == "SleepText");
            mnostext = MW.ClickTexts.FindAll(x => x.Working == "NoSleepyText");
            mnestext = MW.ClickTexts.FindAll(x => x.Working == "NeedSleepText");
            msleepless = MW.ClickTexts.FindAll(x => x.Working == "SleeplessText");
            moversleep = MW.ClickTexts.FindAll(x => x.Working == "OverSleepText");
            Grid grid = MW.Main.ToolBar.gdPanel;
            timepass = (0.0417-MW.GameSavesData.GameSave.Level / (1100* 100)) * (MW.GameSavesData.GameSave.StrengthMax / 100)/ variable.GetMul() ;
            worlvalue = 0.256 * 0.0417 * 0.03;
            AddRow(grid);
            DispatcherTimer mUItimer = new DispatcherTimer();
            mUItimer.Interval = new TimeSpan(0, 0, 0, 5, 3);
            mUItimer.Tick += new EventHandler(MUItimer);
            mUItimer.Tick += new EventHandler(MTexttimer);
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
        private void MTexttimer(object? sender, EventArgs e)
        {
            if (--ERRNUM <= 0) 
            {
                sleeperr = -1;
                nsleeperr = -1;
                SleepErrState = false;
                ERRNUM = 200;
            }
            if(SleepErrState == true)
            {
                return;
            }
            if(--TextNum <= 0 || mstate != nstate)
            {
                TextNum = 1000;
                mstate = nstate;
                /**********说话部分代码实现**************/
                Random random = new Random(DateTime.Now.Nanosecond);
                var rand = random.Next();

                switch (mstate)
                {
                    case State.NSleepy: if (mnostext.Count == 0) return; MW.Main.Say(mnostext[rand % mnostext.Count].TranslateText); break;
                    case State.LSleepy: if (mlstext.Count == 0) return; MW.Main.Say(mlstext[rand % mlstext.Count].TranslateText); break;
                    case State.MSleepy: if (mmstext.Count == 0) return; MW.Main.Say(mmstext[rand % mmstext.Count].TranslateText); break;
                    case State.HSleepy: if (mhstext.Count == 0) return; MW.Main.Say(mhstext[rand % mhstext.Count].TranslateText); break;
                    case State.NSleep: if (mnestext.Count == 0) return; MW.Main.Say(mnestext[rand % mnestext.Count].TranslateText); break;
                    case State.Sleep: if (msltext.Count == 0) return; MW.Main.Say(msltext[rand % msltext.Count].TranslateText); break;
                };
                /**********说话部分代码实现**************/
            }
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
            var TimePass = (lastsleepy - variable.GetNow())/variable.GetSM();
            lastsleepy = variable.GetNow();
            TimePass = 10000 * TimePass;
            if (Math.Abs(TimePass) > 0.1)
                SleepyChange.Text = $"{TimePass:f2}/ht";
            else if (Math.Abs(TimePass) > 0.01)
                SleepyChange.Text = $"{TimePass:f3}/ht";
            else 
                SleepyChange.Text = $"{TimePass:f4}/ht";
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
        /// <summary>
        /// 困意值功能UI
        /// </summary>
        /// <param name="main"></param>
        private void StrengthFunctionUI(Main main)
        {
            StrengthFunction(0.05);
            AutoSleep();
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
            if (value <= 0.5)
            { 
                return MW.Main.FindResource("SuccessProgressBarForeground") as Brush;
            }

            if (value <= 0.85)
            {
                return MW.Main.FindResource("WarningProgressBarForeground") as Brush;
            }

            return MW.Main.FindResource("DangerProgressBarForeground") as Brush;
        }
        /// <summary>
        /// 困意值奖惩数值
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
                    nstate = State.NSleepy;
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
                    nstate = State.LSleepy;
                }
                else if (variable.GetNow() / variable.GetSM() >= 0.4)
                {
                    nstate = State.MSleepy;
                }
                else if (variable.GetNow() / variable.GetSM() >= 0.2)
                {
                    nstate = State.HSleepy;
                }
                else if (variable.GetNow() / variable.GetSM() >= 0.1)
                {
                    nstate = State.NSleep;
                    var feelingdouble = 0.0;
                    if (variable.GetNow() / variable.GetSM() >= 0.15)
                    {
                        feelingdouble = Function.Rnd.Next(0, 3) / 10.0;
                        if ((MW.GameSavesData.GameSave.Feeling - feelingdouble * freedrop) > 0)
                        {
                            MW.GameSavesData.GameSave.FeelingChange(-(feelingdouble * freedrop));
                        }
                    }
                    else
                    {
                        feelingdouble = Function.Rnd.Next(5, 15) / 10.0;
                        if ((MW.GameSavesData.GameSave.Feeling - feelingdouble * freedrop) > 0)
                        {
                            MW.GameSavesData.GameSave.FeelingChange(-(feelingdouble * freedrop));
                        }
                        if (MW.GameSavesData.GameSave.Health - 2 * TimePass > 0)
                        {
                            MW.GameSavesData.GameSave.Health -= Function.Rnd.Next(0, 2) * TimePass;
                        }
                    }
                }
                else
                {
                    nstate = State.Sleep;
                }
            }
            else
            {
                if (variable.GetNow() / variable.GetSM() >= 0.95)
                {
                    nstate = State.NSleepy;
                }
                else if (variable.GetNow() / variable.GetSM() >= 0.7)
                {
                    nstate = State.Sleep;
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
                    nstate = State.Sleep;
                }
            }
        }
        private void AutoSleep()
        {
            if (variable.GetNow() / variable.GetSM() < -5)
            {
                variable.SetNow(0.05 * variable.GetSM());
            }
            else if (variable.GetNow() / variable.GetSM() > 105) 
            {
                variable.SetNow(0.80 * variable.GetSM());
            }
            if(SleepErrState == true && MW.Main.State == Main.WorkingState.Sleep) 
            {
                Random random = new Random(DateTime.Now.Nanosecond);
                var rand = random.Next();
                MW.Main.Say(msleepless[rand % msleepless.Count].Text);
                MW.Main.State = Main.WorkingState.Nomal;
                MW.Main.DisplayCEndtoNomal("sleep");
                return;
            }
            else if(SleepErrState == true)
            {
                return;
            }
            if(nstate == State.NSleep)
            {
                if(MW.Main.State != Main.WorkingState.Work && MW.Main.State != Main.WorkingState.Sleep) 
                {
                    sleeperr += 1;
                    if (sleeperr >= 3)
                    {
                        if(SleepErrState == false)
                        {
                            Random random = new Random(DateTime.Now.Nanosecond);
                            var rand = random.Next();
                            MW.Main.Say(msleepless[rand % msleepless.Count].Text);
                            SleepErrState = true;
                        }
                        return;
                    }
                    MW.Main.State = Main.WorkingState.Sleep;
                    MW.Main.DisplaySleep(true);
                    nstate = State.Sleep;
                }
            }
            else if(nstate == State.Sleep) 
            {
                if(MW.Main.State != Main.WorkingState.Sleep) 
                {
                    sleeperr += 1;
                    if (MW.Main.State == Main.WorkingState.Work)
                    {
                        MW.Main.WorkTimer.Stop();
                    }
                    if (sleeperr >= 3)
                    {
                        if (SleepErrState == false)
                        {
                            Random random = new Random(DateTime.Now.Nanosecond);
                            var rand = random.Next();
                            MW.Main.Say(msleepless[rand % msleepless.Count].Text);
                            SleepErrState = true;
                        }
                        return;
                    }
                    MW.Main.State = Main.WorkingState.Sleep;
                    MW.Main.DisplaySleep(true);
                }
            }
            if(mstate == State.NSleepy)
            {
                if (MW.Main.State == Main.WorkingState.Sleep)
                {
                    nsleeperr += 1;
                    Random random = new Random(DateTime.Now.Nanosecond);
                    var rand = random.Next();
                    MW.Main.State = Main.WorkingState.Nomal;
                    MW.Main.DisplayCEndtoNomal("sleep");
                    nstate = State.NSleepy;
                    if (moversleep.Count <= 0) return;
                    if(nsleeperr > 0) MW.Main.Say(moversleep[rand % moversleep.Count].Text);
                }
            }
            else if(mstate == State.LSleepy) 
            {
                if (MW.Main.State == Main.WorkingState.Sleep)
                {
                    Random random = new Random(DateTime.Now.Nanosecond);
                    var rand = random.Next(1000);
                    if (rand >= 900)
                    {
                        nsleeperr += 1;
                        if (nsleeperr > 0) MW.Main.Say(moversleep[rand % moversleep.Count].Text);
                        MW.Main.State = Main.WorkingState.Nomal;
                        MW.Main.DisplayCEndtoNomal("sleep");
                        nstate = State.LSleepy;
                    }
                }
            }
        }


    }
}

