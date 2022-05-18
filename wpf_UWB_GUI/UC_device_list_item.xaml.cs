using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace wpf_UWB_GUI
{
    /// <summary>
    /// UC_device_list_item.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_device_list_item : UserControl
    {
        List<string> listColor = new List<string>();
        Device_Reference realDeviceInfo = new Device_Reference();
        class_filter clFilter = new class_filter();

        MqttConnect mqttConnect;

        //장치명 설정 불러오기
        private List<class_DevName> list_name_item = new List<class_DevName>();


        const int UPDATE_CHECK = 1000 * 1;

        bool fUpdateText = true;
        long prevUpdateTime = 0;


        public UC_device_list_item()
        {
            InitializeComponent();
        }


        public void UpdateDevice()
        {
            long updateTime = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - prevUpdateTime;
            if (updateTime > UPDATE_CHECK)
            {
                prevUpdateTime = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;
                readDeviceNameText();
                fUpdateText = true;
            }

            if (fUpdateText)
            {
                int resultDev1 = list_name_item.FindIndex(x => x.getDev1().Equals(realDeviceInfo.serial_num.ToLower()));
                int resultDev2 = list_name_item.FindIndex(x => x.getDev2().Equals(realDeviceInfo.serial_num.ToLower()));

                realDeviceInfo.nick_name = "Unknown";

                if (resultDev1 != -1)
                    realDeviceInfo.nick_name = list_name_item[resultDev1].getName();
                else if (resultDev2 != -1)
                    realDeviceInfo.nick_name = list_name_item[resultDev2].getName();

                lbl_devSerialNum.Content = realDeviceInfo.serial_num;
                lbl_devType.Content = realDeviceInfo.type;
                lbl_devName.Content = realDeviceInfo.nick_name;
                lbl_devPosition.Content = "";
                lbl_devUpdateRate.Content = realDeviceInfo.tag_nomUpdateRate + " ms";


                int batValue = realDeviceInfo.battery;

                if (batValue > 100) batValue = 100;
                if (batValue < 0) batValue = 0;

                lbl_devBattery.Content = batValue + "%";

                chk_devLowBattery.IsChecked = realDeviceInfo.tag_low_power_mode;


                //Anchor Position Data
                if (realDeviceInfo.type.Contains("Anchor"))
                {
                    lbl_devPosition.Content =
                        Math.Round(realDeviceInfo.anchor_install_x, 1) + "m, " +
                        Math.Round(realDeviceInfo.anchor_install_y, 1) + "m, " +
                        Math.Round(realDeviceInfo.anchor_install_z, 1) + "m";
                    lbl_devFilterPosition.Content =
                        Math.Round(realDeviceInfo.anchor_install_x, 1) + "m, " +
                        Math.Round(realDeviceInfo.anchor_install_y, 1) + "m, " +
                        Math.Round(realDeviceInfo.anchor_install_z, 1) + "m";
                }
                //Gateway Position Data
                else if (realDeviceInfo.type.Contains("Gateway"))
                {
                    lbl_devPosition.Content = realDeviceInfo.gateway_main_ip;
                    lbl_devFilterPosition.Content = realDeviceInfo.gateway_main_ip;
                }

                if (!realDeviceInfo.type.Contains("Tag"))
                {
                    lbl_devBattery.Visibility = Visibility.Hidden;
                    lbl_devUpdateRate.Visibility = Visibility.Hidden;
                    chk_devLowBattery.Visibility = Visibility.Hidden;
                }


                //Color Setting
                string json_device_string = Properties.Settings.Default.JSON_Device_Info;
                //Console.WriteLine("json_device_string:" + json_device_string);
                var device_data = JsonConvert.DeserializeObject<List<Device_Data>>(json_device_string);
                list_DevData = device_data;

                int resultIndex = 0;

                if (list_DevData != null)
                {
                    resultIndex = list_DevData.FindIndex(x => x.DEVICE_ID.Equals(realDeviceInfo.serial_num));
                    if (resultIndex != -1)
                    {
                        //Console.WriteLine(list_DevData[resultIndex].color);
                        color_devColor.SelectedColor = (Color)ColorConverter.ConvertFromString(list_DevData[resultIndex].color);
                    }
                }


                //CHECK STATUS
                if (realDeviceInfo.type.Contains("Tag"))
                {
                    long nowTime = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - realDeviceInfo.time;
                    if (nowTime > config.Alive_TAG)
                        img_devStatus.Source = new BitmapImage(new Uri("/Resources/status_off.png", UriKind.RelativeOrAbsolute));
                    else
                        img_devStatus.Source = new BitmapImage(new Uri("/Resources/status_on.png", UriKind.RelativeOrAbsolute));
                }

                if (realDeviceInfo.type.Contains("Anchor"))
                {
                    long nowTime = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - realDeviceInfo.time;
                    if (nowTime > config.Alive_ANCHOR)
                        img_devStatus.Source = new BitmapImage(new Uri("/Resources/status_off.png", UriKind.RelativeOrAbsolute));
                    else
                        img_devStatus.Source = new BitmapImage(new Uri("/Resources/status_on.png", UriKind.RelativeOrAbsolute));
                }

                if (realDeviceInfo.type.Contains("Gateway"))
                {
                    long nowTime = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - realDeviceInfo.time;
                    if (nowTime > config.Alive_GATEWAY)
                        img_devStatus.Source = new BitmapImage(new Uri("/Resources/status_off.png", UriKind.RelativeOrAbsolute));
                    else
                        img_devStatus.Source = new BitmapImage(new Uri("/Resources/status_on.png", UriKind.RelativeOrAbsolute));
                }

                fUpdateText = false;
            }

            //Tag Position Data
            if (realDeviceInfo.type.Contains("Tag"))
            {
                lbl_devPosition.Content =
                    Math.Round(realDeviceInfo.tag_pos_x, 1) + "m, " +
                    Math.Round(realDeviceInfo.tag_pos_y, 1) + "m, " +
                    Math.Round(realDeviceInfo.tag_pos_z, 1) + "m";

                lbl_devFilterPosition.Content =
                    Math.Round(realDeviceInfo.tag_lpf_x, 1) + "m, " +
                    Math.Round(realDeviceInfo.tag_lpf_y, 1) + "m, " +
                    Math.Round(realDeviceInfo.tag_lpf_z, 1) + "m";
            }
        }

        //Color Changed
        private void colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            string json_device_string = Properties.Settings.Default.JSON_Device_Info;
            var device_data = JsonConvert.DeserializeObject<List<Device_Data>>(json_device_string);
            list_DevData = device_data;

            int resultIndex = 0;

            if (list_DevData != null)
            {
                resultIndex = list_DevData.FindIndex(x => x.DEVICE_ID.Equals(realDeviceInfo.serial_num));
            }
            else
            {
                list_DevData = new List<Device_Data>();
                resultIndex = -1;
            }

            //Console.WriteLine("resultIndex:" + resultIndex);

            if (resultIndex == -1)
            {
                //Not Exsist
                Device_Data devData = new Device_Data();
                devData.DEVICE_ID = realDeviceInfo.serial_num;
                devData.color = color_devColor.SelectedColorText;
                //devData.nick_name = lbl_name.Text;
                list_DevData.Add(devData);

                string json = JsonConvert.SerializeObject(list_DevData);
                //Console.WriteLine("json:");
                //Console.WriteLine(json);
                Properties.Settings.Default.JSON_Device_Info = json;
                Properties.Settings.Default.Save();
            }
            else
            {
                //Exsist
                list_DevData[resultIndex].DEVICE_ID = realDeviceInfo.serial_num;
                list_DevData[resultIndex].color = color_devColor.SelectedColor.ToString();
                //list_DevData[resultIndex].nick_name = lbl_name.Text;

                string json = JsonConvert.SerializeObject(list_DevData);
                //Console.WriteLine("json:");
                //Console.WriteLine(json);
                Properties.Settings.Default.JSON_Device_Info = json;
                Properties.Settings.Default.Save();
            }
        }

        private void chk_devLowBattery_Click(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine("chk_devLowBattery_Click OCCUR !!! : " + realDeviceInfo.serial_num + " : " + chk_devLowBattery.IsChecked);

            if (realDeviceInfo.type.Contains("Tag"))
            {
                String devName = realDeviceInfo.serial_num.Replace("DW", "");
                devName = devName.ToLower();

                String topic = "dwm/node/" + devName + "/downlink/config";

                String payload = "{\"configuration\":{" +
                    "\"label\":" + "\"" + realDeviceInfo.serial_num + "\"" + "," +
                    "\"ble\":" + "true" + "," +
                    "\"nodeType\":" + "\"TAG\"" + "," +
                    "\"uwbFirmwareUpdate\":" + "false" + "," +
                    "\"leds\":" + "false" + "," +
                    "\"tag\":" + "{" +
                    "\"locationEngine\":" + "true" + "," +
                    "\"responsive\":" + chk_devLowBattery.IsChecked.ToString().ToLower() + "," +
                    "\"nomUpdateRate\":" + lbl_devUpdateRate.Content + "," +
                    "\"statUpdateRate\":" + "100" + "," +
                    "\"stationaryDetection\" : " + "false" +
                    "}" + "}" + "}";

                //if (mqttConnect.IsConnected()) mqttConnect.mqtt_Pub(topic, payload, false);
            }
        }


        //readDeviceName -> setList
        private void readDeviceNameText()
        {
            String readText = fn_DeviceNameRead();
            JObject jsonDeviceName = null;

            if (readText.Length != 0)
            {
                jsonDeviceName = JObject.Parse(readText);
            }

            list_name_item.Clear();

            for (int i = 0; i < config.arNumBucket.Length; i++)
            {
                class_DevName clDevName = new class_DevName();

                clDevName.setName("버킷" + (config.arNumBucket[i]));

                if (readText.Length != 0)
                {
                    String strDevName = jsonDeviceName[clDevName.getName()].ToString();
                    string[] dev = strDevName.Split(',');
                    clDevName.setDev1(dev[0].ToLower());
                    clDevName.setDev2(dev[1].ToLower());
                }
                list_name_item.Add(clDevName);
            }
        }

        //DeviceName file read
        private String fn_DeviceNameRead()
        {
            String DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\UWB\\_UWB_DEVICE_NAME.txt";
            String strRead = "";
            try
            {
                strRead = System.IO.File.ReadAllText(@DirPath);
            }
            catch (Exception e)
            {
                //fn_ErrorFileWrite("fn_DeviceNameRead Error : " + e.ToString());
                return "";
            }
            return strRead;
        }

        public void setMqttConnect(MqttConnect mMqttConnect)
        {
            mqttConnect = mMqttConnect;
        }

        public void setDeviceValue(Device_Reference mRealDeviceInfo) { realDeviceInfo = mRealDeviceInfo; }
        public String getSerialName() { return lbl_devSerialNum.Content.ToString(); }
        public String getDeviceName() { return lbl_devSerialNum.Content.ToString(); }
        public String getDeviceType() { return lbl_devType.Content.ToString(); }
        public String getPositionRaw() { return lbl_devPosition.Content.ToString(); }
        public String getPositionFilter() { return lbl_devFilterPosition.Content.ToString(); }
        public String getUR() { return lbl_devUpdateRate.Content.ToString(); }
        public String getBattery() { return lbl_devBattery.Content.ToString(); }
        public bool getLB_Check() { return (bool)chk_devLowBattery.IsChecked; }
        public Device_Reference getDeviceReference() { return realDeviceInfo; }

        private List<Device_Data> list_DevData = new List<Device_Data>();

    }
}
