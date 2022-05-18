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
    /// UC_device_name_Control.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_device_name_Control : UserControl
    {

        class_DevName clDev;

        public UC_device_name_Control()
        {
            InitializeComponent();

            clDev = new class_DevName();
        }

        public void UpdateDevice(class_DevName m_Dev)
        {
            clDev = m_Dev;
            lbl_sn.Content = clDev.getName();
            textview_name.Text = clDev.getDev1();
            textview_name2.Text = clDev.getDev2();
        }

        public class_DevName getClDevName()
        {
            return clDev;
        }

        //장비 1번 설정 값 입력
        private void textview_name_TextChanged(object sender, TextChangedEventArgs e)
        {
            String strText = textview_name.Text;
            clDev.setDev1(strText);
        }

        //장비 2번 설정 값 입력
        private void textview_name2_TextChanged(object sender, TextChangedEventArgs e)
        {
            String strText = textview_name2.Text;
            clDev.setDev2(strText);
        }
    }
}
