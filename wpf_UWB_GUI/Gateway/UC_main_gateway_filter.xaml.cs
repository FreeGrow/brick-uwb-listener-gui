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

namespace wpf_UWB_GUI
{
    /// <summary>
    /// UC_main_gateway_com.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_main_gateway_filter : UserControl
    {

        private String TAG = "UC_main_gateway_filter.xaml";

        public Main_Setting_Data mainSettingData = new Main_Setting_Data();

        MqttConnect mqttConnect;

        public UC_main_gateway_filter()
        {
            InitializeComponent();
        }

        private void UC_main_gateway_filter_Loaded(object sender, RoutedEventArgs e)
        {


        }
    }
}
