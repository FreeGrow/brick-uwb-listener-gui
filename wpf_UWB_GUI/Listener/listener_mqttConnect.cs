using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Windows;

namespace wpf_UWB_GUI.Listener
{
    public class listener_mqttConnect
    {

        public delegate void retryAnchorHandler();
        public delegate void systemInfoHandler();
        public delegate void lecSendHandler();

        listener_mqttParsing mqttParsing;

        SerialPort sp_listener = new SerialPort();

        public void init()
        {
            mqttParsing = new listener_mqttParsing();
            mqttParsing.retryAnchor += new retryAnchorHandler(sp_AnchorList);
            mqttParsing.systemInfo += new systemInfoHandler(sp_systemInfo);
            mqttParsing.lecSend += new lecSendHandler(sp_lecSend);
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
            try
            {
                //String receiveData = sp_listener.ReadExisting();
                String receiveData = sp_listener.ReadLine();
                mqttParsing.sp_listener_DataReceivedHandler(receiveData);
            }
            catch (Exception e1)
            {

            }

            //Console.Write("sp_listener_DataReceivedHandler()");
            //Console.WriteLine(receiveData);
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

        public void sp_lecSend()
        {
            Console.WriteLine("sp_lecSend()");
            byte[] arByte = new byte[10];
            arByte[0] = (byte)'l';
            arByte[1] = (byte)'e';
            arByte[2] = (byte)'c';
            arByte[3] = 0x0d;
            sp_listener.Write(arByte, 0, 4);
        }

        public void sp_systemInfo()
        {
            Console.WriteLine("sp_systemInfo()");
            byte[] arByte = new byte[10];
            arByte[0] = (byte)'s';
            arByte[1] = (byte)'i';
            arByte[2] = 0x0d;
            sp_listener.Write(arByte, 0, 3);
        }

        public void sp_EnterTwice()
        {
            byte[] arByte = new byte[10];
            arByte[0] = 0x0d;
            sp_listener.Write(arByte, 0, 1);
            //Thread.Sleep(100);

            //sp_systemInfo();

            //arByte[0] = 0x0d;
            //sp_listener.Write(arByte, 0, 1);
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
