using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SendToWP7
{
    public class ItemViewModel : INotifyPropertyChanged
    {
        public int ID { get; set; }

        private string _Title;
        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                if (value != _Title)
                {
                    _Title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        private DateTime _Downloaded;
        public DateTime Downloaded
        {
            get
            {
                return _Downloaded;
            }
            set
            {
                if (value != _Downloaded)
                {
                    _Downloaded = value;
                    NotifyPropertyChanged("Downloaded");
                }
            }
        }

        private bool _Checked = false;
        public bool IsChecked
        {
            get
            {
                return _Checked;
            }
            set
            {
                if (value != _Checked)
                {
                    _Checked = value;
                    NotifyPropertyChanged("IsChecked");
                }
            }
        }

        private string _URL;
        public string URL
        {
            get
            {
                return _URL;
            }
            set
            {
                if (value != _URL)
                {
                    _URL = value;
                    NotifyPropertyChanged("URL");
                }
            }
        }

        private string _Selection;
        public string Selection
        {
            get
            {
                return _Selection;
            }
            set
            {
                if (value != _Selection)
                {
                    _Selection = value;
                    NotifyPropertyChanged("Selection");
                }
            }
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
