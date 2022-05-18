using System;
using System.Drawing;
using System.Reflection;

namespace wpf_UWB_GUI
{
    class class_deviceList
    {

        //Device Unique ID
        public String devSN
        {
            get
            {
                return devReferenceValue.serial_num;
            }
            set 
            { 
            }
        }

        //Device Color
        public Color devColor { get; set; }

        //Device Type ( TAG, ANCHOR, GATEWAY )
        public String devType
        {
            get
            {
                return devReferenceValue.type;
            }
            set
            {
            }
        }

        //Device TAG NAME
        public String devTagName
        {
            get
            {
                return devReferenceValue.nick_name;
            }
            set
            {
            }
        }

        //Device RAW POSITION X
        public double tag_pos_x
        {
            get
            {
                return Math.Round(devReferenceValue.tag_pos_x, 1);
            }
            set
            {
            }
        }

        //Device RAW POSITION Y
        public double tag_pos_y
        {
            get
            {
                return Math.Round(devReferenceValue.tag_pos_y, 1);
            }
            set
            {
            }
        }

        //Device RAW POSITION Z
        public double tag_pos_z
        {
            get
            {
                return Math.Round(devReferenceValue.tag_pos_z, 1);
            }
            set
            {
            }
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
            get
            {
                if (FilterValue == null) return 0;
                return Math.Round(FilterValue.FILTER_X, 1);
            }
            set
            {
            }
        }

        //Device FILTERED POSITION Y
        public double tag_lpf_y
        {
            get
            {
                if (FilterValue == null) return 0;
                return Math.Round(FilterValue.FILTER_Y, 1);
            }
            set
            {
            }
        }

        //Device FILTERED POSITION Z
        public double tag_lpf_z
        {
            get
            {
                if (FilterValue == null) return 0;
                return Math.Round(FilterValue.FILTER_Z, 1);
            }
            set
            {
            }
        }

        //Device FILTER POSITION
        public String devFilter
        {
            get
            {
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
            get
            {
                return devReferenceValue.tag_nomUpdateRate;
            }
            set
            {
            }
        }

        //Device LOW POWER MODE
        public bool tag_low_power_mode
        {
            get
            {
                return devReferenceValue.tag_low_power_mode;
            }
            set
            {
            }
        }

        //Device BATTERY
        public int devBat
        {
            get
            {
                return devReferenceValue.battery;
            }
            set
            {
            }
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
            get
            {
                return devReferenceValue.time;
            }
            set
            {
            }
        }

        //Device LAST FRAMENUMBER
        public int tag_framenumber
        {
            get
            {
                return devReferenceValue.tag_framenumber;
            }
            set
            {
            }
        }

        public PropertyInfo[] listColor { get; set; }


        public Device_Reference devReferenceValue { get; set; }
        public class_filter FilterValue { get; set; }

    }
}
