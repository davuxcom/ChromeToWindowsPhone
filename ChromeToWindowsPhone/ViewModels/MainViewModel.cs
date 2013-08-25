using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using DavuxLibSL;
using System.Net;
using Newtonsoft.Json;


namespace SendToWP7
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public PushNotifications ph = null;
        public MainViewModel()
        {
            this.Items = new ObservableCollection<ItemViewModel>();
            //DavuxLibSL.Settings.Set("__PushURL__", "");
            //DavuxLibSL.Settings.Set("PairCode", 1418055394);
            // DavuxLibSL.Settings.Set("Links", null);
        }

        public ItemViewModel SelectedRecord { get; set; }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<ItemViewModel> Items { get; private set; }


        private string _PairCode = "Undefined";
        public string PairCode
        {
            get
            {
                return _PairCode;
            }
            set
            {
                if (_PairCode != value)
                {
                    _PairCode = value;
                    NotifyPropertyChanged("PairCode");
                }
            }
        }

        public bool IsDataLoaded { get; private set; }

        private bool _IsEmpty = false;
        public bool IsEmpty
        {
            get
            {
                return _IsEmpty;
            }
            set
            {
                if (_IsEmpty != value)
                {
                    _IsEmpty = value;
                    NotifyPropertyChanged("IsEmpty");
                }
            }
        }

        private bool _IsLoadingData = false;
        public bool IsLoadingData
        {
            get
            {
                return _IsLoadingData;
            }
            set
            {
                if (_IsLoadingData != value)
                {
                    _IsLoadingData = value;
                    NotifyPropertyChanged("IsLoadingData");
                }
            }
        }

        private bool _IsEditMode = false;
        public bool IsEditMode
        {
            get
            {
                return _IsEditMode;
            }
            set
            {
                if (_IsEditMode != value)
                {
                    _IsEditMode = value;
                    NotifyPropertyChanged("IsEditMode");
                }
            }
        }


        private string _LoadingDataMsg = "";
        public string LoadingDataMsg
        {
            get
            {
                return _LoadingDataMsg;
            }
            set
            {
                if (_LoadingDataMsg != value)
                {
                    _LoadingDataMsg = value;
                    NotifyPropertyChanged("LoadingDataMsg");
                }
            }
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            IsLoadingData = true;
            LoadingDataMsg = "Connecting...";

            List<ItemViewModel> store = DavuxLibSL.Settings.Get<List<ItemViewModel>>("Links", null);
            if (store != null)
            {
                store.ForEach(i => App.ViewModel.Items.Add(i));
            }

            IsEmpty = App.ViewModel.Items.Count == 0;

            WebPair wp = new WebPair();
            wp.ErrorOccured += (error) =>
                {
                    // return true to retry
                    return MessageBox.Show("Couldn't get a pair code: " + error + "\nTap OK to try again", "Error", MessageBoxButton.OKCancel) == MessageBoxResult.OK;
                };
            wp.PairCodeArrived += code =>
                {
                    // this will fire immediately if we already have a code
                    PushNotifications.IsDoneLoading = true;
                    PairCode = code;
                    Update();
                };
            wp.Register();

            //DavuxLibSL.Settings.Set("__TryPushEnabled__", true);
            //DavuxLibSL.Settings.Set("__PushEnabled__", false);

            // this is a reminder that I'm an idiot.
            PushNotifications.RemoveOldChannel("com.Davux.ChromeT=oWindowsPhone");
            PushNotifications.RemoveOldChannel("com.Davux.ChromeToWindowsPhone");

            PushNotifications.OnToastNotification += (title, msg) => Update();
            PushNotifications.ErrorOccurred += msg => MessageBox.Show("Push Notification error: " + msg);
            PushNotifications.ChannelName = "com.davux.ChromeToWindowsPhone";
            PushNotifications.Initialize();
            PushNotifications.TryEnableOnce();

            IsDataLoaded = true;
        }

        public void Update()
        {
            // if we need to upgrade, do that first so those links are oldest!
            if (Upgrade()) return;


            IsLoadingData = true;
            LoadingDataMsg = "Downloading...";
            string url = "http://www.daveamenta.com/wp7api/com.davux.ChromeToWindowsPhone/fetch.php?paircode={0}&r={1}";
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += (ss, ee) =>
                {
                    try
                    {
                        if (ee.Error != null)
                        {
                            MessageBox.Show("Unable to fetch links: " + ee.Error.Message);
                            IsLoadingData = false;
                            LoadingDataMsg = "";
                        }
                        else
                        {
                            UpdateCompleted(ee.Result, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Unable to fetch links: " + ex.Message);
                        IsLoadingData = false;
                        LoadingDataMsg = "";
                    }
                };
            wc.DownloadStringAsync(new Uri(string.Format(url, PairCode, new Random().Next(0, 99999))));
        }

        private bool Upgrade()
        {
            Debug.WriteLine("Checking for upgrade");
            string legacy_code = DavuxLibSL.Settings.Get("PairCode", 0).ToString();
            if (legacy_code == "0") return false; // no upgrade needed!
            Debug.WriteLine("Upgrading...");
            IsLoadingData = true;
            LoadingDataMsg = "Upgrading...";
            string url = "http://www.daveamenta.com/wp7api/com.davux.ChromeToWindowsPhone/get.php?passcode={0}&r={1}";
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += (ss, ee) =>
            {
                try
                {
                    if (ee.Error != null)
                    {
                        MessageBox.Show("Unable to fetch old links: " + ee.Error.Message);
                        IsLoadingData = false;
                        LoadingDataMsg = "";
                    }
                    else
                    {
                        UpdateCompleted(ee.Result, false);
                        // upgrade success!  don't do it again, though.
                        DavuxLibSL.Settings.Set("PairCode", 0);
                        
                        // now get the new stuff
                        Update();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to fetch old links: " + ex.Message);
                    IsLoadingData = false;
                    LoadingDataMsg = "";
                }
            };
            wc.DownloadStringAsync(new Uri(string.Format(url, legacy_code, new Random().Next(0, 99999))));

            return true; // upgrade in progress
        }

        void UpdateCompleted(string data, bool delete)
        {
            int max_id = -1;
            try
            {
                var myObjects = JsonConvert.DeserializeObject<Dictionary<string, string>[]>(data);

                foreach (var x in myObjects)
                {
                    var item = new ItemViewModel();
                    item.Title = x["Title"];
                    item.URL = x["URL"];
                    item.Selection = x["Selection"];
                    item.Downloaded = DateTime.Now;
                    if (x.ContainsKey("ID"))
                    {
                        // the old API does not have this.
                        item.ID = int.Parse(x["ID"]);
                        if (item.ID > max_id) max_id = item.ID;
                    }
                    App.ViewModel.Items.Insert(0, item);
                }

                if (App.ViewModel.Items.Count > 0)
                {
                    Save();
                }
                IsEmpty = App.ViewModel.Items.Count == 0;

                if (delete && max_id > -1) Delete(max_id);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to understand response: " + ex.Message);
            }
            IsLoadingData = false;
            LoadingDataMsg = "";
        }

        void Delete(int highest)
        {
            Debug.WriteLine("Removing links less than " + highest);
            IsLoadingData = true;
            LoadingDataMsg = "Syncing...";
            string url = "http://www.daveamenta.com/wp7api/com.davux.ChromeToWindowsPhone/delete.php?paircode={0}&n={1}";
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += (ss, ee) =>
            {
                try
                {
                    if (ee.Error != null)
                    {
                        MessageBox.Show("Unable to fetch links: " + ee.Error.Message);
                    }
                    else
                    {
                        if (ee.Result != "OK")
                        {
                            MessageBox.Show("Error removing old links: " + ee.Result);
                        }
                    }
                    IsLoadingData = false;
                    LoadingDataMsg = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to fetch links: " + ex.Message);
                    IsLoadingData = false;
                    LoadingDataMsg = "";
                }
            };
            wc.DownloadStringAsync(new Uri(string.Format(url, PairCode, highest)));
        }

        public void Save()
        {
            List<ItemViewModel> store = new List<ItemViewModel>();
            foreach (var item in App.ViewModel.Items)
            {
                store.Add(item);
            }
            DavuxLibSL.Settings.Set<List<ItemViewModel>>("Links", store);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }


    }
}