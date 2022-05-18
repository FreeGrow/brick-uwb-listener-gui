using MQTTnet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Shapes;

namespace wpf_UWB_GUI
{
    class MqttParsing
    {

        bool DEBUG = Properties.Settings.Default.DEBUG;

        public List<Device_Reference> raw_received_device_info_buf = new List<Device_Reference>();
        public Main_Setting_Data Main_Setting_Data = new Main_Setting_Data();
        //public List<All_Scanned_Infos> my_all_scanned_infos = new List<All_Scanned_Infos>();            // 이거 나중에 지울 예정

        private readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public void initialize_timer()
        {
            try
            {
                Timer timer = new System.Timers.Timer();
                timer.Interval = 1000;
                timer.Elapsed += new ElapsedEventHandler(timer_for_cal_hz);
                timer.Start();
            }
            catch (Exception e)
            {
                fn_ErrorFileWrite("MqttParsing Error : " + e.ToString());
            }
        }

        private void timer_for_cal_hz(object sender, ElapsedEventArgs e)
        {
            try
            {
                for (int i = 0; i < raw_received_device_info_buf.Count; i++)
                {
                    if (raw_received_device_info_buf[i] == null) return;
                    if (raw_received_device_info_buf[i].type == null) return;

                    if (raw_received_device_info_buf[i].type.Equals("Tag"))
                    {
                        raw_received_device_info_buf[i].tag_distance_rcv_hz = raw_received_device_info_buf[i].tag_distance_rcv_cnt;
                        raw_received_device_info_buf[i].tag_location_rcv_hz = raw_received_device_info_buf[i].tag_location_rcv_cnt;

                        raw_received_device_info_buf[i].tag_distance_rcv_cnt = 0;
                        raw_received_device_info_buf[i].tag_location_rcv_cnt = 0;

                        //Console.WriteLine("ID : " + raw_received_device_info_buf[i].serial_num + ", " + raw_received_device_info_buf[i].tag_location_rcv_hz);
                    }
                }
            }
            catch (Exception e1)
            {
                fn_ErrorFileWrite("timer_for_cal_hz Error : " + e.ToString());
            }
        }

        public void parsing_MQTT_data(MqttApplicationMessageReceivedEventArgs e)
        {
            if (e == null) return;
            //Console.WriteLine("parsing_MQTT_data()");
            //print_raw_device_info_buf();

            String Topic_name = e.ApplicationMessage.Topic;
            //Console.WriteLine("topic : " + Topic_name); //ok

            String Topic_Message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            JObject obj_T = JObject.Parse(Topic_Message);

            if (Topic_name.Contains("dwm/"))
            {
                if (Topic_name.Contains("/uplink"))
                {
                    if (Topic_name.Contains("/gateway/"))
                    {
                        //// check Gateway Info
                        parsing_Gateway_config(Topic_name, e);
                    }
                    else if (Topic_name.Contains("/node/") && Topic_name.Contains("/config"))
                    {
                        //// check Anchor Info
                        parsing_node_config(Topic_name, e);
                    }
                    else if (Topic_name.Contains("/node/") && Topic_name.Contains("/status"))
                    {
                        //// check Anchor Info
                        parsing_node_status(Topic_name, e);
                    }
                    else if (Topic_name.Contains("/node/"))
                    {
                        if (Topic_name.Contains("/location"))
                        {
                            //// check Tag Location
                            parsing_Tag_location(Topic_name, e);            // 이름이 잘못됨 config가 아님
                        }
                        else if (Topic_name.Contains("/data"))
                        {
                            /// check Tag Range Data //
                            parsing_Tag_range_data(Topic_name, e);
                        }
                    }
                }
                else if (Topic_name.Contains("/gateway/anchor"))
                {
                    parsing_Alive_config(Topic_name, e);
                }
            }
        }


        private int check_list_added_already(string rcv_serial_num, List<Device_Reference> rcv_device_reference)
        {
            try
            {
                if (rcv_device_reference != null)
                {
                    for (int i = 0; i < rcv_device_reference.Count; i++)
                    {
                        //Null Reference error
                        //Count = 6
                        //i = 4 == null
                        try
                        {
                            if (rcv_device_reference[i] != null)
                            {
                                if ((rcv_serial_num != null) & (rcv_device_reference[i].serial_num != null))
                                {
                                    if (rcv_serial_num.Equals(rcv_device_reference[i].serial_num))
                                    {
                                        return i;
                                    }
                                }
                            }
                        }
                        catch (Exception e1)
                        {
                            fn_ErrorFileWrite("check_list_added_already Error : " + e1.ToString());
                        }
                    }
                }
            }
            catch
            {

            }
            return -1;
        }

        public void parsing_Alive_config(String Topic_name, MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                String Topic_Message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                //Console.WriteLine("parsing_Alive_config : " + Topic_name + "    " + Topic_Message);

                JObject obj_T = JObject.Parse(Topic_Message);
                IList<string> test = obj_T.SelectToken("anchor").Select(s => (string)s).ToList();

                fn_LogWrite(Topic_name + "    " + Topic_Message);

                //Console.WriteLine("Topic_name : " + Topic_name);
                //Console.WriteLine("Topic_Message : " + Topic_Message);

                String debugStr = "";

                for (int i = 0; i < test.Count; i++)
                {
                    int data_exis_num = check_list_added_already(test[i], raw_received_device_info_buf);
                    if (data_exis_num != -1)
                    {
                        // 데이터 이미 존재 //
                        raw_received_device_info_buf[data_exis_num].time = (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
                        debugStr += test[i] + " " + data_exis_num + " ";
                        //Console.Write(test[i] + "  " + data_exis_num + " ");
                    }
                }
                //Console.WriteLine();

                fn_LogWrite(debugStr);

                //Console.WriteLine("parsing_Gateway_config()");
                //Console.WriteLine("Topic_name : " + Topic_name);
                //Console.WriteLine("message : " + Topic_Message);

                //string ip_info = "";
                //if (test.Count > 1) ip_info = test[1];
            }
            catch
            {

            }
        }

        public void parsing_Gateway_config(String Topic_name, MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                int T_Start = Topic_name.IndexOf("deca");
                int T_End = Topic_name.IndexOf("/uplink");
                String rcv_serial_num = "deca" + Topic_name.Substring(T_Start + 4, 12);

                String Topic_Message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                JObject obj_T = JObject.Parse(Topic_Message);
                IList<string> test = obj_T.SelectToken("ipAddress").Select(s => (string)s).ToList();

                //Console.WriteLine("parsing_Gateway_config()");
                //Console.WriteLine("Topic_name : " + Topic_name);
                //Console.WriteLine("message : " + Topic_Message);

                string ip_info = "";
                if (test.Count > 1) ip_info = test[1];

                int data_exis_num = check_list_added_already(rcv_serial_num, raw_received_device_info_buf);
                if (data_exis_num == -1)
                {
                    // 데이터 X... Add 해야됨
                    Device_Reference tmp_device_reference = new Device_Reference();
                    tmp_device_reference.serial_num = rcv_serial_num;
                    tmp_device_reference.type = "Gateway";
                    tmp_device_reference.gateway_main_ip = ip_info;
                    tmp_device_reference.time = (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;

                    raw_received_device_info_buf.Add(tmp_device_reference);
                }
                else
                {
                    // 데이터 이미 존재 //
                    raw_received_device_info_buf[data_exis_num].gateway_main_ip = ip_info;
                    raw_received_device_info_buf[data_exis_num].time = (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
                }
            }
            catch
            {

            }

        }

        public void parsing_node_config(String Topic_name, MqttApplicationMessageReceivedEventArgs e)
        {
            //Console.WriteLine("parsing_node_config()");
            try
            {
                String my_name;
                int T_Start = Topic_name.IndexOf("node/");
                my_name = Topic_name.Substring(T_Start + 5, 4);

                String rcv_serial_num = "DW" + my_name.ToUpper();

                String Topic_Message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                if (Topic_Message.Contains("ANCHOR"))
                {
                    JObject obj_T = JObject.Parse(Topic_Message);

                    //                JObject label_id = (JObject)obj_T.SelectToken("label");

                    //                String position = (string)obj_T.SelectToken("position");
                    JObject configuration = (JObject)obj_T.SelectToken("configuration");
                    string label_id = rcv_serial_num;
                    JObject anchor_info = (JObject)configuration.SelectToken("anchor");
                    JObject anchor_position = (JObject)anchor_info.SelectToken("position");
                    string position_x = (string)anchor_position.SelectToken("x");
                    string position_y = (string)anchor_position.SelectToken("y");
                    string position_z = (string)anchor_position.SelectToken("z");
                    bool initiator = (bool)anchor_info.SelectToken("initiator");

                    //Console.WriteLine("ID : " + label_id + "\t X : " + position_x + "\t Y : " + position_y + "\t Z : " + position_z + "\t Initiator : " + initiator);

                    double pos_x = Convert.ToDouble(position_x);
                    double pos_y = Convert.ToDouble(position_y);
                    double pos_z = Convert.ToDouble(position_z);


                    int data_exis_num = check_list_added_already(rcv_serial_num, raw_received_device_info_buf);
                    if (data_exis_num == -1)
                    {
                        // 데이터 X... Add 해야됨
                        Device_Reference tmp_device_reference = new Device_Reference();
                        tmp_device_reference.serial_num = rcv_serial_num;
                        tmp_device_reference.type = "Anchor";
                        tmp_device_reference.anchor_initiator = initiator;
                        tmp_device_reference.anchor_install_x = pos_x;
                        tmp_device_reference.anchor_install_y = pos_y;
                        tmp_device_reference.anchor_install_z = pos_z;
                        //tmp_device_reference.time = (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;

                        raw_received_device_info_buf.Add(tmp_device_reference);
                    }
                    else
                    {
                        // 데이터 이미 존재 //
                        raw_received_device_info_buf[data_exis_num].anchor_initiator = initiator;
                        raw_received_device_info_buf[data_exis_num].anchor_install_x = pos_x;
                        raw_received_device_info_buf[data_exis_num].anchor_install_y = pos_y;
                        raw_received_device_info_buf[data_exis_num].anchor_install_z = pos_z;
                        //raw_received_device_info_buf[data_exis_num].time = (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
                    }

                }
                else if (Topic_Message.Contains("TAG"))
                {
                    JObject obj_T = JObject.Parse(Topic_Message);

                    //                JObject label_id = (JObject)obj_T.SelectToken("label");

                    //                String position = (string)obj_T.SelectToken("position");
                    JObject configuration = (JObject)obj_T.SelectToken("configuration");
                    string label_id = rcv_serial_num;
                    JObject tag_info = (JObject)configuration.SelectToken("tag");
                    double norm_update_rate = (double)tag_info.SelectToken("nomUpdateRate");
                    //bool low_power_mode = (bool)tag_info.SelectToken("stationaryDetection");
                    bool low_power_mode = (bool)tag_info.SelectToken("responsive");

                    //Console.WriteLine(rcv_serial_num + "  /  " + low_power_mode);

                    int data_exis_num = check_list_added_already(rcv_serial_num, raw_received_device_info_buf);
                    if (data_exis_num == -1)
                    {
                        // 데이터 X... Add 해야됨
                        Device_Reference tmp_device_reference = new Device_Reference();
                        tmp_device_reference.serial_num = rcv_serial_num;
                        tmp_device_reference.type = "Tag";
                        tmp_device_reference.tag_nomUpdateRate = norm_update_rate;
                        tmp_device_reference.tag_low_power_mode = low_power_mode;

                        raw_received_device_info_buf.Add(tmp_device_reference);
                    }
                    else
                    {
                        // 데이터 이미 존재 //
                        raw_received_device_info_buf[data_exis_num].tag_nomUpdateRate = norm_update_rate;
                        raw_received_device_info_buf[data_exis_num].tag_low_power_mode = low_power_mode;
                    }
                }
            }
            catch
            {

            }

        }

        public void parsing_node_status(String Topic_name, MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                String my_name;
                int T_Start = Topic_name.IndexOf("node/");
                my_name = Topic_name.Substring(T_Start + 5, 4);

                String rcv_serial_num = "DW" + my_name.ToUpper();

                String Topic_Message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                JObject obj_T = JObject.Parse(Topic_Message);
                bool present = (bool)obj_T.SelectToken("present");


                int data_exis_num = check_list_added_already(rcv_serial_num, raw_received_device_info_buf);
                if (data_exis_num == -1)
                {
                    // 데이터 X... Add 해야됨
                    //raw_received_device_info_buf[data_exis_num].alive = present;)
                    //if(present == true) raw_received_device_info_buf[data_exis_num].time = (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
                    //Console.WriteLine("rcv_serial_num:" + rcv_serial_num + "  present : " + present + "    " + (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds);
                }
                else
                {
                    // 데이터 이미 존재 갱신 //
                    //raw_received_device_info_buf[data_exis_num].alive = present;
                    //if (present == true) raw_received_device_info_buf[data_exis_num].time = (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
                    //Console.WriteLine("rcv_serial_num:" + rcv_serial_num + "  present : " + present + "    " + (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds);
                }
            }
            catch
            {

            }
        }

        private void Position_from_distance(int rcv_data_exis_num, List<Dist_Info> rcv_dist_info)
        {
            /// Calculate the Position from Distance ///////
            Trilateration trilateration = new Trilateration();

            Anchor[] anchor_searched = new Anchor[raw_received_device_info_buf[rcv_data_exis_num].dist_Infos.Count];
            RectangleF[] circle_searched = new RectangleF[raw_received_device_info_buf[rcv_data_exis_num].dist_Infos.Count];

            try
            {
                for (int j = 0; j < raw_received_device_info_buf[rcv_data_exis_num].dist_Infos.Count; j++)
                {
                    //Error Null Reference
                    if (anchor_searched[j] == null) return;
                    anchor_searched[j] = Function.anchor_search_from_list(raw_received_device_info_buf[rcv_data_exis_num].dist_Infos[j].from_Anchor, raw_received_device_info_buf);

                    if (circle_searched[j] == null) return;
                    circle_searched[j].Width = (float)raw_received_device_info_buf[rcv_data_exis_num].dist_Infos[j].dist * 2;
                    //Error Occur NullReference
                    circle_searched[j].Height = (float)raw_received_device_info_buf[rcv_data_exis_num].dist_Infos[j].dist * 2;
                    circle_searched[j].X = (float)anchor_searched[j].x - (float)raw_received_device_info_buf[rcv_data_exis_num].dist_Infos[j].dist;
                    circle_searched[j].Y = (float)anchor_searched[j].y - (float)raw_received_device_info_buf[rcv_data_exis_num].dist_Infos[j].dist;
                }
                double[] calculated_position = new double[3];
                if (raw_received_device_info_buf[rcv_data_exis_num].dist_Infos.Count == 3)
                {
                    PointF[] result_point = new PointF[3];
                    result_point = trilateration.Trilaterate(circle_searched[0], circle_searched[1], circle_searched[2]);
                    double x, y;
                    x = (result_point[0].X + result_point[1].X + result_point[2].X) / 3.0;
                    y = (result_point[0].Y + result_point[1].Y + result_point[2].Y) / 3.0;
                    calculated_position[0] = x;
                    calculated_position[1] = y;
                    calculated_position[2] = 0;
                }
                else if (raw_received_device_info_buf[rcv_data_exis_num].dist_Infos.Count == 4)
                {
                    PointF[] result_point = new PointF[3];
                    double x1, x2, x3, x4, y1, y2, y3, y4;
                    result_point = trilateration.Trilaterate(circle_searched[0], circle_searched[1], circle_searched[2]);
                    x1 = (result_point[0].X + result_point[1].X + result_point[2].X) / 3.0;
                    y1 = (result_point[0].Y + result_point[1].Y + result_point[2].Y) / 3.0;
                    result_point = trilateration.Trilaterate(circle_searched[0], circle_searched[1], circle_searched[3]);
                    x2 = (result_point[0].X + result_point[1].X + result_point[2].X) / 3.0;
                    y2 = (result_point[0].Y + result_point[1].Y + result_point[2].Y) / 3.0;
                    result_point = trilateration.Trilaterate(circle_searched[0], circle_searched[2], circle_searched[3]);
                    x3 = (result_point[0].X + result_point[1].X + result_point[2].X) / 3.0;
                    y3 = (result_point[0].Y + result_point[1].Y + result_point[2].Y) / 3.0;
                    result_point = trilateration.Trilaterate(circle_searched[1], circle_searched[2], circle_searched[3]);
                    x4 = (result_point[0].X + result_point[1].X + result_point[2].X) / 3.0;
                    y4 = (result_point[0].Y + result_point[1].Y + result_point[2].Y) / 3.0;

                    int NOT_NAN_X_Count = 0;
                    int NOT_NAN_Y_Count = 0;

                    double sum_NOT_NAN_X = 0;
                    double sum_NOT_NAN_Y = 0;

                    if (!Double.IsNaN(x1)) { NOT_NAN_X_Count++; sum_NOT_NAN_X += x1; }
                    if (!Double.IsNaN(x2)) { NOT_NAN_X_Count++; sum_NOT_NAN_X += x2; }
                    if (!Double.IsNaN(x3)) { NOT_NAN_X_Count++; sum_NOT_NAN_X += x3; }
                    if (!Double.IsNaN(x4)) { NOT_NAN_X_Count++; sum_NOT_NAN_X += x4; }

                    if (!Double.IsNaN(y1)) { NOT_NAN_Y_Count++; sum_NOT_NAN_Y += y1; }
                    if (!Double.IsNaN(y2)) { NOT_NAN_Y_Count++; sum_NOT_NAN_Y += y2; }
                    if (!Double.IsNaN(y3)) { NOT_NAN_Y_Count++; sum_NOT_NAN_Y += y3; }
                    if (!Double.IsNaN(y4)) { NOT_NAN_Y_Count++; sum_NOT_NAN_Y += y4; }

                    calculated_position[0] = (sum_NOT_NAN_X) / (double)NOT_NAN_X_Count;
                    calculated_position[1] = (sum_NOT_NAN_Y) / (double)NOT_NAN_Y_Count;
                    calculated_position[2] = 0;
                    //calculated_position[0] = (x1 + x2 + x3 + x4) / 4.0;
                    //calculated_position[1] = (y1 + y2 + y3 + y4) / 4.0;
                    //calculated_position[2] = 0;
                }

                double filter_value = Main_Setting_Data.filter_value;
                if (raw_received_device_info_buf[rcv_data_exis_num].dist_Infos.Count >= 3)
                {
                    double tmp_est_x = raw_received_device_info_buf[rcv_data_exis_num].tag_est_x;
                    double tmp_est_y = raw_received_device_info_buf[rcv_data_exis_num].tag_est_y;
                    double tmp_est_z = raw_received_device_info_buf[rcv_data_exis_num].tag_est_z;

                    if (!Double.IsNaN(calculated_position[0])) raw_received_device_info_buf[rcv_data_exis_num].tag_est_x = tmp_est_x * filter_value + calculated_position[0] * (1 - filter_value);
                    if (!Double.IsNaN(calculated_position[1])) raw_received_device_info_buf[rcv_data_exis_num].tag_est_y = tmp_est_y * filter_value + calculated_position[1] * (1 - filter_value);
                    if (!Double.IsNaN(calculated_position[2])) raw_received_device_info_buf[rcv_data_exis_num].tag_est_z = tmp_est_z * filter_value + calculated_position[2] * (1 - filter_value);

                    raw_received_device_info_buf[rcv_data_exis_num].tag_distance_rcv_cnt++;

                    //Console.WriteLine("1 : " + raw_received_device_info_buf[rcv_data_exis_num]);
                    //Console.WriteLine("EST LPF : " + tags[i].label + "\t : " + tags[i].lpf_est_x);
                }
                //Console.WriteLine(tags[i].label + "\t ] : " + tags[i].est_x + "\t , : " + tags[i].est_y);
            }
            catch { }

        }


        public void parsing_Tag_range_data(String Topic_name, MqttApplicationMessageReceivedEventArgs e)
        {
            String my_name;
            int T_Start = Topic_name.IndexOf("node/");
            my_name = Topic_name.Substring(T_Start + 5, 4);

            String rcv_serial_num = "DW" + my_name.ToUpper();

            int data_exis_num = check_list_added_already(rcv_serial_num, raw_received_device_info_buf);
            if (data_exis_num == -1)
            {

            }
            else
            {
                String strTag = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                JObject obj_T = JObject.Parse(strTag);
                String data = (string)obj_T.SelectToken("data");

                var bytes = Convert.FromBase64String(data);

                UInt32 bat_value = (UInt32)(bytes[1 + 6 * bytes[0]] | bytes[1 + 6 * bytes[0] + 1] << 8 | bytes[1 + 6 * bytes[0] + 2] << 16 | bytes[1 + 6 * bytes[0] + 3] << 24);
                double battery_value = bat_value / 100.0;
                int bat = map((int)bat_value, 271, 410, 1, 100);
                bat = bat + 1;


                //String data = (string)obj_T.SelectToken("data");

                //var bytes = Convert.FromBase64String(data);
                //byte anchor_cnt = bytes[0];

                //raw_received_device_info_buf[data_exis_num].dist_Infos.Clear();
                //int byte_num = 0;
                //for (int i = 0; i < anchor_cnt; i++)
                //{
                //    Dist_Info dist_Info = new Dist_Info();
                //    dist_Info.from_Anchor = "DW" + (bytes[1 + 6 * i + 1].ToString("x2") + bytes[1 + 6 * i].ToString("x2")).ToUpper();
                //    dist_Info.dist = (bytes[1 + 6 * i + 2] | bytes[1 + 6 * i + 3] << 8 | bytes[1 + 6 * i + 4] << 16 | bytes[1 + 6 * i + 5] << 24) / 1000.0;
                //    var Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                //    dist_Info.rcv_time = Timestamp;
                //    raw_received_device_info_buf[data_exis_num].dist_Infos.Add(dist_Info);

                //    byte_num = 1 + 6 * i + 5;
                //}
                //int buf_max_num = bytes.Length - 1;
                //for (int j = buf_max_num; j >= 0; j--)
                //{
                //    if (bytes[j] != 0)
                //    {
                //        buf_max_num = j;
                //        break;
                //    }
                //}
                //UInt32 bat_value = (UInt32)(bytes[1 + 6 * anchor_cnt] | bytes[1 + 6 * anchor_cnt + 1] << 8 | bytes[1 + 6 * anchor_cnt + 2] << 16 | bytes[1 + 6 * anchor_cnt + 3] << 24);
                //double battery_value = bat_value / 100.0;
                //int bat = map((int)bat_value, 271, 419, 1, 100);
                //bat = bat + 1;



                //Console.WriteLine("I am : " + raw_received_device_info_buf[data_exis_num].serial_num + "\tBat Value : " + bat + "\t Buf Max : " + buf_max_num);

                raw_received_device_info_buf[data_exis_num].battery = bat;
                //Console.WriteLine("I am : " + raw_received_device_info_buf[data_exis_num].serial_num);
                //for(int i=0; i<raw_received_device_info_buf[data_exis_num].dist_Infos.Count; i++)
                //{
                //    Console.Write("Anchor : " + raw_received_device_info_buf[data_exis_num].dist_Infos[i].from_Anchor);
                //    Console.WriteLine(" -- Dist : " + raw_received_device_info_buf[data_exis_num].dist_Infos[i].dist);
                //}

                Position_from_distance(data_exis_num, raw_received_device_info_buf[data_exis_num].dist_Infos);
            }
        }

        public int map(int val, int in_min, int in_max, int out_min, int out_max)
        {
            return (int)(val - in_min) * (out_max - out_min) / (int)(in_max - in_min) + out_min;
        }

        public void parsing_Tag_location(String Topic_name, MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {

                int T_Start = Topic_name.IndexOf("e");
                int T_End = Topic_name.IndexOf("u");

                String rcv_serial_num = "DW" + Topic_name.Substring(T_Start + 2, T_End - 10).ToUpper();

                String strTag = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                JObject obj_T = JObject.Parse(strTag);
                String FrameNumber = (string)obj_T.SelectToken("superFrameNumber");
                JObject position = (JObject)obj_T.SelectToken("position");

                string position_x = (string)position.SelectToken("x");
                string position_y = (string)position.SelectToken("y");
                string position_z = (string)position.SelectToken("z");


                int data_exis_num = check_list_added_already(rcv_serial_num, raw_received_device_info_buf);
                if (data_exis_num == -1)
                {

                }
                else
                {
                    // 데이터 이미 존재 //
                    // Low Pass Filter //
                    double double_pos_x, double_pos_y, double_pos_z;
                    double_pos_x = Convert.ToDouble(position_x);
                    double_pos_y = Convert.ToDouble(position_y);
                    double_pos_z = Convert.ToDouble(position_z);


                    raw_received_device_info_buf[data_exis_num].tag_framenumber = Convert.ToInt32(FrameNumber);

                    double filter_value = Main_Setting_Data.filter_value;
                    if (!Double.IsNaN(double_pos_x))
                    {
                        double tmp_tag_pos_x = raw_received_device_info_buf[data_exis_num].tag_pos_x;
                        raw_received_device_info_buf[data_exis_num].tag_pos_x = tmp_tag_pos_x * filter_value + double_pos_x * (1 - filter_value);
                    }
                    if (!Double.IsNaN(double_pos_y))
                    {
                        double tmp_tag_pos_y = raw_received_device_info_buf[data_exis_num].tag_pos_y;
                        raw_received_device_info_buf[data_exis_num].tag_pos_y = tmp_tag_pos_y * filter_value + double_pos_y * (1 - filter_value);
                    }
                    if (!Double.IsNaN(double_pos_z))
                    {
                        double tmp_tag_pos_z = raw_received_device_info_buf[data_exis_num].tag_pos_z;
                        raw_received_device_info_buf[data_exis_num].tag_pos_z = tmp_tag_pos_z * filter_value + double_pos_z * (1 - filter_value);
                    }

                    raw_received_device_info_buf[data_exis_num].tag_location_rcv_cnt++;
                    raw_received_device_info_buf[data_exis_num].time = (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
                }
            }
            catch
            {
            }
        }



        //LOG
        private void fn_StatusLogWrite(string anchorName, string str)
        {
            //2021_09_16_13~14_UWB_Filtered_Position.txt

            //string DirPath = System.Environment.CurrentDirectory;
            string DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\UWB\\";

            //DateTime.Now.ToString("yyyy_MM_dd HH:mm");
            String textFileName = anchorName + "_LOG.txt";

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
                        temp = string.Format("{0}   {1}   {2}", DateTime.Now.ToString("yyyy_MM_dd HH:mm"), anchorName, str);
                        //temp = string.Format("[{0}] {1}", DateTime.Now, str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(FilePath))
                    {
                        temp = string.Format("{0}   {1}   {2}", DateTime.Now.ToString("yyyy_MM_dd HH:mm"), anchorName, str);
                        //temp = string.Format("[{0}] {1}", DateTime.Now, str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
            }
            catch (Exception e)
            {
                fn_ErrorFileWrite("fn_StatusLogWrite Error : " + e.ToString());
            }
        }


        //LOG
        private void fn_LogWrite(string str)
        {
            //2021_09_16_13~14_receive_data.txt

            //string DirPath = System.Environment.CurrentDirectory;
            string DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\UWB\\";

            string strFormat = "yyyy_MM_dd_HH~";
            String textFileName = DateTime.Now.ToString(strFormat) + (int.Parse(DateTime.Now.ToString("HH")) + 1) + "_receive_data.txt";
            //String textFileName = DateTime.Now.ToString(strFormat) + "_receive_data.txt";

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
                        //temp = string.Format("{0}", str);
                        temp = string.Format("[{0}] {1}", DateTime.Now, str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(FilePath))
                    {
                        //temp = string.Format("{0}", str);
                        temp = string.Format("[{0}] {1}", DateTime.Now, str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
            }
            catch (Exception e)
            {
                fn_ErrorFileWrite("fn_LogWrite Error : " + e.ToString());
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
