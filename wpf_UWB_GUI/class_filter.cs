using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpf_UWB_GUI
{
    public class class_filter
    {
        //Unique Device ID
        private String devID;

        //Moving Average
        public List<double> arFilter_X = new List<double>();
        public List<double> arFilter_Y = new List<double>();
        public List<double> arFilter_Z = new List<double>();

        //LPF
        private double filter_X = 0, filter_Y = 0, filter_Z = 0;
        private double prevfilter_X = 0, prevfilter_Y = 0, prevfilter_Z = 0;

        private long tag_framenumber = 0, prev_tag_framenumber = 0;

        public string DEVICE_ID
        {
            get
            {
                return devID;
            }
            set
            {
                devID = value;
            }
        }

        public double FILTER_X
        {
            get
            {
                return filter_X;
            }
            set
            {
                filter_X = value;
            }
        }

        public double FILTER_Y
        {
            get
            {
                return filter_Y;
            }
            set
            {
                filter_Y = value;
            }
        }

        public double FILTER_Z
        {
            get
            {
                return filter_Z;
            }
            set
            {
                filter_Z = value;
            }
        }

        public double prevFILTER_X
        {
            get
            {
                return prevfilter_X;
            }
            set
            {
                prevfilter_X = value;
            }
        }

        public double prevFILTER_Y
        {
            get
            {
                return prevfilter_Y;
            }
            set
            {
                prevfilter_Y = value;
            }
        }

        public double prevFILTER_Z
        {
            get
            {
                return prevfilter_Z;
            }
            set
            {
                prevfilter_Z = value;
            }
        }

        public long TAG_FRAMENUMBER
        {
            get
            {
                return tag_framenumber;
            }
            set
            {
                tag_framenumber = value;
            }
        }

        public long PREV_TAG_FRAMENUMBER
        {
            get
            {
                return prev_tag_framenumber;
            }
            set
            {
                prev_tag_framenumber = value;
            }
        }

    }
}
