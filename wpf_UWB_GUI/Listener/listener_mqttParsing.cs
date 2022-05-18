using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace wpf_UWB_GUI.Listener
{
    class listener_mqttParsing
    {

        public listener_mqttConnect.retryAnchorHandler retryAnchor;

        DispatcherTimer timer10hz = new DispatcherTimer();
        String strReceiveData = "";
        public List<class_listener_list> cl_devList = new List<class_listener_list>();

        private readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        List<class_listener_list> listAnchor = new List<class_listener_list>();

        public void init()
        {
            //timer10hz.Interval = TimeSpan.FromMilliseconds(1000);
            timer10hz.Interval = TimeSpan.FromMilliseconds(0.01);
            timer10hz.Tick += new EventHandler(timer10hz_Tick);
            timer10hz.Start();
        }


        public void sp_listener_DataReceivedHandler(String receiveData)
        {
            strReceiveData += receiveData;
        }

        bool f_LaStart = false;
        long prevLaStartMillis = 0;

        private void timer10hz_Tick(object sender, EventArgs e)
        {
            if (strReceiveData.Length == 0) return;

            if (strReceiveData.Contains("la"))
            {
                f_LaStart = true;
            }

            if ((long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - prevLaStartMillis > 1000 * 3)
            {
                //Time OUT
                strReceiveData = "";
                f_LaStart = false;
            }

            if (strReceiveData.Contains("dwm>"))
            {
                if (strReceiveData.IndexOf("la") != -1)
                {
                    String strAnchorText = "";

                    try
                    {
                        strAnchorText = strReceiveData.Substring(strReceiveData.IndexOf("la"), strReceiveData.IndexOf("dwm>") - strReceiveData.IndexOf("la"));
                    }
                    catch (Exception e1)
                    {
                        return;
                    }

                    strReceiveData = strReceiveData.Substring(strReceiveData.IndexOf("dwm>") + 4);

                    String[] strSplit = strAnchorText.Split('\n');

                    for (int i = 0; i < strSplit.Length; i++)
                    {
                        if (strSplit[i].IndexOf("id=") != -1 &&
                            strSplit[i].IndexOf("seat=") != -1 &&
                            strSplit[i].IndexOf("seens=") != -1 &&
                            strSplit[i].IndexOf("map=") != -1 &&
                            strSplit[i].IndexOf("pos=") != -1)
                        {
                            try
                            {
                                //Console.WriteLine("strSplit : " + strSplit[i]);

                                class_listener_list clTmp = new class_listener_list();
                                clTmp.devSN = "DW" + strSplit[i].Substring(strSplit[i].IndexOf("id=") + 15, 4);

                                String strPos = strSplit[i].Substring(strSplit[i].IndexOf("pos=") + 4);
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
                            catch (Exception e1)
                            {
                                Console.WriteLine("Error : " + e1.ToString());
                            }
                        }
                    }
                }
                else
                {
                    retryAnchor();
                }
                f_LaStart = false;
            }

            if (f_LaStart)
            {
                return;
            }

            prevLaStartMillis = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;

            if (strReceiveData.IndexOf('P') != -1)
                strReceiveData = strReceiveData.Substring(strReceiveData.IndexOf('P'));
            if (strReceiveData.IndexOf('P') < 0 || strReceiveData.IndexOf('\n') < 0)
                return;

            String strTmp2 = strReceiveData.Substring(strReceiveData.IndexOf('P'), strReceiveData.IndexOf('\n') + 1);

            //Console.WriteLine("strTmp2 : " + strTmp2);

            parseData(strTmp2);

            strReceiveData = strReceiveData.Substring(strReceiveData.IndexOf('\n') + 1);
        }

        private void parseData(String mStr)
        {
            char[] split1 = { ',' };
            string[] result = mStr.Split(split1);

            if (result.Length != 8) return;
            if (result[2].Length != 4) return;

            int num_result_Tag = cl_devList.FindIndex(x => x.devSN.Equals("DW" + result[2]));
            //Console.WriteLine("num_result_Tag : " + num_result_Tag);

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
