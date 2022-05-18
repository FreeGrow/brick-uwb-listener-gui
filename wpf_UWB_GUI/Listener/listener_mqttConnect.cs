using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows;

namespace wpf_UWB_GUI.Listener
{
    public class listener_mqttConnect
    {

        public delegate void retryAnchorHandler();

        listener_mqttParsing mqttParsing;

        SerialPort sp_listener = new SerialPort();

        public void init()
        {
            mqttParsing = new listener_mqttParsing();
            mqttParsing.retryAnchor += new retryAnchorHandler(sp_AnchorList);
            mqttParsing.init();
        }

        public void setSerialPort(String mCom, int baudRate)
        {
            if (sp_listener.IsOpen) return;

            sp_listener.PortName = mCom;
            sp_listener.BaudRate = baudRate;
            sp_listener.DataReceived += new SerialDataReceivedEventHandler(sp_listener_DataReceivedHandler);
        }

        public void sp_listener_DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            mqttParsing.sp_listener_DataReceivedHandler(sp_listener.ReadExisting());
        }

        public List<class_listener_list> getList()
        {
            if (mqttParsing != null)
                return mqttParsing.getList();
            return null;
        }

        public List<class_listener_list> getAnchorList()
        {
            if (mqttParsing != null)
                return mqttParsing.getAnchorList();
            return null;
        }

        public void clearList()
        {
            mqttParsing.clearList();
        }

        public void sp_AnchorList()
        {
            byte[] arByte = new byte[10];
            arByte[0] = (byte)'l';
            arByte[1] = (byte)'a';
            arByte[2] = 0x0d;
            sp_listener.Write(arByte, 0, 3);
        }

        public void sp_Connect()
        {
            Console.WriteLine("sp_listener.IsOpen : " + sp_listener.IsOpen);
            if (!sp_listener.IsOpen)
                sp_listener.Open();
            else
                MessageBox.Show("Port Access deniend.");
        }

        public void sp_DisConnect()
        {
            sp_listener.Close();
        }

        public bool fConnectState()
        {
            return sp_listener.IsOpen;
        }

    }
}
