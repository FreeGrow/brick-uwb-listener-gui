using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace wpf_UWB_GUI
{
    /// <summary>
    /// UC_main_gateway_com.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_main_listener_setting : Page
    {
        String TAG = "UC_main_listener_setting.xaml";

        SerialPort sp_Anchor = new SerialPort();

        DispatcherTimer timer10hz = new DispatcherTimer();

        List<String> strList = new List<string>();

        public UC_main_listener_setting()
        {
            Console.WriteLine("UC_main_listener_setting()");

            InitializeComponent();

            var portnames = SerialPort.GetPortNames();
            for (int i = 0; i < portnames.Length; i++) cbx_serialPort.Items.Add(portnames[i]);

            timer10hz.Tick += new EventHandler(timer10hz_Tick);
            timer10hz.Start();
        }

        private void UC_main_gateway_setting_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("UC_main_gateway_com_Loaded()");

        }

        long prevMillis = 0;
        long prevSystemInfoMillis = 0;

        private void timer10hz_Tick(object sender, EventArgs e)
        {
            if ((long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - prevMillis > 1000 * 1)
            {
                prevMillis = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;

                var portnames = SerialPort.GetPortNames();

                for (int i = 0; i < portnames.Length; i++)
                    if (cbx_serialPort.Items.IndexOf(portnames[i]) == -1)
                        cbx_serialPort.Items.Add(portnames[i]);
            }

            if (fSettingClick)
            {
                if ((long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - prevSystemInfoMillis > 1500 * 1)
                {
                    prevSystemInfoMillis = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;

                    if (settingStatus == 0)
                    {
                        settingStatus = 1;

                        byte[] arByte = new byte[10];
                        arByte[0] = (byte)'s';
                        arByte[1] = (byte)'i';
                        arByte[2] = 0x0d;
                        sp_Anchor.Write(arByte, 0, 3);
                    }
                    else if (settingStatus == 1)
                    {
                        fSettingClick = false;

                        byte[] arByte = new byte[10];
                        arByte[0] = (byte)'a';
                        arByte[1] = (byte)'p';
                        arByte[2] = (byte)'g';
                        arByte[3] = 0x0d;
                        sp_Anchor.Write(arByte, 0, 4);
                    }
                }
            }


            if (strList.Count < 1)
            {
                return;
            }

            String strTmp = strList[0];
            strList.RemoveAt(0);

            Console.WriteLine("strTmp : " + strTmp);

            //[000013.240 INF] sys: fw2 fw_ver = x01030001 cfg_ver = x00010700
            //[000013.240 INF] uwb0: panid = xD438 addr = xDECAC54A6C402E4B
            //[000013.250 INF] mode: an(act, -)
            //[000013.260 INF] uwbmac: connected
            //[000013.260 INF] uwbmac: bh disconnected
            //[000013.260 INF] cfg: sync = 0 fwup = 0 ble = 1 leds = 0 init = 0 upd_rate_stat = 120 label = DW2E4B
            //[000013.270 INF] enc: off
            //[000013.280 INF] ble: addr = E9:D4: D1: E4: 71:39

            if (strTmp.Contains("panid="))
            {
                String panid = strTmp.Substring(strTmp.IndexOf("panid=") + 6, 5);
                panid += "0";
                Console.WriteLine("Panid : " + panid);
                lbl_devPanid.Content = panid;
            }

            if (strTmp.Contains("mode:"))
            {
                String mode = strTmp.Substring(strTmp.IndexOf("mode:") + 6);
                Console.WriteLine("Mode : " + mode);
                lbl_devMode.Content = mode;
            }

            if (strTmp.Contains("label="))
            {
                String label = strTmp.Substring(strTmp.IndexOf("label=") + 6, 6);
                Console.WriteLine("label : " + label);
                lbl_devName.Content = label;
            }

            if (strTmp.Contains("init="))
            {
                String init = strTmp.Substring(strTmp.IndexOf("init=") + 5, 1);
                Console.WriteLine("init : " + init);
                if (init.Contains("0"))
                    lbl_devInit.Content = "false";
                if (init.Contains("1"))
                    lbl_devInit.Content = "true";
            }
            //apg: x: 0 y: 0 z: 0 qf: 0
            if (strTmp.Contains("apg:"))
            {
                String pos = strTmp.Substring(strTmp.IndexOf("apg:") + 5);
                Console.WriteLine("pos : " + pos);
                lbl_devPosition.Content = pos;

                char[] split1 = { ' ' };
                string[] result = pos.Split(split1);

                String pos_x = result[0].Substring(result[0].IndexOf("x:") + 2);
                String pos_y = result[1].Substring(result[0].IndexOf("y:") + 3);
                String pos_z = result[2].Substring(result[0].IndexOf("z:") + 3);

                txt_pos_x.Text = pos_x;
                txt_pos_y.Text = pos_y;
                txt_pos_z.Text = pos_z;

            }
        }

        private void connected_btn_Click(object sender, RoutedEventArgs e)
        {
            //none select
            if (cbx_serialPort.SelectedIndex == -1) return;
            if (sp_Anchor.IsOpen) return;

            sp_Anchor.PortName = cbx_serialPort.SelectedItem.ToString();
            sp_Anchor.BaudRate = 115200;
            sp_Anchor.DataReceived += new SerialDataReceivedEventHandler(sp_listener_DataReceivedHandler);

            try
            {
                sp_Anchor.Open();
            }
            catch (Exception e1)
            {
                MessageBox.Show("Port is opened. Not Connect.");
                return;
            }

            serialPort_Status(true);
        }

        public void sp_listener_DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            //String receiveData = sp_listener.ReadExisting();
            try
            {
                String receiveData = sp_Anchor.ReadLine();
                strList.Add(receiveData);
            }
            catch (Exception e1)
            {

            }

            //Console.Write("sp_listener_DataReceivedHandler()");
            //Console.WriteLine(receiveData);
        }

        private void stop_btn_Click(object sender, RoutedEventArgs e)
        {
            sp_Anchor.Close();

            serialPort_Status(false);
        }

        private void cbx_serialPort_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                //UP
                if (cbx_serialPort.SelectedIndex > 0)
                {
                    cbx_serialPort.SelectedIndex -= 1;
                }
            }
            else if (e.Delta < 0)
            {
                //DOWN
                if (cbx_serialPort.SelectedIndex < cbx_serialPort.Items.Count)
                {
                    cbx_serialPort.SelectedIndex += 1;
                }
            }
        }

        private void serialPort_Status(bool fState)
        {
            if (fState)
            {
                connected_btn.IsEnabled = false;
                stop_btn.Background = new SolidColorBrush(config.titleSelectColor);
                connected_btn.Background = new SolidColorBrush(config.titleUnSelectColor);
                img_connect_status.Source = new BitmapImage(new Uri("/Resources/img_connected.png", UriKind.RelativeOrAbsolute));
                label_connect_status.Content = "Connected";
                label_connect_status.Foreground = new SolidColorBrush(config.connectColor);
            }
            else
            {
                connected_btn.IsEnabled = true;
                connected_btn.Background = new SolidColorBrush(config.titleSelectColor);
                stop_btn.Background = new SolidColorBrush(config.titleUnSelectColor);
                img_connect_status.Source = new BitmapImage(new Uri("/Resources/img_disconnected.png", UriKind.RelativeOrAbsolute));
                label_connect_status.Content = "DisConnected";
                label_connect_status.Foreground = new SolidColorBrush(config.disconnectColor);
            }
        }

        int settingStatus = 0;
        bool fSettingClick = false;

        private void btn_getSetting_Click(object sender, RoutedEventArgs e)
        {
            settingStatus = 0;
            fSettingClick = true;
        }

        private void btn_setSetting_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
