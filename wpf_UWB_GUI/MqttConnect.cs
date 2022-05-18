using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace wpf_UWB_GUI
{
    public class MqttConnect
    {

        private bool DEBUG = Properties.Settings.Default.DEBUG;

        public string MQTT_IP, MQTT_PORT;

        Thread connectThread, retryConnectThread;

        const int def_DISCONNECT = 0;
        const int def_CONNECTING = 1;
        const int def_CONNECTED = 2;

        //public bool conneectButtonClick = false;
        public bool startClicked = true;
        public bool stopClicked = false;
        private bool isReconnect = true;
        public string connectedStatus = "DISCONNECTED";

        private IMqttClient client = null;

        public Main_Setting_Data mainSettingData = new Main_Setting_Data();
        private MqttParsing parsing = new MqttParsing();
        public List<Device_Reference> device_References = new List<Device_Reference>();

        public void init()
        {
            try
            {
                connectThread = new Thread(new ThreadStart(Run1));
                connectThread.Start();

                retryConnectThread = new Thread(new ThreadStart(RunRetry));
                retryConnectThread.Start();

                parsing.initialize_timer();
            }
            catch (Exception e)
            {
                fn_ErrorFileWrite("MqttConnect Error : " + e.ToString());
            }
        }

        int retry_count = 0;

        //AUTO RECONNECT
        private void RunRetry()
        {
            while (true)
            {
                if (connectedStatus.Equals("DISCONNECTED"))
                {
                    retry_count++;

                    if (retry_count % 10 == 0)
                    {
                        Console.WriteLine("Retry " + retry_count / 10);
                    }

                    if (retry_count > 50)
                    {
                        retry_count = 0;
                        startClicked = true;
                    }
                }
                else
                {
                    retry_count = 0;
                }

                Thread.Sleep(100);
            }
        }

        //AUTO RECONNECT
        void Run1()
        {
            while (true)
            {
                if (MQTT_IP != null && MQTT_PORT != null)
                {
                    if (startClicked)
                    {
                        if (DEBUG) Console.WriteLine("START MQTT_IP:" + MQTT_IP + "  MQTT_PORT:" + MQTT_PORT);
                        startClicked = false;

                        start_button_click();
                    }

                    if (stopClicked)
                    {
                        if (DEBUG) Console.WriteLine("STOP MQTT_IP:" + MQTT_IP + "  MQTT_PORT:" + MQTT_PORT);
                        stopClicked = false;

                        stop_button_click();
                    }
                }
            }
        }

        private void start_button_click()
        {
            if (DEBUG) Console.WriteLine("start_button_click()");
            connectedStatus = "CONNECTING";
            mainSettingData.MQTT_status_connect = def_CONNECTING;
            isReconnect = true;
            ConnectMqttServerAsync();
        }

        private void stop_button_click()
        {
            if (DEBUG) Console.WriteLine("stop_button_click()");
            connectedStatus = "DISCONNECTED";
            mainSettingData.MQTT_status_connect = def_DISCONNECT;
            isReconnect = false;
            client.DisconnectAsync();
        }

        //private void mqtt_connect_button_click()
        //{
        //    if (connectedStatus.Equals("DISCONNECTED"))
        //    {
        //        connectedStatus = "CONNECTING";
        //        isReconnect = true;
        //        ConnectMqttServerAsync();
        //    }
        //    else
        //    {
        //        connectedStatus = "DISCONNECTED";
        //        mainSettingData.MQTT_status_connect = def_DISCONNECT;
        //        isReconnect = false;
        //        client.DisconnectAsync();
        //    }
        //}

        public void ConnectMqttServerAsync()
        {
            if (DEBUG) Console.WriteLine("ConnectMqttServerAsync()");
            if (client == null)
            {
                var factory = new MqttFactory();
                client = factory.CreateMqttClient();


                //client.ApplicationMessageReceivedHandler += MqttClient_ApplicationMessageReceived;
                //client.ConnectedHandler += MqttClient_Connected;
                //client.DisconnectedHandler += MqttClient_Disconnected;
            }

            try
            {
                string MQTT_Server_IP, MQTT_Server_Port;
                MQTT_Server_IP = MQTT_IP;
                MQTT_Server_Port = MQTT_PORT;

                mainSettingData.MQTT_status_connect = def_CONNECTING;

                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(MQTT_Server_IP, Int32.Parse(MQTT_Server_Port))
                    //.WithCleanSession(true)
                    //.WithCleanSession(false)
                    .WithCleanSession()
                    .Build();

                client.UseConnectedHandler(e => OnClientConnected(e));
                client.UseDisconnectedHandler(e => OnClientDisconnected(e));
                client.UseApplicationMessageReceivedHandler(e => OnMessageReceived(e));

                client.ConnectAsync(options);

                Console.WriteLine("Connecting to Server : " + MQTT_Server_IP + ", " + MQTT_Server_Port);
            }
            catch (Exception ex)
            {
                fn_ErrorFileWrite("MQTT 서버에 연결하지 못했습니다. Error : " + Environment.NewLine + ex.Message + Environment.NewLine);
                mqtt_disconnected();
            }
        }



        ////Get MQTT Message()
        private void OnMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            //Console.WriteLine($"OnMessageReceived: {e.ApplicationMessage.Topic}, {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            try
            {
                parsing.Main_Setting_Data = mainSettingData;
                parsing.parsing_MQTT_data(e);
                device_References = parsing.raw_received_device_info_buf;
                checkConnect();
            }
            catch (Exception ex)
            {
                //fn_ErrorFileWrite("MqttClient_ApplicationMessageReceived Error : " + ex.ToString());
            }
        }

        //Connect Server
        private void OnClientConnected(MqttClientConnectedEventArgs e)
        {
            Console.WriteLine("MQTT 서버에 연결됨" + Environment.NewLine);
            mqtt_connected();
        }

        //DisConnect Server
        private void OnClientDisconnected(MqttClientDisconnectedEventArgs e)
        {
            DateTime curTime = new DateTime();
            curTime = DateTime.UtcNow;
            Console.WriteLine("MQTT 연결이 끊어졌습니다." + Environment.NewLine);

            if (isReconnect)
            {
                Console.WriteLine("다시 연결 시도" + Environment.NewLine);

                string MQTT_Server_IP, MQTT_Server_Port;
                MQTT_Server_IP = MQTT_IP;
                MQTT_Server_Port = MQTT_PORT;
                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(MQTT_Server_IP, Int32.Parse(MQTT_Server_Port))
                    .WithCleanSession()
                    .Build();
            }
            else
            {
                Console.WriteLine("오프라인" + Environment.NewLine);
            }
            mqtt_disconnected();
        }


        //private void MqttClient_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        //{
        //    try
        //    {
        //        parsing.Main_Setting_Data = mainSettingData;
        //        parsing.parsing_MQTT_data(e);
        //        device_References = parsing.raw_received_device_info_buf;
        //        checkConnect();
        //    }
        //    catch (Exception ex)
        //    {
        //        //fn_ErrorFileWrite("MqttClient_ApplicationMessageReceived Error : " + ex.ToString());
        //    }
        //}

        //private void MqttClient_Connected(object sender, EventArgs e)
        //{
        //    Console.WriteLine("MQTT 서버에 연결됨" + Environment.NewLine);
        //    mqtt_connected();
        //}

        //private void MqttClient_Disconnected(object sender, EventArgs e)
        //{
        //    DateTime curTime = new DateTime();
        //    curTime = DateTime.UtcNow;
        //    Console.WriteLine("MQTT 연결이 끊어졌습니다." + Environment.NewLine);

        //    if (isReconnect)
        //    {
        //        Console.WriteLine("다시 연결 시도" + Environment.NewLine);

        //        string MQTT_Server_IP, MQTT_Server_Port;
        //        MQTT_Server_IP = MQTT_IP;
        //        MQTT_Server_Port = MQTT_PORT;
        //        var options = new MqttClientOptionsBuilder()
        //            .WithTcpServer(MQTT_Server_IP, Int32.Parse(MQTT_Server_Port))
        //            .WithCleanSession()
        //            .Build();
        //    }
        //    else
        //    {
        //        Console.WriteLine("오프라인" + Environment.NewLine);
        //    }
        //    mqtt_disconnected();
        //}

        private async void mqtt_connected()
        {
            //if (DEBUG) Console.WriteLine("mqtt_connected()");
            connectedStatus = "CONNECTED";
            mainSettingData.MQTT_status_connect = def_CONNECTED;
            await Subscribe();
            checkConnect();
        }

        private void mqtt_disconnected()
        {
            if (DEBUG) Console.WriteLine("mqtt_disconnected()");
            connectedStatus = "DISCONNECTED";
            mainSettingData.MQTT_status_connect = def_DISCONNECT;
            checkConnect();

            //materialButton_mqtt_login.Text = "MQTT Login";
        }

        public void mqtt_Sub(String topic)
        {
            mqttSub(topic);
        }

        private async void mqttSub(String topic)
        {
            //연결 끊겼을 시 데이터를 퍼블리시 할 수 없음
            //연결이 올바르지 않을 시 퍼블리시 타임아웃 시간 정해야할듯
            //MQTTnet.Exceptions.MqttCommunicationTimedOutException: ''MQTTnet.Exceptions.MqttCommunicationTimedOutException' 형식의 예외가 Throw되었습니다.'
            try
            {
                await SubscribeTopic(topic);
            }
            catch (Exception e)
            {
                fn_ErrorFileWrite("mqttSub Error : " + e.ToString());
            }
        }

        private async Task SubscribeTopic(String topic)
        {
            //if (DEBUG) Console.WriteLine("SubscribeTopic() : " + topic);

            //Console.WriteLine("Test MQTT Topic List ");
            await client.SubscribeAsync(new TopicFilterBuilder()
                .WithTopic(topic)
                .WithExactlyOnceQoS()
                //.WithAtMostOnceQoS()
                .Build());
        }

        private async Task Subscribe()
        {
            //if (DEBUG) Console.WriteLine("Subscribe()");
            string topic;

            //Console.WriteLine("Test MQTT Topic List ");
            topic = "#";
            if (client != null)
                await client.SubscribeAsync(new TopicFilterBuilder()
                .WithTopic(topic)
                .WithAtMostOnceQoS()
                .Build());
        }

        public void mqtt_Pub(String topic, String message, bool retain)
        {
            mqttPub(topic, message, retain);
        }

        private async void mqttPub(String topic, String message, bool retain)
        {
            //연결 끊겼을 시 데이터를 퍼블리시 할 수 없음
            //연결이 올바르지 않을 시 퍼블리시 타임아웃 시간 정해야할듯
            //MQTTnet.Exceptions.MqttCommunicationTimedOutException: ''MQTTnet.Exceptions.MqttCommunicationTimedOutException' 형식의 예외가 Throw되었습니다.'
            try
            {
                await Publisher(topic, message, retain);
            }
            catch (Exception e)
            {
                //fn_ErrorFileWrite("mqttPub Error : " + e.ToString());
            }
        }

        private async Task Publisher(String topic, String message, bool retain)
        {
            //if (DEBUG) Console.WriteLine("Publisher()");
            //if (DEBUG) Console.WriteLine("topic:" + topic);
            //if (DEBUG) Console.WriteLine("message:" + message);

            try
            {
                if (client != null)
                    await client.PublishAsync(new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload(message)
                        .WithExactlyOnceQoS()
                        .WithRetainFlag(retain)
                        .Build());

                //public MqttApplicationMessageBuilder WithAtLeastOnceQoS();
                //public MqttApplicationMessageBuilder WithAtMostOnceQoS();
                //public MqttApplicationMessageBuilder WithExactlyOnceQoS();
                //At most once = 0
                //At least once = 1
                //Exactly once = 2
            }
            catch (TaskCanceledException)
            {
                //Console.WriteLine("\nTasks cancelled : timed out.\n");
            }
        }

        public bool IsConnected()
        {
            if (client == null) return false;
            return client.IsConnected;
        }

        private void checkConnect()
        {
            if (client.IsConnected)
            {
                mainSettingData.MQTT_status_connect = def_CONNECTED;
            }
            else
            {
                mainSettingData.MQTT_status_connect = def_DISCONNECT;
            }
        }

        //Error file write
        private void fn_ErrorFileWrite(string str)
        {
            string DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\UWB\\";

            string strFormat = "yyyy_MM";
            String textFileName = DateTime.Now.ToString(strFormat) + "_ERROR_LOG.txt";

            string FilePath = DirPath + "\\" + textFileName;
            string temp;

            DirectoryInfo di = new DirectoryInfo(DirPath);
            FileInfo fi = new FileInfo(FilePath);

            try
            {
                if (!di.Exists) Directory.CreateDirectory(DirPath);
                if (!fi.Exists)
                {
                    using (StreamWriter sw = new StreamWriter(FilePath))
                    {
                        temp = string.Format("[{0}] {1}", DateTime.Now, str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(FilePath))
                    {
                        temp = string.Format("[{0}] {1}", DateTime.Now, str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("fn_ErrorFileWrite : " + e.ToString());
            }
        }
    }
}
