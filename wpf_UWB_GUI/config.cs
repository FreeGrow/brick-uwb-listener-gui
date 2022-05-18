using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace wpf_UWB_GUI
{
    public static class config
    {

        public static DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static int Alive_TAG = 60 * 1000 * 1;
        public static int Alive_ANCHOR = 60 * 1000 * 1;
        public static int Alive_GATEWAY = 60 * 1000 * 1;

        public static int printTime = 1000 * 60 * 5;
        public static int aliveTime = 1000 * 10;
        public static int deviceNameTime = 1000 * 60 * 1;
        //test
        public static int batteryTime = 1000 * 60 * 10;

        static public int[] arNumBucket = { 1, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15, 22, 23, 24, 25, 26, 27, 28, 29, 30 };

        public static int num_aliveTag = 0;
        public static int num_aliveAnchor = 0;
        public static int num_aliveGateway = 0;

        public static Color disconnectColor = (Color)ColorConverter.ConvertFromString("#FFFF0000");
        public static Color connectColor = (Color)ColorConverter.ConvertFromString("#FF53FF53");
        public static Color titleSelectColor = (Color)ColorConverter.ConvertFromString("#FF1457ED");
        public static Color titleUnSelectColor = (Color)ColorConverter.ConvertFromString("#FF161618");

        public static void set_AliveTag(int n1)
        {
            num_aliveTag = n1;
        }
        public static void set_AliveAnchor(int n1)
        {
            num_aliveAnchor = n1;
        }
        public static void set_AliveGateway(int n1)
        {
            num_aliveGateway = n1;
        }

        public static int get_AliveTag()
        {
            return num_aliveTag;
        }

        public static int get_AliveAnchor()
        {
            return num_aliveAnchor;
        }

        public static int get_AliveGateway()
        {
            return num_aliveGateway;
        }
        public static byte[] StringToByte(string str) {
            byte[] StrByte = Encoding.UTF8.GetBytes(str);
            return StrByte; 
        }
        public static string ConvertByteToHexString(byte[] convertArr)
        {
            string convertArrString = string.Empty;
            convertArrString = string.Concat(Array.ConvertAll(convertArr, byt => byt.ToString("X2")));
            return convertArrString;
        }

        public static bool openFormCheck(String formName)
        {
            bool result = false;

            var allWindows = System.Windows.Application.Current.Windows;
            foreach (var window in allWindows)
            {
                Window win = window as Window;
                string name = win.Name;
                //Console.WriteLine("name : " + name);
                if (name == formName) // FormTwo my form name as "FormTwo"
                    result = true; // check its open
            }
            return result;
        }

        public static void closeFormCheck(String formName)
        {
            var allWindows = System.Windows.Application.Current.Windows;
            foreach (var window in allWindows)
            {
                Window win = window as Window;
                string name = win.Name;
                //Console.WriteLine("name : " + name);
                if (name == formName) // FormTwo my form name as "FormTwo"
                    win.Close(); // check its open
            }
        }


        //FilterWriteLOG
        public static void fn_FilterTextWrite(string TAG, String strSavePath, string str)
        {
            //2021_09_16_13~14_UWB_Filtered_Position.txt

            string DirPath = strSavePath;
            if (strSavePath.Length == 0)
            {
                DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\UWB\\";
            }

            string strFormat = "yyyy_MM_dd_HH~";
            String textFileName = DateTime.Now.ToString(strFormat) + (int.Parse(DateTime.Now.ToString("HH")) + 1) + "_UWB_Filtered_Position.txt";

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
                        temp = string.Format("[{0}] {1} {2}", DateTime.Now, TAG, str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(FilePath))
                    {
                        temp = string.Format("[{0}] {1} {2}", DateTime.Now, TAG, str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
            }
            catch (Exception e)
            {
                config.fn_ErrorFileWrite(TAG, "fn_TextWrite Error : " + e.ToString());
            }
        }

        //DeviceAdd Read Textfile
        public static String fn_DeviceAddRead(String TAG)
        {
            String DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\UWB\\_UWB_Device.txt";
            String strRead = "";
            try
            {
                strRead = System.IO.File.ReadAllText(@DirPath);
            }
            catch (Exception e)
            {
                fn_ErrorFileWrite(TAG, "fn_DeviceAddRead Error : " + e.ToString());
                return "";
            }
            return strRead;
        }

        //DeviceAdd TextFile
        public static void fn_DeviceAdd(string TAG, string str)
        {
            string DirPath;
            DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\UWB\\";

            String textFileName = "_UWB_Device.txt";

            string FilePath = DirPath + "\\" + textFileName;
            string temp;

            DirectoryInfo di = new DirectoryInfo(DirPath);
            FileInfo fi = new FileInfo(FilePath);

            try
            {
                if (!di.Exists) Directory.CreateDirectory(DirPath);
                using (StreamWriter sw = new StreamWriter(FilePath))
                {
                    temp = string.Format("{0}", str);
                    sw.WriteLine(temp);
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                config.fn_ErrorFileWrite(TAG, "fn_DeviceAdd Error : " + e.ToString());
            }
        }

        //DeviceInsert TextFile
        public static void fn_DeviceRemove(string TAG)
        {
            string DirPath;
            DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\UWB\\";

            String textFileName = "_UWB_Device.txt";

            string FilePath = DirPath + "\\" + textFileName;
            string temp;

            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }

        //DeviceInsert TextFile
        public static void fn_DeviceInsert(string TAG, string str)
        {
            string DirPath;
            DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\UWB\\";

            String textFileName = "_UWB_Device.txt";

            string FilePath = DirPath + "\\" + textFileName;
            string temp;

            DirectoryInfo di = new DirectoryInfo(DirPath);
            FileInfo fi = new FileInfo(FilePath);

            try
            {
                if (!di.Exists) Directory.CreateDirectory(DirPath);
                using (StreamWriter sw = new StreamWriter(FilePath))
                {
                    temp = string.Format("{0}", str);
                    sw.WriteLine(temp);
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                config.fn_ErrorFileWrite(TAG, "fn_TextWrite Error : " + e.ToString());
            }
        }


        //BatteryWriteLog
        public static void fn_BatteryTextWrite(string TAG, String strSavePath, string str)
        {
            //2021_09_16_13~14_UWB_Filtered_Position.txt

            string DirPath = strSavePath;
            if (strSavePath.Length == 0)
            {
                DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\UWB\\";
            }

            String textFileName = "_UWB_Battery.txt";

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
                        temp = string.Format("[{0}] {1} {2}", DateTime.Now, TAG, str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(FilePath))
                    {
                        temp = string.Format("[{0}] {1} {2}", DateTime.Now, TAG, str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
            }
            catch (Exception e)
            {
                config.fn_ErrorFileWrite(TAG, "fn_TextWrite Error : " + e.ToString());
            }
        }


        //Error file write
        public static void fn_ErrorFileWrite(string TAG, string str)
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
                        temp = string.Format("[{0}] {1} {2}", DateTime.Now, TAG, str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(FilePath))
                    {
                        temp = string.Format("[{0}] {1} {2}", DateTime.Now, TAG, str);
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


        //DeviceName file read
        public static String fn_DeviceNameRead(String TAG)
        {
            String DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\UWB\\_UWB_DEVICE_NAME.txt";
            String strRead = "";
            try
            {
                strRead = System.IO.File.ReadAllText(@DirPath);
            }
            catch (Exception e)
            {
                fn_ErrorFileWrite(TAG, "fn_DeviceNameRead Error : " + e.ToString());
                return "";
            }
            return strRead;
        }


        //DeviceName file write
        public static void fn_DeviceNameWrite(string str)
        {
            //2021_09_16_13~14_UWB_Filtered_Position.txt

            //string DirPath = System.Environment.CurrentDirectory;
            string DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\UWB\\";

            String textFileName = "_UWB_DEVICE_NAME.txt";

            string FilePath = DirPath + textFileName;
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
                        temp = string.Format("{0}", str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = File.CreateText(FilePath))
                    {
                        temp = string.Format("{0}", str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        //LOG
        public static void fn_LogWrite(string str)
        {
            //2021_09_16_13~14_UWB_Filtered_Position.txt

            //string DirPath = System.Environment.CurrentDirectory;
            string DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\UWB\\";

            string strFormat = "yyyy_MM";
            String textFileName = DateTime.Now.ToString(strFormat) + "_UWB_COMMUNICATION_LOG.txt";

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
                        temp = string.Format("{0}", str);
                        //temp = string.Format("[{0}] {1}", DateTime.Now, str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(FilePath))
                    {
                        temp = string.Format("{0}", str);
                        //temp = string.Format("[{0}] {1}", DateTime.Now, str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
            }
            catch (Exception e)
            {
                fn_ErrorFileWrite("MainWindow", "fn_LogWrite Error : " + e.ToString());
            }
        }

    }
}
