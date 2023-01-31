using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace wpf_UWB_GUI.Listener
{
    class listener_mqttParsing
    {

        public listener_mqttConnect.retryAnchorHandler retryAnchor;
        public listener_mqttConnect.systemInfoHandler systemInfo;
        public listener_mqttConnect.lecSendHandler lecSend;

        DispatcherTimer timer10hz = new DispatcherTimer();
        String strReceiveData = "";
        public List<class_listener_list> cl_devList = new List<class_listener_list>();

        private readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        List<class_listener_list> listAnchor = new List<class_listener_list>();

        public void init()
        {
            timer10hz.Interval = TimeSpan.FromMilliseconds(0.01);
            timer10hz.Tick += new EventHandler(timer10hz_Tick);
            timer10hz.Start();

        }

        List<String> getSerialData = new List<string>();


        public void sp_listener_DataReceivedHandler(String receiveData)
        {
            getSerialData.Add(receiveData);
        }

        long prevSystemInfoMillis = 0;
        String prevStrReceiveData = "";

        bool fGetPos = false;

        private void timer10hz_Tick(object sender, EventArgs e)
        {
            //return;
            if (!fGetPos)
            {
                if (strReceiveData.Length != prevStrReceiveData.Length)
                {
                    prevSystemInfoMillis = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;
                }

                if ((long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - prevSystemInfoMillis > 1000 * 5)
                {
                    prevSystemInfoMillis = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;
                    lecSend();
                }

                prevStrReceiveData = strReceiveData;
            }

            if (getSerialData.Count < 1)
            {
                return;
            }

            Console.WriteLine(getSerialData[0]);
            String strParse = getSerialData[0];
            getSerialData.RemoveAt(0);

            if (strParse.Contains("POS,"))
            {
                parseData(strParse);
            }

            if (strParse.IndexOf("id=") != -1 &&
                strParse.IndexOf("seat=") != -1 &&
                strParse.IndexOf("seens=") != -1 &&
                strParse.IndexOf("rssi=") != -1 &&
                strParse.IndexOf("cl=") != -1 &&
                strParse.IndexOf("nbr=") != -1 &&
                strParse.IndexOf("pos=") != -1)
            {
                class_listener_list clTmp = new class_listener_list();
                clTmp.devSN = "DW" + strParse.Substring(strParse.IndexOf("id=") + 15, 4);

                String strPos = strParse.Substring(strParse.IndexOf("pos=") + 4);
                String[] initPos = strPos.Split(':');

                clTmp.time = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;
                clTmp.devType = "Anchor";
                clTmp.devTagName = "Unknown";
                clTmp.tag_pos_x = Double.Parse(initPos[0]);
                clTmp.tag_pos_y = Double.Parse(initPos[1]);
                clTmp.tag_pos_z = Double.Parse(initPos[2]);

                int resultAnchor = listAnchor.FindIndex(x => x.devSN.Equals(clTmp.devSN));
                if (resultAnchor == -1)
                {
                    listAnchor.Add(clTmp);
                }
                else
                {
                    class_listener_list clDevice = listAnchor.Find(x => x.devSN.Equals(clTmp.devSN));
                    clDevice.devTagName = "Unknown";
                    clDevice.tag_pos_x = clTmp.tag_pos_x;
                    clDevice.tag_pos_y = clTmp.tag_pos_y;
                    clDevice.tag_pos_z = clTmp.tag_pos_z;
                    clDevice.time = clTmp.time;
                }
            }
        }

        private void parseData(String mStr)
        {
            int cntChk = mStr.Split(',').Length - 1;
            if (cntChk != 7)
            {
                return;
            }

            char[] split1 = { ',' };
            string[] result = mStr.Split(split1);

            int num_result_Tag = cl_devList.FindIndex(x => x.devSN.Equals("DW" + result[2]));

            if (num_result_Tag != -1)
            {
                //UPDATE ITEM
                class_listener_list cl_item = cl_devList.Find(x => x.devSN.Equals("DW" + result[2]));

                //if (result[3].Contains("nan")) cl_item.tag_pos_x = 0;
                if (result[3].Contains("nan")) return;
                else
                {
                    if (result[3].Length == 0) cl_item.tag_pos_x = 0;
                    else
                    {
                        try
                        {
                            cl_item.tag_pos_x = Double.Parse(result[3]);
                        }
                        catch (Exception e)
                        {
                            cl_item.tag_pos_x = 0;
                        }
                    }
                }
                if (result[4].Contains("nan")) cl_item.tag_pos_y = 0;
                else
                {
                    if (result[4].Length == 0) cl_item.tag_pos_y = 0;
                    else
                    {
                        try
                        {
                            cl_item.tag_pos_y = Double.Parse(result[4]);
                        }
                        catch (Exception e)
                        {
                            cl_item.tag_pos_y = 0;
                        }
                    }
                }
                if (result[5].Contains("nan")) cl_item.tag_pos_z = 0;
                else
                {
                    if (result[5].Length == 0) cl_item.tag_pos_z = 0;
                    else
                    {
                        try
                        {
                            cl_item.tag_pos_z = Double.Parse(result[5]);
                        }
                        catch (Exception e)
                        {
                            cl_item.tag_pos_z = 0;
                        }
                    }
                }
                if (result[6].Length == 0)
                    cl_item.pos_quality = 0;
                else
                {
                    result[6] = result[6].Replace("\r\n", "");
                    try
                    {
                        cl_item.pos_quality = int.Parse(result[6]);
                    }
                    catch (Exception e)
                    {
                        cl_item.pos_quality = 0;
                    }
                }
                try
                {
                    cl_item.checkSum = result[7];
                }
                catch (Exception e)
                {
                    cl_item.checkSum = "";
                }

                cl_item.devType = "Tag";
                cl_item.time = (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
            }
            else
            {
                //ADD ITEM
                class_listener_list tmpListener = new class_listener_list();
                if (result[2].Length != 4) return;
                tmpListener.devSN = "DW" + result[2];
                if (result[3].Contains("nan")) return;
                //if (result[3].Contains("nan")) tmpListener.tag_pos_x = 0;
                else
                {
                    if (result[3].Length == 0) tmpListener.tag_pos_x = 0;
                    else
                    {
                        try
                        {
                            tmpListener.tag_pos_x = Double.Parse(result[3]);
                        }
                        catch (Exception e)
                        {
                            tmpListener.tag_pos_x = 0;
                        }
                    }
                }
                if (result[4].Contains("nan")) tmpListener.tag_pos_y = 0;
                else
                {
                    if (result[4].Length == 0) tmpListener.tag_pos_y = 0;
                    else
                    {
                        try
                        {
                            tmpListener.tag_pos_y = Double.Parse(result[4]);
                        }
                        catch (Exception e)
                        {
                            tmpListener.tag_pos_y = 0;
                        }
                    }
                }
                if (result[5].Contains("nan")) tmpListener.tag_pos_z = 0;
                else
                {
                    if (result[5].Length == 0) tmpListener.tag_pos_z = 0;
                    else
                    {
                        try
                        {
                            tmpListener.tag_pos_z = Double.Parse(result[5]);
                        }
                        catch (Exception e)
                        {
                            tmpListener.tag_pos_z = 0;
                        }
                    }
                }
                if (result[6].Length == 0)
                    tmpListener.pos_quality = 0;
                else
                {
                    result[6] = result[6].Replace("\r\n", "");
                    try
                    {
                        tmpListener.pos_quality = int.Parse(result[6]);
                    }
                    catch (Exception e)
                    {
                        tmpListener.pos_quality = 0;
                    }
                }
                try
                {
                    tmpListener.checkSum = result[7];
                }
                catch (Exception e)
                {
                    tmpListener.checkSum = "";
                }

                tmpListener.devType = "Tag";
                tmpListener.time = (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;

                cl_devList.Add(tmpListener);
                fGetPos = true;
            }

        }

        public List<class_listener_list> getList()
        {
            return cl_devList;
        }

        public void clearList()
        {
            cl_devList.Clear();
        }

        public List<class_listener_list> getAnchorList()
        {
            return listAnchor;
        }

        public void removeAnchorData(String mdevSN)
        {
            int resultNum = listAnchor.FindIndex(x => x.devSN.Equals(mdevSN));
            if (resultNum != -1)
            {
                listAnchor.RemoveAt(resultNum);
            }
        }

    }
}
