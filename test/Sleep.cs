using LinePutScript;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;

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
        public TextBlock SleepyChange;
        ProgressBar SleepyProgressBarMax;
        public double timepass = new double();
        private string percent = new string("");
        private Main mmain;
        private double worlvalue = 0.0;
        private double LastValue = 0.0;
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
        public bool Mode = false;
        State mstate = State.Sleep;
        public double AwakeHour = 2.8;
        public double SleepHour = 0.2;
        public bool Enable = true;
        private double Value = 5; 
        public DispatcherTimer mUItimer;
        private double FoodChange = 0;
        private bool FoodErr = false;
        private bool FoodErrMax = false;
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

            if (!string.IsNullOrEmpty(MW.GameSavesData["Sleep"].GetString("Value")))
                Value = MW.GameSavesData["Sleep"][(gdbe)"Value"];
            else
                MW.GameSavesData["Sleep"][(gdbe)"Value"] = Value;
            if (!string.IsNullOrEmpty(MW.GameSavesData["Sleep"].GetString("Enable")))
                Enable = MW.GameSavesData["Sleep"][(gbol)"Enable"];
            else
                Enable = true;
            if (!string.IsNullOrEmpty(MW.GameSavesData["Sleep"].GetString("Multiple")))
            {
                variable.SetMul(MW.GameSavesData["Sleep"][(gint)"Multiple"]);
            }
            else
            {
                MW.GameSavesData["Sleep"][(gint)"Multiple"] = variable.GetMul();
            }
            if (!string.IsNullOrEmpty(MW.GameSavesData["Sleep"].GetString("Mode")))
            {
                Mode = MW.GameSavesData["Sleep"][(gbol)"Mode"];
            }
            else
            {
                MW.GameSavesData["Sleep"][(gbol)"Mode"] = false;
            }
            if (!string.IsNullOrEmpty(MW.GameSavesData["Sleep"].GetString("AwakeHour")))
            {
                AwakeHour = MW.GameSavesData["Sleep"][(gdbe)"AwakeHour"];
            }
            else
            {
                MW.GameSavesData["Sleep"][(gdbe)"AwakeHour"] = AwakeHour;
            }
            if (!string.IsNullOrEmpty(MW.GameSavesData["Sleep"].GetString("SleepHour")))
            {
                SleepHour = MW.GameSavesData["Sleep"][(gdbe)"SleepHour"];
            }
            else
            {
                MW.GameSavesData["Sleep"][(gdbe)"SleepHour"] = SleepHour;
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
            mUItimer = new DispatcherTimer();
            mUItimer.Interval = new TimeSpan(0, 0, 0, 5);
            mUItimer.Tick += new EventHandler(MUItimer);
            mUItimer.Tick += new EventHandler(MTexttimer);
            if(Enable)
            {
                mUItimer.Start();
            }
            if(Value < 0)
            {
                Value = 5;
            }
            variable.SetNow((1 - Value / 100) * variable.GetSM());
            LastValue = Value;
            mmain = MW.Main;
            MW.Event_TakeItem += MTakeItemEvent;
            ///base.LoadPlugin();
        }
        public void MTakeItemEvent(Food food)
        {
            if (food == null)
            {
                return;
            }
            if (food.Type == Food.FoodType.Functional)
            {
                var Strength = food.Strength;
                while (Strength > 8.5)
                {
                    Strength /= 10;
                }
                FoodChange += Strength;
            }
        }
        private void MUItimer(object? sender, EventArgs e)
        {
            if(Mode)
            {
                ChangeStrengthTrue(mmain);
            }
            else
            {
                ChangeStrength(mmain);
            }
            SleepyChangeUI(mmain);
            StrengthFunctionUI(mmain);
            Punish();
        }
        private void Punish()
        {
            if (FoodChange < 0) 
            {
                FoodChange = 0;
            }
            if(FoodChange > 50)
            {
                FoodChange -= 1;
                MW.GameSavesData.GameSave.Health -= 0.1;
                if(!FoodErr)
                {
                    FoodErr = true;
                    MW.Main.SayRnd("感觉有点不舒服，是不是摄入太多功能性产品了".Translate());
                }
            }
            if(FoodChange > 100)
            {
                MW.GameSavesData.GameSave.Health -= 2;
                MW.GameSavesData.GameSave.StrengthFood -= 0.06 * MW.GameSavesData.GameSave.StrengthMax;
                MW.GameSavesData.GameSave.StrengthDrink -= 0.06 * MW.GameSavesData.GameSave.StrengthMax;
                FoodChange -= 20;
                MW.GameSavesData.GameSave.Mode = IGameSave.ModeType.PoorCondition;
                if (!FoodErrMax)
                {
                    FoodErrMax = true;
                    MW.Main.Say("呕...感觉好难受...下次再也不吃这么多功能性产品了".Translate(), "squat");
                }
            }
            if(FoodChange < 10)
            {
                FoodErr = false;
                FoodErrMax = false;
            }
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
            var TimePass = LastValue - Value;
            LastValue = Value;
            TimePass = -100 * TimePass;
            if (Math.Abs(TimePass) > 0.1)
                SleepyChange.Text = $"{TimePass:f2}/ht";
            else if (Math.Abs(TimePass) > 0.01)
                SleepyChange.Text = $"{TimePass:f3}/ht";
            else 
                SleepyChange.Text = $"{TimePass:f4}/ht";
            var value = Value;
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
            var value = Value;
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
            progressBar.Foreground = GetForeground(Value / 100);
            var value = Value;
            e.Text = $"{value:f2} / {100:f0}";
        }
        /// <summary>
        /// 改变困意值
        /// </summary>
        private void ChangeStrengthTrue(Main main)
        {
            if (!MW.Core.Controller.EnableFunction)
            {
                return;
            }
            var delnum = 0.0;
            if (MW.Main.State == Main.WorkingState.Nomal)
            {
                delnum = 5 / (36 * AwakeHour);
            }
            else if (MW.Main.State == Main.WorkingState.Sleep)
            {
                delnum = -5 / (36 * SleepHour);
            }
            else if (MW.Main.State == Main.WorkingState.Work && MW.Main.NowWork.Type != GraphHelper.Work.WorkType.Play)
            {
                var SCWork = 1.0 * (Math.Abs(MW.Main.NowWork.Feeling) + MW.Main.NowWork.StrengthDrink + MW.Main.NowWork.StrengthFood) / 3;
                SCWork *= 5 / (36 * AwakeHour) / 600;
                delnum = SCWork + 5 / (36 * AwakeHour);
            }
            else if (MW.Main.State == Main.WorkingState.Work && MW.Main.NowWork.Type == GraphHelper.Work.WorkType.Play)
            {
                var SCWork = 1.0 * (-Math.Abs(MW.Main.NowWork.Feeling) + MW.Main.NowWork.StrengthDrink + MW.Main.NowWork.StrengthFood) / 3;
                SCWork *= 5 / (36 * AwakeHour) / 600;
                delnum = SCWork + 5 / (36 * AwakeHour);
            }
            if (MW.Main.State == Main.WorkingState.Sleep)
            {
                delnum += FoodChange / 160 / SleepHour;
                FoodChange -= FoodChange / 480 / SleepHour;
            }
            else
            {
                delnum -= FoodChange / 32 /AwakeHour;
                FoodChange -= FoodChange / 96 /AwakeHour;
            }
            Value += delnum;
            if(Value < 0)
            {
                Value = 0;
            }    
            else if(Value > 100)
            {
                Value = 95;
            }
            MW.GameSavesData["Sleep"][(gdbe)"Value"] = Value;
        }
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
            var delnum = 0.0;
            if (MW.Main.State == Main.WorkingState.Sleep)
            {
                StrengthChange(-FoodChange / 16 / 5 / variable.GetMul());
                FoodChange -= FoodChange / 80 / 5 / variable.GetMul();
            }
            else
            {
                StrengthChange((FoodChange / 16) / variable.GetMul());
                FoodChange -= (FoodChange / 80) / variable.GetMul();
            }
            Value = 100 - variable.GetNow() / variable.GetSM() * 100;
            MW.GameSavesData["Sleep"][(gdbe)"Value"] = Value;
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
                if (Value <= 30)
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
                else if (Value <= 50)
                {
                    nstate = State.LSleepy;
                }
                else if (Value <= 60)
                {
                    nstate = State.MSleepy;
                }
                else if (Value <= 70)
                {
                    nstate = State.HSleepy;
                }
                else if (Value <= 90)
                {
                    nstate = State.NSleep;
                    var feelingdouble = 0.0;
                    if (Value <= 85)
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
                if (Value <= 5)
                {
                    nstate = State.NSleepy;
                }
                else if (Value <= 30)
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
                else if (Value <= 50)
                {
                    nstate = State.Sleep;
                }
            }
        }
        private void AutoSleep()
        {
            if (Value < 0 && Mode == true)
            {
                variable.SetNow(0.05 * variable.GetSM());
            }
            else if (Value > 100 && Mode == true) 
            {
                variable.SetNow(0.80 * variable.GetSM());
            }
            else if(Value < 0)
            {
                Value = 5;
            }
            else if(Value >100)
            {
                Value = 80;
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

