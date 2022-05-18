using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace wpf_UWB_GUI
{
    /// <summary>
    /// UC_device_list_item.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_listener_map_conrol : UserControl
    {
        String TAG = "UC_listener_map_conrol";

        DispatcherTimer timer10hz = new DispatcherTimer();

        private List<class_Marker> listMarker = new List<class_Marker>();
        List<class_listener_list> listDevice = new List<class_listener_list>();

        //CheckBox Status
        bool fDeviceID = false;
        bool fDeviceFilter = false;
        bool fDeviceName = false;
        bool fDevicePosition = false;

        int num_AliveTag = 0, num_AliveAnchor = 0, num_AliveGateway = 0;

        //MapZoom
        float zoom = 1;

        //Origin Get Image
        double imgWidth = 0, imgHeight = 0;
        int imageMaxWidth = 900;
        int imageMaxHeight = 900;

        bool fChange = true;

        public UC_listener_map_conrol()
        {
            InitializeComponent();

            timer10hz.Interval = TimeSpan.FromMilliseconds(100);
            timer10hz.Tick += new EventHandler(timer10hz_Tick);
            timer10hz.Start();
        }

        public void set_ListDevice(List<class_listener_list> mClList)
        {
            listDevice = mClList;
        }

        public List<class_listener_list> get_ListDevice()
        {
            return listDevice;
        }

        public void UC_MapUpdate()
        {
            if (!fChange) return;

            string filePath = Properties.Settings.Default.map_value;
            ImageBrush ib = new ImageBrush();

            if (filePath.Length == 0)
            {
                ib.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/default.jpg"));
            }
            else
            {
                ib.ImageSource = new BitmapImage(new Uri(filePath, UriKind.RelativeOrAbsolute));

                if (ib.ImageSource.Width > ib.ImageSource.Height)
                {
                    if (ib.ImageSource.Width > imageMaxWidth)
                    {
                        float f1 = (float)(imageMaxWidth / ib.ImageSource.Width);
                        ib.ImageSource = new TransformedBitmap((BitmapImage)ib.ImageSource, new ScaleTransform(f1, f1));
                    }
                }
                else
                {
                    if (ib.ImageSource.Height > imageMaxHeight)
                    {
                        float f1 = (float)(imageMaxHeight / ib.ImageSource.Height);
                        ib.ImageSource = new TransformedBitmap((BitmapImage)ib.ImageSource, new ScaleTransform(f1, f1));
                    }
                }
            }

            imgWidth = ib.ImageSource.Width;
            imgHeight = ib.ImageSource.Height;

            imageMap.Background = ib;

            if (this.Height < imgHeight && this.Width < imgWidth)
            {
                double d1 = (this.Width / imgWidth);
                double d2 = (this.Height / imgHeight);

                if (d1 > d2) zoom = (float)d2;
                else zoom = (float)d1;
            }
            else if (this.Width < imgWidth)
            {
                double d1 = (this.Width / imgWidth);

                zoom = (float)d1 - 0.1f;
            }
            else if (this.Height < imgHeight)
            {
                double d1 = (this.Height / imgHeight);

                zoom = (float)d1 - 0.1f;
            }

            this.Width = imgWidth * zoom;
            this.Height = imgHeight * zoom;

            imageMap.Width = imgWidth * zoom;
            imageMap.Height = imgHeight * zoom;

            imageMapMarker.Width = imgWidth * zoom;
            imageMapMarker.Height = imgHeight * zoom;

            fChange = false;

            //Console.WriteLine("zoom : " + zoom);

        }

        const int checkTime_AddMarker = 1000;
        long prevAddMarkerTime = 0;

        private void timer10hz_Tick(object sender, EventArgs e)
        {
            //Topic Pub Device Name
            if ((long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - prevAddMarkerTime > (checkTime_AddMarker))
            {
                prevAddMarkerTime = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;
                addMarker();
                addAnchorMarker();
            }

            updateMarker();
        }


        public void setZoomLevel(float f1)
        {
            zoom = f1;
        }

        public void setCheckBoxValue(bool f1, bool f2, bool f3, bool f4)
        {
            fDeviceID = f1;
            fDeviceFilter = f2;
            fDeviceName = f3;
            fDevicePosition = f4;
        }

        public void UC_FormFirstChange(double mWidth, double mHeight)
        {
            this.Width = mWidth;
            this.Height = mHeight;

            imageMap.Width = mWidth;
            imageMap.Height = mHeight;

            imageMapMarker.Width = mWidth;
            imageMapMarker.Height = mHeight;

            fChange = true;
        }

        public void FormZoomLevel(float f1)
        {
            if (f1 > 0)
            {
                if (zoom < 3.0)
                    zoom += 0.1f;
            }
            else
            {
                if (zoom > 0.4)
                    zoom -= 0.1f;
            }

            this.Width = imgWidth * zoom;
            this.Height = imgHeight * zoom;

            imageMap.Width = imgWidth * zoom;
            imageMap.Height = imgHeight * zoom;

            imageMapMarker.Width = imgWidth * zoom;
            imageMapMarker.Height = imgHeight * zoom;

            fChange = true;
        }

        public void UC_FormSize(double mWidth, double mHeight)
        {
            if (imgWidth < imgHeight)
            {
                double d1 = (mHeight / imgHeight);
                zoom = (float)d1 - 0.1f;
            }
            else
            {
                double d1 = (mWidth / imgWidth);
                zoom = (float)d1 - 0.1f;
            }

            this.Width = imgWidth * zoom;
            this.Height = imgHeight * zoom;

            imageMap.Width = imgWidth * zoom;
            imageMap.Height = imgHeight * zoom;

            imageMapMarker.Width = imgWidth * zoom;
            imageMapMarker.Height = imgHeight * zoom;

            fChange = true;
        }

        private void addMarkerMap(class_Marker mMarker)
        {
            listMarker.Add(mMarker);
            imageMapMarker.Children.Add(mMarker.mEllipse);
            imageMapMarker.Children.Add(mMarker.mLabel);

            Console.WriteLine("addMarkerMap()");
            Console.WriteLine(listMarker.Count);
        }

        //////////////////////
        //Marker Add Start
        //////////////////////
        private void addMarker()
        {
            if (listDevice == null) return;
            //First

            for (int i = 0; i < listDevice.Count; i++)
            {
                int resultNumTag = listMarker.FindIndex(x => x.devListener.devSN.Equals(listDevice[i].devSN));
                if (resultNumTag == -1)
                {
                    //Not Exsist
                    class_Marker clMarker = new class_Marker();

                    clMarker.mEllipse = new Ellipse();
                    clMarker.mEllipse.Stroke = new SolidColorBrush(Colors.Black);
                    clMarker.mEllipse.Fill = new SolidColorBrush(Colors.Black);
                    clMarker.mEllipse.Width = 10;
                    clMarker.mEllipse.Height = 10;
                    clMarker.mEllipse.Visibility = Visibility.Hidden;
                    clMarker.devListener = listDevice[i];
                    clMarker.mLabel = new Label();
                    clMarker.mLabel.Foreground = new SolidColorBrush(Colors.Black);
                    clMarker.mLabel.Visibility = Visibility.Hidden;

                    addMarkerMap(clMarker);
                }
            }
        }

        private void addAnchorMarker()
        {

            //ANCHOR DATA
            string json_data = config.fn_DeviceAddRead(TAG);
            var device_data = JsonConvert.DeserializeObject<List<class_listener_list>>(json_data);
            List<class_listener_list> mcl_list = new List<class_listener_list>();
            if (device_data != null) mcl_list = device_data;

            List<class_listener_list> mcl_AnchorList = new List<class_listener_list>();

            int num_result_Anchor = mcl_list.FindIndex(x => x.devType.Equals("Anchor"));
            if (num_result_Anchor != -1)
            {
                mcl_AnchorList = mcl_list.FindAll(x => x.devType.Equals("Anchor"));
                mcl_AnchorList = mcl_AnchorList.OrderBy(x => x.devSN).ToList();

                for (int i = 0; i < mcl_AnchorList.Count; i++)
                {
                    int num_exsist_Anchor = listMarker.FindIndex(x => x.devListener.devSN.Equals(mcl_AnchorList[i].devSN));

                    if (num_exsist_Anchor == -1)
                    {
                        //Not Exsist Add Anchor
                        class_Marker clAnchorMarker = new class_Marker();

                        clAnchorMarker.mEllipse = new Ellipse();
                        clAnchorMarker.mEllipse.Stroke = new SolidColorBrush(Colors.Black);
                        clAnchorMarker.mEllipse.Fill = new SolidColorBrush(Colors.Black);
                        clAnchorMarker.mEllipse.Width = 10;
                        clAnchorMarker.mEllipse.Height = 10;
                        clAnchorMarker.mEllipse.Visibility = Visibility.Hidden;
                        clAnchorMarker.devListener = mcl_AnchorList[i];
                        clAnchorMarker.mLabel = new Label();
                        clAnchorMarker.mLabel.Foreground = new SolidColorBrush(Colors.Black);
                        clAnchorMarker.mLabel.Visibility = Visibility.Hidden;

                        addMarkerMap(clAnchorMarker);
                    }
                    else
                    {
                        //Refresh Anchor
                        listMarker[num_exsist_Anchor].devListener = mcl_AnchorList[i];
                    }
                }
            }

            //Remove Event Detect
            List<class_Marker> markerListTmp = listMarker.FindAll(x => x.devListener.devType.Equals("Anchor"));
            if (markerListTmp.Count != mcl_AnchorList.Count)
            {
                listMarker.Clear();
                imageMapMarker.Children.Clear();
                prevAddMarkerTime = 0;
            }
        }
        //////////////////////
        //Marker Add End
        //////////////////////

        private string[] getMapSize()
        {
            string strMapSize = Properties.Settings.Default.map_size;
            char[] sep = { ',' };

            string[] result = strMapSize.Split(sep);

            return result;
        }

        int Marker_MarginSize = 5;


        private void updateMarker()
        {
            double realWidth = 0, realHeight = 0;
            String sWidth = "", sHeight = "";

            ///////////////////////////
            //Get Real Map Size START
            ///////////////////////////

            string[] result = getMapSize();

            if (result.Length == 2)
            {
                sWidth = result[0];
                sHeight = result[1];
            }
            try
            {
                realWidth = double.Parse(sWidth) * 100;
                realHeight = double.Parse(sHeight) * 100;
            }
            catch (Exception e)
            {
                return;
            }

            double ratioX = (float)((float)realWidth / (float)imageMapMarker.Width);
            double ratioY = (float)((float)realHeight / (float)imageMapMarker.Height);

            ///////////////////////////
            //Get Real Map Size END
            ///////////////////////////

            num_AliveTag = 0;
            num_AliveAnchor = 0;
            num_AliveGateway = 0;


            ///////////////////////////
            //Draw Marker START
            ///////////////////////////

            for (int i = 0; i < listMarker.Count; i++)
            {
                if (listMarker[i].devListener.devType.Contains("Tag"))
                {

                    Color mColor = Colors.Red;

                    //Color Setting
                    string json_data = config.fn_DeviceAddRead(TAG);
                    var device_data = JsonConvert.DeserializeObject<List<class_listener_list>>(json_data);
                    List<class_listener_list> list_DevData = new List<class_listener_list>();
                    if (device_data != null) list_DevData = device_data;

                    int resultIndex = 0;

                    if (list_DevData != null)
                    {
                        resultIndex = list_DevData.FindIndex(x => x.devSN.Equals(listMarker[i].devListener.devSN));
                        if (resultIndex != -1)
                        {
                            if (list_DevData[resultIndex].devColor != null)
                                mColor = (Color)ColorConverter.ConvertFromString(list_DevData[resultIndex].devColor);
                        }
                    }


                    listMarker[i].mEllipse.Stroke = new SolidColorBrush(mColor);
                    listMarker[i].mEllipse.Visibility = Visibility.Visible;
                    listMarker[i].mLabel.Foreground = new SolidColorBrush(mColor);
                    listMarker[i].mLabel.Visibility = Visibility.Visible;

                    long nowTime = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - listMarker[i].devListener.time;

                    if (nowTime > config.Alive_TAG)
                    {
                        listMarker[i].mEllipse.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                    }
                    else
                    {
                        listMarker[i].mEllipse.Fill = new SolidColorBrush(mColor);
                        num_AliveTag = num_AliveTag + 1;
                    }

                    String getMarkerText = setMarkerText(listMarker[i]);

                    double tagX, tagY;

                    if (fDeviceFilter)
                    {
                        tagX = listMarker[i].devListener.tag_lpf_x * 100;
                        tagY = listMarker[i].devListener.tag_lpf_y * 100;
                    }
                    else
                    {
                        tagX = listMarker[i].devListener.tag_pos_x * 100;
                        tagY = listMarker[i].devListener.tag_pos_y * 100;
                    }

                    int drawX = (int)(tagX / ratioX);
                    int drawY = (int)(tagY / ratioY);

                    Thickness thick = listMarker[i].mEllipse.Margin;
                    thick.Left = drawX - Marker_MarginSize;
                    thick.Top = imageMapMarker.Height - drawY - Marker_MarginSize;
                    listMarker[i].mEllipse.Margin = thick;

                    listMarker[i].mLabel.Content = getMarkerText;
                    thick = listMarker[i].mLabel.Margin;
                    thick.Left = drawX + 10 - Marker_MarginSize;
                    thick.Top = imageMapMarker.Height - drawY - 10 - Marker_MarginSize;
                    listMarker[i].mLabel.Margin = thick;

                    //imageMapMarker
                }
                else if (listMarker[i].devListener.devType.Contains("Anchor"))
                {

                    Color mColor = Colors.Blue;


                    //Color Setting
                    string json_data = config.fn_DeviceAddRead(TAG);
                    var device_data = JsonConvert.DeserializeObject<List<class_listener_list>>(json_data);
                    List<class_listener_list> list_DevData = new List<class_listener_list>();
                    if (device_data != null) list_DevData = device_data;

                    int resultIndex = 0;

                    if (list_DevData != null)
                    {
                        resultIndex = list_DevData.FindIndex(x => x.devSN.Equals(listMarker[i].devListener.devSN));
                        if (resultIndex != -1)
                        {
                            if (list_DevData[resultIndex].devColor != null)
                                mColor = (Color)ColorConverter.ConvertFromString(list_DevData[resultIndex].devColor);
                        }
                    }

                    listMarker[i].mEllipse.Stroke = new SolidColorBrush(mColor);
                    listMarker[i].mEllipse.Visibility = Visibility.Visible;
                    listMarker[i].mLabel.Foreground = new SolidColorBrush(mColor);
                    listMarker[i].mLabel.Visibility = Visibility.Visible;

                    listMarker[i].mEllipse.Fill = new SolidColorBrush(mColor);
                    num_AliveAnchor = num_AliveAnchor + 1;

                    String getMarkerText = setMarkerText(listMarker[i]);

                    double anchorX = listMarker[i].devListener.tag_pos_x * 100;
                    double anchorY = listMarker[i].devListener.tag_pos_y * 100;

                    int drawX = (int)(anchorX / ratioX);
                    int drawY = (int)(anchorY / ratioY);

                    Thickness thick = listMarker[i].mEllipse.Margin;
                    thick.Left = drawX - Marker_MarginSize;
                    thick.Top = imageMapMarker.Height - drawY - Marker_MarginSize;
                    listMarker[i].mEllipse.Margin = thick;

                    listMarker[i].mLabel.Content = getMarkerText;
                    thick = listMarker[i].mLabel.Margin;
                    thick.Left = drawX + 10 - Marker_MarginSize;
                    thick.Top = imageMapMarker.Height - drawY - 10 - Marker_MarginSize;
                    listMarker[i].mLabel.Margin = thick;
                }

                ///////////////////////////
                //Draw Marker END
                ///////////////////////////

                config.set_AliveTag(num_AliveTag);
                config.set_AliveAnchor(num_AliveAnchor);
                config.set_AliveGateway(num_AliveGateway);

            }
        }

        private string setMarkerText(class_Marker clMarker)
        {
            String retVal = "";

            if (fDeviceID)
            {
                retVal += clMarker.devListener.devSN;
            }
            if (fDeviceName)
            {
                if (fDeviceID) retVal += ",";
                retVal += clMarker.devListener.devTagName;
            }

            if (fDevicePosition)
            {
                if (fDeviceID || fDeviceName) retVal += ",";
                if (clMarker.devListener.devType.Contains("Tag"))
                {
                    if (!fDeviceFilter)
                    {
                        retVal += Math.Round(clMarker.devListener.tag_pos_x, 2) +
                            "," + Math.Round(clMarker.devListener.tag_pos_y, 2) +
                            "," + Math.Round(clMarker.devListener.tag_pos_z, 2);
                    }
                    else
                    {
                        retVal += Math.Round(clMarker.devListener.tag_lpf_x, 2) +
                            "," + Math.Round(clMarker.devListener.tag_lpf_y, 2) +
                            "," + Math.Round(clMarker.devListener.tag_lpf_z, 2);
                    }
                }
                else if (clMarker.devListener.devType.Contains("Anchor"))
                {
                    retVal += Math.Round(clMarker.devListener.tag_pos_x, 2) +
                        "," + Math.Round(clMarker.devListener.tag_pos_y, 2) +
                        "," + Math.Round(clMarker.devListener.tag_pos_z, 2);
                }
            }

            return retVal;
        }


    }
}
