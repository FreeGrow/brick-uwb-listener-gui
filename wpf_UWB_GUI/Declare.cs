using System;
using System.Collections.Generic;

namespace wpf_UWB_GUI
{
    class Declare
    {
    }

    public class Anchor
    {
        public string name;
        public string device_serial_num;
        public string uwb_serial_num;
        public bool connected_status;
        public double x, y, z;
        public bool initiator;
        public string color;
        //////////
        public string label;
    }

    public class Tag
    {
        public string name;
        public string device_serial_num;
        public string uwb_serial_num;
        public bool connected_status;
        public double nomUpdateRate;
        public bool low_power_mode;
        public string color;
        //////////
        public string label;
        public double x, y, z;
        public double lpf_x, lpf_y, lpf_z;
        public int framenumber;
        public long rcv_time;
        // For Range //
        public List<Dist_Info> dist_Infos = new List<Dist_Info>();
        public double est_x, est_y, est_z;
        public double lpf_est_x, lpf_est_y, lpf_est_z;
    }

    public class Gateway
    {
        public string name;
        public string device_serial_num;
        public string uwb_serial_num;
        public bool connected_status;
        public string main_ip;
        //////////
        public string id;
        public List<string> ip_address;
        public List<string> ip_info;

        public void init()
        {
            ip_address = new List<string>();
            ip_info = new List<string>();
        }
    }

    public class Device_Reference
    {
        // 순서 정렬의 기준이되는 값임 //
        public string nick_name = "Unknown";
        public string serial_num = "";
        public string color_info = "";
        public string type = "";     // Gateway, Anchor, Tag
        public bool connected_state;        // Connect = true, Disconnected = false //

        // // // // Gateway // // // // //
        public string gateway_main_ip = "";

        // // // // Anchor // // // // //
        public double anchor_install_x, anchor_install_y, anchor_install_z;
        public bool anchor_initiator;

        // // // // Tag // // // // // //
        public double tag_nomUpdateRate;
        public bool tag_low_power_mode;
        public double tag_pos_x, tag_pos_y, tag_pos_z;
        public double tag_lpf_x, tag_lpf_y, tag_lpf_z;
        public int tag_framenumber;
        public List<double> tag_ave_x = new List<double>();
        public List<double> tag_ave_y = new List<double>();
        public List<double> tag_ave_z = new List<double>();
        //public long tag_location_rcv_time;
        //public long tag_distance_rcv_time;
        public int tag_location_rcv_cnt;
        public int tag_distance_rcv_cnt;
        public double tag_location_rcv_hz;
        public double tag_distance_rcv_hz;
        // For Range //
        public List<Dist_Info> dist_Infos = new List<Dist_Info>();
        public double tag_est_x, tag_est_y, tag_est_z;
        //public double tag_lpf_est_x, tag_lpf_est_y, tag_lpf_est_z;
        public int battery;

        public bool alive = true;
        public long time;
    }

    public class Dist_Info
    {
        public string from_Anchor = "";
        public double dist;
        public long rcv_time;
    }

    public class Setting_Each_Details_Data
    {
        public string label;
        public string map_filepath;
        public double map_matching_left_bottom_x;
        public double map_matching_left_bottom_y;
        public double map_matching_right_top_x;
        public double map_matching_right_top_y;

        public List<Setting_Device_Info_List> List_Setting_Device_Info_list = new List<Setting_Device_Info_List>();
    }

    public class Setting_Device_Info_List
    {
        public string nick_name;
        public string serial_num;
        public string color_info;
    }

    public class Main_Setting_Data
    {
        /*
         * 0 : not connect
         * 1 : connecting
         * 2 : connected
         */
        public int MQTT_status_connect;
        public string MQTT_IP;
        public string MQTT_port;

        public int list_selecting_num;
        public string Mode_Triangulation = "";
        public double filter_value = 0.8;
        public List<Setting_Each_Details_Data> List_Setting_Details = new List<Setting_Each_Details_Data>();

        public static implicit operator List<object>(Main_Setting_Data v)
        {
            throw new NotImplementedException();
        }
    }

    public class Device_Data
    {
        public string DEVICE_ID = "";
        public string color = "";
        public string nick_name = "";
    }
}
