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

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            vts.winSetting = null;
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
