using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
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


            txt_pos_x.PreviewKeyDown += textbox_Position_KeyPress;
            txt_pos_y.PreviewKeyDown += textbox_Position_KeyPress;
            txt_pos_z.PreviewKeyDown += textbox_Position_KeyPress;

            txt_pos_x.GotFocus += textbox_GotFocus;
            txt_pos_y.GotFocus += textbox_GotFocus;
            txt_pos_z.GotFocus += textbox_GotFocus;
        }

        private void UC_main_gateway_setting_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("UC_main_gateway_com_Loaded()");

        }

        byte[] arSendByte;

        long prevMillis = 0;
        long prevSystemInfoMillis = 0;
        long prevPositionMillis = 0;

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

            if (fPositionClick)
            {
                if ((long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - prevPositionMillis > 1500 * 1)
                {
                    prevPositionMillis = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;

                    if (positionStatus == 0)
                    {
                        positionStatus = 1;

                        //aps 0 0 0
                        int arCnt = 0;

                        arSendByte = new byte[64];
                        arSendByte[arCnt++] = (byte)'a';
                        arSendByte[arCnt++] = (byte)'p';
                        arSendByte[arCnt++] = (byte)'s';
                        arSendByte[arCnt++] = (byte)' ';

                        //sp_Anchor.Write(arByte, 0, 4);

                        int iposX = (int)(double.Parse(txt_pos_x.Text.ToString()) * 1000);
                        int iposY = (int)(double.Parse(txt_pos_y.Text.ToString()) * 1000);
                        int iposZ = (int)(double.Parse(txt_pos_z.Text.ToString()) * 1000);

                        String strPosX = iposX.ToString();
                        String strPosY = iposY.ToString();
                        String strPosZ = iposZ.ToString();

                        for (int i = 0; i < strPosX.Length; i++)
                        {
                            arSendByte[arCnt++] = (byte)strPosX[i];
                        }
                        arSendByte[arCnt++] = (byte)' ';

                        for (int i = 0; i < strPosY.Length; i++)
                        {
                            arSendByte[arCnt++] = (byte)strPosY[i];
                        }
                        arSendByte[arCnt++] = (byte)' ';

                        for (int i = 0; i < strPosZ.Length; i++)
                        {
                            arSendByte[arCnt++] = (byte)strPosZ[i];
                        }

                        arSendByte[arCnt++] = 0x0d;

                        for (int i = 0; i < arSendByte.Length; i++)
                        {
                            if (i % 5 == 0)
                                Thread.Sleep(100);
                            sendByte(arSendByte[i]);
                        }
                    }
                    else if (positionStatus == 1)
                    {
                        fPositionClick = false;

                        //nmi
                        //nma

                        if ((bool)chk_initiator.IsChecked)
                        {
                            Console.WriteLine("Set Checked True");
                            Console.WriteLine("Send nmi");

                            sendByte((byte)'n');
                            sendByte((byte)'m');
                            sendByte((byte)'i');
                            sendByte(0x0d);
                        }
                        else
                        {
                            Console.WriteLine("Set Checked False");
                            Console.WriteLine("Send nma");

                            sendByte((byte)'n');
                            sendByte((byte)'m');
                            sendByte((byte)'a');
                            sendByte(0x0d);

                        }
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
                {
                    lbl_devInit.Content = "false";
                    chk_initiator.IsChecked = false;
                }
                if (init.Contains("1"))
                {
                    lbl_devInit.Content = "true";
                    chk_initiator.IsChecked = true;
                }
            }

            //apg: x: 0 y: 0 z: 0 qf: 0
            if (strTmp.Contains("apg:"))
            {
                String pos = strTmp.Substring(strTmp.IndexOf("apg:") + 5);
                Console.WriteLine("pos : " + pos);
                //lbl_devPosition.Content = pos;

                char[] split1 = { ' ' };
                string[] result = pos.Split(split1);

                String pos_x = result[0].Substring(result[0].IndexOf("x:") + 2);
                String pos_y = result[1].Substring(result[0].IndexOf("y:") + 3);
                String pos_z = result[2].Substring(result[0].IndexOf("z:") + 3);

                float fpos_x = (float)int.Parse(pos_x) / 1000;
                float fpos_y = (float)int.Parse(pos_y) / 1000;
                float fpos_z = (float)int.Parse(pos_z) / 1000;

                fpos_x = fpos_x * 100;
                fpos_y = fpos_y * 100;
                fpos_z = fpos_z * 100;

                fpos_x = (float)Math.Truncate(fpos_x);
                fpos_y = (float)Math.Truncate(fpos_y);
                fpos_z = (float)Math.Truncate(fpos_z);

                fpos_x = fpos_x / 100;
                fpos_y = fpos_y / 100;
                fpos_z = fpos_z / 100;

                Console.WriteLine("fpos_x : " + fpos_x);
                Console.WriteLine("fpos_y : " + fpos_y);
                Console.WriteLine("fpos_z : " + fpos_z);

                string spos_x = string.Format("{0:0.00}", fpos_x);
                string spos_y = string.Format("{0:0.00}", fpos_y);
                string spos_z = string.Format("{0:0.00}", fpos_z);

                txt_pos_x.Text = spos_x.ToString();
                txt_pos_y.Text = spos_y.ToString();
                txt_pos_z.Text = spos_z.ToString();

                lbl_devPosition.Content = "x:" + spos_x.ToString() 
                                        + "y:" + spos_y.ToString() 
                                        + "z:" + spos_z.ToString();
            }
        }

        public void sendByte(byte tmpByte)
        {
            byte[] arByte = new byte[2];
            arByte[0] = tmpByte;
            sp_Anchor.Write(arByte, 0, 1);
        }

        public byte[] AddByteToArray(byte[] bArray, byte newByte)
        {
            byte[] newArray = new byte[bArray.Length + 1];
            bArray.CopyTo(newArray, 1);
            newArray[0] = newByte;
            return newArray;
        }


        // String을 바이트 배열로 변환 
        private byte[] StringToByte(string str)
        {
            byte[] StrByte = Encoding.UTF8.GetBytes(str);
            return StrByte;
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
            if (e.Key == Key.Decimal || e.Key == Key.OemPeriod)
            {
                if (tmpTextBox.Text.ToString().Contains("."))
                {
                    e.Handled = true;
                }
            }

            //string strTmp = tmpTextBox.Text;

            //if (strTmp.IndexOf(".") != -1)
            //{
            //    if (strTmp.Length > strTmp.IndexOf(".") + 2)
            //    {
            //        if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Tab)
            //        {
            //            ;
            //        }
            //        else
            //        {
            //            e.Handled = true;
            //        }
            //    }
            //}

        }

        private void textbox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tmpTextBox = (TextBox)sender;
            tmpTextBox.Select(tmpTextBox.Text.Length, 0);
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

        private void stop_btn_Click(object sender, RoutedEventArgs e)
        {
            sp_Anchor.Close();

            serialPort_Status(false);
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
                cbx_serialPort.IsEnabled = false;
                connected_btn.IsEnabled = false;
                stop_btn.Background = new SolidColorBrush(config.titleSelectColor);
                connected_btn.Background = new SolidColorBrush(config.titleUnSelectColor);
                img_connect_status.Source = new BitmapImage(new Uri("/Resources/img_connected.png", UriKind.RelativeOrAbsolute));
                label_connect_status.Content = "Connected";
                label_connect_status.Foreground = new SolidColorBrush(config.connectColor);
            }
            else
            {
                cbx_serialPort.IsEnabled = true;
                connected_btn.IsEnabled = true;
                connected_btn.Background = new SolidColorBrush(config.titleSelectColor);
                stop_btn.Background = new SolidColorBrush(config.titleUnSelectColor);
                img_connect_status.Source = new BitmapImage(new Uri("/Resources/img_disconnected.png", UriKind.RelativeOrAbsolute));
                label_connect_status.Content = "DisConnected";
                label_connect_status.Foreground = new SolidColorBrush(config.disconnectColor);
            }
        }

        //Get SystemInfo
        int settingStatus = 0;
        bool fSettingClick = false;

        //Set Position Setting
        int positionStatus = 0;
        bool fPositionClick = false;

        private void btn_getSetting_Click(object sender, RoutedEventArgs e)
        {
            if (!sp_Anchor.IsOpen) return;

            settingStatus = 0;
            fSettingClick = true;
        }

        private void btn_setSetting_Click(object sender, RoutedEventArgs e)
        {
            if (!sp_Anchor.IsOpen) return;

            positionStatus = 0;
            fPositionClick = true;
        }
    }
}
