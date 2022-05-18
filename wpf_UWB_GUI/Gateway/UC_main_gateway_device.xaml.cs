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

namespace wpf_UWB_GUI
{
    /// <summary>
    /// UC_main_gateway_com.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_main_gateway_device : UserControl
    {

        private List<UC_device_list_item> list_device_item = new List<UC_device_list_item>();

        public UC_main_gateway_device()
        {
            InitializeComponent();
        }

        private void UC_main_gateway_device_Loaded(object sender, RoutedEventArgs e)
        {

            for (int i = 0; i < 12; i++)
            {
                UC_device_list_item deviceList = new UC_device_list_item();

                //deviceList.UpdateDevice();

                addControl(deviceList);

            }
        }

        private void addControl(UC_device_list_item tmp_devlist)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                list_device_item.Add(tmp_devlist);
                listPanelItem.Children.Add(tmp_devlist);
            }));
        }
    }
}
