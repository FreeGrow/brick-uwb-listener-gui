using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace wpf_UWB_GUI
{
    /// <summary>
    /// UC_main_gateway_com.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_main_listener_filter : Page
    {

        private String TAG = "UC_main_listener_filter.xaml";


        public MainSetFilterDataHandler setFilterHandler;
        public MainGetConnectStateHandler getMainConnectHandler;

        public MainGetParseDataAllHandler getParseDataAll;


        bool fStart = false;

        DispatcherTimer timer10hz = new DispatcherTimer();

        String strSaveRadio = "";
        String strSaveAveValue = "";
        String strSaveLpfValue = "";
        String strSavePath = "";

        List<Control> listCursorControl = new List<Control>();

        public UC_main_listener_filter()
        {
            InitializeComponent();

            listCursorControl.Add(chk_none);
            listCursorControl.Add(label_none);
            listCursorControl.Add(chk_movingAverage);
            listCursorControl.Add(label_ave);
            listCursorControl.Add(chk_lpf);
            listCursorControl.Add(label_lpf);
            listCursorControl.Add(btn_start);
            listCursorControl.Add(btn_stop);
            listCursorControl.Add(btn_filepath);

            for (int i = 0; i < listCursorControl.Count; i++)
            {
                listCursorControl[i].MouseEnter += new MouseEventHandler(mouseEnterHandler);
                listCursorControl[i].MouseLeave += new MouseEventHandler(mouseLeaveHandler);
            }

            String str_filterValue = Properties.Settings.Default.filter_value;

            string[] words = str_filterValue.Split('#');

            if (words[0].Contains("1"))
            {
                fStart = true;
            }
            else if (words[0].Contains("0"))
            {
                fStart = false;
            }
            strSaveRadio = words[1];
            strSaveAveValue = words[2];
            strSaveLpfValue = words[3];
            strSavePath = words[4];

            if (strSaveRadio == "0")
            {
                radioCheckBox(0);
            }
            if (strSaveRadio == "1")
            {
                radioCheckBox(1);
                slider_fliterSlider.Maximum = 10;
                slider_fliterSlider.Value = int.Parse(strSaveAveValue);
                label_filterValue.Content = strSaveAveValue;
            }
            if (strSaveRadio == "2")
            {
                radioCheckBox(2);
                slider_fliterSlider.Maximum = 100;
                slider_fliterSlider.Value = int.Parse(strSaveLpfValue);
                label_filterValue.Content = strSaveLpfValue;
            }

            if (strSavePath.Length == 0)
            {
                strSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\UWB\\";
            }

            textBox_filepath.Text = strSavePath;

            defineEvent();
            filter_Status(false);

            timer10hz.Interval = TimeSpan.FromMilliseconds(100);
            timer10hz.Tick += new EventHandler(timer10hz_Tick);
            timer10hz.Start();
        }

        private void mouseEnterHandler(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void mouseLeaveHandler(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        int prevMaxCount = 0;

        private void timer10hz_Tick(object sender, EventArgs e)
        {
            filter_connect_state(getMainConnectHandler());


            if (!fStart) return;

            List<class_listener_list> cl_ListTmp = getParseDataAll();
            if (cl_ListTmp != null)
            {
                for (int i = 0; i < cl_ListTmp.Count; i++)
                {
                    class_listener_list cl_Tmp = cl_ListTmp[i];
                    if (cl_Tmp != null && cl_Tmp.devType.Equals("Tag"))
                    {
                        //NONE
                        if (strSaveRadio.Contains("0"))
                        {
                            cl_Tmp.tag_lpf_x = cl_Tmp.tag_pos_x;
                            cl_Tmp.tag_lpf_y = cl_Tmp.tag_pos_y;
                            cl_Tmp.tag_lpf_z = cl_Tmp.tag_pos_z;
                        }
                        //MOVING AVERAGE
                        else if (strSaveRadio.Contains("1"))
                        {
                            int maxCount = (int)slider_fliterSlider.Value;

                            if (prevMaxCount != maxCount)
                            {
                                cl_Tmp.tag_ave_x.Clear();
                                cl_Tmp.tag_ave_y.Clear();
                                cl_Tmp.tag_ave_z.Clear();
                            }

                            if (maxCount == 0) return;

                            if (cl_Tmp.tag_ave_x.Count > maxCount - 1)
                            {
                                cl_Tmp.tag_ave_x.RemoveAt(0);
                                cl_Tmp.tag_ave_y.RemoveAt(0);
                                cl_Tmp.tag_ave_z.RemoveAt(0);
                            }

                            cl_Tmp.tag_ave_x.Add(cl_Tmp.tag_pos_x);
                            cl_Tmp.tag_ave_y.Add(cl_Tmp.tag_pos_y);
                            cl_Tmp.tag_ave_z.Add(cl_Tmp.tag_pos_z);

                            if (cl_Tmp.tag_ave_x.Count == maxCount)
                            {
                                double sumX = 0, sumY = 0, sumZ = 0;

                                for (int k = 0; k < maxCount; k++)
                                {
                                    sumX += cl_Tmp.tag_ave_x[k];
                                    sumY += cl_Tmp.tag_ave_y[k];
                                    sumZ += cl_Tmp.tag_ave_z[k];
                                }

                                sumX = sumX / maxCount;
                                sumY = sumY / maxCount;
                                sumZ = sumZ / maxCount;

                                cl_Tmp.tag_lpf_x = sumX;
                                cl_Tmp.tag_lpf_y = sumY;
                                cl_Tmp.tag_lpf_z = sumZ;

                                cl_Tmp.tag_lpf_x = Math.Round(cl_Tmp.tag_lpf_x, 4);
                                cl_Tmp.tag_lpf_y = Math.Round(cl_Tmp.tag_lpf_y, 4);
                                cl_Tmp.tag_lpf_z = Math.Round(cl_Tmp.tag_lpf_z, 4);
                            }

                            prevMaxCount = maxCount;
                        }
                        //LOW PASS FILTER
                        else if (strSaveRadio.Contains("2"))
                        {
                            float weightValue = (float)slider_fliterSlider.Value / 100.0f;
                            cl_Tmp.tag_lpf_x = cl_Tmp.tag_lpf_x * weightValue + cl_Tmp.tag_pos_x * (1 - weightValue);
                            cl_Tmp.tag_lpf_y = cl_Tmp.tag_lpf_y * weightValue + cl_Tmp.tag_pos_y * (1 - weightValue);
                            cl_Tmp.tag_lpf_z = cl_Tmp.tag_lpf_z * weightValue + cl_Tmp.tag_pos_z * (1 - weightValue);

                            cl_Tmp.tag_lpf_x = Math.Round(cl_Tmp.tag_lpf_x, 4);
                            cl_Tmp.tag_lpf_y = Math.Round(cl_Tmp.tag_lpf_y, 4);
                            cl_Tmp.tag_lpf_z = Math.Round(cl_Tmp.tag_lpf_z, 4);
                        }


                        String writeText = "";

                        writeText = cl_Tmp.devSN + "," +
                            string.Format("{0, 10:N2}", cl_Tmp.tag_pos_x) + "," +
                            string.Format("{0, 10:N2}", cl_Tmp.tag_pos_y) + "," +
                            string.Format("{0, 10:N2}", cl_Tmp.tag_pos_z);

                        config.fn_RawTextWrite("", strSavePath, writeText);

                        writeText = cl_Tmp.devSN + "," +
                            string.Format("{0, 10:N2}", cl_Tmp.tag_lpf_x) + "," +
                            string.Format("{0, 10:N2}", cl_Tmp.tag_lpf_y) + "," +
                            string.Format("{0, 10:N2}", cl_Tmp.tag_lpf_z);

                        config.fn_FilterTextWrite("", strSavePath, writeText);

                        setFilterHandler(cl_Tmp);
                    }
                }
            }
        }

        private void UC_main_listener_filter_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void defineEvent()
        {
            chk_none.Click += chk_none_Click;
            chk_movingAverage.Click += chk_movingAverage_Click;
            chk_lpf.Click += chk_lpf_Click;

            label_none.MouseUp += label_none_MouseUp;
            label_ave.MouseUp += label_ave_MouseUp;
            label_lpf.MouseUp += label_lpf_MouseUp;

            slider_fliterSlider.ValueChanged += slider_filterSlider_ValueChanged;

            btn_filepath.Click += btn_filePath_Click;

            btn_start.Click += btn_start_Click;
            btn_stop.Click += btn_stop_Click;
        }

        private void radioCheckBox(int num)
        {
            chk_none.IsChecked = false;
            chk_movingAverage.IsChecked = false;
            chk_lpf.IsChecked = false;
            grid_filterSetting.Visibility = Visibility.Visible;

            switch (num)
            {
                case 0:
                    strSaveRadio = "0";
                    grid_filterSetting.Visibility = Visibility.Hidden;
                    chk_none.IsChecked = true;
                    break;
                case 1:
                    strSaveRadio = "1";
                    slider_fliterSlider.Value = int.Parse(strSaveAveValue);
                    slider_fliterSlider.Maximum = 10;
                    label_filterValue.Content = strSaveAveValue;
                    chk_movingAverage.IsChecked = true;
                    break;
                case 2:
                    strSaveRadio = "2";
                    slider_fliterSlider.Value = int.Parse(strSaveLpfValue);
                    slider_fliterSlider.Maximum = 100;
                    label_filterValue.Content = strSaveLpfValue;
                    chk_lpf.IsChecked = true;
                    break;
            }
        }

        private void filter_connect_state(bool fState)
        {
            if (fState)
            {
                img_connect_status.Source = new BitmapImage(new Uri("/Resources/img_connected.png", UriKind.RelativeOrAbsolute));
                label_connect_status.Content = "Connected";
                label_connect_status.Foreground = new SolidColorBrush(config.connectColor);
            }
            else
            {
                img_connect_status.Source = new BitmapImage(new Uri("/Resources/img_disconnected.png", UriKind.RelativeOrAbsolute));
                label_connect_status.Content = "DisConnected";
                label_connect_status.Foreground = new SolidColorBrush(config.disconnectColor);
            }
        }

        private void filter_button_state(bool fState)
        {
            if (fState)
            {
                btn_start.IsEnabled = true;
                btn_start.Background = new SolidColorBrush(config.titleSelectColor);
                btn_stop.Background = new SolidColorBrush(config.titleUnSelectColor);
            }
            else
            {
                btn_start.IsEnabled = false;
                btn_stop.Background = new SolidColorBrush(config.titleSelectColor);
                btn_start.Background = new SolidColorBrush(config.titleUnSelectColor);
            }
        }

        private void filter_Status(bool fState)
        {
            if (fState)
            {
                filter_button_state(false);
                chk_none.IsEnabled = false;
                label_none.IsEnabled = false;
                chk_movingAverage.IsEnabled = false;
                label_ave.IsEnabled = false;
                chk_lpf.IsEnabled = false;
                label_lpf.IsEnabled = false;
                slider_fliterSlider.IsEnabled = false;
                label_filterValue.IsEnabled = false;
                btn_filepath.IsEnabled = false;
                textBox_filepath.IsEnabled = false;
            }
            else
            {
                filter_button_state(true);
                chk_none.IsEnabled = true;
                label_none.IsEnabled = true;
                chk_movingAverage.IsEnabled = true;
                label_ave.IsEnabled = true;
                chk_lpf.IsEnabled = true;
                label_lpf.IsEnabled = true;
                slider_fliterSlider.IsEnabled = true;
                label_filterValue.IsEnabled = true;
                btn_filepath.IsEnabled = true;
                textBox_filepath.IsEnabled = true;
            }
        }

        private void btn_filePath_Click(object sender, RoutedEventArgs e)
        {
            String file_path = null;
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                file_path = dialog.SelectedPath;
                textBox_filepath.Text = file_path;

                strSavePath = file_path;

                String tmpStart = "0";
                if (fStart) tmpStart = "1";
                else tmpStart = "0";
                Properties.Settings.Default.filter_value = tmpStart + "#" + strSaveRadio + "#" + strSaveAveValue + "#" + strSaveLpfValue + "#" + strSavePath;
                Properties.Settings.Default.Save();
            }
        }

        private void chk_none_Click(object sender, RoutedEventArgs e)
        {
            radioCheckBox(0);
        }

        private void chk_movingAverage_Click(object sender, RoutedEventArgs e)
        {
            radioCheckBox(1);
        }

        private void chk_lpf_Click(object sender, RoutedEventArgs e)
        {
            radioCheckBox(2);
        }

        private void slider_filterSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (chk_movingAverage.IsChecked == true)
            {
                strSaveAveValue = slider_fliterSlider.Value + "";
            }
            if (chk_lpf.IsChecked == true)
            {
                strSaveLpfValue = slider_fliterSlider.Value + "";
            }

            label_filterValue.Content = slider_fliterSlider.Value.ToString();

            String tmpStart = "0";
            Properties.Settings.Default.filter_value = tmpStart + "#" + strSaveRadio + "#" + strSaveAveValue + "#" + strSaveLpfValue + "#" + strSavePath;
            Properties.Settings.Default.Save();
        }

        private void label_none_MouseUp(object sender, MouseButtonEventArgs e)
        {
            radioCheckBox(0);
        }

        private void label_ave_MouseUp(object sender, MouseButtonEventArgs e)
        {
            radioCheckBox(1);
        }

        private void label_lpf_MouseUp(object sender, MouseButtonEventArgs e)
        {
            radioCheckBox(2);
        }


        private void btn_start_Click(object sender, RoutedEventArgs e)
        {
            filter_Status(true);

            fStart = true;

            String tmpStart = "0";
            Properties.Settings.Default.filter_value = tmpStart + "#" + strSaveRadio + "#" + strSaveAveValue + "#" + strSaveLpfValue + "#" + strSavePath;
            Properties.Settings.Default.Save();
        }

        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            filter_Status(false);

            fStart = false;
        }
    }
}
