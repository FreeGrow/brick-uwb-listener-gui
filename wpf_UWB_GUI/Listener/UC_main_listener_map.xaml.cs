using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace wpf_UWB_GUI
{
    /// <summary>
    /// UC_main_gateway_com.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_main_listener_map : Page
    {

        private String TAG = "UC_main_listener_map.xaml";


        public MainGetConnectStateHandler getMainConnectHandler;

        public MainGetParseDataAllHandler getParseDataAll;

        UC_listener_map_conrol ucMapControl;


        bool fStart = false;

        DispatcherTimer timer10hz = new DispatcherTimer();

        String strSaveRadio = "";
        String strSaveAveValue = "";
        String strSaveLpfValue = "";
        String strSavePath = "";

        List<Control> listCursorControl = new List<Control>();

        public UC_main_listener_map()
        {
            Console.WriteLine("UC_main_listener_map()");

            InitializeComponent();

            string filePath = Properties.Settings.Default.map_value;
            textBox_filepath.Text = filePath;

            string strMapSize = Properties.Settings.Default.map_size;
            char[] sep = { ',' };

            string[] result = strMapSize.Split(sep);

            if (result.Length == 2)
            {
                txtfield_width.Text = result[0];
                txtfield_height.Text = result[1];
            }

            listCursorControl.Add(btn_filepath);
            listCursorControl.Add(chk_device);
            listCursorControl.Add(chk_filter);
            listCursorControl.Add(chk_name);
            listCursorControl.Add(chk_position);
            listCursorControl.Add(btn_zoom_fit);
            listCursorControl.Add(btn_zoom_plus);
            listCursorControl.Add(btn_zoom_minus);

            for (int i = 0; i < listCursorControl.Count; i++)
            {
                listCursorControl[i].MouseEnter += new MouseEventHandler(mouseEnterHandler);
                listCursorControl[i].MouseLeave += new MouseEventHandler(mouseLeaveHandler);
            }

            btn_sync.MouseEnter += new MouseEventHandler(mouseEnterHandler);
            btn_sync.MouseLeave += new MouseEventHandler(mouseLeaveHandler);

            txtfield_width.MouseEnter += new MouseEventHandler(mouseFieldEnterHandler);
            txtfield_width.MouseLeave += new MouseEventHandler(mouseLeaveHandler);

            txtfield_height.MouseEnter += new MouseEventHandler(mouseFieldEnterHandler);
            txtfield_height.MouseLeave += new MouseEventHandler(mouseLeaveHandler);

            ucMapControl = new UC_listener_map_conrol();
            ucMapControl.UC_MapUpdate();

            mMapViewLayout.Children.Add(ucMapControl);


            timer10hz.Interval = TimeSpan.FromMilliseconds(100);
            timer10hz.Tick += new EventHandler(timer10hz_Tick);
            timer10hz.Start();
        }

        private void mouseEnterHandler(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void mouseLeaveHandler(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void mouseFieldEnterHandler(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.IBeam;
        }

        //CheckBox Status
        bool fDeviceID = false;
        bool fDeviceFilter = false;
        bool fDeviceName = false;
        bool fDevicePosition = false;

        private void timer10hz_Tick(object sender, EventArgs e)
        {
            map_connect_state(getMainConnectHandler());

            Properties.Settings.Default.map_size = txtfield_width.Text + "," + txtfield_height.Text;
            Properties.Settings.Default.Save();

            List<class_listener_list> cl_ListTmp = getParseDataAll();
            ucMapControl.set_ListDevice(cl_ListTmp);

            //ucMapControl.setCheckBoxValue();

            label_tagCount.Content = "T : " + config.get_AliveTag() + "";
            label_anchorCount.Content = "A : " + config.get_AliveAnchor() + "";

            if (chk_device.IsChecked == true)
                fDeviceID = true;
            else
                fDeviceID = false;
            if (chk_name.IsChecked == true)
                fDeviceName = true;
            else
                fDeviceName = false;
            if (chk_position.IsChecked == true)
                fDevicePosition = true;
            else
                fDevicePosition = false;
            if (chk_filter.IsChecked == true)
                fDeviceFilter = true;
            else
                fDeviceFilter = false;

            ucMapControl.setCheckBoxValue(fDeviceID, fDeviceFilter, fDeviceName, fDevicePosition);
        }

        private void UC_main_listener_map_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("UC_main_listener_map_Loaded()");
        }

        private void map_connect_state(bool fState)
        {
            if (fState)
            {
                img_connect_status.Source = new BitmapImage(new Uri("/Resources/img_connected.png", UriKind.RelativeOrAbsolute));
                label_connect_status.Content = "Connected";
                label_connect_status.Foreground = new SolidColorBrush(config.connectColor);
            }
            else
            {
                img_connect_status.Source = new BitmapImage(new Uri("/Resources/img_disconnected.png", UriKind.RelativeOrAbsolute));
                label_connect_status.Content = "DisConnected";
                label_connect_status.Foreground = new SolidColorBrush(config.disconnectColor);
            }
        }

        private void btn_filePath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();

            string OpenFilePath = System.Environment.CurrentDirectory;

            //ofd.InitialDirectory = OpenFilePath;         //초기경로
            //ofd.RestoreDirectory = true;

            ofd.Title = "맵 이미지 파일";
            ofd.Filter = "그림 파일 (*.jpg, *.gif, *.bmp) | *.jpg; *.gif; *.bmp;";

            //파일 오픈창 로드
            System.Windows.Forms.DialogResult dr = ofd.ShowDialog();

            //OK버튼 클릭시
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                //File명과 확장자를 가지고 온다.
                string fileName = ofd.SafeFileName;
                //File경로와 File명을 모두 가지고 온다.
                string fileFullName = ofd.FileName;
                //File경로만 가지고 온다.
                string filePath = fileFullName.Replace(fileName, "");

                Properties.Settings.Default.map_value = fileFullName;
                Properties.Settings.Default.Save();
                textBox_filepath.Text = fileFullName;

                ucMapControl.setZoomLevel(1);
                ucMapControl.UC_FormFirstChange(mMapViewLayout.Width, mMapViewLayout.Height);
                ucMapControl.UC_MapUpdate();

                //fCenter = true;
            }
        }

        //ZOOM FIT BUTTON
        private void btn_zoom_fit_click(object sender, RoutedEventArgs e)
        {
            //ucMapControl
            Thickness thick = ucMapControl.Margin;
            thick.Left = 0;
            thick.Top = 0;
            ucMapControl.Margin = thick;

            if (ucMapControl != null)
            {
                ucMapControl.setZoomLevel(1);
                ucMapControl.UC_FormFirstChange(mMapViewLayout.Width, mMapViewLayout.Height);
                ucMapControl.UC_MapUpdate();
            }
        }

        //ZOOM PLUS BUTTON
        private void btn_zoom_plus_click(object sender, RoutedEventArgs e)
        {
            ucMapControl.FormZoomLevel(1);
        }

        //ZOOM MINUS BUTTON
        private void btn_zoom_minus_click(object sender, RoutedEventArgs e)
        {
            ucMapControl.FormZoomLevel(-1);
        }

        private void label_device_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)     return;
            chk_device.IsChecked = !chk_device.IsChecked;
        }

        private void label_name_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed) return;
            chk_name.IsChecked = !chk_name.IsChecked;
        }

        private void label_position_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed) return;
            chk_position.IsChecked = !chk_position.IsChecked;
        }

        private void label_filtered_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed) return;
            chk_filter.IsChecked = !chk_filter.IsChecked;
        }



        //Map Drag 기능 Start
        bool fMouseDown = false;
        Point prevP;

        private void listener_Map_Page_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            fMouseDown = true;
            prevP = e.GetPosition(listener_Map_Page);
        }

        private void listener_Map_Page_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            fMouseDown = false;
        }

        private void listener_Map_Page_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!fMouseDown) return;

            Point p = e.GetPosition(listener_Map_Page);

            double deltaX = prevP.X - p.X;
            double deltaY = prevP.Y - p.Y;

            //ucMapControl
            Thickness thick = ucMapControl.Margin;
            thick.Left = thick.Left - deltaX * 1.5;
            thick.Top = thick.Top - deltaY * 1.5;
            ucMapControl.Margin = thick;

            //Console.WriteLine("x : " + p.X + "  y :  " + p.Y);

            prevP = p;
        }

        private void listener_Map_Page_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            fMouseDown = false;
        }

        private void listener_Map_Page_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ucMapControl.FormZoomLevel(e.Delta);
        }

        private void listener_Map_Page_SizeChagned(object sender, SizeChangedEventArgs e)
        {
            //mMapViewLayout.Width = e.NewSize.Width - 10;
            //mMapViewLayout.Height = e.NewSize.Height - 180;

            //if (ucMapControl != null)
            //    ucMapControl.UC_FormSize(mMapViewLayout.Width, mMapViewLayout.Height);

            ////UC_map_control Size CHange Start
            //Thickness thick = btn_zoom_fit.Margin;
            //thick.Left = e.NewSize.Width - 75 - 33;
            //btn_zoom_fit.Margin = thick;

            //thick = btn_zoom_plus.Margin;
            //thick.Left = e.NewSize.Width - 75 - 33;
            //btn_zoom_plus.Margin = thick;

            //thick = btn_zoom_minus.Margin;
            //thick.Left = e.NewSize.Width - 75 - 33;
            //btn_zoom_minus.Margin = thick;
        }
    }
}
