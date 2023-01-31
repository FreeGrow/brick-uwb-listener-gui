using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using wpf_UWB_GUI.Listener;
using static wpf_UWB_GUI.UC_Menu_listener;

namespace wpf_UWB_GUI
{
    public delegate bool MainGetConnectStateHandler();
    public delegate List<class_listener_list> MainGetAnchorListHandler();
    public delegate void MainClickAutoAnchorHandler();
    public delegate void MainRemoveDataHandler(String mdevSN);
    public delegate List<class_listener_list> MainGetParseDataAllHandler();
    public delegate void MainSetFilterDataHandler(class_listener_list cl_Tmp);

    public delegate void MainSetMqttConnectHandler(listener_mqttConnect serialConnect);

    public partial class MainWindow : Window
    {
        String TAG = "MainWindow";

        const int StateTitleListener = 2;
        const int StateTab_Gat_com = 1;
        const int StateTab_Gat_fil = 2;
        const int StateTab_Gat_dev = 3;
        const int StateTab_Gat_map = 4;
        const int StateTab_Gat_set = 5;
        const int StateTab_Gat_sys = 6;

        int nowTitleState = StateTitleListener;
        int prevTitleState = 0;
        int nowTabState = StateTab_Gat_com;
        int prevTabState = 0;
        int nowMainState = StateTab_Gat_com;
        int prevMainState = 0;

        public static Frame _frame_listener_com;
        public static Frame _frame_listener_filter;

        UC_Menu_listener ucMenuListener = new UC_Menu_listener();

        UC_main_listener_com ucMainListenerCom = new UC_main_listener_com();
        UC_main_listener_filter ucMainListenerFilter = new UC_main_listener_filter();
        UC_main_listener_device ucMainListenerDevice = new UC_main_listener_device();
        UC_main_listener_map ucMainListenerMap = new UC_main_listener_map();
        UC_main_listener_setting ucMainListenerSetting = new UC_main_listener_setting();
        UC_main_listener_info ucMainListenerInfo = new UC_main_listener_info();


        Color titleSelectColor = (Color)ColorConverter.ConvertFromString("#FF1457ED");
        Color titleUnSelectColor = (Color)ColorConverter.ConvertFromString("#FF161618");

        bool fListenerConnectState = false;

        List<class_listener_list> listListenerParse = new List<class_listener_list>();


        DispatcherTimer timer10hz = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            //Properties.Settings.Default.Reset();
        }

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("MainForm_Loaded()");

            prevTitleState = nowTitleState;
            prevTabState = nowTabState;
            prevMainState = nowMainState;

            timer10hz.Interval = TimeSpan.FromMilliseconds(0.01);
            timer10hz.Tick += new EventHandler(timer10hz_Tick);
            timer10hz.Start();

            ucMenuListener.setListenerMenuButton += new MainGetMenuListenerDataEventHandler(this.setListenerMenuButton);

            tabBarLayout.Children.Clear();
            tabBarLayout.Children.Add(ucMenuListener);

            //Connection State Hanlder
            ucMainListenerFilter.getMainConnectHandler += new MainGetConnectStateHandler(getMainConnectState);
            ucMainListenerDevice.getMainConnectHandler += new MainGetConnectStateHandler(getMainConnectState);
            ucMainListenerMap.getMainConnectHandler += new MainGetConnectStateHandler(getMainConnectState);

            //com -> Main ( UWB DATA )
            ucMainListenerCom.listenerParse += new UC_main_listener_com.OnListenerParseHandler(setListenerParse);
            ucMainListenerCom.listenerConnect += new UC_main_listener_com.OnListenerConnectHandler(setListenerState);
            ucMainListenerCom.setSerialHandler += new MainSetMqttConnectHandler(setSerialHandler);

            //Main -> Filter ( UWB DATA )
            ucMainListenerFilter.setFilterHandler += new MainSetFilterDataHandler(setFilterHandler);
            ucMainListenerFilter.getParseDataAll += new MainGetParseDataAllHandler(getListenerParseDataAll);

            //Main -> Device ( UWB DATA )
            ucMainListenerDevice.getParseDataAll += new MainGetParseDataAllHandler(getListenerParseDataAll);
            ucMainListenerDevice.removeParseData += new MainRemoveDataHandler(removeListenerParseData);
            ucMainListenerDevice.getAnchorList += new MainGetAnchorListHandler(getAnchorList);
            ucMainListenerDevice.clickAutoAnchor += new MainClickAutoAnchorHandler(clickAutoAnchor);
            //Main -> Map ( UWB DATA )
            ucMainListenerMap.getParseDataAll += new MainGetParseDataAllHandler(getListenerParseDataAll);

            mainPageLayout.Navigate(ucMainListenerCom);

        }

        listener_mqttConnect serialConnect;

        private void setSerialHandler(listener_mqttConnect mSerial)
        {
            Console.WriteLine("setSerialHandler()");
            serialConnect = mSerial;

            if (serialConnect == null) return;
            if (serialConnect.fConnectState())
                serialConnect.sp_EnterTwice();
        }

        private void clickAutoAnchor()
        {
            if (serialConnect != null)
            {
                serialConnect.sp_AnchorList();
            }
        }

        private List<class_listener_list> getAnchorList()
        {
            if (serialConnect != null)
            {
                return serialConnect.getAnchorList();
            }
            return null;
        }


        private void setFilterHandler(class_listener_list cl_Tmp)
        {
            if (listListenerParse.Count == 0) return;
            int result_num = listListenerParse.FindIndex(x => x.devSN.Equals(cl_Tmp.devSN));

            if (result_num != -1)
            {
                class_listener_list cl_item = listListenerParse.Find(x => x.devSN.Equals(cl_Tmp.devSN));
                cl_item.tag_lpf_x = cl_Tmp.tag_lpf_x;
                cl_item.tag_lpf_y = cl_Tmp.tag_lpf_y;
                cl_item.tag_lpf_z = cl_Tmp.tag_lpf_z;
            }
        }

        private bool getMainConnectState()
        {
            return fListenerConnectState;
        }

        
        private void setListenerParse(List<class_listener_list> mlistListenerParse)
        {
            if (listListenerParse.Count == 0)
            {
                for (int i = 0; i < mlistListenerParse.Count; i++)
                {
                    listListenerParse.Add(mlistListenerParse[i]);
                }
            }
            else
            {
                for (int i = 0; i < mlistListenerParse.Count; i++)
                {
                    int result_num = listListenerParse.FindIndex(x => x.devSN.Equals(mlistListenerParse[i].devSN));

                    if (result_num != -1)
                    {
                        class_listener_list cl_item = listListenerParse.Find(x => x.devSN.Equals(mlistListenerParse[i].devSN));
                        cl_item.tag_pos_x = mlistListenerParse[i].tag_pos_x;
                        cl_item.tag_pos_y = mlistListenerParse[i].tag_pos_y;
                        cl_item.tag_pos_z = mlistListenerParse[i].tag_pos_z;
                        cl_item.checkSum = mlistListenerParse[i].checkSum;
                        cl_item.pos_quality = mlistListenerParse[i].pos_quality;
                        cl_item.time = mlistListenerParse[i].time;
                    }
                    else
                    {
                        listListenerParse.Add(mlistListenerParse[i]);
                    }
                }

            }
        }

        private void removeListenerParseData(String mdevSN)
        {
            int resultNum = listListenerParse.FindIndex(x => x.devSN.Equals(mdevSN));
            if (resultNum != -1)
            {
                listListenerParse.RemoveAt(resultNum);
            }
        }

        private List<class_listener_list> getListenerParseDataAll()
        {
            if (listListenerParse.Count != 0)
                return listListenerParse;
            else
                return null;
        }

        private void removeListenerParseDataAll()
        {
            listListenerParse.Clear();
        }

        private class_listener_list getListenerParseDataOne()
        {
            if (listListenerParse.Count != 0)
                return listListenerParse[0];
            else
                return null;
        }

        private void removeListenerParseDataOne()
        {
            listListenerParse.RemoveAt(0);
        }

        private void setListenerState(bool fState)
        {
            fListenerConnectState = fState;
        }

        private void timer10hz_Tick(object sender, EventArgs e)
        {
            if (nowTitleState != prevTitleState)
            {
                prevTitleState = nowTitleState;
            }

            if (nowTabState != prevTabState)
            {
                prevTabState = nowTabState;
            }

            if (nowMainState != prevMainState)
            {
                prevMainState = nowMainState;
            }
        }

        //private void btn_gateway_click(object sender, RoutedEventArgs e)
        //{
        //    nowTitleState = StateTitleGateway;
        //    btn_Title_menu_click(nowTitleState);
        //}

        private void btn_listener_click(object sender, RoutedEventArgs e)
        {
            nowTitleState = StateTitleListener;
            btn_Title_menu_click(nowTitleState);
        }

        private void btn_Title_menu_click(int state)
        {
            tabBarLayout.Children.Clear();

            //if (state == StateTitleListener)
            //{
            //    //btn_mode_gateway.Background = new SolidColorBrush(titleUnSelectColor);
            //    btn_mode_listener.Background = new SolidColorBrush(titleSelectColor);

            //    tabBarLayout.Children.Add(ucMenuListener);
            //}
        }

        public void setGatewayMenuButton(int btnNumber)
        {

            //mainPageLayout.Children.Clear();

            //switch (btnNumber)
            //{
            //    case StateTab_Gat_com:
            //        mainPageLayout.Children.Add(ucMainGatewayCom);
            //        break;
            //    case StateTab_Gat_fil:
            //        mainPageLayout.Children.Add(ucMainGatewayFilter);
            //        break;
            //    case StateTab_Gat_dev:
            //        mainPageLayout.Children.Add(ucMainGatewayDevice);
            //        break;
            //    case StateTab_Gat_map:
            //        mainPageLayout.Children.Add(ucMainGatewayMap);
            //        break;
            //    case StateTab_Gat_set:
            //        mainPageLayout.Children.Add(ucMainGatewaySetting);
            //        break;
            //    case StateTab_Gat_sys:
            //        mainPageLayout.Children.Add(ucMainGatewayInfo);
            //        break;
            //}
        }

        public void setListenerMenuButton(int btnNumber)
        {
            switch (btnNumber)
            {
                case StateTab_Gat_com:
                    lbl_TopTitle.Content = "통신 설정";
                    lbl_TopSubTitle.Content = "리스너 장비와 통신 연결을 관리합니다.";
                    mainPageLayout.Navigate(ucMainListenerCom);
                    break;
                case StateTab_Gat_fil:
                    lbl_TopTitle.Content = "필터 설정";
                    lbl_TopSubTitle.Content = "장치 필터의 설정값을 관리합니다.";
                    mainPageLayout.Navigate(ucMainListenerFilter);
                    break;
                case StateTab_Gat_dev:
                    lbl_TopTitle.Content = "장비 관리";
                    lbl_TopSubTitle.Content = "장비의 연결상태, 표시색상 등 상태를 관리합니다.";
                    mainPageLayout.Navigate(ucMainListenerDevice);
                    break;
                case StateTab_Gat_map:
                    lbl_TopTitle.Content = "맵";
                    lbl_TopSubTitle.Content = "장비의 현재상태, 위치를 나타냅니다.";
                    mainPageLayout.Navigate(ucMainListenerMap);
                    break;
                case StateTab_Gat_set:
                    lbl_TopTitle.Content = "설정";
                    lbl_TopSubTitle.Content = "각종 설정을 합니다.";
                    mainPageLayout.Navigate(ucMainListenerSetting);
                    break;
                case StateTab_Gat_sys:
                    lbl_TopTitle.Content = "정보";
                    lbl_TopSubTitle.Content = "회사 정보, 프로그램 정보를 나타냅니다.";
                    mainPageLayout.Navigate(ucMainListenerInfo);
                    break;
            }
        }


        /********************************
         * CUSTOM TITLE BAR START
         ********************************/

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void MaximizeClick(object sender, RoutedEventArgs e)
        {
            if (App.Current.MainWindow.WindowState == WindowState.Maximized)
            {
                App.Current.MainWindow.WindowState = WindowState.Normal;
            }
            else if (App.Current.MainWindow.WindowState == WindowState.Normal)
            {
                App.Current.MainWindow.WindowState = WindowState.Maximized;
            }
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.Close();
        }

        private void TitleDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (App.Current.MainWindow.WindowState == WindowState.Maximized)
            {
                App.Current.MainWindow.WindowState = WindowState.Normal;
            }
            else if (App.Current.MainWindow.WindowState == WindowState.Normal)
            {
                App.Current.MainWindow.WindowState = WindowState.Maximized;
            }
        }

        private void TitleMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                App.Current.MainWindow.DragMove();
            }
        }

        /********************************
         * CUSTOM TITLE BAR END
         ********************************/

        //Form Size Changed Event
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double thisWidth = this.Height / 0.75f;
            this.Width = thisWidth;
        }
    }
}
