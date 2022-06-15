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
    /// UC_.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_Menu_listener : UserControl
    {

        //MenuListener -> MainWindow
        public delegate void MainGetMenuListenerDataEventHandler(int btnNumber);
        public event MainGetMenuListenerDataEventHandler setListenerMenuButton;

        const int stateComport = 1;
        const int stateFilter = 2;
        const int stateDevice = 3;
        const int stateMap = 4;
        const int stateSetting = 5;
        const int stateInfo = 6;

        int stateClick = 1;

        List<Button> listButton = new List<Button>();

        public UC_Menu_listener()
        {
            InitializeComponent();

            listButton.Add(btn_menu_communication);
            listButton.Add(btn_menu_filter);
            listButton.Add(btn_menu_device);
            listButton.Add(btn_menu_map);
            listButton.Add(btn_menu_setting);
            listButton.Add(btn_menu_info);

            for (int i=0;i< listButton.Count; i++)
            {
                listButton[i].MouseEnter += new MouseEventHandler(mouseEnterHandler);
                listButton[i].MouseLeave += new MouseEventHandler(mouseLeaveHandler);
            }
        }

        private void mouseEnterHandler(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
            Button tmpButton = (Button)sender;

            Color mColor = (Color)ColorConverter.ConvertFromString("#FF25262A");
            tmpButton.Background = new SolidColorBrush(mColor);
        }

        private void mouseLeaveHandler(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
            Button tmpButton = (Button)sender;

            int btnState = int.Parse(tmpButton.Tag.ToString());
            if (btnState == stateClick)
            {
                return;
            }

            Color mColor = (Color)ColorConverter.ConvertFromString("#FF161618");
            tmpButton.Background = new SolidColorBrush(mColor);
        }

        private void btn_menu_communication_Click(object sender, RoutedEventArgs e)
        {
            stateClick = stateComport;

            Color mColor = (Color)ColorConverter.ConvertFromString("#FF161618");

            for (int i = 0; i < listButton.Count; i++)
            {
                listButton[i].Background = new SolidColorBrush(mColor);
            }

            mColor = (Color)ColorConverter.ConvertFromString("#FF25262A");
            btn_menu_communication.Background = new SolidColorBrush(mColor);

            setListenerMenuButton(1);
        }

        private void btn_menu_filter_Click(object sender, RoutedEventArgs e)
        {
            stateClick = stateFilter;

            Color mColor = (Color)ColorConverter.ConvertFromString("#FF161618");

            for (int i = 0; i < listButton.Count; i++)
            {
                listButton[i].Background = new SolidColorBrush(mColor);
            }

            mColor = (Color)ColorConverter.ConvertFromString("#FF25262A");
            btn_menu_filter.Background = new SolidColorBrush(mColor);

            setListenerMenuButton(2);
        }

        private void btn_menu_device_Click(object sender, RoutedEventArgs e)
        {
            stateClick = stateDevice;

            Color mColor = (Color)ColorConverter.ConvertFromString("#FF161618");

            for (int i = 0; i < listButton.Count; i++)
            {
                listButton[i].Background = new SolidColorBrush(mColor);
            }

            mColor = (Color)ColorConverter.ConvertFromString("#FF25262A");
            btn_menu_device.Background = new SolidColorBrush(mColor);

            setListenerMenuButton(3);
        }

        private void btn_menu_map_Click(object sender, RoutedEventArgs e)
        {
            stateClick = stateMap;

            Color mColor = (Color)ColorConverter.ConvertFromString("#FF161618");

            for (int i = 0; i < listButton.Count; i++)
            {
                listButton[i].Background = new SolidColorBrush(mColor);
            }

            mColor = (Color)ColorConverter.ConvertFromString("#FF25262A");
            btn_menu_map.Background = new SolidColorBrush(mColor);

            setListenerMenuButton(4);
        }

        private void btn_menu_setting_Click(object sender, RoutedEventArgs e)
        {
            stateClick = stateSetting;

            Color mColor = (Color)ColorConverter.ConvertFromString("#FF161618");

            for (int i = 0; i < listButton.Count; i++)
            {
                listButton[i].Background = new SolidColorBrush(mColor);
            }

            mColor = (Color)ColorConverter.ConvertFromString("#FF25262A");
            btn_menu_setting.Background = new SolidColorBrush(mColor);

            setListenerMenuButton(5);
        }

        private void btn_menu_info_Click(object sender, RoutedEventArgs e)
        {
            stateClick = stateInfo;

            Color mColor = (Color)ColorConverter.ConvertFromString("#FF161618");

            for (int i = 0; i < listButton.Count; i++)
            {
                listButton[i].Background = new SolidColorBrush(mColor);
            }

            mColor = (Color)ColorConverter.ConvertFromString("#FF25262A");
            btn_menu_info.Background = new SolidColorBrush(mColor);

            setListenerMenuButton(6);
        }
    }
}
