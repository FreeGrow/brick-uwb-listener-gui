using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public partial class UC_main_listener_info : Page
    {
        String TAG = "UC_main_listener_info.xaml";

        public UC_main_listener_info()
        {
            InitializeComponent();

            Hyperlink hyperlink = new Hyperlink();
            hyperlink.Inlines.Clear();
            hyperlink.Inlines.Add("https://www.robofrien.com/");
            hyperlink.Click += Hyperlink_homePage_Click;
            label_homepage.Content = hyperlink;

            Hyperlink hyperlink2 = new Hyperlink();
            hyperlink2.Inlines.Clear();
            hyperlink2.Inlines.Add("https://blog.naver.com/inc_robo");
            hyperlink2.Click += Hyperlink_blog_Click;
            label_blog.Content = hyperlink2;
        }

        private void UC_main_listener_info_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void timer10hz_Tick(object sender, EventArgs e)
        {

        }
        private void Hyperlink_homePage_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("Chrome.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Minimized;
            startInfo.Arguments = "https://www.robofrien.com/";
            Process.Start(startInfo);
        }
        private void Hyperlink_blog_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("Chrome.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Minimized;
            startInfo.Arguments = "https://blog.naver.com/inc_robo";
            Process.Start(startInfo);
        }

    }
}
