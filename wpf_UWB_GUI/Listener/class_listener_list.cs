using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media;

namespace wpf_UWB_GUI
{
    public class class_listener_list
    {

        //Device Unique ID
        public String devSN
        {
            get;
            //{
            //    return devReferenceValue.serial_num;
            //}
            set; 
            //{ 
            //}
        }

        //Device Color
        public String devColor { get; set; }

        //Device Type ( TAG, ANCHOR, GATEWAY )
        public String devType
        {
            get;
            //{
            //    return devReferenceValue.type;
            //}
            set;
            //{
            //}
        }

        //Device TAG NAME
        public String devTagName
        {
            get;
            //{
            //    return devReferenceValue.nick_name;
            //}
            set;
            //{
            //}
        }

        //Device RAW POSITION X
        public double tag_pos_x
        {
            get;
            //{
            //    return Math.Round(devReferenceValue.tag_pos_x, 1);
            //}
            set;
            //{
            //}
        }

        //Device RAW POSITION Y
        public double tag_pos_y
        {
            get;
            //{
            //    return Math.Round(devReferenceValue.tag_pos_y, 1);
            //}
            set;
            //{
            //}
        }

        //Device RAW POSITION Z
        public double tag_pos_z
        {
            get;
            //{
            //    return Math.Round(devReferenceValue.tag_pos_z, 1);
            //}
            set;
            //{
            //}
        }

        //Device RAW POSITION
        public String devPosition
        {
            get
            {
                String tmp =
                    tag_pos_x + "m, " +
                    tag_pos_y + "m, " +
                    tag_pos_z + "m";
                return tmp;
            }
            set
            {

            }
        }

        //Device FILTERED POSITION X
        public double tag_lpf_x
        {
            get;set;
        }

        //Device FILTERED POSITION Y
        public double tag_lpf_y
        {
            get; set;
        }

        //Device FILTERED POSITION Z
        public double tag_lpf_z
        {
            get; set;
        }

        //Device FILTER POSITION
        public String devFilter
        {
            get
            {
                tag_lpf_x = Math.Round(tag_lpf_x, 2);
                tag_lpf_y = Math.Round(tag_lpf_y, 2);
                tag_lpf_z = Math.Round(tag_lpf_z, 2);

                String tmp =
                    tag_lpf_x + "m, " +
                    tag_lpf_y + "m, " +
                    tag_lpf_z + "m";
                return tmp;
            }
            set
            {

            }
        }

        //Device UPDATERATE
        public double tag_nomUpdateRate
        {
            get; set;
        }

        //Device LOW POWER MODE
        public bool tag_low_power_mode
        {
            get; set;
        }

        //Device BATTERY
        public int devBat
        {
            get; set;
        }

        //Device String BATTERY
        public String devStrBat
        {
            get
            {
                String tmp =
                    devBat + "%";
                return tmp;
            }
            set
            {

            }
        }

        //Device ALIVE STATE
        public bool alive { get; set; }

        public String devAliveImg
        {
            get
            {
                String tmpImgPath = "";

                if (alive == true) tmpImgPath = "/Resources/status_on.png";
                if (alive == false) tmpImgPath = "/Resources/status_off.png";

                return tmpImgPath;
            }
            set
            {

            }
        }

        //Device ALIVE CHECK TIME
        public long time
        {
            get; set;
        }

        //Device LAST FRAMENUMBER
        public int tag_framenumber
        {
            get; set;
        }

        public PropertyInfo[] listColor { get; set; }


        public Device_Reference devReferenceValue { get; set; }
        public class_filter FilterValue { get; set; }

        public int pos_quality
        {
            get; set;
        }

        public String checkSum
        {
            get; set;
        }

        public List<double> tag_ave_x = new List<double>();
        public List<double> tag_ave_y = new List<double>();
        public List<double> tag_ave_z = new List<double>();

    }
}
