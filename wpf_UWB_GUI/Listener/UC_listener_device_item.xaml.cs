using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using wpf_UWB_GUI.Listener;

namespace wpf_UWB_GUI
{
    /// <summary>
    /// UC_device_list_item.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_listener_device_item : UserControl
    {

        public delegate void OnDevColorPickHandler(class_listener_list cl_ColorChange);
        public event OnDevColorPickHandler devColorPick;
        public delegate void OnDevEditItemHandler(class_listener_list cl_Addlistener);
        public event OnDevEditItemHandler devEditItemHandler;
        public delegate void OnDevRemoveItemHandler(class_listener_list cl_Addlistener);
        public event OnDevRemoveItemHandler devRemoveItemHandler;

        class_listener_list clList = new class_listener_list();

        public UC_listener_device_item()
        {
            InitializeComponent();

        }

        public void set_clList(class_listener_list mClList)
        {
            clList = mClList;

            if (clList.devColor != null)
            {
                if (clList.devColor.Length != 0)
                {
                    Color mColor = (Color)ColorConverter.ConvertFromString(clList.devColor);
                    color_devColor.SelectedColor = mColor;
                }
            }
        }

        public class_listener_list get_clList()
        {
            return clList;
        }

        public void Update()
        {
            lbl_devSerialNum.Content = clList.devSN;
            lbl_devType.Content = clList.devType;
            lbl_devName.Content = clList.devTagName;
            lbl_devPosition.Content = clList.devPosition;
            lbl_devFilterPosition.Content = clList.devFilter;
        }

        //Color Changed
        private void colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            clList.devColor = color_devColor.SelectedColor.ToString();

            devColorPick(clList);
        }


        DeviceEditWindow devEditWindow;

        private void btn_edit_Click(object sender, RoutedEventArgs e)
        {
            bool fReslt = false;
            fReslt = config.openFormCheck("DeviceEdit");

            if (!fReslt)
            {
                devEditWindow = new DeviceEditWindow();
                devEditWindow.devEditHandler += new DeviceEditWindow.OnDevEditHandler(DeviceWindow_DevEditHandler);
                devEditWindow.set_clList(clList);
                devEditWindow.UpdateUI();
                devEditWindow.Show();
            }
            else
            {
                config.closeFormCheck("DeviceEdit");
                devEditWindow = new DeviceEditWindow();
                devEditWindow.devEditHandler += new DeviceEditWindow.OnDevEditHandler(DeviceWindow_DevEditHandler);
                devEditWindow.set_clList(clList);
                devEditWindow.UpdateUI();
                devEditWindow.Show();
            }
        }

        //DeviceEdit callback Method
        private void DeviceWindow_DevEditHandler(class_listener_list cl_List)
        {
            class_listener_list clTmp = get_clList();
            clTmp.devTagName = cl_List.devTagName;
            clTmp.tag_pos_x = cl_List.tag_pos_x;
            clTmp.tag_pos_y = cl_List.tag_pos_y;
            clTmp.tag_pos_z = cl_List.tag_pos_z;
            clTmp.time = cl_List.time;

            set_clList(clTmp);
            Update();

            Console.WriteLine("DeviceWindow_DevEditHandler()");
            Console.WriteLine(cl_List.devSN + ":" +
                cl_List.devTagName + ":"
                + cl_List.tag_pos_x + ":"
                + cl_List.tag_pos_y + ":"
                + cl_List.tag_pos_z);

            devEditItemHandler(clTmp);
        }

        private void btn_remove_Click(object sender, RoutedEventArgs e)
        {
            devRemoveItemHandler(clList);
        }
    }
}
