using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace wpf_UWB_GUI
{
    /// <summary>
    /// UC_main_gateway_com.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_main_gateway_com : UserControl
    {
        String TAG = "UC_main_gateway_com.xaml";

        MqttConnect mqttConnect;
        public Main_Setting_Data mainSettingData = new Main_Setting_Data();
        private List<Device_Reference> realDeviceInfo = new List<Device_Reference>();

        DispatcherTimer timer10hz = new DispatcherTimer();

        Color titleSelectColor = (Color)ColorConverter.ConvertFromString("#FF1457ED");
        Color titleUnSelectColor = (Color)ColorConverter.ConvertFromString("#FF161618");

        //mqtt Pub Alive Thread
        Thread thread_Alive_Pub;

        public UC_main_gateway_com()
        {
            InitializeComponent();
        }

        private void UC_main_gateway_com_Loaded(object sender, RoutedEventArgs e)
        {
            update_setting_control();

            mqttConnect = new MqttConnect();
            mqttConnect.init();

            mqttConnect.MQTT_IP = mainSettingData.MQTT_IP;
            mqttConnect.MQTT_PORT = mainSettingData.MQTT_port;

            timer10hz.Interval = TimeSpan.FromMilliseconds(0.01);
            timer10hz.Tick += new EventHandler(timer10hz_Tick);
            timer10hz.Start();

        }

        long prevAliveMillis = 0;
        long prevDevNameMillis = 0;
        long prevPrintMillis = 0;
        //Test
        long prevBatteryMillis = 0;

        private void timer10hz_Tick(object sender, EventArgs e)
        {
            //topic received
            mqttConnect.mainSettingData = mainSettingData;
            realDeviceInfo = mqttConnect.device_References;

            UpdateMQTT_Status(mainSettingData.MQTT_status_connect);


            //Battery Test Code
            if ((long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - prevBatteryMillis > (config.batteryTime))
            {
                prevBatteryMillis = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;

                String strBat = "";

                for (int i = 0; i < realDeviceInfo.Count; i++)
                {
                    if (realDeviceInfo[i].type.Equals("Tag"))
                    {
                        strBat += realDeviceInfo[i].serial_num;
                        strBat += ":";
                        strBat += realDeviceInfo[i].battery;
                        strBat += "//";
                    }
                }

                String strSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\UWB\\";
                config.fn_BatteryTextWrite("", strSavePath, strBat);
            }

            //Topic Pub Alive Signal
            if ((long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - prevAliveMillis > (config.aliveTime))
            {
                prevAliveMillis = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;

                thread_Alive_Pub = new Thread(mqtt_Pub_Alive);
                thread_Alive_Pub.Start();
            }

            //Topic Pub Device Name
            if ((long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - prevDevNameMillis > (config.deviceNameTime))
            {
                prevDevNameMillis = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;

                String topic = "dwm/node/tag/name", message = "";

                message = config.fn_DeviceNameRead(TAG);

                if (mqttConnect.IsConnected()) mqttConnect.mqtt_Pub(topic, message, true);
            }

            if ((long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - prevPrintMillis > (config.printTime))
            {
                printDeviceState();
                prevPrintMillis = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;
            }

        }

        private void connected_btn_Click(object sender, RoutedEventArgs e)
        {
            mainSettingData.MQTT_IP = IPTextBox1.Text + "." + IPTextBox2.Text + "." + IPTextBox3.Text + "." + IPTextBox4.Text;
            mainSettingData.MQTT_port = PortTextBox.Text;
            mqttConnect.MQTT_IP = mainSettingData.MQTT_IP;
            mqttConnect.MQTT_PORT = mainSettingData.MQTT_port;
            mqttConnect.startClicked = true;

            json_generate();
        }

        private void stop_btn_Click(object sender, RoutedEventArgs e)
        {
            mainSettingData.MQTT_IP = IPTextBox1.Text + "." + IPTextBox2.Text + "." + IPTextBox3.Text + "." + IPTextBox4.Text;
            mainSettingData.MQTT_port = PortTextBox.Text;
            mqttConnect.MQTT_IP = mainSettingData.MQTT_IP;
            mqttConnect.MQTT_PORT = mainSettingData.MQTT_port;
            mqttConnect.stopClicked = true;

            json_generate();
        }

        //UPDATE Setting DATA
        private void update_setting_control()
        {
            request_json();
            string txt = mainSettingData.MQTT_IP;
            if (txt != null)
            {
                string[] ip = txt.Split('.');
                IPTextBox1.Text = ip[0];
                IPTextBox2.Text = ip[1];
                IPTextBox3.Text = ip[2];
                IPTextBox4.Text = ip[3];
            }

            PortTextBox.Text = mainSettingData.MQTT_port;
        }

        //JSON SAVE Setting DATA
        private void json_generate()
        {
            string json = JsonConvert.SerializeObject(mainSettingData);
            Properties.Settings.Default.JSON_Setting_Info = json;
            Properties.Settings.Default.Save();
        }

        //JSON LOAD Setting DATA
        private void request_json()
        {
            string json_setting_string = Properties.Settings.Default.JSON_Setting_Info;
            var setting_data = JsonConvert.DeserializeObject<Main_Setting_Data>(json_setting_string);
            mainSettingData = setting_data;
        }

        //UPDATE MQTT STATUS
        private void UpdateMQTT_Status(int status)
        {
            IPTextBox1.IsEnabled = false;
            IPTextBox2.IsEnabled = false;
            IPTextBox3.IsEnabled = false;
            IPTextBox4.IsEnabled = false;
            PortTextBox.IsEnabled = false;

            switch (mainSettingData.MQTT_status_connect)
            {
                case 0: // disconnect
                    IPTextBox1.IsEnabled = true;
                    IPTextBox2.IsEnabled = true;
                    IPTextBox3.IsEnabled = true;
                    IPTextBox4.IsEnabled = true;
                    PortTextBox.IsEnabled = true;

                    connected_btn.IsEnabled = true;
                    stop_btn.IsEnabled = false;
                    connected_btn.Background = new SolidColorBrush(titleSelectColor);
                    stop_btn.Background = new SolidColorBrush(titleUnSelectColor);

                    label_connect_status.Content = "Disconnect";
                    label_connect_status.Foreground = new SolidColorBrush(Color.FromRgb(0xff, 0x00, 0x00));
                    break;
                case 1: // connecting
                    connected_btn.IsEnabled = false;
                    stop_btn.IsEnabled = true;
                    connected_btn.Background = new SolidColorBrush(titleUnSelectColor);
                    stop_btn.Background = new SolidColorBrush(titleSelectColor);

                    label_connect_status.Content = "Connecting...";
                    label_connect_status.Foreground = new SolidColorBrush(Color.FromRgb(0xff, 0xB3, 0x2E));
                    break;
                case 2: // connected
                    connected_btn.IsEnabled = false;
                    stop_btn.IsEnabled = true;
                    connected_btn.Background = new SolidColorBrush(titleUnSelectColor);
                    stop_btn.Background = new SolidColorBrush(titleSelectColor);

                    label_connect_status.Content = "Connected";
                    label_connect_status.Foreground = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
                    break;
            }
        }


        //MQTT_PUB_ALIVE SIGNAL
        private void mqtt_Pub_Alive()
        {
            //Console.WriteLine("mqtt_Pub_Alive()");

            List<Device_Reference> tmpDev = realDeviceInfo;

            int num_result_Tag = tmpDev.FindIndex(x => x.type.Equals("Tag"));
            int num_result_Anchor = tmpDev.FindIndex(x => x.type.Equals("Anchor"));
            int num_result_Gateway = tmpDev.FindIndex(x => x.type.Equals("Gateway"));

            List<Device_Reference> resultTag = new List<Device_Reference>();
            List<Device_Reference> resultAnchor = new List<Device_Reference>();
            List<Device_Reference> resultGateway = new List<Device_Reference>();

            if (num_result_Tag != -1)
            {
                resultTag = tmpDev.FindAll(x => x.type.Equals("Tag"));
                resultTag = resultTag.OrderBy(x => x.serial_num).ToList();
            }

            if (num_result_Anchor != -1)
            {
                resultAnchor = tmpDev.FindAll(x => x.type.Equals("Anchor"));
                resultAnchor = resultAnchor.OrderBy(x => x.serial_num).ToList();
            }

            if (num_result_Gateway != -1)
            {
                resultGateway = tmpDev.FindAll(x => x.type.Equals("Gateway"));
                resultGateway = resultGateway.OrderBy(x => x.serial_num).ToList();
            }

            for (int i = 0; i < resultTag.Count; i++)
            {
                String topic = "dwm/node/", message = "";
                String strTmp = resultTag[i].serial_num;
                strTmp = strTmp.Substring(2);
                strTmp = strTmp.ToLower();
                topic += strTmp + "/uplink/alive";

                long nowTime = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - resultTag[i].time;
                if (nowTime > config.Alive_TAG)
                {
                    message = "{\"alive\":false}";
                }
                else
                {
                    message = "{\"alive\":true}";
                }

                //Console.WriteLine("topic : " + topic + "  message : " + message);

                if (mqttConnect.IsConnected()) mqttConnect.mqtt_Pub(topic, message, true);
            }

            for (int i = 0; i < resultAnchor.Count; i++)
            {
                String topic = "dwm/node/", message = "";
                String strTmp = resultAnchor[i].serial_num;
                strTmp = strTmp.Substring(2);
                strTmp = strTmp.ToLower();
                topic += strTmp + "/uplink/alive";

                long nowTime = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - resultAnchor[i].time;
                if (nowTime > config.Alive_ANCHOR)
                {
                    message = "{\"alive\":false}";
                }
                else
                {
                    message = "{\"alive\":true}";
                }
                //Console.WriteLine("topic : " + topic + "  message : " + message);

                if (mqttConnect.IsConnected()) mqttConnect.mqtt_Pub(topic, message, true);
            }

            for (int i = 0; i < resultGateway.Count; i++)
            {
                String topic = "dwm/node/", message = "";
                String strTmp = resultGateway[i].serial_num;
                strTmp = strTmp.ToLower();
                topic += strTmp + "/uplink/alive";

                long nowTime = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - resultGateway[i].time;
                if (nowTime > config.Alive_GATEWAY)
                {
                    message = "{\"alive\":false}";
                }
                else
                {
                    message = "{\"alive\":true}";
                }
                //Console.WriteLine("topic : " + topic + "  message : " + message);

                if (mqttConnect.IsConnected()) mqttConnect.mqtt_Pub(topic, message, true);
            }
            thread_Alive_Pub.Interrupt();
        }

        private void printDeviceState()
        {
            String strLog = "";

            List<Device_Reference> resultTag = realDeviceInfo.FindAll(x => x.type.Equals("Tag"));
            List<Device_Reference> resultAnchor = realDeviceInfo.FindAll(x => x.type.Equals("Anchor"));
            List<Device_Reference> resultGateway = realDeviceInfo.FindAll(x => x.type.Equals("Gateway"));

            resultTag = resultTag.OrderBy(x => x.serial_num).ToList();
            resultAnchor = resultAnchor.OrderBy(x => x.serial_num).ToList();
            resultGateway = resultGateway.OrderBy(x => x.serial_num).ToList();

            strLog = "================================================\r\n";
            strLog += "===============Device State Check===============\r\n";
            strLog += "================================";
            strLog += DateTime.Now.ToString("yyyy_MM_dd HH:mm");
            strLog += "\r\n";

            Console.WriteLine("================================================");
            Console.WriteLine("===============Device State Check===============");
            Console.Write("================================");
            Console.Write(DateTime.Now.ToString("yyyy_MM_dd HH:mm"));
            Console.WriteLine();

            for (int i = 0; i < resultTag.Count; i++)
            {
                strLog += "TAG " + resultTag[i].serial_num + " 테스트중 ....                  ";
                Console.Write("TAG " + resultTag[i].serial_num + " 테스트중 ....                  ");

                long nowTime = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - resultTag[i].time;
                if (nowTime > config.Alive_TAG)
                {
                    strLog += "Fail";
                    Console.Write("Fail");
                }
                else
                {
                    strLog += "OK";
                    Console.Write("OK");
                }
                strLog += "\r\n";
                Console.WriteLine();
            }

            for (int i = 0; i < resultAnchor.Count; i++)
            {
                strLog += "Anchor " + resultAnchor[i].serial_num + " 테스트중 ....               ";
                Console.Write("Anchor " + resultAnchor[i].serial_num + " 테스트중 ....               ");

                long nowTime = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - resultAnchor[i].time;
                if (nowTime > config.Alive_ANCHOR)
                {
                    if (resultAnchor[i].anchor_initiator == true)
                    {
                        strLog += "OK";
                        Console.Write("OK");
                    }
                    else
                    {
                        strLog += "Fail";
                        Console.Write("Fail");
                    }
                }
                else
                {
                    strLog += "OK";
                    Console.Write("OK");
                }
                strLog += "\r\n";
                Console.WriteLine();
            }

            for (int i = 0; i < resultGateway.Count; i++)
            {
                strLog += "Gateway " + resultGateway[i].serial_num + " 테스트중 ....    ";
                Console.Write("Gateway " + resultGateway[i].serial_num + " 테스트중 ....    ");

                long nowTime = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - resultGateway[i].time;
                if (nowTime > config.Alive_GATEWAY)
                {
                    strLog += "Fail";
                    Console.Write("Fail");
                }
                else
                {
                    strLog += "OK";
                    Console.Write("OK");
                }
                strLog += "\r\n";
                Console.WriteLine();
            }

            config.fn_LogWrite(strLog);
        }

    }
}
