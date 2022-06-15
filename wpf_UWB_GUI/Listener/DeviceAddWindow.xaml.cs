using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace wpf_UWB_GUI.Listener
{
    /// <summary>
    /// DeviceInformationWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DeviceAddWindow : Window
    {
        public delegate void OnDevAddHandler(class_listener_list cl_Addlistener);
        public event OnDevAddHandler devAddHandler;

        List<Control> listCursorControl = new List<Control>();
        List<Control> listFiledCursorControl = new List<Control>();


        public DeviceAddWindow()
        {
            InitializeComponent();

            listFiledCursorControl.Add(textBox_devSN);
            listFiledCursorControl.Add(textBox_devName);
            listFiledCursorControl.Add(textBox_positionX);
            listFiledCursorControl.Add(textBox_positionY);
            listFiledCursorControl.Add(textBox_positionZ);
            listCursorControl.Add(btn_save);
            listCursorControl.Add(btn_close);

            for (int i = 0; i < listFiledCursorControl.Count; i++)
            {
                listFiledCursorControl[i].MouseEnter += new MouseEventHandler(mouseFieldEnterHandler);
                listFiledCursorControl[i].MouseLeave += new MouseEventHandler(mouseLeaveHandler);
            }

            for (int i = 0; i < listCursorControl.Count; i++)
            {
                listCursorControl[i].MouseEnter += new MouseEventHandler(mouseEnterHandler);
                listCursorControl[i].MouseLeave += new MouseEventHandler(mouseLeaveHandler);
            }

            textBox_devSN.Focus();

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void DeviceAddWindow_Loaded(object sender, RoutedEventArgs e)
        {
            textBox_devSN.PreviewKeyUp += textboxSN_Position_KeyPress;
            textBox_positionX.PreviewKeyDown += textbox_Position_KeyPress;
            textBox_positionY.PreviewKeyDown += textbox_Position_KeyPress;
            textBox_positionZ.PreviewKeyDown += textbox_Position_KeyPress;

            textBox_devSN.GotFocus += textbox_GotFocus;
            textBox_devName.GotFocus += textbox_GotFocus;
            textBox_positionX.GotFocus += textbox_GotFocus;
            textBox_positionY.GotFocus += textbox_GotFocus;
            textBox_positionZ.GotFocus += textbox_GotFocus;
        }

        private void mouseEnterHandler(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void mouseFieldEnterHandler(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.IBeam;
        }

        private void mouseLeaveHandler(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void textbox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tmpTextBox = (TextBox)sender;
            tmpTextBox.Select(tmpTextBox.Text.Length, 0);
        }

        private void textboxSN_Position_KeyPress(object sender, KeyEventArgs e)
        {
            TextBox tmpTextBox = (TextBox)sender;
            tmpTextBox.Text = tmpTextBox.Text.ToUpper();
            tmpTextBox.Select(tmpTextBox.Text.Length, 0);
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            if (textBox_devSN.Text.Length == 0)
            {
                MessageBox.Show("Device S/N 의 값이 비었습니다.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                textBox_devSN.Focus();
                return;
            }
            if (textBox_positionX.Text.Length == 0)
            {
                MessageBox.Show("Position X 의 값이 비었습니다.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                textBox_positionX.Focus();
                return;
            }
            if (textBox_positionY.Text.Length == 0)
            {
                MessageBox.Show("Position Y 의 값이 비었습니다.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                textBox_positionY.Focus();
                return;
            }
            if (textBox_positionZ.Text.Length == 0)
            {
                MessageBox.Show("Position Z 의 값이 비었습니다.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                textBox_positionZ.Focus();
                return;
            }

            if (devAddHandler != null)
            {
                class_listener_list cl_List = new class_listener_list();
                cl_List.devSN = textBox_devSN.Text;
                cl_List.devType = "Anchor";
                cl_List.tag_pos_x = Double.Parse(textBox_positionX.Text);
                cl_List.tag_pos_y = Double.Parse(textBox_positionY.Text);
                cl_List.tag_pos_z = Double.Parse(textBox_positionZ.Text);
                cl_List.devTagName = textBox_devName.Text;

                devAddHandler(cl_List);
            }
            this.Close();
        }

        private void textbox_Position_KeyPress(object sender, KeyEventArgs e)
        {
            TextBox tmpTextBox = (TextBox)sender;
            if (!(((Key.D0 <= e.Key) && (e.Key <= Key.D9))
                 || ((Key.NumPad0 <= e.Key) && (e.Key <= Key.NumPad9))
                 || e.Key == Key.Decimal
                 || e.Key == Key.OemPeriod
                 || e.Key == Key.Tab
                 || e.Key == Key.Left
                 || e.Key == Key.Right
                 || e.Key == Key.Up
                 || e.Key == Key.Down
                 || e.Key == Key.Back))
            {
                e.Handled = true;
            }
            if(e.Key == Key.Decimal || e.Key == Key.OemPeriod)
            {
                if (tmpTextBox.Text.ToString().Contains("."))
                {
                    e.Handled = true;
                }
            }
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
