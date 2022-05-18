using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using wpf_UWB_GUI.Listener;

namespace wpf_UWB_GUI
{
    /// <summary>
    /// UC_main_gateway_com.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_main_listener_setting : Page
    {
        String TAG = "UC_main_listener_setting.xaml";

        public UC_main_listener_setting()
        {
            Console.WriteLine("UC_main_listener_setting()");

            InitializeComponent();
        }

        private void UC_main_gateway_setting_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("UC_main_gateway_com_Loaded()");

        }

        private void timer10hz_Tick(object sender, EventArgs e)
        {

        }
    }
}
